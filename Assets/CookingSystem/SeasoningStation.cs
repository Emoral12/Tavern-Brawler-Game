using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SeasoningStation : MonoBehaviour
{
    [SerializeField] private Item rewardItem;
    private bool playerInRange = false;
    private SeasoningGame seasoningGame;
    public Item RewardItem => rewardItem;
    
    private static Canvas uiCanvas;
    private TextMeshProUGUI interactText;
    
    private void Start()
    {
        seasoningGame = FindObjectOfType<SeasoningGame>();
        if (seasoningGame == null)
        {
            GameObject gameObj = new GameObject("SeasoningGame");
            seasoningGame = gameObj.AddComponent<SeasoningGame>();
        }
        
        SetupUIElements();
    }
    
    private void SetupUIElements()
    {
        if (uiCanvas == null)
        {
            GameObject canvasObj = new GameObject("UICanvas");
            uiCanvas = canvasObj.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasObj);
        }
        
        GameObject textObj = new GameObject("InteractText");
        textObj.transform.SetParent(uiCanvas.transform, false);
        
        interactText = textObj.AddComponent<TextMeshProUGUI>();
        interactText.text = "Press 'E' to season";
        interactText.fontSize = 36;
        interactText.alignment = TextAlignmentOptions.Center;
        interactText.color = Color.white;
        
        RectTransform rectTransform = interactText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(300, 50);
        rectTransform.anchoredPosition = new Vector2(0, -50);
        
        interactText.gameObject.SetActive(false);
    }

    private void ShowPrompt(bool show)
    {
        if (interactText != null)
        {
            interactText.gameObject.SetActive(show);
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
}
