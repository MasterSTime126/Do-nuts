using UnityEngine;

public class DonutLogic : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 1.5f;

    [Header("Damage Settings")]
    [SerializeField] private float sadnessDamagePerSecond = 1f;
    [SerializeField] private float fearDamage = 5f;
    [SerializeField] private float angerDamage = 5f;
    [SerializeField] private float angerDamageCooldown = 2f;
    [SerializeField] private float disgustDamage = 10f;
    [SerializeField] private float explosionTime = 5f;

    [Header("Fear Screamer Settings")]
    [SerializeField] private float screamerDuration = 0.5f;
    [SerializeField] private float screamerMaxScale = 5f;

    [Header("Fear Ghost Settings")]
    [SerializeField] private float ghostMinAlpha = 0.3f;
    [SerializeField] private float ghostMaxAlpha = 0.8f;
    [SerializeField] private float ghostFlickerSpeed = 2f;
    [SerializeField] private float ghostAppearDuration = 0.5f;

    // CHECKPOINTHEREIDIOT - Donut Shader Animation Settings
    // Change these values to adjust the appear animation:
    // shaderAnimationDuration = how long the animation takes (default 0.5f)
    // shaderProgressProperty = the property name in your shader (default "_Progress")
    // startValue = starting value for appear animation (0 = invisible)
    // endValue = ending value for appear animation (1 = fully visible)
    [Header("Shader Animation Settings")]
    [SerializeField] private float shaderAnimationDuration = 0.5f;
    [SerializeField] private string shaderProgressProperty = "_Progress";
    [SerializeField] private bool useAppearAnimation = true;

    private Material materialInstance;

    [Header("References")]
    [SerializeField] private GameObject tracePrefab;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private GameObject childObject;

    private MaskManager maskManager;
    private Transform player;
    private DonutAnimator donutAnimator;
    private float damageTimer = 0f;
    private float explosionTimer = 0f;
    private bool isNearPlayer = false;
    private bool isDestroying = false;
    private bool isBeingDestroyed = false;

    private MaskManager.MaskState currentMaskState;
    private MaskManager.MaskState newMaskState;
    private bool isGhostMode = false;
    private Color originalColor;

    private void Start()
    {
        childObject = transform.GetChild(0).gameObject;
        maskManager = MaskManager.Instance;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        donutAnimator = GetComponent<DonutAnimator>();

        currentMaskState = maskManager != null ? maskManager.GetMaskState() : MaskManager.MaskState.Happiness;

        // Initialize based on mask state
        if (currentMaskState == MaskManager.MaskState.Disgust)
        {
            explosionTimer = explosionTime;
        }
        
        // Fear mode: Enable ghost mode (pass through walls)
        if (currentMaskState == MaskManager.MaskState.Fear)
        {
            EnableGhostMode();
        }

        moveSpeed = Random.Range(moveSpeed, moveSpeed * 2f);

        // Create material instance for shader animation
        if (spriteRenderer != null && spriteRenderer.material != null)
        {
            materialInstance = new Material(spriteRenderer.material);
            spriteRenderer.material = materialInstance;
            originalColor = spriteRenderer.color;
        }
    }

    private void Update()
    {
        if (maskManager == null || player == null || isDestroying) return;

        newMaskState = maskManager.GetMaskState();
        if(newMaskState != currentMaskState){
            Debug.Log("Donut destroyed due to mask state change");
            Destroy(gameObject);
        }

        switch (currentMaskState)
        {
            case MaskManager.MaskState.Happiness:
                BehaviorHappiness();
                break;
            case MaskManager.MaskState.Sadness:
                BehaviorSadness();
                break;
            case MaskManager.MaskState.Fear:
                BehaviorFear();
                break;
            case MaskManager.MaskState.Anger:
                BehaviorAnger();
                break;
            case MaskManager.MaskState.Disgust:
                BehaviorDisgust();
                break;
        }

        
    }

    #region Mask Behaviors
    private void BehaviorHappiness()
    {
        // Donuts just sit there, player can eat them
        GetComponent<Collider>().isTrigger = true;
    }

    private void BehaviorSadness()
    {
        // Chase player, stop near and deal damage over time
        GetComponent<Collider>().isTrigger = false;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > stopDistance)
        {
            MoveTowards(player.position);
            isNearPlayer = false;
            if (donutAnimator != null) donutAnimator.SetPaused(false);
        }
        else
        {
            isNearPlayer = true;
            if (donutAnimator != null) donutAnimator.SetPaused(true);
            // Deal damage over time when near
            maskManager.TakeDamage(sadnessDamagePerSecond * Time.deltaTime);
        }
    }

    private void BehaviorFear()
    {
        // Chase player, on collision: screamer, damage, blur, then disappear
        MoveTowards(player.position);
        
        // Ghost flickering effect
        if (isGhostMode && spriteRenderer != null)
        {
            float flicker = Mathf.Lerp(ghostMinAlpha, ghostMaxAlpha, 
                (Mathf.Sin(Time.time * ghostFlickerSpeed) + 1f) * 0.5f);
            Color c = originalColor;
            c.a = flicker;
            spriteRenderer.color = c;
        }
    }

    private void EnableGhostMode()
    {
        isGhostMode = true;
        
        // Make collider a trigger to pass through walls but still detect player
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
            Debug.Log($"[DonutLogic] Ghost collider isTrigger={col.isTrigger}");
        }
        
        // Add Rigidbody if not present (required for trigger detection)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;  // Don't use physics, just for trigger detection
            rb.useGravity = false;
            Debug.Log("[DonutLogic] Added kinematic Rigidbody for trigger detection");
        }
        
        // Start with ghost appear animation
        if (spriteRenderer != null)
        {
            StartCoroutine(GhostAppearAnimation());
        }
        
        Debug.Log("[DonutLogic] Ghost mode enabled");
    }

    private System.Collections.IEnumerator GhostAppearAnimation()
    {
        // Fade in from invisible
        float elapsed = 0f;
        Color c = originalColor;
        c.a = 0f;
        spriteRenderer.color = c;

        while (elapsed < ghostAppearDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / ghostAppearDuration;
            c.a = Mathf.Lerp(0f, ghostMaxAlpha, t);
            spriteRenderer.color = c;
            yield return null;
        }
    }

    private System.Collections.IEnumerator GhostDisappearAnimation()
    {
        // Fade out before destroying
        float elapsed = 0f;
        Color c = spriteRenderer.color;
        float startAlpha = c.a;

        while (elapsed < ghostAppearDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / ghostAppearDuration;
            c.a = Mathf.Lerp(startAlpha, 0f, t);
            spriteRenderer.color = c;
            yield return null;
        }
    }

    private void BehaviorAnger()
    {
        // Chase player, deal damage on collision with cooldown
        GetComponent<Collider>().isTrigger = false;
        MoveTowards(player.position);

        // Cooldown timer for damage
        if (damageTimer > 0)
        {
            damageTimer -= Time.deltaTime;
        }
    }

    private void BehaviorDisgust()
    {
        // Countdown to explosion
        GetComponent<Collider>().isTrigger = false;
        explosionTimer -= Time.deltaTime;

        // Visual feedback for explosion countdown
        if (spriteRenderer != null)
        {
            float t = explosionTimer / explosionTime;
            spriteRenderer.color = Color.Lerp(Color.red, Color.white, t);
        }

        if (explosionTimer <= 0)
        {
            Explode();
        }

        // Move towards player
        MoveTowards(player.position);
    }
    #endregion

    #region Movement
    private void MoveTowards(Vector3 target)
    {
        if (isDestroying) return;

        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;
        if (maskManager != null && maskManager.GetMaskState() != MaskManager.MaskState.Happiness)
        {
            if (childObject != null)
            {
                // Face right if player is to the right, otherwise face left
                if (player != null && player.position.x < transform.position.x)
                    childObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                else
                    childObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
        }
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void MoveAwayFrom(Vector3 target)
    {
        Vector3 direction = (transform.position - target).normalized;
        direction.y = 0;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }
    #endregion

    #region Collision Handling
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[DonutLogic] OnTriggerEnter: {other.name}, tag={other.tag}, isDestroying={isDestroying}");
        
        if (!other.CompareTag("Player") || isDestroying) return;

        MaskManager.MaskState state = maskManager.GetMaskState();
        Debug.Log($"[DonutLogic] Player triggered! state={state}, isGhostMode={isGhostMode}");

        if (state == MaskManager.MaskState.Happiness)
        {
            // Player eats donut
            Debug.Log("[DonutLogic] Happiness - eating donut");
            maskManager.OnDonutEaten();
            BeforeDestroy();
        }
        else if (state == MaskManager.MaskState.Fear && isGhostMode)
        {
            // Ghost donut hits player - trigger screamer
            Debug.Log("[DonutLogic] FEAR - Ghost hit player - SCREAMER!");
            maskManager.TakeDamage(fearDamage);
            
            // Trigger screamer sound
            PlayerAudio playerAudio = other.GetComponent<PlayerAudio>();
            if (playerAudio != null)
            {
                playerAudio.PlayScreamer();
                Debug.Log("[DonutLogic] Screamer sound played");
            }
            else
            {
                Debug.LogWarning("[DonutLogic] PlayerAudio not found on player!");
            }
            
            // Play screamer visual effect
            StartCoroutine(ScreamerEffect());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player") || isDestroying) return;

        MaskManager.MaskState currentMask = maskManager.GetMaskState();

        switch (currentMask)
        {
            // Note: Fear mode uses OnTriggerEnter since ghosts are triggers
            
            case MaskManager.MaskState.Anger:
                // Deal damage with cooldown, don't destroy
                if (damageTimer <= 0)
                {
                    maskManager.TakeDamage(angerDamage);
                    damageTimer = angerDamageCooldown;
                    if (donutAnimator != null) donutAnimator.TriggerAttack();
                }
                break;

            case MaskManager.MaskState.Disgust:
                // Instant damage and create trace
                maskManager.TakeDamage(disgustDamage);
                CreateTrace();
                BeforeDestroy();
                break;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player") || isDestroying) return;

        if (maskManager.GetMaskState() == MaskManager.MaskState.Anger)
        {
            if (damageTimer <= 0)
            {
                maskManager.TakeDamage(angerDamage);
                damageTimer = angerDamageCooldown;
            }
        }
    }
    #endregion

    #region Special Effects
    private void TriggerScreamer()
    {
        // TODO: Implement screamer effect
        // - Play screamer sound
        // - Apply blur effect to camera
        Debug.Log("SCREAMER!");
    }

    private void Explode()
    {
        if (donutAnimator != null) donutAnimator.TriggerExplosion();
        CreateTrace();
        BeforeDestroy();
    }

    private void CreateTrace()
    {
        if (tracePrefab != null)
        {
            Vector3 tracePos = transform.position;
            tracePos.y = 0.01f; // Slightly above ground
            Instantiate(tracePrefab, tracePos, Quaternion.identity);
        }
    }
    #endregion

    #region Destruction
    public void BeforeDestroy()
    {
        if (isDestroying) return;
        isDestroying = true;

        // TODO: Play death animation
        // For now, just destroy immediately
        // You can add animation coroutine here
        StartCoroutine(DestroyWithAnimation());
    }

    private System.Collections.IEnumerator DestroyWithAnimation()
    {
        // Use ghost fade out in Fear mode
        if (isGhostMode)
        {
            yield return StartCoroutine(GhostDisappearAnimation());
        }
        else
        {
            // Use shader toggle for other modes
            ShaderToggle shader = GetComponentInChildren<ShaderToggle>();
            if (shader != null)
            {
                yield return StartCoroutine(shader.Disappear(true, false));
            }
        }
        
        Destroy(gameObject);
    }

    private System.Collections.IEnumerator ScreamerEffect()
    {
        // Disable movement and collision during screamer
        isBeingDestroyed = true;
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Get sprite renderer for alpha fade
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * screamerMaxScale;
        Color originalColor = sr != null ? sr.color : Color.white;

        float elapsed = 0f;
        while (elapsed < screamerDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / screamerDuration;

            // Scale up
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);

            // Fade out alpha
            if (sr != null)
            {
                Color newColor = originalColor;
                newColor.a = Mathf.Lerp(1f, 0f, t);
                sr.color = newColor;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
    #endregion

    #region Shader Animation
    

    /// <summary>
    /// Play disappear animation (1 to 0)
    /// </summary>
    public void PlayDisappearAnimation()
    {
        if (materialInstance == null) return;
        StartCoroutine(GetComponent<ShaderToggle>().Disappear(true, false));
    }

    /// <summary>
    /// Core animation coroutine - animates shader progress from startValue to endValue
    /// </summary>
    
    public void SetShaderProgress(float value)
    {
        if (materialInstance != null)
        {
            materialInstance.SetFloat(Shader.PropertyToID(shaderProgressProperty), value);
        }
    }

    #endregion
}
