using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Sprite Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Transition Animation Settings")]
    [SerializeField] private bool useTransitionAnimation = true;

    [Header("Happiness Sprites")]
    [SerializeField] private Sprite happinessIdle;
    [SerializeField] private Sprite[] happinessWalk = new Sprite[2];

    [Header("Sadness Sprites")]
    [SerializeField] private Sprite sadnessIdle;
    [SerializeField] private Sprite[] sadnessWalk = new Sprite[2];

    [Header("Fear Sprites")]
    [SerializeField] private Sprite fearIdle;
    [SerializeField] private Sprite[] fearWalk = new Sprite[2];

    [Header("Anger Sprites")]
    [SerializeField] private Sprite angerIdle;
    [SerializeField] private Sprite[] angerWalk = new Sprite[2];

    [Header("Disgust Sprites")]
    [SerializeField] private Sprite disgustIdle;
    [SerializeField] private Sprite[] disgustWalk = new Sprite[2];

    [Header("Action Sprites")]
    [SerializeField] private Sprite[] attackSprites = new Sprite[3];
    [SerializeField] private Sprite[] cleanSprites = new Sprite[2];

    [Header("Animation Speeds")]
    [SerializeField] private float walkSpeed = 0.15f;
    [SerializeField] private float attackSpeed = 0.1f;
    [SerializeField] private float cleanSpeed = 0.3f;

    private MaskManager maskManager;
    private ShaderToggle shaderToggle;
    
    private int walkFrame = 0;
    private float walkTimer = 0f;
    private bool isWalking = false;
    private bool isPlayingAction = false;
    private bool isFacingRight = true;

    #region Unity Lifecycle

    private void Start()
    {
        maskManager = MaskManager.Instance;
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Get ShaderToggle component
        shaderToggle = GetComponent<ShaderToggle>();
        if (shaderToggle == null)
            shaderToggle = GetComponentInChildren<ShaderToggle>();

        // Subscribe to mask change event
        if (maskManager != null)
            maskManager.OnMaskChanged += OnMaskChanged;

        UpdateIdleSprite();
        
        Debug.Log($"[PlayerAnimator] Started. ShaderToggle found: {shaderToggle != null}");
    }

    private void OnDestroy()
    {
        if (maskManager != null)
            maskManager.OnMaskChanged -= OnMaskChanged;
    }

    private void Update()
    {
        if (spriteRenderer == null || isPlayingAction) return;

        if (isWalking)
            AnimateWalk();
    }

    #endregion

    #region Mask Transition

    private void OnMaskChanged(MaskManager.MaskState newMask)
    {
        Debug.Log($"[PlayerAnimator] OnMaskChanged: {newMask}");
        StartCoroutine(AppearTransition(newMask));
    }

    /// <summary>
    /// Play disappear animation before teleporting
    /// </summary>
    public void PlayDisappearAnimation(System.Action onComplete = null)
    {
        Debug.Log($"[PlayerAnimator] PlayDisappearAnimation called");
        
        if (useTransitionAnimation && shaderToggle != null)
        {
            StartCoroutine(DisappearTransition(onComplete));
        }
        else
        {
            Debug.Log("[PlayerAnimator] No ShaderToggle or transition disabled, invoking callback immediately");
            onComplete?.Invoke();
        }
    }

    private System.Collections.IEnumerator DisappearTransition(System.Action onComplete)
    {
        Debug.Log("[PlayerAnimator] DISAPPEAR animation starting...");
        
        // Use ShaderToggle.Disappear() like DonutLogic
        yield return StartCoroutine(shaderToggle.Disappear(false, true));
        
        Debug.Log("[PlayerAnimator] DISAPPEAR complete, invoking callback");
        onComplete?.Invoke();
    }

    private System.Collections.IEnumerator AppearTransition(MaskManager.MaskState newMask)
    {
        Debug.Log($"[PlayerAnimator] AppearTransition started for {newMask}");
        
        // Small delay for teleport to complete
        yield return new WaitForSeconds(0.05f);
        
        UpdateIdleSprite();
        
        if (useTransitionAnimation && shaderToggle != null)
        {
            Debug.Log("[PlayerAnimator] APPEAR animation starting...");
            
            // Use ShaderToggle.Appear() like DonutLogic
            yield return StartCoroutine(shaderToggle.Appear(false, true));
            
            Debug.Log("[PlayerAnimator] APPEAR complete");
        }
    }

    #endregion

    #region Walk Animation

    public void SetWalking(bool walking)
    {
        if (isPlayingAction) return;

        isWalking = walking;
        if (!walking)
            UpdateIdleSprite();
    }

    private void AnimateWalk()
    {
        walkTimer += Time.deltaTime;
        if (walkTimer < walkSpeed) return;

        walkTimer = 0f;
        Sprite[] sprites = GetCurrentWalkSprites();
        
        if (sprites != null && sprites.Length > 0)
        {
            walkFrame = (walkFrame + 1) % sprites.Length;
            if (sprites[walkFrame] != null)
                spriteRenderer.sprite = sprites[walkFrame];
        }
    }

    #endregion

    #region Facing Direction

    public void SetFacingDirection(float horizontalInput)
    {
        if (Mathf.Abs(horizontalInput) < 0.01f) return;

        bool shouldFaceRight = horizontalInput > 0;
        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            spriteRenderer.flipX = !isFacingRight;
        }
    }

    public void SetFacingRight(bool faceRight)
    {
        if (faceRight != isFacingRight)
        {
            isFacingRight = faceRight;
            spriteRenderer.flipX = !isFacingRight;
        }
    }

    public void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !isFacingRight;
    }

    public bool IsFacingRight() => isFacingRight;

    #endregion

    #region Action Animations

    public void TriggerAttack()
    {
        if (attackSprites.Length > 0)
            StartCoroutine(PlayActionAnimation(attackSprites, attackSpeed));
    }

    public void TriggerClean()
    {
        if (cleanSprites.Length > 0)
            StartCoroutine(PlayActionAnimation(cleanSprites, cleanSpeed));
    }

    private System.Collections.IEnumerator PlayActionAnimation(Sprite[] sprites, float frameSpeed)
    {
        isPlayingAction = true;
        isWalking = false;

        foreach (Sprite sprite in sprites)
        {
            if (sprite != null)
                spriteRenderer.sprite = sprite;
            yield return new WaitForSeconds(frameSpeed);
        }

        isPlayingAction = false;
        UpdateIdleSprite();
    }

    #endregion

    #region Sprite Helpers

    private void UpdateIdleSprite()
    {
        Sprite idle = GetCurrentIdleSprite();
        if (idle != null && spriteRenderer != null)
            spriteRenderer.sprite = idle;
    }

    private Sprite GetCurrentIdleSprite()
    {
        if (maskManager == null) return happinessIdle;

        return maskManager.GetMaskState() switch
        {
            MaskManager.MaskState.Happiness => happinessIdle,
            MaskManager.MaskState.Sadness => sadnessIdle,
            MaskManager.MaskState.Fear => fearIdle,
            MaskManager.MaskState.Anger => angerIdle,
            MaskManager.MaskState.Disgust => disgustIdle,
            _ => happinessIdle
        };
    }

    private Sprite[] GetCurrentWalkSprites()
    {
        if (maskManager == null) return happinessWalk;

        return maskManager.GetMaskState() switch
        {
            MaskManager.MaskState.Happiness => happinessWalk,
            MaskManager.MaskState.Sadness => sadnessWalk,
            MaskManager.MaskState.Fear => fearWalk,
            MaskManager.MaskState.Anger => angerWalk,
            MaskManager.MaskState.Disgust => disgustWalk,
            _ => happinessWalk
        };
    }

    #endregion
}
