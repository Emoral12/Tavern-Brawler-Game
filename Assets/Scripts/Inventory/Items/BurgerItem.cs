using UnityEngine;

[CreateAssetMenu(fileName = "Burger", menuName = "Inventory/Items/Burger")]
public class BurgerItem : Item
{
    public int healthRestoreAmount = 25;
    
    public override void Use()
    {
        Debug.Log($"Eating {itemName} and restoring {healthRestoreAmount} health!");
    }
} 