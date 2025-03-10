using UnityEngine;

[CreateAssetMenu(fileName = "Steak", menuName = "Inventory/Items/Steak")]
public class SteakItem : Item
{
    public int healthRestoreAmount = 40;
    
    public override void Use()
    {
        Debug.Log($"Eating {itemName} and restoring {healthRestoreAmount} health!");
    }
} 