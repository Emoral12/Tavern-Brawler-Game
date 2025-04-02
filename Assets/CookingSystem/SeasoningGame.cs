using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class SeasoningGame : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 7f;
    [SerializeField] private int requiredShakes = 20;
    [SerializeField] private KeyCode shakeKey = KeyCode.Space;
    [SerializeField] private float shakeCooldown = 0.1f;  
    
    private bool isActive;
    private int currentShakes;
    private GameObject uiRoot;
    private PlayerController playerController;
    private Action<bool> onCompleteCallback;
    private float nextShakeTime;
    
    
    private TextMeshProUGUI instructionText;
    private TextMeshProUGUI countText;
    private TextMeshProUGUI timerText;
    private Slider progressBar;
    private RectTransform progressBarRect;
    
    public bool IsActive => isActive;
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }
    
    public void StartGame(Action<bool> onComplete)
    {
        if (isActive) return;
        
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        isActive = true;
        currentShakes = 0;
        nextShakeTime = 0f;
        onCompleteCallback = onComplete;
        
        if (uiRoot == null)
        {
            CreateUI();
        }
        
        uiRoot.SetActive(true);
        StartCoroutine(GameLoop());
    }
    
    private void CreateUI()
    {
        uiRoot = new GameObject("SeasoningGameUI");
        Canvas canvas = uiRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        uiRoot.AddComponent<CanvasScaler>();
        uiRoot.AddComponent<GraphicRaycaster>();
        
         
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(uiRoot.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.85f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
         
        GameObject container = new GameObject("Container");
        container.transform.SetParent(panel.transform, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(600, 400);
        
         
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(container.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Season the Dish!";
        titleText.fontSize = 48;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(1f, 0.8f, 0.2f);  
        titleText.alignment = TextAlignmentOptions.Center;
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -50);
        titleRect.sizeDelta = new Vector2(600, 60);
        
         
        GameObject instructionObj = new GameObject("Instructions");
        instructionObj.transform.SetParent(container.transform, false);
        instructionText = instructionObj.AddComponent<TextMeshProUGUI>();
        instructionText.text = $"Press {shakeKey} to shake the seasoning!";
        instructionText.fontSize = 28;
        instructionText.color = Color.white;
        instructionText.alignment = TextAlignmentOptions.Center;
        RectTransform instructionRect = instructionText.GetComponent<RectTransform>();
        instructionRect.anchorMin = new Vector2(0.5f, 1f);
        instructionRect.anchorMax = new Vector2(0.5f, 1f);
        instructionRect.anchoredPosition = new Vector2(0, -120);
        instructionRect.sizeDelta = new Vector2(500, 40);
        
        
        GameObject timerObj = new GameObject("Timer");
        timerObj.transform.SetParent(container.transform, false);
        timerText = timerObj.AddComponent<TextMeshProUGUI>();
        timerText.fontSize = 32;
        timerText.color = Color.white;
        timerText.alignment = TextAlignmentOptions.Center;
        RectTransform timerRect = timerText.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0.5f, 0.5f);
        timerRect.anchorMax = new Vector2(0.5f, 0.5f);
        timerRect.anchoredPosition = new Vector2(0, 50);
        timerRect.sizeDelta = new Vector2(200, 40);
        
        
        GameObject progressBg = new GameObject("ProgressBackground");
        progressBg.transform.SetParent(container.transform, false);
        Image progressBgImage = progressBg.AddComponent<Image>();
        progressBgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        RectTransform progressBgRect = progressBg.GetComponent<RectTransform>();
        progressBgRect.anchorMin = new Vector2(0.5f, 0.5f);
        progressBgRect.anchorMax = new Vector2(0.5f, 0.5f);
        progressBgRect.sizeDelta = new Vector2(400, 30);
        progressBgRect.anchoredPosition = new Vector2(0, 0);
        
         
        GameObject progressObj = new GameObject("ProgressBar");
        progressObj.transform.SetParent(progressBg.transform, false);
        progressBar = progressObj.AddComponent<Slider>();
        progressBar.minValue = 0;
        progressBar.maxValue = requiredShakes;
        progressBar.value = 0;
        progressBarRect = progressObj.GetComponent<RectTransform>();
        progressBarRect.anchorMin = Vector2.zero;
        progressBarRect.anchorMax = Vector2.one;
        progressBarRect.sizeDelta = Vector2.zero;
        
         
        GameObject fillArea = new GameObject("Fill");
        fillArea.transform.SetParent(progressObj.transform, false);
        Image fillImage = fillArea.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.8f, 0.2f);  
        progressBar.fillRect = fillArea.GetComponent<RectTransform>();
        RectTransform fillRect = fillArea.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        
         
        GameObject countObj = new GameObject("Count");
        countObj.transform.SetParent(container.transform, false);
        countText = countObj.AddComponent<TextMeshProUGUI>();
        countText.fontSize = 24;
        countText.color = Color.white;
        countText.alignment = TextAlignmentOptions.Center;
        RectTransform countRect = countText.GetComponent<RectTransform>();
        countRect.anchorMin = new Vector2(0.5f, 0.5f);
        countRect.anchorMax = new Vector2(0.5f, 0.5f);
        countRect.anchoredPosition = new Vector2(0, -40);
        countRect.sizeDelta = new Vector2(200, 30);
        
        UpdateUI();
    }
    
    private IEnumerator GameLoop()
    {
        float timer = gameDuration;
        
        while (timer > 0 && currentShakes < requiredShakes)
        {
            timer -= Time.deltaTime;
            timerText.text = $"Time: {timer:F1}s";
            
            if (Input.GetKeyDown(shakeKey) && Time.time >= nextShakeTime)
            {
                currentShakes++;
                nextShakeTime = Time.time + shakeCooldown;
                UpdateUI();
                
                if (currentShakes >= requiredShakes)
                {
                    EndGame(true);
                    yield break;
                }
            }
            
            yield return null;
        }
        
        EndGame(currentShakes >= requiredShakes);
    }
    
    private void UpdateUI()
    {
        countText.text = $"Progress: {currentShakes} / {requiredShakes}";
        progressBar.value = currentShakes;
    }
    
    private void EndGame(bool success)
    {
        isActive = false;
        
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        StartCoroutine(ShowResult(success));
    }
    
    private IEnumerator ShowResult(bool success)
    {
         
        instructionText.text = success ? "Perfect Seasoning!" : "Time's up!";
        instructionText.color = success ? Color.green : Color.red;
        instructionText.fontSize = 36;
        
        yield return new WaitForSeconds(1.5f);
        
        uiRoot.SetActive(false);
        onCompleteCallback?.Invoke(success);
    }
    
    private void OnDestroy()
    {
        if (uiRoot != null)
        {
            Destroy(uiRoot);
        }
    }
}
