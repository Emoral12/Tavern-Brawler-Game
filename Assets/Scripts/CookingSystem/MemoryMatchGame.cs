using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class MemoryMatchGame : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float showDurationSeconds = 3f;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private int sequenceLength = 4;
    
    private bool isActive = false;
    private Action<bool> onCompleteCallback;
    private GameObject uiRoot;
    private PlayerController playerController;
    private List<int> correctSequence = new List<int>();
    private List<int> playerSequence = new List<int>();
    private List<Image> ingredientButtons = new List<Image>();
    
    private TextMeshProUGUI instructionText;
    private TextMeshProUGUI resultText;
    
    public bool IsActive => isActive;
    
    [System.Serializable]
    public struct Ingredient
    {
        public string name;
        public Sprite icon;
    }
    
    [SerializeField] private Ingredient[] pizzaIngredients = new Ingredient[]
    {
        new Ingredient { name = "Dough" },
        new Ingredient { name = "Sauce" },
        new Ingredient { name = "Cheese" },
        new Ingredient { name = "Pepperoni" },
        new Ingredient { name = "Mushrooms" },
        new Ingredient { name = "Olives" }
    };
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }
    
    private void CreateUI()
    {
        uiRoot = new GameObject("MemoryMatchUI");
        Canvas canvas = uiRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        CanvasScaler scaler = uiRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        uiRoot.AddComponent<GraphicRaycaster>();
        
         
        GameObject panel = CreatePanel();
        panel.transform.SetParent(uiRoot.transform, false);
        
         
        GameObject instructionObj = new GameObject("Instructions");
        instructionObj.transform.SetParent(panel.transform, false);
        
        instructionText = instructionObj.AddComponent<TextMeshProUGUI>();
        instructionText.fontSize = 36;
        instructionText.alignment = TextAlignmentOptions.Center;
        instructionText.color = Color.white;
        
        RectTransform instructionRect = instructionText.GetComponent<RectTransform>();
        instructionRect.anchorMin = new Vector2(0.5f, 0.8f);
        instructionRect.anchorMax = new Vector2(0.5f, 0.9f);
        instructionRect.sizeDelta = new Vector2(800, 100);
        
         
        GameObject gridObj = new GameObject("IngredientGrid");
        gridObj.transform.SetParent(panel.transform, false);
        
        GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(150, 150);
        grid.spacing = new Vector2(20, 20);
        grid.padding = new RectOffset(20, 20, 20, 20);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        
        RectTransform gridRect = gridObj.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0.5f, 0.5f);
        gridRect.anchorMax = new Vector2(0.5f, 0.5f);
        gridRect.pivot = new Vector2(0.5f, 0.5f);
        gridRect.sizeDelta = new Vector2(510, 340);
        
         
        for (int i = 0; i < pizzaIngredients.Length; i++)
        {
            GameObject buttonObj = CreateIngredientButton(i);
            buttonObj.transform.SetParent(gridObj.transform, false);
            
            Image buttonImage = buttonObj.GetComponent<Image>();
            ingredientButtons.Add(buttonImage);
            
            int index = i;
            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => OnIngredientClicked(index));
        }
        
         
        GameObject resultObj = new GameObject("Result");
        resultObj.transform.SetParent(panel.transform, false);
        
        resultText = resultObj.AddComponent<TextMeshProUGUI>();
        resultText.fontSize = 48;
        resultText.alignment = TextAlignmentOptions.Center;
        resultText.color = Color.white;
        
        RectTransform resultRect = resultText.GetComponent<RectTransform>();
        resultRect.anchorMin = new Vector2(0.5f, 0.2f);
        resultRect.anchorMax = new Vector2(0.5f, 0.3f);
        resultRect.sizeDelta = new Vector2(600, 100);
        
        uiRoot.SetActive(false);
    }
    
    private GameObject CreatePanel()
    {
        GameObject panel = new GameObject("Panel");
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.9f);
        
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        
        return panel;
    }
    
    private GameObject CreateIngredientButton(int index)
    {
        GameObject buttonObj = new GameObject($"Ingredient_{index}");
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.sprite = pizzaIngredients[index].icon;
        buttonImage.preserveAspect = true;
         
        buttonImage.color = new Color(1, 1, 1, 0.7f);
        
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1, 1, 1, 0.7f);   
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 0.8f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 0.9f);
        button.colors = colors;
        
         
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = pizzaIngredients[index].name;
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;   
        
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 0.3f);
        textRect.sizeDelta = Vector2.zero;
        
        return buttonObj;
    }
    
    public void StartGame(Action<bool> onComplete)
    {
        if (isActive) return;
        
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
         
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        onCompleteCallback = onComplete;
        isActive = true;
        
        if (uiRoot == null)
        {
            CreateUI();
        }
        
        uiRoot.SetActive(true);
        GenerateSequence();
        StartCoroutine(GameLoop());
    }
    
    private void GenerateSequence()
    {
        correctSequence.Clear();
        playerSequence.Clear();
        
        for (int i = 0; i < sequenceLength; i++)
        {
            correctSequence.Add(UnityEngine.Random.Range(0, pizzaIngredients.Length));
        }
    }
    
    private IEnumerator GameLoop()
    {
        instructionText.text = "Watch the ingredient sequence...";
        yield return new WaitForSecondsRealtime(1f);
        
         
        for (int i = 0; i < correctSequence.Count; i++)
        {
            int ingredientIndex = correctSequence[i];
            Image button = ingredientButtons[ingredientIndex];
            
             
            button.color = Color.yellow;
            yield return new WaitForSecondsRealtime(showDurationSeconds);
            button.color = Color.white;
            yield return new WaitForSecondsRealtime(0.5f);
        }
        
        instructionText.text = "Now click the ingredients in the correct order!";
    }
    
    private void OnIngredientClicked(int index)
    {
        if (!isActive || playerSequence.Count >= correctSequence.Count) return;
        
        playerSequence.Add(index);
        StartCoroutine(FlashIngredient(ingredientButtons[index]));
        
        if (playerSequence.Count == correctSequence.Count)
        {
            CheckResult();
        }
    }
    
    private IEnumerator FlashIngredient(Image ingredient)
    {
        ingredient.color = Color.green;
        yield return new WaitForSecondsRealtime(0.2f);
        ingredient.color = Color.white;
    }
    
    private void CheckResult()
    {
        bool success = true;
        for (int i = 0; i < correctSequence.Count; i++)
        {
            if (correctSequence[i] != playerSequence[i])
            {
                success = false;
                break;
            }
        }
        
        StartCoroutine(ShowResult(success));
    }
    
    private IEnumerator ShowResult(bool success)
    {
        resultText.text = success ? "Perfect Pizza!" : "Wrong ingredients...";
        resultText.color = success ? Color.green : Color.red;
        
        yield return new WaitForSecondsRealtime(2f);
        EndGame(success);
    }
    
    private void EndGame(bool success)
    {
        isActive = false;
        
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
         
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
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


