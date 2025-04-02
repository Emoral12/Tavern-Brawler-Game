using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PizzaCookingStation : MonoBehaviour
{
    [SerializeField] private Item rewardItem;
    [SerializeField] private float interactionRange = 3f;
    private bool playerInRange = false;
    private MemoryMatchGame cookingGame;
    
     
    private static Canvas uiCanvas;
    private TextMeshProUGUI interactText;
    
    private void Start()
    {
        cookingGame = FindObjectOfType<MemoryMatchGame>();
        if (cookingGame == null)
        {
            GameObject gameObj = new GameObject("MemoryMatchGame");
            cookingGame = gameObj.AddComponent<MemoryMatchGame>();
        }
        
        SetupUIElements();
    }
    
    private void SetupUIElements()
    {
        if (uiCanvas == null)
        {
            GameObject canvasObj = new GameObject("StationCanvas");
            uiCanvas = canvasObj.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.WorldSpace;
            uiCanvas.worldCamera = Camera.main;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            DontDestroyOnLoad(canvasObj);
        }
        
        GameObject textObj = new GameObject("InteractText");
        textObj.transform.SetParent(uiCanvas.transform, false);
        
        interactText = textObj.AddComponent<TextMeshProUGUI>();
        interactText.text = "Press 'E' to make pizza";
        interactText.fontSize = 36;
        interactText.alignment = TextAlignmentOptions.Center;
        interactText.color = Color.white;
        
         
        GameObject bgObj = new GameObject("TextBackground");
        bgObj.transform.SetParent(textObj.transform, false);
        bgObj.transform.SetAsFirstSibling();
        
        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.5f);
        
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = new Vector2(20, 10);
        
        RectTransform rectTransform = interactText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(300, 50);
        
         
        uiCanvas.transform.position = transform.position + Vector3.up * 2f;
        uiCanvas.transform.localScale = Vector3.one * 0.01f;
        
         
        uiCanvas.gameObject.AddComponent<Billboard>();
        
        interactText.gameObject.SetActive(false);
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
        if (interactText != null)
        {
            interactText.gameObject.SetActive(show);
        }
    }
}