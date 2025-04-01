using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int rows = 4;
    [SerializeField] private int slotsPerRow = 2;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private Hotbar hotbar;

    [Header("Dragging References")]
    [SerializeField] private Image draggedItemImage;
    
    [Header("Testing")]
    [SerializeField] private Item[] testItems;

    [SerializeField] private int inventorySize = 10;
    [SerializeField] private List<Item> items = new List<Item>();
    [SerializeField] private Transform inventoryUIParent;
    [SerializeField] private GameObject inventorySlotPrefab;
    
    [Header("Selection")]
    [SerializeField] private int selectedSlot = 0;
    [SerializeField] private Color normalSlotColor = Color.white;
    [SerializeField] private Color selectedSlotColor = Color.yellow;
    
    private Item[] inventoryItems;
    private List<InventorySlot> slots = new List<InventorySlot>();
    private bool isInventoryOpen = false;
    private InventorySlot currentDraggedSlot;
    private int totalSlots;
    private bool wasMouseVisible;
    private bool wasMouseLocked;
    private PlayerController playerController;
    private List<GameObject> uiSlots = new List<GameObject>();

    private void Start()
    {
        totalSlots = rows * slotsPerRow;
        inventoryItems = new Item[totalSlots];
        InitializeInventoryUI();
        inventoryPanel.SetActive(false);
        draggedItemImage.gameObject.SetActive(false);
        draggedItemImage.rectTransform.position = new Vector3(-1000, -1000, 0);
        playerController = FindObjectOfType<PlayerController>();
        
        for (int i = items.Count; i < inventorySize; i++)
        {
            items.Add(null);
        }
        
        if (inventoryUIParent != null && inventorySlotPrefab != null)
        {
            CreateInventoryUI();
        }
        
        SelectSlot(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }

        if (Input.GetKeyDown(KeyCode.T) && testItems.Length > 0)
        {
            AddItem(testItems[0]);
        }

        if (isInventoryOpen)
        {
            UpdateDraggedItem();
        }

        for (int i = 0; i < 10; i++)
        {
            KeyCode keyCode = (i < 9) ? KeyCode.Alpha1 + i : KeyCode.Alpha0;
            
            if (Input.GetKeyDown(keyCode))
            {
                SelectSlot(i);
                Debug.Log($"Selected inventory slot {i}");
                break;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.F) && selectedSlot >= 0 && selectedSlot < items.Count)
        {
            UseSelectedItem();
        }
    }

    private void InitializeInventoryUI()
    {
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            slot.Initialize(i, this);
            slots.Add(slot);
            RectTransform rectTransform = slotObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(60, 60);
        }
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            playerController.enabled = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
        }
        else
        {
            playerController.enabled = true;
            if (currentDraggedSlot != null)
            {
                EndDrag();
            }
            Time.timeScale = 1f;
        }
    }

    public void BeginDrag(InventorySlot slot)
    {
        if (slot.HasItem)
        {
            Debug.Log($"Beginning drag... DraggedItemImage: {draggedItemImage != null}");
            currentDraggedSlot = slot;
            if (draggedItemImage != null)
            {
                draggedItemImage.sprite = slot.GetItem().icon;
                draggedItemImage.color = Color.white;
                draggedItemImage.gameObject.SetActive(true);
                Debug.Log($"DraggedItemImage activated, sprite: {draggedItemImage.sprite != null}");
            }
            else
            {
                Debug.LogError("DraggedItemImage is null!");
            }
        }
    }

    public void EndDrag()
    {
        Debug.Log("Ending drag...");
        currentDraggedSlot = null;
        draggedItemImage.sprite = null;
        draggedItemImage.gameObject.SetActive(false);
    }

    private void UpdateDraggedItem()
    {
        if (draggedItemImage != null && draggedItemImage.gameObject.activeSelf)
        {
            Vector3 newPos = Input.mousePosition;
            draggedItemImage.transform.position = newPos;
            Debug.Log($"Updating dragged item position: {newPos}");
        }
    }

    public void DropItemIntoSlot(InventorySlot targetSlot)
    {
        if (currentDraggedSlot == null) return;

        Item draggedItem = currentDraggedSlot.GetItem();
        Item targetItem = targetSlot.GetItem();

        if (targetSlot.IsHotbarSlot)
        {
            hotbar.SetItem(targetSlot.SlotIndex, draggedItem);
            currentDraggedSlot.SetItem(null);
        }
        else
        {
            inventoryItems[targetSlot.SlotIndex] = draggedItem;
            inventoryItems[currentDraggedSlot.SlotIndex] = targetItem;
            targetSlot.SetItem(draggedItem);
            currentDraggedSlot.SetItem(targetItem);
        }

        EndDrag();
    }

    public bool AddItem(Item item)
    {
        Debug.Log($"Trying to add item {item.name}");
        if (hotbar != null && hotbar.AddItem(item))
        {
            Debug.Log("Added to hotbar");
            return true;
        }

        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i] == null)
            {
                Debug.Log($"Adding to inventory slot {i}");
                inventoryItems[i] = item;
                slots[i].SetItem(item);
                return true;
            }
        }
        return false;
    }

    public Item GetItem(int index)
    {
        if (index >= 0 && index < inventoryItems.Length)
        {
            return inventoryItems[index];
        }
        return null;
    }

    private void CreateInventoryUI()
    {
        
        foreach (var slot in uiSlots)
        {
            Destroy(slot);
        }
        uiSlots.Clear();
        
        
        for (int i = 0; i < inventorySize; i++)
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, inventoryUIParent);
            uiSlots.Add(slotObj);
            
            
            Transform slotNumberTransform = slotObj.transform.Find("SlotNumber");
            if (slotNumberTransform != null)
            {
                TMPro.TextMeshProUGUI slotNumber = slotNumberTransform.GetComponent<TMPro.TextMeshProUGUI>();
                if (slotNumber != null)
                {
                    slotNumber.text = (i + 1).ToString();
                }
            }
            
            
            UpdateSlotUI(i);
        }
    }
    
    private void UpdateSlotUI(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= uiSlots.Count) return;
        
        GameObject slotObj = uiSlots[slotIndex];
        Item item = (slotIndex < items.Count) ? items[slotIndex] : null;
        
        
        Transform iconTransform = slotObj.transform.Find("Icon");
        if (iconTransform != null)
        {
            Image iconImage = iconTransform.GetComponent<Image>();
            if (iconImage != null)
            {
                if (item != null)
                {
                    iconImage.sprite = item.icon;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.sprite = null;
                    iconImage.enabled = false;
                }
            }
        }
    }
    
    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize) return;
        
        
        if (selectedSlot >= 0 && selectedSlot < uiSlots.Count)
        {
            Image slotImage = uiSlots[selectedSlot].GetComponent<Image>();
            if (slotImage != null)
            {
                slotImage.color = normalSlotColor;
            }
        }
        
        
        selectedSlot = slotIndex;
        if (selectedSlot >= 0 && selectedSlot < uiSlots.Count)
        {
            Image slotImage = uiSlots[selectedSlot].GetComponent<Image>();
            if (slotImage != null)
            {
                slotImage.color = selectedSlotColor;
            }
        }
    }
    
    private void UseSelectedItem()
    {
        if (selectedSlot >= 0 && selectedSlot < items.Count)
        {
            Item item = items[selectedSlot];
            if (item != null && item.isUsable)
            {
                item.Use();
                
                
                items[selectedSlot] = null;
                UpdateSlotUI(selectedSlot);
            }
        }
    }
}