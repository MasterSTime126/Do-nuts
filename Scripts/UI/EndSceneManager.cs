using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text bestTimeText;

    [Header("Text Settings")]
    [SerializeField] private string winTitle = "Congratulations!";
    [SerializeField] private string loseTitle = "Game Over";
    [SerializeField] private Color winColor = Color.yellow;
    [SerializeField] private Color loseColor = Color.red;

    [Header("Scene Navigation")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    // Static data passed from MaskManager
    private static bool isWin = false;
    private static MaskManager.MaskState deathMask = MaskManager.MaskState.Happiness;
    private static float playTime = 0f;

    public static void SetEndData(bool won, MaskManager.MaskState mask, float time)
    {
        isWin = won;
        deathMask = mask;
        playTime = time;

        Debug.Log($"[EndSceneManager] Data set: isWin={won}, mask={mask}, time={time:F2}s");
    }

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Set title text
        if (titleText != null)
        {
            titleText.text = isWin ? winTitle : loseTitle;
            titleText.color = isWin ? winColor : loseColor;
        }

        // Set stats text
        if (statsText != null)
        {
            if (isWin)
            {
                int minutes = Mathf.FloorToInt(playTime / 60f);
                int seconds = Mathf.FloorToInt(playTime % 60f);
                statsText.text = $"Time: {minutes:00}:{seconds:00}";
            }
            else
            {
                statsText.text = $"Defeated at: {deathMask} level\nTime: {FormatTime(playTime)}";
            }
        }

        // Set best time text
        if (bestTimeText != null)
        {
            float bestTime = MaskManager.GetBestTime();
            if (bestTime > 0)
            {
                bestTimeText.text = $"Best Time: {FormatTime(bestTime)}";
            }
            else
            {
                bestTimeText.text = "Best Time: --:--";
            }
        }

        Debug.Log($"[EndSceneManager] UI Updated - Win: {isWin}, Time: {playTime:F2}s");
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("[EndSceneManager] Returning to main menu");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void RestartGame()
    {
        Debug.Log("[EndSceneManager] Restarting game");
        // Load the game scene (assumes it's scene index 1 or you can set a specific name)
        SceneManager.LoadScene(1);
    }
}
