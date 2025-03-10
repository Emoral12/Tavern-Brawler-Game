using UnityEngine;

public class SteakCookingStation : MonoBehaviour
{
    [SerializeField] private Item rewardItem; 
    
    private bool playerInRange = false;
    private TimingCookingGame cookingGame;
    
    private void Start()
    {
        
        cookingGame = FindObjectOfType<TimingCookingGame>();
        if (cookingGame == null)
        {
            GameObject gameObj = new GameObject("TimingCookingGame");
            cookingGame = gameObj.AddComponent<TimingCookingGame>();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowPrompt(true);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            ShowPrompt(false);
        }
    }
    
    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !cookingGame.IsActive)
        {
            StartCooking();
        }
    }
    
    private void StartCooking()
    {
        cookingGame.StartGame(OnGameCompleted);
    }
    
    private void OnGameCompleted(bool success)
    {
        if (success)
        {
            GiveReward();
        }
    }
    
    private void GiveReward()
    {
        if (rewardItem != null)
        {
            InventorySystem inventorySystem = FindObjectOfType<InventorySystem>();
            if (inventorySystem != null)
            {
                bool added = inventorySystem.AddItem(rewardItem);
                Debug.Log(added ? $"Added {rewardItem.itemName} to inventory!" : "Inventory is full!");
            }
        }
    }
    
    private void ShowPrompt(bool show)
    {
        Debug.Log(show ? "Press E to cook a steak" : "");
        
    }
} 