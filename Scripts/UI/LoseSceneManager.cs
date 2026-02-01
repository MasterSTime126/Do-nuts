using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoseSceneManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text levelReachedText;
    [SerializeField] private TMP_Text timePlayedText;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Messages")]
    [SerializeField] private string[] deathMessages = new string[]
    {
        "The donuts got you...",
        "Better luck next time!",
        "Don't give up!",
        "You can do it!",
        "Try again?"
    };

    [Header("Animation")]
    [SerializeField] private float textFadeInDuration = 0.75f;
    [SerializeField] private float delayBetweenElements = 0.3f;

    // Static variables to receive data from game scene
    private static MaskManager.MaskState lastMaskState = MaskManager.MaskState.Happiness;
    private static float lastTimePlayed = 0f;

    public static void SetDeathData(MaskManager.MaskState maskState, float timePlayed)
    {
        lastMaskState = maskState;
        lastTimePlayed = timePlayed;
    }

    private void Start()
    {
        SetupUI();

        // Set up buttons
        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.AddListener(TryAgain);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        // Start animation
        StartCoroutine(AnimateUI());
    }

    private void SetupUI()
    {
        if (titleText != null)
        {
            titleText.text = "Game Over";
        }

        if (messageText != null)
        {
            messageText.text = deathMessages[Random.Range(0, deathMessages.Length)];
        }

        if (levelReachedText != null)
        {
            string levelName = GetMaskName(lastMaskState);
            levelReachedText.text = $"Level Reached: {levelName}";
        }

        if (timePlayedText != null)
        {
            timePlayedText.text = $"Time Played: {FormatTime(lastTimePlayed)}";
        }
    }

    private string GetMaskName(MaskManager.MaskState mask)
    {
        return mask switch
        {
            MaskManager.MaskState.Happiness => "Happiness",
            MaskManager.MaskState.Sadness => "Sadness",
            MaskManager.MaskState.Fear => "Fear",
            MaskManager.MaskState.Anger => "Anger",
            MaskManager.MaskState.Disgust => "Disgust",
            MaskManager.MaskState.TheEnd => "Victory",
            _ => "Unknown"
        };
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);

        if (minutes > 0)
        {
            return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
        }
        else
        {
            return $"{seconds:00}.{milliseconds:00}";
        }
    }

    private System.Collections.IEnumerator AnimateUI()
    {
        // Hide all elements initially
        SetAlpha(titleText, 0f);
        SetAlpha(messageText, 0f);
        SetAlpha(levelReachedText, 0f);
        SetAlpha(timePlayedText, 0f);

        if (tryAgainButton != null) tryAgainButton.gameObject.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.3f);

        // Fade in title
        yield return StartCoroutine(FadeInText(titleText));
        yield return new WaitForSeconds(delayBetweenElements);

        // Fade in message
        yield return StartCoroutine(FadeInText(messageText));
        yield return new WaitForSeconds(delayBetweenElements);

        // Fade in level reached
        yield return StartCoroutine(FadeInText(levelReachedText));
        yield return new WaitForSeconds(delayBetweenElements);

        // Fade in time played
        yield return StartCoroutine(FadeInText(timePlayedText));
        yield return new WaitForSeconds(delayBetweenElements);

        // Show buttons
        if (tryAgainButton != null) tryAgainButton.gameObject.SetActive(true);
        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(true);
    }

    private System.Collections.IEnumerator FadeInText(TMP_Text text)
    {
        if (text == null) yield break;

        float elapsed = 0f;
        while (elapsed < textFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / textFadeInDuration);
            SetAlpha(text, alpha);
            yield return null;
        }
        SetAlpha(text, 1f);
    }

    private void SetAlpha(TMP_Text text, float alpha)
    {
        if (text == null) return;
        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }

    public void TryAgain()
    {
        Debug.Log("Trying again...");
        SceneManager.LoadScene(gameSceneName);
    }

    public void GoToMainMenu()
    {
        Debug.Log("Going to main menu...");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void Update()
    {
        // Quick restart with Enter or Space
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            TryAgain();
        }

        // Quick exit with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoToMainMenu();
        }
    }
}
