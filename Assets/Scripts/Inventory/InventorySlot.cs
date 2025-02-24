using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;

    private Item currentItem;
    public InventorySystem inventorySystem;
    private int slotIndex;
    private bool isHotbarSlot;

    public bool HasItem => currentItem != null;
    public int SlotIndex => slotIndex;
    public bool IsHotbarSlot => isHotbarSlot;

    public void Initialize(int index, InventorySystem system, bool isHotbar = false)
    {
        slotIndex = index;
        inventorySystem = system;
        isHotbarSlot = isHotbar;

        if (iconImage != null)
        {
            RectTransform rt = iconImage.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(5, 5);
            rt.offsetMax = new Vector2(-5, -5);
            iconImage.enabled = false;
        }
    }

    public void SetItem(Item item)
    {
        currentItem = item;
        if (item != null && iconImage != null)
        {
            Debug.Log($"Setting item {item.name} with icon {item.icon != null}");
            iconImage.sprite = item.icon;
            iconImage.color = Color.white;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.color = Color.clear;
            iconImage.enabled = false;
        }
        else
        {
            Debug.LogError($"IconImage not assigned on slot {gameObject.name}");
        }
    }

    public Item GetItem()
    {
        return currentItem;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"OnBeginDrag called on {gameObject.name}, HasItem: {HasItem}, Icon: {iconImage != null}, System: {inventorySystem != null}");
        if (currentItem != null && iconImage != null && inventorySystem != null)
        {
            inventorySystem.BeginDrag(this);
            iconImage.color = new Color(1, 1, 1, 0.5f);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentItem != null && iconImage != null)
        {
            iconImage.color = Color.white;
        }
        if (inventorySystem != null)
        {
            inventorySystem.EndDrag();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (inventorySystem != null)
        {
            inventorySystem.DropItemIntoSlot(this);
        }
        else
        {
            Debug.LogError($"InventorySystem not assigned on slot {gameObject.name}");
        }
    }

    private void Start()
    {
        if (iconImage != null)
        {
            RectTransform rt = iconImage.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(5, 5);
            rt.offsetMax = new Vector2(-5, -5);
        }
    }
} 