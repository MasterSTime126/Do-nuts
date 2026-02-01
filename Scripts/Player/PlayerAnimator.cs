    using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Sprite Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Shader Material")]
    [SerializeField] private Material shaderMaterial;  // Assign your shader material here
    private Material materialInstance;  // Unique instance for this player

    [Header("Shader Animation Settings")]
    [SerializeField] private float shaderAnimationDuration = 0.5f;
    [SerializeField] private string shaderProgressProperty = "_Progress";  // Property name in shader
    [SerializeField] private float maskChangeTransitionDuration = 0.75f;  // Duration for mask change effect

    [Header("Mask Materials (Optional - one per mask)")]
    [SerializeField] private Material happinessMaterial;
    [SerializeField] private Material sadnessMaterial;
    [SerializeField] private Material fearMaterial;
    [SerializeField] private Material angerMaterial;
    [SerializeField] private Material disgustMaterial;

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

    [Header("Action Sprites (shared)")]
    [SerializeField] private Sprite[] attackSprites = new Sprite[3];
    [SerializeField] private Sprite[] cleanSprites = new Sprite[2];

    [Header("Animation Settings")]
    [SerializeField] private float walkAnimationSpeed = 0.15f;
    [SerializeField] private float attackAnimationSpeed = 0.1f;
    [SerializeField] private float cleanAnimationSpeed = 0.3f;

    private MaskManager maskManager;
    private int currentWalkFrame = 0;
    private float walkTimer = 0f;
    private bool isWalking = false;
    private bool isPlayingAction = false;
    private bool isFacingRight = true;

    // Shader animation
    private bool isShaderAnimating = false;
    private float shaderAnimationTimer = 0f;
    private bool shaderAnimationForward = true;  // true = 0 to 1, false = 1 to 0

    private void Start()
    {
        maskManager = MaskManager.Instance;
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Subscribe to mask change event
        if (maskManager != null)
        {
            maskManager.OnMaskChanged += OnMaskChanged;
            Debug.Log("PlayerAnimator: Subscribed to OnMaskChanged");
        }
        else
        {
            Debug.LogWarning("PlayerAnimator: MaskManager.Instance is null!");
        }

        // Initialize material - use existing material if no shader material assigned
        if (shaderMaterial != null)
        {
            materialInstance = new Material(shaderMaterial);
            spriteRenderer.material = materialInstance;
            Debug.Log("PlayerAnimator: Created material instance from shaderMaterial");
        }
        else if (spriteRenderer.material != null)
        {
            // Use the existing material on the sprite renderer
            materialInstance = new Material(spriteRenderer.material);
            spriteRenderer.material = materialInstance;
            Debug.Log("PlayerAnimator: Created material instance from existing material");
        }

        UpdateIdleSprite();
        UpdateMaterialForCurrentMask();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent errors
        if (maskManager != null)
        {
            maskManager.OnMaskChanged -= OnMaskChanged;
        }

        // Clean up material instance
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }

    private void OnMaskChanged(MaskManager.MaskState newMask)
    {
        Debug.Log($"PlayerAnimator: OnMaskChanged called with {newMask}");
        // Play appear animation when changing masks
        StartCoroutine(MaskChangeTransition(newMask));
    }

    private System.Collections.IEnumerator MaskChangeTransition(MaskManager.MaskState newMask)
    {
        Debug.Log($"PlayerAnimator: Starting mask transition to {newMask}");
        
        // Change material and sprite first
        UpdateMaterialForMask(newMask);
        UpdateIdleSprite();

        // Play appear animation (fade in from 0 to 1)
        if (materialInstance != null)
        {
            SetShaderProgress(0f);  // Start invisible
            PlayShaderAnimationForward(maskChangeTransitionDuration);  // Fade in
        }
        
        yield return new WaitForSeconds(maskChangeTransitionDuration);
        
        Debug.Log($"PlayerAnimator: Mask transition to {newMask} complete");
    }

    private void UpdateMaterialForCurrentMask()
    {
        if (maskManager != null)
        {
            UpdateMaterialForMask(maskManager.GetMaskState());
        }
    }

    private void UpdateMaterialForMask(MaskManager.MaskState mask)
    {
        Material newMaterial = GetMaterialForMask(mask);
        
        if (newMaterial != null && spriteRenderer != null)
        {
            Debug.Log($"PlayerAnimator: Changing material for {mask}");
            
            // Create new instance from the mask-specific material
            if (materialInstance != null)
            {
                Destroy(materialInstance);
            }
            materialInstance = new Material(newMaterial);
            spriteRenderer.material = materialInstance;
        }
        else
        {
            Debug.LogWarning($"PlayerAnimator: No material found for {mask} (newMaterial={newMaterial}, spriteRenderer={spriteRenderer})");
        }
    }

    private Material GetMaterialForMask(MaskManager.MaskState mask)
    {
        Material result = mask switch
        {
            MaskManager.MaskState.Happiness => happinessMaterial,
            MaskManager.MaskState.Sadness => sadnessMaterial,
            MaskManager.MaskState.Fear => fearMaterial,
            MaskManager.MaskState.Anger => angerMaterial,
            MaskManager.MaskState.Disgust => disgustMaterial,
            _ => null
        };
        
        // Fallback to shaderMaterial if specific material not assigned
        if (result == null)
        {
            result = shaderMaterial;
        }
        
        // Fallback to current material instance
        if (result == null && materialInstance != null)
        {
            result = materialInstance;
        }
        
        return result;
    }

    private void Update()
    {
        if (spriteRenderer == null) return;
        if (isPlayingAction) return;

        // Handle shader animation
        if (isShaderAnimating)
        {
            UpdateShaderAnimation();
        }

        // Handle walk animation
        if (isWalking)
        {
            walkTimer += Time.deltaTime;
            if (walkTimer >= walkAnimationSpeed)
            {
                walkTimer = 0f;
                Sprite[] walkSprites = GetCurrentWalkSprites();
                if (walkSprites != null && walkSprites.Length > 0)
                {
                    currentWalkFrame = (currentWalkFrame + 1) % walkSprites.Length;
                    if (walkSprites[currentWalkFrame] != null)
                    {
                        spriteRenderer.sprite = walkSprites[currentWalkFrame];
                    }
                }
            }
        }
    }

    #region Shader Animation

    /// <summary>
    /// Start shader animation from 0 to 1 over the specified duration
    /// </summary>
    public void PlayShaderAnimationForward()
    {
        PlayShaderAnimationForward(shaderAnimationDuration);
    }

    public void PlayShaderAnimationForward(float duration)
    {
        if (materialInstance == null) return;
        shaderAnimationDuration = duration;
        shaderAnimationTimer = 0f;
        shaderAnimationForward = true;
        isShaderAnimating = true;
        SetShaderProgress(0f);
    }

    /// <summary>
    /// Start shader animation from 1 to 0 over the specified duration
    /// </summary>
    public void PlayShaderAnimationBackward()
    {
        PlayShaderAnimationBackward(shaderAnimationDuration);
    }

    public void PlayShaderAnimationBackward(float duration)
    {
        if (materialInstance == null) return;
        shaderAnimationDuration = duration;
        shaderAnimationTimer = 0f;
        shaderAnimationForward = false;
        isShaderAnimating = true;
        SetShaderProgress(1f);
    }

    /// <summary>
    /// Play shader animation forward then backward (ping-pong)
    /// </summary>
    public void PlayShaderAnimationPingPong(float duration)
    {
        StartCoroutine(ShaderPingPongCoroutine(duration));
    }

    private System.Collections.IEnumerator ShaderPingPongCoroutine(float duration)
    {
        float halfDuration = duration / 2f;
        PlayShaderAnimationForward(halfDuration);
        yield return new WaitForSeconds(halfDuration);
        PlayShaderAnimationBackward(halfDuration);
    }

    private void UpdateShaderAnimation()
    {
        shaderAnimationTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(shaderAnimationTimer / shaderAnimationDuration);

        if (shaderAnimationForward)
        {
            SetShaderProgress(progress);
        }
        else
        {
            SetShaderProgress(1f - progress);
        }

        if (progress >= 1f)
        {
            isShaderAnimating = false;
        }
    }

    public void SetShaderProgress(float value)
    {
        if (materialInstance != null)
        {
            materialInstance.SetFloat(Shader.PropertyToID(shaderProgressProperty), value);
        }
    }

    public void SetShaderColor(Color color)
    {
        if (materialInstance != null)
        {
            materialInstance.SetColor(Shader.PropertyToID("_Color"), color);
        }
    }

    public void SetShaderTexture(Texture2D texture)
    {
        if (materialInstance != null)
        {
            materialInstance.SetTexture(Shader.PropertyToID("_MainTex"), texture);
        }
    }

    public void SetShaderFloat(string propertyName, float value)
    {
        if (materialInstance != null)
        {
            materialInstance.SetFloat(Shader.PropertyToID(propertyName), value);
        }
    }

    #endregion

    #region Sprite Animation

    public void SetWalking(bool walking)
    {
        if (isPlayingAction) return;

        isWalking = walking;
        if (!walking)
        {
            UpdateIdleSprite();
        }
    }

    /// <summary>
    /// Flip sprite based on horizontal movement direction
    /// </summary>
    public void SetFacingDirection(float horizontalInput)
    {
        if (Mathf.Abs(horizontalInput) < 0.01f) return;  // No change if no input

        bool shouldFaceRight = horizontalInput > 0;
        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            FlipSprite();
        }
    }

    /// <summary>
    /// Flip sprite to face right or left
    /// </summary>
    public void SetFacingRight(bool faceRight)
    {
        if (faceRight != isFacingRight)
        {
            isFacingRight = faceRight;
            FlipSprite();
        }
    }

    /// <summary>
    /// Flip the sprite horizontally
    /// </summary>
    public void Flip()
    {
        isFacingRight = !isFacingRight;
        FlipSprite();
    }

    private void FlipSprite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !isFacingRight;
        }
    }

    public bool IsFacingRight() => isFacingRight;

    public void TriggerAttack()
    {
        if (attackSprites.Length > 0)
        {
            StartCoroutine(PlayActionAnimation(attackSprites, attackAnimationSpeed));
        }
    }

    public void TriggerClean()
    {
        if (cleanSprites.Length > 0)
        {
            StartCoroutine(PlayActionAnimation(cleanSprites, cleanAnimationSpeed));
        }
    }

    private System.Collections.IEnumerator PlayActionAnimation(Sprite[] sprites, float frameSpeed)
    {
        isPlayingAction = true;
        isWalking = false;

        foreach (Sprite sprite in sprites)
        {
            if (sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }
            yield return new WaitForSeconds(frameSpeed);
        }

        isPlayingAction = false;
        UpdateIdleSprite();
    }

    private void UpdateIdleSprite()
    {
        Sprite idleSprite = GetCurrentIdleSprite();
        if (idleSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = idleSprite;
        }
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
