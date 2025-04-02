using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SequenceCookingGame : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int sequenceLength = 5;
    [SerializeField] private float timePerStep = 1.5f;
    [SerializeField] private KeyCode[] possibleKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };
    
    private List<KeyCode> currentSequence = new List<KeyCode>();
    private int currentStep = 0;
    private bool isActive = false;
    private Action<bool> onCompleteCallback;
    private GameObject uiRoot;
    private PlayerController playerController;
    
    private TextMeshProUGUI instructionText;
    private TextMeshProUGUI sequenceText;
    private TextMeshProUGUI resultText;
    private Slider progressBar;
    private List<Image> keyImages = new List<Image>();
    
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
        
        // Disable player controller
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
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
        }
        
        
        GenerateSequence();
        
        
        Time.timeScale = 0;
        
        
        StartCoroutine(GameLoop());
    }
    
    private void CreateUI()
    {
        
        GameObject canvasObj = new GameObject("CookingGameCanvas");
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
        titleText.text = "COOK THE BURGER";
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
        instructionText.text = "Press the keys in the correct sequence:";
        instructionText.fontSize = 24;
        instructionText.alignment = TextAlignmentOptions.Center;
        instructionText.color = Color.white;
        RectTransform instructionRect = instructionObj.GetComponent<RectTransform>();
        instructionRect.anchorMin = new Vector2(0.5f, 0.8f);
        instructionRect.anchorMax = new Vector2(0.5f, 0.85f);
        instructionRect.sizeDelta = new Vector2(500, 50);
        instructionRect.anchoredPosition = Vector2.zero;
        
        
        GameObject sequenceObj = new GameObject("SequenceText");
        sequenceObj.transform.SetParent(panelObj.transform, false);
        sequenceText = sequenceObj.AddComponent<TextMeshProUGUI>();
        sequenceText.text = "";
        sequenceText.fontSize = 36;
        sequenceText.alignment = TextAlignmentOptions.Center;
        sequenceText.color = Color.yellow;
        RectTransform sequenceRect = sequenceObj.GetComponent<RectTransform>();
        sequenceRect.anchorMin = new Vector2(0.5f, 0.7f);
        sequenceRect.anchorMax = new Vector2(0.5f, 0.75f);
        sequenceRect.sizeDelta = new Vector2(500, 50);
        sequenceRect.anchoredPosition = Vector2.zero;
        
        
        GameObject keyAreaObj = new GameObject("KeyArea");
        keyAreaObj.transform.SetParent(panelObj.transform, false);
        HorizontalLayoutGroup keyLayout = keyAreaObj.AddComponent<HorizontalLayoutGroup>();
        keyLayout.spacing = 10;
        keyLayout.childAlignment = TextAnchor.MiddleCenter;
        keyLayout.childForceExpandWidth = false;
        keyLayout.childForceExpandHeight = false;
        RectTransform keyAreaRect = keyAreaObj.GetComponent<RectTransform>();
        keyAreaRect.anchorMin = new Vector2(0.5f, 0.5f);
        keyAreaRect.anchorMax = new Vector2(0.5f, 0.6f);
        keyAreaRect.sizeDelta = new Vector2(500, 100);
        keyAreaRect.anchoredPosition = Vector2.zero;
        
        
        for (int i = 0; i < sequenceLength; i++)
        {
            GameObject keyObj = new GameObject($"Key_{i}");
            keyObj.transform.SetParent(keyAreaObj.transform, false);
            Image keyImage = keyObj.AddComponent<Image>();
            keyImage.color = Color.gray;
            RectTransform keyRect = keyObj.GetComponent<RectTransform>();
            keyRect.sizeDelta = new Vector2(60, 60);
            
            
            GameObject keyTextObj = new GameObject("KeyText");
            keyTextObj.transform.SetParent(keyObj.transform, false);
            TextMeshProUGUI keyText = keyTextObj.AddComponent<TextMeshProUGUI>();
            keyText.text = "?";
            keyText.fontSize = 24;
            keyText.alignment = TextAlignmentOptions.Center;
            keyText.color = Color.white;
            RectTransform keyTextRect = keyTextObj.GetComponent<RectTransform>();
            keyTextRect.anchorMin = Vector2.zero;
            keyTextRect.anchorMax = Vector2.one;
            keyTextRect.offsetMin = Vector2.zero;
            keyTextRect.offsetMax = Vector2.zero;
            
            keyImages.Add(keyImage);
        }
        
        
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
    
    private void GenerateSequence()
    {
        currentSequence.Clear();
        
        for (int i = 0; i < sequenceLength; i++)
        {
            KeyCode randomKey = possibleKeys[UnityEngine.Random.Range(0, possibleKeys.Length)];
            currentSequence.Add(randomKey);
        }
        
        
        UpdateSequenceDisplay();
    }
    
    private void UpdateSequenceDisplay()
    {
        // Use the text display
        string sequenceString = "";
        for (int i = 0; i < currentSequence.Count; i++)
        {
            if (i < currentStep)
            {
                sequenceString += "<color=green>" + currentSequence[i].ToString() + "</color> ";
            }
            else if (i == currentStep)
            {
                sequenceString += "<color=yellow>" + currentSequence[i].ToString() + "</color> ";
            }
            else
            {
                sequenceString += "<color=white>" + currentSequence[i].ToString() + "</color> ";
            }
        }
        
        sequenceText.text = sequenceString;
        
        // 
        /*
        for (int i = 0; i < keyImages.Count; i++)
        {
            if (i < currentSequence.Count)
            {
                Transform keyTextTransform = keyImages[i].transform.GetChild(0);
                TextMeshProUGUI keyText = keyTextTransform.GetComponent<TextMeshProUGUI>();
                keyText.text = currentSequence[i].ToString();
                
                if (i < currentStep)
                {
                    keyImages[i].color = Color.green;
                }
                else if (i == currentStep)
                {
                    keyImages[i].color = Color.yellow;
                }
                else
                {
                    keyImages[i].color = Color.gray;
                }
            }
        }
        */
        
        progressBar.value = (float)currentStep / sequenceLength;
    }
    
    private IEnumerator GameLoop()
    {
        
        instructionText.text = "Remember this sequence:";
        yield return new WaitForSecondsRealtime(2f);
        
        
        instructionText.text = "Press the keys in the correct order:";
        
        bool gameOver = false;
        float timeLeft = timePerStep * sequenceLength;
        
        while (!gameOver && currentStep < sequenceLength)
        {
            timeLeft -= Time.unscaledDeltaTime;
            
            
            foreach (KeyCode key in possibleKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    if (key == currentSequence[currentStep])
                    {
                        
                        currentStep++;
                        UpdateSequenceDisplay();
                        
                        if (currentStep >= sequenceLength)
                        {
                            
                            ShowResult(true);
                            gameOver = true;
                        }
                    }
                    else
                    {
                        
                        ShowResult(false);
                        gameOver = true;
                    }
                    break;
                }
            }
            
            
            if (timeLeft <= 0)
            {
                ShowResult(false);
                gameOver = true;
            }
            
            yield return null;
        }
        
        
        yield return new WaitForSecondsRealtime(2f);
        
        
        EndGame();
    }
    
    private void ShowResult(bool success)
    {
        resultText.gameObject.SetActive(true);
        
        if (success)
        {
            resultText.text = "Success! Burger cooked perfectly!";
            resultText.color = Color.green;
        }
        else
        {
            resultText.text = "Failed! Burger burned!";
            resultText.color = Color.red;
        }
    }
    
    private void EndGame()
    {
        
        uiRoot.SetActive(false);
        
        
        Time.timeScale = 1;
        
        
        bool success = currentStep >= sequenceLength;
        onCompleteCallback?.Invoke(success);
        
        
        isActive = false;
        
        
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        
        if (playerController != null && playerController.GetComponent<Rigidbody>() != null)
        {
            playerController.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
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
