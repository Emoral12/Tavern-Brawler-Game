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
    [SerializeField] private Image[] slotBackgrounds;
    [SerializeField] private Image[] slotIcons;
    [SerializeField] private TextMeshProUGUI[] slotNumbers;
    
    private Item[] items;
    private int selectedSlot = 0;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        items = new Item[slotCount];
        
        for (int i = 0; i < slotNumbers.Length; i++)
        {
            slotNumbers[i].text = (i + 1).ToString();
            slotNumbers[i].color = numberColor;
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
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                UpdateUI();
                return true;
            }
        }
        return false;
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
        UpdateUI();
    }
    
    private void UseSelectedItem()
    {
        if (items[selectedSlot] != null && items[selectedSlot].isUsable)
        {
            items[selectedSlot].Use();
        }
    }
    
    private void UpdateUI()
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (items[i] != null)
            {
                slotIcons[i].sprite = items[i].icon;
                slotIcons[i].color = Color.white;
            }
            else
            {
                slotIcons[i].sprite = null;
                slotIcons[i].color = Color.clear;
            }
            
            slotBackgrounds[i].color = (i == selectedSlot) ? selectedSlotColor : unselectedSlotColor;
        }
    }
}
