using UnityEngine;
using UnityEngine.UI;

public class StationIconDisplay : MonoBehaviour
{
    [SerializeField] private float iconHeight = 1.5f;
    [SerializeField] private float iconScale = 1f;
    
    private void Start()
    {
        
        Item rewardItem = null;
        
        if (TryGetComponent<PizzaCookingStation>(out var pizzaStation))
            rewardItem = pizzaStation.RewardItem;
        else if (TryGetComponent<SteakCookingStation>(out var steakStation))
            rewardItem = steakStation.RewardItem;
        else if (TryGetComponent<SeasoningStation>(out var seasoningStation))
            rewardItem = seasoningStation.RewardItem;
        else if (TryGetComponent<CookingStation>(out var cookingStation))
            rewardItem = cookingStation.RewardItem;

        if (rewardItem != null && rewardItem.icon != null)
        {
            CreateFloatingIcon(rewardItem.icon);
        }
        else
        {
            Debug.LogWarning("No reward item or icon found for station: " + gameObject.name);
        }
    }

    private void CreateFloatingIcon(Sprite iconSprite)
    {
        
        GameObject container = new GameObject("FloatingIcon");
        container.transform.SetParent(transform);
        container.transform.localPosition = Vector3.up * iconHeight;
        container.transform.localRotation = Quaternion.identity;
        container.transform.localScale = Vector3.one * iconScale;

        
        Billboard billboard = container.AddComponent<Billboard>();

        
        GameObject iconObj = new GameObject("IconSprite");
        iconObj.transform.SetParent(container.transform);
        iconObj.transform.localPosition = Vector3.zero;
        iconObj.transform.localRotation = Quaternion.identity;
        iconObj.transform.localScale = Vector3.one;

        
        SpriteRenderer spriteRenderer = iconObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = iconSprite;
        spriteRenderer.sortingOrder = 1;
        spriteRenderer.material = new Material(Shader.Find("Sprites/Default"));

        
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(container.transform);
        bgObj.transform.localPosition = Vector3.zero;
        bgObj.transform.localRotation = Quaternion.identity;
        bgObj.transform.localScale = Vector3.one * 1.2f; 

        SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = CreateCircleSprite();
        bgRenderer.color = new Color(0f, 0f, 0f, 0.5f);
        bgRenderer.sortingOrder = 0;
        bgRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    private Sprite CreateCircleSprite()
    {
        int resolution = 64;
        Texture2D texture = new Texture2D(resolution, resolution);
        
        float center = resolution / 2f;
        float radius = resolution / 2f;
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                Color color = distance < radius ? Color.white : Color.clear;
                texture.SetPixel(x, y, color);
            }
        }
        
        texture.Apply();
        
        return Sprite.Create(texture, 
            new Rect(0, 0, resolution, resolution), 
            new Vector2(0.5f, 0.5f), 
            100f);
    }
}

