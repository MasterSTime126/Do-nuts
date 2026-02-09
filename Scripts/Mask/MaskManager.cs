using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MaskManager : MonoBehaviour
{
    public static MaskManager Instance { get; private set; }
    [SerializeField] private GameObject maskPrefab;

    public enum MaskState
    {
        Happiness = 0,
        Sadness = 1,
        Fear = 2,
        Anger = 3,
        Disgust = 4,
        TheEnd = 5
    }

    [Header("UI")]
    [SerializeField] private TMP_Text HPText;
    [SerializeField] private TMP_Text MissionText;

    [Header("Player Stats")]
    [SerializeField] private float maxHP = 100f;
    private float currentHP;

    [Header("Level Progress")]
    private MaskState currentMask = MaskState.Happiness;
    private int donutsEaten = 0;
    private int donutsKilled = 0;
    private int tracesCleared = 0;
    private float survivalTimer = 0f;
    private float totalPlayTime = 0f;
    private static float bestTime = 0f;
    private static float lastTime = 0f;

    [SerializeField] private Vector3[] maskSpawnPositions = new Vector3[5];
    [SerializeField] private Vector3[] playerSpawnPositions = new Vector3[5];
    [SerializeField] private string endSceneName = "EndScene";
    [SerializeField] private string loseSceneName = "LoseScene";

    [Header("Level Requirements")]
    private const int HAPPINESS_DONUTS_REQUIRED = 10;
    private const float SADNESS_SURVIVAL_TIME = 45f;
    private const int ANGER_KILLS_REQUIRED = 10;
    private const int DISGUST_TRACES_REQUIRED = 10;

    // Events for UI and other systems
    public event Action<float, float> OnHPChanged;
    public event Action<MaskState> OnMaskChanged;
    public event Action<int> OnProgressChanged;
    public event Action OnMaskCollected;
    public event Action<float> OnGameEnd;
    public event Action OnGameLost;  // Event for game over (loss)

    private bool maskSpawned = false;
    private bool isInMainMenu = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        currentHP = maxHP * 0.25f;

        // Load best and last times if present (0 means no record yet)
        bestTime = PlayerPrefs.GetFloat("BestTime", 0f);
        lastTime = PlayerPrefs.GetFloat("LastTime", 0f);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Destroy self if returning to main menu
        if (scene.name == "MainMenu")
        {
            Destroy(gameObject);
            return;
        }

        // Reset level-specific progress when entering a new level
        ResetLevelProgress();
    }

    private void Update()
    {
        if(HPText == null) return;
        HPText.text = $"HP: {Mathf.RoundToInt(currentHP)} / {Mathf.RoundToInt(maxHP)}";
        totalPlayTime += Time.deltaTime;
        ProcessStatusEffects();

        switch (currentMask)
        {
            case MaskState.Happiness:
                // Passive HP regen could be added here
                break;

            case MaskState.Sadness:
                survivalTimer += Time.deltaTime;
                Heal(0.5f * Time.deltaTime); // 0.5 HP per second
                if (survivalTimer >= SADNESS_SURVIVAL_TIME && !maskSpawned)
                {
                    MissionText.text = "Find the Mask";
                    SpawnMask();
                }
                break;

            case MaskState.Fear:
                // No healing in fear level
                break;

            case MaskState.Anger:
                // Handled by kills
                break;

            case MaskState.Disgust:
                // Handled by trace clearing
                break;
        }
    }

    #region HP Management
    public void TakeDamage(float damage)
    {
        currentHP = Mathf.Clamp(currentHP - damage, 0f, maxHP);
        OnHPChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0f)
        {
            HandlePlayerDeath();
        }
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0f, maxHP);
        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    public float GetCurrentHP() => currentHP;
    public float GetMaxHP() => maxHP;
    #endregion

    #region Progress Tracking
    public void OnDonutEaten()
    {
        if (currentMask != MaskState.Happiness) return;

        donutsEaten++;
        Heal(10f);
        OnProgressChanged?.Invoke(donutsEaten);
        Debug.Log("Donut eaten: " + donutsEaten);

        if (donutsEaten >= HAPPINESS_DONUTS_REQUIRED && !maskSpawned)
        {
            MissionText.text = "Find the Mask";
            SpawnMask();
        }
    }

    public void OnDonutKilled()
    {
        if (currentMask != MaskState.Anger) return;

        donutsKilled++;
        Heal(2f);
        OnProgressChanged?.Invoke(donutsKilled);

        if (donutsKilled >= ANGER_KILLS_REQUIRED && !maskSpawned)
        {
            MissionText.text = "Find the Mask";
            SpawnMask();
        }
    }

    public void OnTraceCleared()
    {
        if (currentMask != MaskState.Disgust) return;

        tracesCleared++;
        OnProgressChanged?.Invoke(tracesCleared);

        if (tracesCleared >= DISGUST_TRACES_REQUIRED && !maskSpawned)
        {
            MissionText.text = "Find the Mask";
            SpawnMask();
        }
    }

    public float GetSurvivalTimer() => survivalTimer;
    public float GetSurvivalRequired() => SADNESS_SURVIVAL_TIME;
    #endregion

    #region Mask Management
    public MaskState GetMaskState() => currentMask;
    public int GetMaskStateInt() => (int)currentMask;

    public void CollectMask()
    {
        Debug.Log("[MaskManager] CollectMask called");
        OnMaskCollected?.Invoke();
        StartCoroutine(CollectMaskWithTransition());
    }

    private System.Collections.IEnumerator CollectMaskWithTransition()
    {
        // Step 1: Play disappear animation
        PlayerAnimator playerAnimator = FindAnyObjectByType<PlayerAnimator>();
        
        if (playerAnimator != null)
        {
            bool animationComplete = false;
            
            Debug.Log("[MaskManager] Starting disappear animation...");
            playerAnimator.PlayDisappearAnimation(() => animationComplete = true);
            
            // Wait for animation (with 3s timeout)
            float elapsed = 0f;
            while (!animationComplete && elapsed < 3f)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            Debug.Log($"[MaskManager] Disappear animation done ({elapsed:F2}s)");
        }
        
        // Step 2: Advance to next mask (teleport happens here)
        Debug.Log("[MaskManager] Advancing to next mask...");
        AdvanceToNextMask();
    }

    private void TeleportPlayer(int levelIndex)
    {
        if (playerSpawnPositions == null || levelIndex >= playerSpawnPositions.Length)
        {
            Debug.LogError($"[MaskManager] Invalid teleport: levelIndex={levelIndex}");
            return;
        }
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 targetPos = playerSpawnPositions[levelIndex];
            Debug.Log($"[MaskManager] Teleporting to {(MaskState)levelIndex}: {targetPos}");
            player.transform.position = targetPos;
        }
        else
        {
            Debug.LogError("[MaskManager] Player not found!");
        }
    }

    private void AdvanceToNextMask()
    {
        maskSpawned = false;
        
        if (currentMask >= MaskState.TheEnd) return;
        
        currentMask++;
        Debug.Log($"[MaskManager] Advanced to: {currentMask}");
        
        // Notify listeners (triggers appear animation)
        OnMaskChanged?.Invoke(currentMask);
        
        // Teleport player
        TeleportPlayer((int)currentMask);
        
        // Update mission text and spawn mask if needed
        switch (currentMask)
        {
            case MaskState.Happiness:
                MissionText.text = $"Eat {HAPPINESS_DONUTS_REQUIRED} Donuts";
                break;
            case MaskState.Sadness:
                MissionText.text = $"Survive for {SADNESS_SURVIVAL_TIME} Seconds";
                break;
            case MaskState.Fear:
                MissionText.text = $"Find the Mask";
                SpawnMask();
                break;
            case MaskState.Anger:
                MissionText.text = $"Eliminate {ANGER_KILLS_REQUIRED} Donuts (press LMB)";
                break;
            case MaskState.Disgust:
                MissionText.text = $"Clear {DISGUST_TRACES_REQUIRED} Traces (hold E)";
                break;
            case MaskState.TheEnd:
                UpdateBestAndLastTimes();
                OnGameEnd?.Invoke(totalPlayTime);
                Debug.Log($"[MaskManager] Game complete! Time: {totalPlayTime:F2}s");
                StartCoroutine(LoadEndScene());
                break;
        }
    }

    private System.Collections.IEnumerator LoadEndScene()
    {
        Debug.Log($"[MaskManager] Loading win scene: {endSceneName}");
        yield return new WaitForSeconds(0.5f);
        
        if (!string.IsNullOrEmpty(endSceneName) && Application.CanStreamedLevelBeLoaded(endSceneName))
        {
            SceneManager.LoadScene(endSceneName);
        }
        else
        {
            Debug.LogWarning($"[MaskManager] End scene '{endSceneName}' not found!");
        }
    }

    private void SpawnMask()
    {
        maskSpawned = true;
        Debug.Log($"[MaskManager] Spawning mask for {currentMask}");
        GameObject mask = Instantiate(maskPrefab, maskSpawnPositions[(int)currentMask], Quaternion.identity);
        mask.SetActive(true);
    }

    private void ResetLevelProgress()
    {
        donutsEaten = 0;
        donutsKilled = 0;
        tracesCleared = 0;
        survivalTimer = 0f;
        maskSpawned = false;
    }
    #endregion

    #region Player Death
    private void HandlePlayerDeath()
    {
        Debug.Log("[MaskManager] Player died!");
        OnGameLost?.Invoke();
        LoseSceneManager.SetDeathData(currentMask, totalPlayTime);
        StartCoroutine(LoadLoseScene());
    }

    private System.Collections.IEnumerator LoadLoseScene()
    {
        Debug.Log($"[MaskManager] Loading lose scene: {loseSceneName}");
        yield return new WaitForSeconds(0.5f);
        
        if (!string.IsNullOrEmpty(loseSceneName) && Application.CanStreamedLevelBeLoaded(loseSceneName))
        {
            SceneManager.LoadScene(loseSceneName);
        }
        else
        {
            Debug.LogWarning("[MaskManager] Lose scene not found, resetting level");
            currentHP = maxHP * 0.25f;
            ResetLevelProgress();
            OnHPChanged?.Invoke(currentHP, maxHP);
            TeleportPlayer((int)currentMask);
        }
    }
    #endregion

    #region Status Effects
    private float slowMultiplier = 1f;
    private float poisonDamagePerSecond = 0f;
    private float poisonDuration = 0f;
    private float slowDuration = 0f;

    public void ApplyPoison(float damagePerSecond, float duration)
    {
        poisonDamagePerSecond = damagePerSecond;
        poisonDuration = duration;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        slowMultiplier = multiplier;
        slowDuration = duration;
    }

    public float GetSpeedMultiplier() => slowDuration > 0 ? slowMultiplier : 1f;

    private void ProcessStatusEffects()
    {
        if (poisonDuration > 0)
        {
            poisonDuration -= Time.deltaTime;
            TakeDamage(poisonDamagePerSecond * Time.deltaTime);
        }

        if (slowDuration > 0)
        {
            slowDuration -= Time.deltaTime;
        }
    }
    #endregion

    #region Best / Last Time Persistence
    private void UpdateBestAndLastTimes()
    {
        lastTime = totalPlayTime;
        PlayerPrefs.SetFloat("LastTime", lastTime);

        if (bestTime <= 0f || totalPlayTime < bestTime)
        {
            bestTime = totalPlayTime;
            PlayerPrefs.SetFloat("BestTime", bestTime);
            Debug.Log($"[MaskManager] New best time: {bestTime:F2}s");
        }

        PlayerPrefs.Save();
    }

    public static float GetBestTime() => bestTime;
    public static float GetLastTime() => lastTime;
    #endregion
}