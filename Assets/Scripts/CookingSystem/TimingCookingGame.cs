using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimingCookingGame : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int totalSteps = 5;
    [SerializeField] private float minTimingWindow = 0.2f;
    [SerializeField] private float maxTimingWindow = 0.8f;
    [SerializeField] private float indicatorSpeed = 2f;
    [SerializeField] private KeyCode actionKey = KeyCode.Space;
    
    private int currentStep = 0;
    private bool isActive = false;
    private Action<bool> onCompleteCallback;
    private GameObject uiRoot;
    private Coroutine gameLoopCoroutine;
    private Coroutine indicatorCoroutine;
    
    
    private TextMeshProUGUI instructionText;
    private TextMeshProUGUI stepText;
    private TextMeshProUGUI resultText;
    private Slider progressBar;
    private RectTransform indicatorRect;
    private RectTransform targetWindowRect;
    private Image feedbackImage;
    
    public bool IsActive => isActive;
    
    private void Awake()
    {
        
        DontDestroyOnLoad(gameObject);
    }
    
    public void StartGame(Action<bool> onComplete)
    {
        if (isActive) return;
        
        onCompleteCallback = onComplete;
        isActive = true;
        currentStep = 0;
        
        
        if (uiRoot == null)
        {
            CreateUI();
        }
        else
        {
            uiRoot.SetActive(true);
            
            
            if (resultText != null && resultText.gameObject != null)
            {
                resultText.gameObject.SetActive(false);
            }
            
            if (feedbackImage != null)
            {
                feedbackImage.enabled = false;
            }
            
            if (progressBar != null)
            {
                progressBar.value = 0;
            }
        }
        
        
        Time.timeScale = 0;
        
        
        gameLoopCoroutine = StartCoroutine(GameLoop());
    }
    
    private void CreateUI()
    {
        
        GameObject canvasObj = new GameObject("TimingGameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "GRILL THE STEAK";
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.95f);
        titleRect.sizeDelta = new Vector2(500, 50);
        titleRect.anchoredPosition = Vector2.zero;
        
        
        GameObject instructionObj = new GameObject("InstructionText");
        instructionObj.transform.SetParent(panelObj.transform, false);
        instructionText = instructionObj.AddComponent<TextMeshProUGUI>();
        instructionText.text = "Press SPACE when the indicator is in the green zone!";
        instructionText.fontSize = 24;
        instructionText.alignment = TextAlignmentOptions.Center;
        instructionText.color = Color.white;
        RectTransform instructionRect = instructionObj.GetComponent<RectTransform>();
        instructionRect.anchorMin = new Vector2(0.5f, 0.8f);
        instructionRect.anchorMax = new Vector2(0.5f, 0.85f);
        instructionRect.sizeDelta = new Vector2(700, 50);
        instructionRect.anchoredPosition = Vector2.zero;
        
        
        GameObject stepObj = new GameObject("StepText");
        stepObj.transform.SetParent(panelObj.transform, false);
        stepText = stepObj.AddComponent<TextMeshProUGUI>();
        stepText.text = "Step 1: Preheat the grill";
        stepText.fontSize = 28;
        stepText.alignment = TextAlignmentOptions.Center;
        stepText.color = Color.yellow;
        RectTransform stepRect = stepObj.GetComponent<RectTransform>();
        stepRect.anchorMin = new Vector2(0.5f, 0.7f);
        stepRect.anchorMax = new Vector2(0.5f, 0.75f);
        stepRect.sizeDelta = new Vector2(500, 50);
        stepRect.anchoredPosition = Vector2.zero;
        
        
        GameObject timingBarObj = new GameObject("TimingBar");
        timingBarObj.transform.SetParent(panelObj.transform, false);
        Image timingBarImage = timingBarObj.AddComponent<Image>();
        timingBarImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        RectTransform timingBarRect = timingBarObj.GetComponent<RectTransform>();
        timingBarRect.anchorMin = new Vector2(0.5f, 0.5f);
        timingBarRect.anchorMax = new Vector2(0.5f, 0.55f);
        timingBarRect.sizeDelta = new Vector2(800, 50);
        timingBarRect.anchoredPosition = Vector2.zero;
        
        
        GameObject targetWindowObj = new GameObject("TargetWindow");
        targetWindowObj.transform.SetParent(timingBarObj.transform, false);
        Image targetWindowImage = targetWindowObj.AddComponent<Image>();
        targetWindowImage.color = new Color(0, 1, 0, 0.5f);
        targetWindowRect = targetWindowObj.GetComponent<RectTransform>();
        targetWindowRect.anchorMin = new Vector2(0.4f, 0);
        targetWindowRect.anchorMax = new Vector2(0.6f, 1);
        targetWindowRect.offsetMin = Vector2.zero;
        targetWindowRect.offsetMax = Vector2.zero;
        
        
        GameObject indicatorObj = new GameObject("Indicator");
        indicatorObj.transform.SetParent(timingBarObj.transform, false);
        Image indicatorImage = indicatorObj.AddComponent<Image>();
        indicatorImage.color = Color.white;
        indicatorRect = indicatorObj.GetComponent<RectTransform>();
        indicatorRect.anchorMin = new Vector2(0, 0);
        indicatorRect.anchorMax = new Vector2(0, 1);
        indicatorRect.sizeDelta = new Vector2(10, 0);
        indicatorRect.anchoredPosition = new Vector2(0, 0);
        
        
        GameObject progressObj = new GameObject("ProgressBar");
        progressObj.transform.SetParent(panelObj.transform, false);
        progressBar = progressObj.AddComponent<Slider>();
        progressBar.minValue = 0;
        progressBar.maxValue = 1;
        progressBar.value = 0;
        RectTransform progressRect = progressObj.GetComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(0.5f, 0.4f);
        progressRect.anchorMax = new Vector2(0.5f, 0.45f);
        progressRect.sizeDelta = new Vector2(400, 30);
        progressRect.anchoredPosition = Vector2.zero;
        
        
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(progressObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(progressObj.transform, false);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0);
        fillAreaRect.anchorMax = new Vector2(1, 1);
        fillAreaRect.offsetMin = new Vector2(5, 0);
        fillAreaRect.offsetMax = new Vector2(-5, 0);
        
        
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        progressBar.fillRect = fillRect;
        
        
        GameObject feedbackObj = new GameObject("FeedbackImage");
        feedbackObj.transform.SetParent(panelObj.transform, false);
        feedbackImage = feedbackObj.AddComponent<Image>();
        feedbackImage.color = Color.green;
        feedbackImage.enabled = false;
        RectTransform feedbackRect = feedbackObj.GetComponent<RectTransform>();
        feedbackRect.anchorMin = new Vector2(0.5f, 0.6f);
        feedbackRect.anchorMax = new Vector2(0.5f, 0.65f);
        feedbackRect.sizeDelta = new Vector2(50, 50);
        feedbackRect.anchoredPosition = Vector2.zero;
        
        
        GameObject resultObj = new GameObject("ResultText");
        resultObj.transform.SetParent(panelObj.transform, false);
        resultText = resultObj.AddComponent<TextMeshProUGUI>();
        resultText.text = "";
        resultText.fontSize = 36;
        resultText.alignment = TextAlignmentOptions.Center;
        resultText.color = Color.white;
        resultObj.SetActive(false);
        RectTransform resultRect = resultObj.GetComponent<RectTransform>();
        resultRect.anchorMin = new Vector2(0.5f, 0.3f);
        resultRect.anchorMax = new Vector2(0.5f, 0.35f);
        resultRect.sizeDelta = new Vector2(500, 50);
        resultRect.anchoredPosition = Vector2.zero;
        
        
        uiRoot = canvasObj;
    }
    
    private IEnumerator GameLoop()
    {
        
        yield return new WaitForSecondsRealtime(1f);
        
        string[] cookingSteps = {
            "Preheat the grill",
            "Season the steak",
            "Place on the grill",
            "Flip the steak",
            "Check temperature"
        };
        
        while (currentStep < totalSteps)
        {
            
            string stepText = currentStep < cookingSteps.Length ? 
                $"Step {currentStep + 1}: {cookingSteps[currentStep]}" : $"Step {currentStep + 1}";
            
            if (this.stepText != null)
            {
                this.stepText.text = stepText;
            }
            
            
            float windowStart = UnityEngine.Random.Range(0.1f, 0.6f);
            float windowSize = UnityEngine.Random.Range(minTimingWindow, maxTimingWindow);
            
            SetTimingWindow(windowStart, windowStart + windowSize);
            
            
            if (indicatorRect != null)
            {
                indicatorRect.anchorMin = new Vector2(0, 0);
                indicatorRect.anchorMax = new Vector2(0, 1);
            }
            
            
            indicatorCoroutine = StartCoroutine(MoveIndicator(indicatorSpeed));
            
            bool stepCompleted = false;
            bool stepFailed = false;
            bool canPress = true;
            
            
            float timer = 0;
            while (timer < 3f && !stepCompleted && !stepFailed)
            {
                timer += Time.unscaledDeltaTime;
                
                
                if (canPress && Input.GetKeyDown(actionKey))
                {
                    canPress = false;
                    
                    
                    float indicatorPosition = 0;
                    if (indicatorRect != null)
                    {
                        indicatorPosition = indicatorRect.anchorMin.x;
                    }
                    
                    
                    bool isInTimingWindow = indicatorPosition >= windowStart && 
                                           indicatorPosition <= (windowStart + windowSize);
                    
                    if (isInTimingWindow)
                    {
                        
                        ShowFeedback(true);
                        currentStep++;
                        UpdateProgress(currentStep, totalSteps);
                        stepCompleted = true;
                    }
                    else
                    {
                        
                        ShowFeedback(false);
                        stepFailed = true;
                    }
                }
                
                
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    EndGame(false);
                    yield break;
                }
                
                yield return null;
            }
            
            
            if (indicatorCoroutine != null)
            {
                StopCoroutine(indicatorCoroutine);
                indicatorCoroutine = null;
            }
            
            if (stepFailed)
            {
                
                ShowResult(false);
                yield return new WaitForSecondsRealtime(2f);
                EndGame(false);
                yield break;
            }
            
            if (!stepCompleted)
            {
                
                ShowFeedback(false);
                ShowResult(false);
                yield return new WaitForSecondsRealtime(2f);
                EndGame(false);
                yield break;
            }
            
            
            yield return new WaitForSecondsRealtime(0.5f);
        }
        
        
        ShowResult(true);
        yield return new WaitForSecondsRealtime(2f);
        EndGame(true);
    }
    
    private void SetTimingWindow(float start, float end)
    {
        if (targetWindowRect != null)
        {
            targetWindowRect.anchorMin = new Vector2(start, 0);
            targetWindowRect.anchorMax = new Vector2(end, 1);
        }
    }
    
    private IEnumerator MoveIndicator(float speed)
    {
        float elapsed = 0f;
        float duration = 3f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime * speed;
            float normalizedTime = Mathf.Clamp01(elapsed / duration);
            
            if (indicatorRect != null)
            {
                indicatorRect.anchorMin = new Vector2(normalizedTime, 0);
                indicatorRect.anchorMax = new Vector2(normalizedTime, 1);
            }
            
            yield return null;
        }
    }
    
    private void UpdateProgress(int current, int total)
    {
        if (progressBar != null)
        {
            progressBar.value = (float)current / total;
        }
    }
    
    private void ShowFeedback(bool success)
    {
        if (feedbackImage != null)
        {
            feedbackImage.color = success ? Color.green : Color.red;
            feedbackImage.enabled = true;
            StartCoroutine(HideFeedback(0.5f));
        }
    }
    
    private IEnumerator HideFeedback(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        
        if (feedbackImage != null && feedbackImage.gameObject != null)
        {
            feedbackImage.enabled = false;
        }
    }
    
    private void ShowResult(bool success)
    {
        if (resultText != null && resultText.gameObject != null)
        {
            resultText.gameObject.SetActive(true);
            
            if (success)
            {
                resultText.text = "Success! Steak cooked perfectly!";
                resultText.color = Color.green;
            }
            else
            {
                resultText.text = "Failed! Steak overcooked!";
                resultText.color = Color.red;
            }
        }
    }
    
    private void EndGame(bool success)
    {
        
        StopAllCoroutines();
        
        
        if (uiRoot != null)
        {
            uiRoot.SetActive(false);
        }
        
        
        Time.timeScale = 1;
        
        
        onCompleteCallback?.Invoke(success);
        
        
        isActive = false;
        gameLoopCoroutine = null;
        indicatorCoroutine = null;
    }
    
    private void OnDestroy()
    {
        
        Time.timeScale = 1;
        
        
        if (uiRoot != null)
        {
            Destroy(uiRoot);
        }
    }
} 