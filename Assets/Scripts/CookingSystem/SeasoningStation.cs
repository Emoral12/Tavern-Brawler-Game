using UnityEngine;
using TMPro;

public class SeasoningStation : MonoBehaviour
{
    [SerializeField] private Item rewardItem;
    [SerializeField] private float interactionRange = 3f;
    private bool playerInRange = false;
    private SeasoningGame seasoningGame;
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI interactPrompt;
    [SerializeField] private GameObject promptCanvas;
    
    private void Start()
    {
        seasoningGame = FindObjectOfType<SeasoningGame>();
        if (seasoningGame == null)
        {
            GameObject gameObj = new GameObject("SeasoningGame");
            seasoningGame = gameObj.AddComponent<SeasoningGame>();
        }
        
        if (promptCanvas != null)
        {
            promptCanvas.SetActive(false);
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
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !seasoningGame.IsActive)
        {
            StartSeasoning();
        }
    }
    
    private void StartSeasoning()
    {
        seasoningGame.StartGame(OnGameCompleted);
    }
    
    private void OnGameCompleted(bool success)
    {
        if (success)
        {
            GiveReward();
            ShowPrompt(true);
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
        if (promptCanvas != null)
        {
            promptCanvas.SetActive(show);
        }
    }
}