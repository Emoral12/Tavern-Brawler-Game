using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Hotbar : MonoBehaviour
{
    [Header("Hotbar Settings")]
    [SerializeField] private int slotCount = 3;
    [SerializeField] private float dropForce = 5f;
    [SerializeField] private float dropDistance = 2f;
    
    [Header("Colors")]
    [SerializeField] private Color selectedSlotColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color unselectedSlotColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color numberColor = new Color(1f, 1f, 1f, 0.8f);
    
    [Header("References")]
    [SerializeField] private InventorySlot[] slots; 
    [SerializeField] private TextMeshProUGUI[] slotNumbers; 
    
    private Item[] items;
    private int selectedSlot = 0;
    private Camera mainCamera;
    private InventorySystem inventorySystem;

    private void Start()
    {
        mainCamera = Camera.main;
        items = new Item[slotCount];
        
        // find inventory system
        inventorySystem = FindObjectOfType<InventorySystem>();
        if (inventorySystem == null)
        {
            Debug.LogError("Could not find InventorySystem in scene! Make sure it exists.");
            return;
        }

        // check slot
        if (slots == null || slots.Length == 0)
        {
            Debug.LogError("Hotbar slots not assigned in inspector!");
            return;
        }

        Debug.Log("Starting to initialize slots...");
       
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                Debug.Log($"Initializing hotbar slot {i}");
                slots[i].Initialize(i, inventorySystem, true);
                
                
                if (slots[i].GetComponent<InventorySlot>().inventorySystem == null)
                {
                    Debug.LogError($"Failed to set inventorySystem on slot {i}");
                }
                
                if (slotNumbers != null && i < slotNumbers.Length && slotNumbers[i] != null)
                {
                    slotNumbers[i].text = (i + 1).ToString();
                    slotNumbers[i].color = numberColor;
                }
            }
            else
            {
                Debug.LogError($"Slot {i} is null!");
            }
        }
        
        UpdateUI();
    }
    
    private void Update()
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
        
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel > 0f)
        {
            SelectSlot((selectedSlot + 1) % slotCount);
        }
        else if (scrollWheel < 0f)
        {
            SelectSlot((selectedSlot - 1 + slotCount) % slotCount);
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropSelectedItem();
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseSelectedItem();
        }
    }
    
    public void SelectSlot(int index)
    {
        selectedSlot = Mathf.Clamp(index, 0, slotCount - 1);
        UpdateUI();
    }
    
    public bool AddItem(Item item)
    {
        if (slots == null || items == null) return false;  

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null && slots[i] != null)  
            {
                items[i] = item;
                slots[i].SetItem(item);
                UpdateUI();
                return true;
            }
        }
        return false;
    }
    
    public void SetItem(int index, Item item)
    {
        if (index >= 0 && index < items.Length)
        {
            items[index] = item;
            slots[index].SetItem(item);
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                
                Image backgroundImage = slots[i].GetComponent<Image>();
                if (backgroundImage == null)
                {
                    
                    backgroundImage = slots[i].transform.GetComponent<Image>();
                }

                if (backgroundImage != null)
                {
                    backgroundImage.color = (i == selectedSlot) ? selectedSlotColor : unselectedSlotColor;
                }
                else
                {
                    Debug.LogError($"No Image component found on slot {i}");
                }
            }
        }
    }

    private void DropSelectedItem()
    {
        if (items[selectedSlot] == null) return;
        
        Vector3 spawnPos = mainCamera.transform.position + mainCamera.transform.forward * dropDistance;
        GameObject droppedItem = Instantiate(items[selectedSlot].prefab, spawnPos, Quaternion.identity);
        
        if (droppedItem.TryGetComponent(out Rigidbody rb))
        {
            rb.AddForce(mainCamera.transform.forward * dropForce, ForceMode.Impulse);
        }
        
        items[selectedSlot] = null;
        slots[selectedSlot].SetItem(null);
        UpdateUI();
    }
    
    private void UseSelectedItem()
    {
        if (items[selectedSlot] != null && items[selectedSlot].isUsable)
        {
            items[selectedSlot].Use();
        }
    }

    public Item GetItem(int index)
    {
        if (index >= 0 && index < items.Length)
        {
            return items[index];
        }
        return null;
    }

    public Item GetSelectedItem()
    {
        if (selectedSlot >= 0 && selectedSlot < slots.Length)
        {
            return items[selectedSlot];
        }
        return null;
    }

    public void RemoveSelectedItem()
    {
        if (selectedSlot >= 0 && selectedSlot < slots.Length)
        {
            items[selectedSlot] = null;
            slots[selectedSlot].SetItem(null);
            UpdateUI();
        }
    }
}

