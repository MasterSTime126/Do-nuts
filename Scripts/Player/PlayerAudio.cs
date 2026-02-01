using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource musicSource;  // For background music

    [Header("Movement Sounds")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float footstepInterval = 0.3f;

    [Header("Action Sounds")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip cleanSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip healSound;
    [SerializeField] private AudioClip deathSound;

    [Header("Pickup Sounds")]
    [SerializeField] private AudioClip maskPickupSound;
    [SerializeField] private AudioClip donutEatSound;

    [Header("Mask-Specific Sounds")]
    [SerializeField] private AudioClip happinessAmbience;
    [SerializeField] private AudioClip sadnessAmbience;
    [SerializeField] private AudioClip fearAmbience;
    [SerializeField] private AudioClip angerAmbience;
    [SerializeField] private AudioClip disgustAmbience;

    [Header("Fear Level")]
    [SerializeField] private AudioClip screamerSound;

    [Header("Volume Settings")]
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;

    private MaskManager maskManager;
    private PlayerMovement playerMovement;
    private float footstepTimer = 0f;
    private bool isWalking = false;

    private void Start()
    {
        maskManager = MaskManager.Instance;
        playerMovement = GetComponent<PlayerMovement>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        // Subscribe to events
        if (maskManager != null)
        {
            maskManager.OnMaskChanged += OnMaskChanged;
            maskManager.OnHPChanged += OnHPChanged;
            maskManager.OnMaskCollected += OnMaskCollected;
            maskManager.OnGameEnd += OnGameEnd;
            maskManager.OnGameLost += OnGameLost;
        }

        // Play initial ambience
        PlayAmbienceForCurrentMask();
    }

    private void OnDestroy()
    {
        if (maskManager != null)
        {
            maskManager.OnMaskChanged -= OnMaskChanged;
            maskManager.OnHPChanged -= OnHPChanged;
            maskManager.OnMaskCollected -= OnMaskCollected;
            maskManager.OnGameEnd -= OnGameEnd;
            maskManager.OnGameLost -= OnGameLost;
        }
    }

    private void Update()
    {
        // Handle footstep sounds
        if (isWalking)
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                footstepTimer = 0f;
                PlayFootstep();
            }
        }
    }

    #region Walking
    public void SetWalking(bool walking)
    {
        isWalking = walking;
        if (!walking)
        {
            footstepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        if (footstepSounds != null && footstepSounds.Length > 0)
        {
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            PlaySFX(clip, sfxVolume * 0.5f);  // Footsteps slightly quieter
        }
    }
    #endregion

    #region Action Sounds
    public void PlayAttack()
    {
        PlaySFX(attackSound);
    }

    public void PlayClean()
    {
        PlaySFX(cleanSound);
    }

    public void PlayHurt()
    {
        PlaySFX(hurtSound);
    }

    public void PlayHeal()
    {
        PlaySFX(healSound, sfxVolume * 0.7f);
    }

    public void PlayDeath()
    {
        PlaySFX(deathSound);
    }

    public void PlayDonutEat()
    {
        PlaySFX(donutEatSound);
    }

    public void PlayMaskPickup()
    {
        PlaySFX(maskPickupSound);
    }

    public void PlayScreamer()
    {
        PlaySFX(screamerSound, 1f);  // Full volume for screamer
    }
    #endregion

    #region Event Handlers
    private void OnMaskChanged(MaskManager.MaskState newMask)
    {
        PlayAmbienceForMask(newMask);
    }

    private void OnHPChanged(float currentHP, float maxHP)
    {
        // Could play hurt/heal sounds here based on HP change
    }

    private void OnMaskCollected()
    {
        PlayMaskPickup();
    }

    private void OnGameEnd(float totalTime)
    {
        Debug.Log("PlayerAudio: Game won, stopping music");
        StopMusic();
        StopAllSFX();
    }

    private void OnGameLost()
    {
        Debug.Log("PlayerAudio: Game lost, stopping music");
        StopMusic();
        StopAllSFX();
    }
    #endregion

    #region Ambience/Music
    private void PlayAmbienceForCurrentMask()
    {
        if (maskManager != null)
        {
            PlayAmbienceForMask(maskManager.GetMaskState());
        }
    }

    private void PlayAmbienceForMask(MaskManager.MaskState mask)
    {
        AudioClip ambience = mask switch
        {
            MaskManager.MaskState.Happiness => happinessAmbience,
            MaskManager.MaskState.Sadness => sadnessAmbience,
            MaskManager.MaskState.Fear => fearAmbience,
            MaskManager.MaskState.Anger => angerAmbience,
            MaskManager.MaskState.Disgust => disgustAmbience,
            _ => null
        };

        if (ambience != null && musicSource != null)
        {
            // Fade out old, fade in new (simple version - just switch)
            musicSource.clip = ambience;
            musicSource.volume = musicVolume;
            musicSource.Play();
            Debug.Log($"PlayerAudio: Playing ambience for {mask}");
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void StopAllSFX()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
    #endregion

    #region Utility
    private void PlaySFX(AudioClip clip, float volume = -1f)
    {
        if (clip == null || audioSource == null) return;

        float vol = volume < 0 ? sfxVolume : volume;
        audioSource.PlayOneShot(clip, vol);
    }

    public void PlaySound(AudioClip clip)
    {
        PlaySFX(clip);
    }

    public void PlaySoundAtVolume(AudioClip clip, float volume)
    {
        PlaySFX(clip, volume);
    }
    #endregion
}
