using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderRequest : MonoBehaviour
{   
    [Header("UI Settings")]
    [SerializeField] private float bubbleHeight = 2.5f;
    [SerializeField] private float bubbleScale = 0.005f;
    [SerializeField] private float interactionRange = 3f;

    [Header("Order Settings")]
    [SerializeField] private Item[] possibleOrders;

    private GameObject orderBubble;
    private Image foodIcon;
    private Item currentOrder;
    private bool hasOrder = false;
    private bool isPlayerInRange = false;
    private PlayerController player;
    private Hotbar playerHotbar;
    
    
    private GameObject servePrompt;
    private TextMeshProUGUI servePromptText;

    private void Start()
    {
        CreateOrderBubble();
        CreateServePrompt();
        GenerateNewOrder();
    }

    private void CreateOrderBubble()
    {
        
        orderBubble = new GameObject("OrderBubble");
        orderBubble.transform.SetParent(transform, false);
        orderBubble.transform.localPosition = Vector3.up * bubbleHeight;
        orderBubble.transform.localScale = Vector3.one * bubbleScale;

        
        Canvas canvas = orderBubble.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

       
        CanvasScaler scaler = orderBubble.AddComponent<CanvasScaler>();
        scaler.scaleFactor = 1;
        scaler.dynamicPixelsPerUnit = 100;

        
        orderBubble.AddComponent<GraphicRaycaster>();

        
        GameObject background = new GameObject("Background");
        background.transform.SetParent(orderBubble.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.3f); 
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = new Vector2(25, 25);
        bgRect.localPosition = Vector3.zero;

        
        GameObject iconObj = new GameObject("FoodIcon");
        iconObj.transform.SetParent(background.transform, false);
        foodIcon = iconObj.AddComponent<Image>();
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.sizeDelta = new Vector2(80, 80);
        iconRect.localPosition = Vector3.zero;

       
        orderBubble.AddComponent<Billboard>();

        
        orderBubble.SetActive(false);
    }

    private void CreateServePrompt()
    {
        
        servePrompt = new GameObject("ServePrompt");
        servePrompt.transform.SetParent(orderBubble.transform, false);

        
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(servePrompt.transform, false);
        
        servePromptText = textObj.AddComponent<TextMeshProUGUI>();
        servePromptText.text = "Press F to serve!";
        servePromptText.color = Color.white;
        servePromptText.fontSize = 36; // Increased font size
        servePromptText.alignment = TextAlignmentOptions.Center;
        
        
        RectTransform textRect = servePromptText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0);
        textRect.anchorMax = new Vector2(0.5f, 0);
        textRect.pivot = new Vector2(0.5f, 1);
        textRect.sizeDelta = new Vector2(400, 50); 
        textRect.anchoredPosition = new Vector2(0, -50); 

        // add background for better visibility
        GameObject bgObj = new GameObject("TextBackground");
        bgObj.transform.SetParent(textObj.transform, false);
        bgObj.transform.SetAsFirstSibling(); // put background behind text
        
        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.5f); 
        
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = new Vector2(20, 10); 
        bgRect.anchoredPosition = Vector2.zero;

        
        servePrompt.SetActive(false);
    }

    private void GenerateNewOrder()
    {
        if (possibleOrders.Length == 0)
        {
            Debug.LogError("No possible orders assigned to " + gameObject.name);
            return;
        }

        currentOrder = possibleOrders[Random.Range(0, possibleOrders.Length)];
        if (currentOrder == null)
        {
            Debug.LogError("Selected order is null!");
            return;
        }

        Debug.Log($"Setting new order: {currentOrder.itemName} with icon {currentOrder.icon != null}");
        foodIcon.sprite = currentOrder.icon;
        foodIcon.color = Color.white;
        hasOrder = true;
        orderBubble.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            player = other.GetComponent<PlayerController>();
            
           
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                Transform hotbarTransform = canvas.transform.Find("Hotbar");
                if (hotbarTransform != null)
                {
                    playerHotbar = hotbarTransform.GetComponent<Hotbar>();
                    if (playerHotbar != null) break;
                }
            }
            
            // 
            if (playerHotbar == null)
            {
                playerHotbar = FindObjectOfType<Hotbar>();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            player = null;
            playerHotbar = null;
            servePrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (!hasOrder || !isPlayerInRange || player == null || playerHotbar == null) return;

        Item selectedItem = playerHotbar.GetSelectedItem();
        bool hasCorrectItem = (selectedItem != null && selectedItem.itemName == currentOrder.itemName);
        servePrompt.SetActive(hasCorrectItem);

        if (hasCorrectItem && Input.GetKeyDown(KeyCode.F))
        {
            playerHotbar.RemoveSelectedItem();
            hasOrder = false;
            orderBubble.SetActive(false);
            servePrompt.SetActive(false);
            Invoke("GenerateNewOrder", Random.Range(5f, 15f));
        }
    }
}












