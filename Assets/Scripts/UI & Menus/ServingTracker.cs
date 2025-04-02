using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ServingTracker : MonoBehaviour
{
    [SerializeField] private int requiredServings = 6;
    private int currentServings = 0;
    private TextMeshProUGUI trackerText;
    
    private static ServingTracker instance;
    public static ServingTracker Instance => instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            CreateUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void CreateUI()
    {
        GameObject uiObj = new GameObject("ServingTrackerUI");
        uiObj.transform.SetParent(transform);
        
        Canvas canvas = uiObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        uiObj.AddComponent<CanvasScaler>();
        
        GameObject textObj = new GameObject("TrackerText");
        textObj.transform.SetParent(uiObj.transform, false);
        
        trackerText = textObj.AddComponent<TextMeshProUGUI>();
        trackerText.fontSize = 36;
        trackerText.color = Color.white;
        trackerText.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = trackerText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 1);
        textRect.anchorMax = new Vector2(0, 1);
        textRect.anchoredPosition = new Vector2(100, -50);
        textRect.sizeDelta = new Vector2(200, 50);
        
        UpdateUI();
    }
    
    public void AddServing()
    {
        currentServings++;
        UpdateUI();
        
        if (currentServings >= requiredServings)
        {
            OnAllServingsCompleted();
        }
    }
    
    private void UpdateUI()
    {
        if (trackerText != null)
        {
            trackerText.text = $"Servings: {currentServings}/{requiredServings}";
        }
    }
    
    private void OnAllServingsCompleted()
    {
        Debug.Log("All servings completed!");
    }
}
