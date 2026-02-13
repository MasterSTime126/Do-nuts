using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("UI Settings")]
    [SerializeField] private TMP_FontAsset achievementFont;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private int fontSize = 36;
    [SerializeField] private Color textColor = Color.yellow;

    [Header("Position")]
    [SerializeField] private Vector2 anchorPosition = new Vector2(-20, 20);

    private GameObject achievementCanvas;
    private Queue<string> achievementQueue = new Queue<string>();
    private bool isDisplaying = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        CreateAchievementCanvas();
    }

    private void CreateAchievementCanvas()
    {
        achievementCanvas = new GameObject("AchievementCanvas");
        achievementCanvas.transform.SetParent(transform);
        
        Canvas canvas = achievementCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        achievementCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        achievementCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
    }

    public void UnlockAchievement(string achievementName)
    {
        Debug.Log($"[Achievement] Unlocked: {achievementName}");
        
        achievementQueue.Enqueue(achievementName);
        
        if (!isDisplaying)
        {
            StartCoroutine(DisplayAchievements());
        }
    }

    private IEnumerator DisplayAchievements()
    {
        isDisplaying = true;

        while (achievementQueue.Count > 0)
        {
            string achievement = achievementQueue.Dequeue();
            yield return StartCoroutine(ShowAchievement(achievement));
        }

        isDisplaying = false;
    }

    private IEnumerator ShowAchievement(string achievementName)
    {
        GameObject textObj = new GameObject("AchievementText");
        textObj.transform.SetParent(achievementCanvas.transform, false);

        // Bottom-right corner
        RectTransform rectTransform = textObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 0);
        rectTransform.anchorMax = new Vector2(1, 0);
        rectTransform.pivot = new Vector2(1, 0);
        rectTransform.anchoredPosition = anchorPosition;
        rectTransform.sizeDelta = new Vector2(1000, 100);

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = $"🏆 Achievement Unlocked!\n{achievementName}";
        text.fontSize = fontSize;
        text.color = textColor;
        text.alignment = TextAlignmentOptions.BottomRight;
        
        if (achievementFont != null)
        {
            text.font = achievementFont;
        }

        yield return new WaitForSeconds(displayDuration);

        float elapsed = 0f;
        Color startColor = text.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            Color newColor = startColor;
            newColor.a = Mathf.Lerp(1f, 0f, t);
            text.color = newColor;
            
            yield return null;
        }

        Destroy(textObj);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
