using UnityEngine;

public class DonutLogic : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 3f;

    [Header("Damage Settings")]
    [SerializeField] private float sadnessDamagePerSecond = 1f;
    [SerializeField] private float fearDamage = 5f;
    [SerializeField] private float angerDamage = 5f;
    [SerializeField] private float angerDamageCooldown = 2f;
    [SerializeField] private float disgustDamage = 10f;
    [SerializeField] private float explosionTime = 5f;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Fear Settings")]
    [SerializeField] private float fearSpeedMultiplier = 5f;
    [SerializeField] private AudioClip screamerClip;
    [SerializeField] private float screamerScale = 3f;
    [SerializeField] private float screamerDuration = 0.6f;

    [Header("References")]
    [SerializeField] private GameObject tracePrefab;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private MaskManager maskManager;
    private Transform player;
    private float damageTimer = 0f;
    private float explosionTimer = 0f;
    private bool isNearPlayer = false;
    private bool isDestroying = false;

    private MaskManager.MaskState currentMaskState;
    private MaskManager.MaskState newMaskState;

    // Fear runtime vars
    private float originalMoveSpeed = 0f;
    private bool fearInitialized = false;

    private void Start()
    {
        maskManager = MaskManager.Instance;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize explosion timer for Disgust level
        if (maskManager != null && maskManager.GetMaskState() == MaskManager.MaskState.Disgust)
        {
            explosionTimer = explosionTime;
        }

        if (maskManager != null && maskManager.GetMaskState() != MaskManager.MaskState.Happiness)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
        }

        moveSpeed = Random.Range(moveSpeed, moveSpeed * 2f);

        currentMaskState = maskManager.GetMaskState();
    }

    private void Update()
    {
        if (maskManager == null || player == null || isDestroying) return;

        newMaskState = maskManager.GetMaskState();
        if (newMaskState != currentMaskState)
        {
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

        MoveTowards(player.position);

        if (damageTimer >= 0)
        {
            damageTimer -= Time.deltaTime;
        }
        if (damageTimer < 0)
        {
            damageTimer = 3;
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        }
    }

    private void BehaviorFear()
    {
        // Fast chase: pass through scene objects and jumpscare on contact
        // Make collider a trigger so donut can pass through obstacles
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        // Initialize high speed once per donut
        if (!fearInitialized)
        {
            originalMoveSpeed = moveSpeed;
            moveSpeed = Mathf.Max(1f, moveSpeed) * fearSpeedMultiplier;
            fearInitialized = true;
        }

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
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;
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
        if (maskManager == null) return;

        var state = maskManager.GetMaskState();

        if (state == MaskManager.MaskState.Happiness)
        {
            // Player eats donut
            maskManager.OnDonutEaten();
            BeforeDestroy();
        }
        else if (state == MaskManager.MaskState.Fear)
        {
            // Screamer effect, damage, blur then disappear
            maskManager.TakeDamage(fearDamage);
            TriggerScreamer();
            BeforeDestroy();
        }
        // Other states keep using collision-based handlers (OnCollisionEnter/Stay)
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player") || isDestroying) return;

        MaskManager.MaskState currentMask = maskManager.GetMaskState();

        switch (currentMask)
        {
            case MaskManager.MaskState.Fear:
                // If donut for some reason still uses collisions, handle screamer here too
                maskManager.TakeDamage(fearDamage);
                TriggerScreamer();
                BeforeDestroy();
                break;

            case MaskManager.MaskState.Anger:
                // Deal damage with cooldown, don't destroy
                if (damageTimer <= 0)
                {
                    maskManager.TakeDamage(angerDamage);
                    damageTimer = angerDamageCooldown;
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
        // Play screamer sound at player's position so it continues even if donut is destroyed
        if (screamerClip != null && player != null)
        {
            AudioSource.PlayClipAtPoint(screamerClip, player.position);
        }

        // Start a small visual jump-scare on the donut itself if possible
        StartCoroutine(ScreamerVisual());

        Debug.Log("SCREAMER!");
    }

    private System.Collections.IEnumerator ScreamerVisual()
    {
        if (spriteRenderer == null)
        {
            yield break;
        }

        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * screamerScale;

        // Quick scale-up
        transform.localScale = targetScale;

        yield return new WaitForSeconds(screamerDuration);

        // If not yet destroying, try to restore scale (may be destroyed soon)
        if (!isDestroying)
            transform.localScale = originalScale;
    }

    private void Explode()
    {
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
    #endregion
}