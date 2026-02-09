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

    private void Start()
    {
        childObject = transform.GetChild(0).gameObject;
        maskManager = MaskManager.Instance;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        donutAnimator = GetComponent<DonutAnimator>();

        // Initialize explosion timer for Disgust level
        if (maskManager != null && maskManager.GetMaskState() == MaskManager.MaskState.Disgust)
        {
            explosionTimer = explosionTime;
        }

        

        moveSpeed = Random.Range(moveSpeed, moveSpeed * 2f);

        currentMaskState = maskManager.GetMaskState();

        // Create material instance for shader animation
        if (spriteRenderer != null && spriteRenderer.material != null)
        {
            materialInstance = new Material(spriteRenderer.material);
            spriteRenderer.material = materialInstance;
            
            // Play appear animation on spawn
            if (useAppearAnimation)
            {
                PlayAppearAnimation();
            }
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
        GetComponent<Collider>().isTrigger = false;
        MoveTowards(player.position);
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
        if (!other.CompareTag("Player") || isDestroying) return;

        if (maskManager.GetMaskState() == MaskManager.MaskState.Happiness)
        {
            // Player eats donut
            maskManager.OnDonutEaten();
            BeforeDestroy();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player") || isDestroying) return;

        MaskManager.MaskState currentMask = maskManager.GetMaskState();

        switch (currentMask)
        {
            case MaskManager.MaskState.Fear:
                // Deal damage on touch, trigger screamer, then destroy
                maskManager.TakeDamage(fearDamage);
                
                // Trigger screamer sound on player
                PlayerAudio playerAudio = collision.gameObject.GetComponent<PlayerAudio>();
                if (playerAudio != null)
                {
                    playerAudio.PlayScreamer();
                    Debug.Log("DonutLogic: Screamer triggered!");
                }
                
                BeforeDestroy();
                break;

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
        // Simple scale down animation
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 originalScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            yield return null;
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
    /// Play appear animation (0 to 1)
    /// </summary>
    public void PlayAppearAnimation()
    {
        if (materialInstance == null) return;
        StartCoroutine(AnimateShaderProgress(0f, 1f, shaderAnimationDuration));
    }

    /// <summary>
    /// Play disappear animation (1 to 0)
    /// </summary>
    public void PlayDisappearAnimation()
    {
        if (materialInstance == null) return;
        StartCoroutine(AnimateShaderProgress(1f, 0f, shaderAnimationDuration));
    }

    /// <summary>
    /// Core animation coroutine - animates shader progress from startValue to endValue
    /// </summary>
    private System.Collections.IEnumerator AnimateShaderProgress(float startValue, float endValue, float duration)
    {
        if (materialInstance == null) yield break;

        int propertyID = Shader.PropertyToID(shaderProgressProperty);
        float elapsed = 0f;

        // Set initial value
        materialInstance.SetFloat(propertyID, startValue);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentValue = Mathf.Lerp(startValue, endValue, t);
            materialInstance.SetFloat(propertyID, currentValue);
            yield return null;
        }

        // Ensure we hit the exact end value
        materialInstance.SetFloat(propertyID, endValue);
    }

    public void SetShaderProgress(float value)
    {
        if (materialInstance != null)
        {
            materialInstance.SetFloat(Shader.PropertyToID(shaderProgressProperty), value);
        }
    }

    #endregion
}
