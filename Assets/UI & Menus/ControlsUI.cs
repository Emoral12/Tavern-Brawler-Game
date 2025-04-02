using UnityEngine;
using TMPro;

public class ControlsUI : MonoBehaviour
{
    private void Start()
    {
        CreateControlsUI();
    }

    private void CreateControlsUI()
    {
        
        GameObject uiObj = new GameObject("ControlsUI");
        uiObj.transform.SetParent(transform);

        
        Canvas canvas = uiObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1;
        
        
        uiObj.AddComponent<UnityEngine.UI.CanvasScaler>();

        
        GameObject textObj = new GameObject("ControlsText");
        textObj.transform.SetParent(uiObj.transform, false);
        
        
        TextMeshProUGUI controlsText = textObj.AddComponent<TextMeshProUGUI>();
        controlsText.text = "Controls:\n" +
                          "[I] Inventory\n" +
                          "[E] Interact\n" +
                          "[F] Serve";
        controlsText.fontSize = 24;
        controlsText.color = Color.white;
        controlsText.alignment = TextAlignmentOptions.Left;

        
        RectTransform textRect = controlsText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(0, 0);
        textRect.pivot = new Vector2(0, 0);
        textRect.anchoredPosition = new Vector2(20, 20); 
        textRect.sizeDelta = new Vector2(200, 100);

        
        GameObject panelObj = new GameObject("ControlsPanel");
        panelObj.transform.SetParent(uiObj.transform);
        panelObj.transform.SetSiblingIndex(0); 

        UnityEngine.UI.Image panel = panelObj.AddComponent<UnityEngine.UI.Image>();
        panel.color = new Color(0, 0, 0, 0.5f); 

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(0, 0);
        panelRect.pivot = new Vector2(0, 0);
        panelRect.anchoredPosition = new Vector2(10, 10); 
        panelRect.sizeDelta = new Vector2(220, 120); 
    }
}