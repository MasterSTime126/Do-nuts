using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EndSceneManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text bestTimeText;
    [SerializeField] private TMP_Text newRecordText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Animation")]
    [SerializeField] private float textFadeInDuration = 1f;
    [SerializeField] private float delayBetweenElements = 0.5f;

    private void Start()
    {
        // Get times from MaskManager
        float lastTime = MaskManager.GetLastTime();
        float bestTime = MaskManager.GetBestTime();
        bool isNewRecord = Mathf.Approximately(lastTime, bestTime) && lastTime > 0;

        // Try to find WONTIME object in scene if timeText not assigned
        if (timeText == null)
        {
            GameObject winTimeObj = GameObject.Find("WONTIME");
            if (winTimeObj != null)
            {
                timeText = winTimeObj.GetComponent<TMP_Text>();
                Debug.Log("EndSceneManager: Found WONTIME object");
            }
            else{
                winTimeObj = GameObject.FindWithTag("WONTIME");
                if (winTimeObj != null)
                {
                    timeText = winTimeObj.GetComponent<TMP_Text>();
                    Debug.Log("EndSceneManager: Found WONTIME object by tag");
                }
            }
        }

        // Set up UI
        SetupUI(lastTime, bestTime, isNewRecord);

        // Set up buttons
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(PlayAgain);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        // Start animation
        StartCoroutine(AnimateUI());
    }

    private void SetupUI(float lastTime, float bestTime, bool isNewRecord)
    {
        if (titleText != null)
        {
            titleText.text = "Congratulations!";
        }

        if (timeText != null)
        {
            timeText.text = $"Your Time: {FormatTime(lastTime)}";
        }

        if (bestTimeText != null)
        {
            bestTimeText.text = $"Best Time: {FormatTime(bestTime)}";
        }

        if (newRecordText != null)
        {
            newRecordText.gameObject.SetActive(isNewRecord);
            if (isNewRecord)
            {
                newRecordText.text = "NEW RECORD!";
            }
        }
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
        SetAlpha(timeText, 0f);
        SetAlpha(bestTimeText, 0f);
        SetAlpha(newRecordText, 0f);

        if (playAgainButton != null) playAgainButton.gameObject.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        // Fade in title
        yield return StartCoroutine(FadeInText(titleText));
        yield return new WaitForSeconds(delayBetweenElements);

        // Fade in time
        yield return StartCoroutine(FadeInText(timeText));
        yield return new WaitForSeconds(delayBetweenElements);

        // Fade in best time
        yield return StartCoroutine(FadeInText(bestTimeText));
        yield return new WaitForSeconds(delayBetweenElements);

        // Fade in new record (if applicable)
        if (newRecordText != null && newRecordText.gameObject.activeSelf)
        {
            yield return StartCoroutine(FadeInText(newRecordText));
            yield return new WaitForSeconds(delayBetweenElements);
        }

        // Show buttons
        if (playAgainButton != null) playAgainButton.gameObject.SetActive(true);
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

    public void PlayAgain()
    {
        Debug.Log("Playing again...");
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
            PlayAgain();
        }

        // Quick exit with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoToMainMenu();
        }
    }
}
