using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoseSceneManager : MonoBehaviour
{
    // Static data stored before scene loads
    private static MaskManager.MaskState deathMask;
    private static float deathPlayTime;
    private static bool hasData = false;

    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text instructionText;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameScene = "GameScene";

    /// <summary>
    /// Call this BEFORE loading the lose scene to pass data
    /// </summary>
    public static void SetDeathData(MaskManager.MaskState mask, float playTime)
    {
        deathMask = mask;
        deathPlayTime = playTime;
        hasData = true;
        Debug.Log($"[LoseSceneManager] Death data set: mask={mask}, time={playTime:F2}s");
    }

    private void Start()
    {
        Debug.Log("[LoseSceneManager] Started");
        
        if (hasData)
        {
            DisplayStats();
        }
        else
        {
            Debug.LogWarning("[LoseSceneManager] No death data available!");
            if (statsText != null)
            {
                statsText.text = "Game Over";
            }
        }

        if (instructionText != null)
        {
            instructionText.text = "Press R to Retry | Press ESC for Main Menu";
        }
    }

    private void DisplayStats()
    {
        if (titleText != null)
        {
            titleText.text = "You Died!";
        }

        if (statsText != null)
        {
            string maskName = deathMask.ToString();
            int minutes = Mathf.FloorToInt(deathPlayTime / 60f);
            int seconds = Mathf.FloorToInt(deathPlayTime % 60f);
            
            statsText.text = $"Died during: {maskName} level\n" +
                            $"Time survived: {minutes:00}:{seconds:00}";
        }
    }

    private void Update()
    {
        // Retry
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[LoseSceneManager] Retrying...");
            hasData = false;
            SceneManager.LoadScene(gameScene);
        }

        // Main Menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("[LoseSceneManager] Going to main menu...");
            hasData = false;
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}
