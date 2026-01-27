using UnityEngine;

public class TraceLogic : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float poisonDamagePerSecond = 2f;
    [SerializeField] private float effectDuration = 3f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private MaskManager maskManager;
    private bool isBeingCleaned = false;

    private void Start()
    {
        maskManager = MaskManager.Instance;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Apply slow and poison to player
        if (maskManager != null)
        {
            maskManager.ApplySlow(slowMultiplier, effectDuration);
            maskManager.ApplyPoison(poisonDamagePerSecond, effectDuration);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Re-apply effects while standing on trace
        if (maskManager != null)
        {
            maskManager.ApplySlow(slowMultiplier, effectDuration);
            maskManager.ApplyPoison(poisonDamagePerSecond, effectDuration);
        }
    }

    // Call this when player cleans the trace
    public void CleanTrace()
    {
        if (isBeingCleaned) return;
        isBeingCleaned = true;

        if (maskManager != null)
        {
            maskManager.OnTraceCleared();
        }

        StartCoroutine(CleanAnimation());
    }

    private System.Collections.IEnumerator CleanAnimation()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (spriteRenderer != null)
            {
                Color c = originalColor;
                c.a = Mathf.Lerp(1f, 0f, t);
                spriteRenderer.color = c;
            }

            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
