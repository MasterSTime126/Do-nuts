using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MaskManager : MonoBehaviour
{
    public static MaskManager Instance { get; private set; }

    public enum MaskState
    {
        Happiness = 0,
        Sadness = 1,
        Fear = 2,
        Anger = 3,
        Disgust = 4,
        TheEnd = 5
    }

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
        currentHP = maxHP;
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

        if (donutsEaten >= HAPPINESS_DONUTS_REQUIRED && !maskSpawned)
        {
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
        OnMaskCollected?.Invoke();
        AdvanceToNextMask();
    }

    private void AdvanceToNextMask()
    {
        if (currentMask < MaskState.TheEnd)
        {
            currentMask++;
            OnMaskChanged?.Invoke(currentMask);

            if (currentMask == MaskState.TheEnd)
            {
                OnGameEnd?.Invoke(totalPlayTime);
            }
            else
            {
                // Load next level or reset current scene
                ResetLevelProgress();
                currentHP = maxHP * 0.25f; // Partial heal between levels
                OnHPChanged?.Invoke(currentHP, maxHP);
            }
        }
    }

    private void SpawnMask()
    {
        maskSpawned = true;
        // Find and activate the mask object in the scene
        GameObject mask = GameObject.FindGameObjectWithTag("Mask");
        if (mask != null)
        {
            mask.SetActive(true);
        }
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
        Debug.Log("Player died!");
        // Reset level or show game over
        currentHP = maxHP;
        ResetLevelProgress();
        OnHPChanged?.Invoke(currentHP, maxHP);
        // Optionally reload the current scene
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
}
