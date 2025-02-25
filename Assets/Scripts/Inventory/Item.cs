using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject prefab;
    public bool isUsable = true;
    
    public virtual void Use()
    {
        Debug.Log($"Using {itemName}");
    }
} 
