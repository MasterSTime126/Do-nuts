using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Sprite Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Transition Animation Settings")]
    [SerializeField] private float transitionDuration = 0.75f;
    [SerializeField] private string shaderProperty = "_Vertical";  // Your shader property
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
    private Material material;
    private int shaderPropertyID;
    
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

        // Create material instance from existing material
        if (spriteRenderer != null && spriteRenderer.material != null)
        {
            material = new Material(spriteRenderer.material);
            spriteRenderer.material = material;
            shaderPropertyID = Shader.PropertyToID(shaderProperty);
        }

        // Subscribe to mask change event
        if (maskManager != null)
            maskManager.OnMaskChanged += OnMaskChanged;

        UpdateIdleSprite();
    }

    private void OnDestroy()
    {
        if (maskManager != null)
            maskManager.OnMaskChanged -= OnMaskChanged;

        if (material != null)
            Destroy(material);
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
    /// Play disappear animation before teleporting (0 -> 1)
    /// </summary>
    public void PlayDisappearAnimation(System.Action onComplete = null)
    {
        Debug.Log($"[PlayerAnimator] PlayDisappearAnimation called");
        
        if (useTransitionAnimation && material != null)
        {
            StartCoroutine(DisappearTransition(onComplete));
        }
        else
        {
            Debug.Log("[PlayerAnimator] Transition disabled, invoking callback immediately");
            onComplete?.Invoke();
        }
    }

    private System.Collections.IEnumerator DisappearTransition(System.Action onComplete)
    {
        Debug.Log($"[PlayerAnimator] DISAPPEAR: {shaderProperty} 0 -> 1 over {transitionDuration}s");
        
        // Animate 0 -> 1 (visible to invisible)
        yield return AnimateShader(0f, 1f);
        
        Debug.Log("[PlayerAnimator] DISAPPEAR complete, invoking callback");
        onComplete?.Invoke();
    }

    private System.Collections.IEnumerator AppearTransition(MaskManager.MaskState newMask)
    {
        Debug.Log($"[PlayerAnimator] AppearTransition started for {newMask}");
        
        if (useTransitionAnimation && material != null)
        {
            // Small delay for teleport to complete
            yield return new WaitForSeconds(0.05f);
            
            UpdateIdleSprite();
            
            Debug.Log($"[PlayerAnimator] APPEAR: {shaderProperty} 1 -> 0 over {transitionDuration}s");
            
            // Animate 1 -> 0 (invisible to visible)
            yield return AnimateShader(1f, 0f);
            
            Debug.Log("[PlayerAnimator] APPEAR complete");
        }
        else
        {
            UpdateIdleSprite();
            if (material != null)
                material.SetFloat(shaderPropertyID, 0f);
        }
    }

    private System.Collections.IEnumerator AnimateShader(float from, float to)
    {
        float elapsed = 0f;
        material.SetFloat(shaderPropertyID, from);

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            material.SetFloat(shaderPropertyID, Mathf.Lerp(from, to, t));
            yield return null;
        }

        material.SetFloat(shaderPropertyID, to);
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
