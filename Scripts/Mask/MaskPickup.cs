using UnityEngine;

public class MaskPickup : MonoBehaviour
{
    [Header("Mask Sprites (one per level)")]
    [SerializeField] private Sprite happinessMaskSprite;
    [SerializeField] private Sprite sadnessMaskSprite;
    [SerializeField] private Sprite fearMaskSprite;
    [SerializeField] private Sprite angerMaskSprite;
    [SerializeField] private Sprite disgustMaskSprite;

    [Header("Animation Settings")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    private Vector3 startPosition;
    private MaskManager maskManager;
    private SpriteRenderer spriteRenderer;
    private bool isCollected = false;
    private float spawnProtectionTime = 1f;  // Prevent instant pickup

    private void OnDestroy()
    {
        if (maskManager != null)
        {
            Debug.Log("MaskPickup destroyed " + maskManager.GetMaskState());
        }
    }

    private void Start()
    {
        // Get the SpriteRenderer (could be on the same object or a child)
        var srTemp = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        if (srTemp != null)
        {
            var mat = srTemp.material; // creates an instance if using a shared material
            if (mat != null)
            {
                Debug.Log("MaskPickup shader: " + (mat.shader != null ? mat.shader.name : "null"));

                // Example property changes - adjust property names/values to match your shader
                if (mat.HasProperty("_OutlineColor"))
                    mat.SetColor("_OutlineColor", Color.blue); // change tint

                if (mat.HasProperty("_SpiralStrength")) // check for main texture scale/offset prop group
                {
                    mat.SetFloat("_SpiralStrength", 1f); // example of changing a custom shader property
                }

                if (mat.HasProperty("_DissolvePattern"))
                    mat.SetFloat("_DissolvePattern", 15f);

                //GetComponent<ShaderToggle>()?.Appear(false, true); // Ensure shader is toggled on
            }

        }
        startPosition = transform.position;
        maskManager = MaskManager.Instance;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        // Set the correct sprite based on current mask state
        UpdateMaskSprite();
        
        // Disable collider briefly to prevent instant pickup after teleport
        GetComponent<Collider>().enabled = false;
        StartCoroutine(EnableColliderAfterDelay());
    }

    private void UpdateMaskSprite()
    {
        if (spriteRenderer == null || maskManager == null) return;
        
        Sprite maskSprite = GetSpriteForCurrentMask();
        if (maskSprite != null)
        {
            spriteRenderer.sprite = maskSprite;
        }
    }

    private Sprite GetSpriteForCurrentMask()
    {
        return maskManager.GetMaskState() switch
        {
            MaskManager.MaskState.Happiness => happinessMaskSprite,
            MaskManager.MaskState.Sadness => sadnessMaskSprite,
            MaskManager.MaskState.Fear => fearMaskSprite,
            MaskManager.MaskState.Anger => angerMaskSprite,
            MaskManager.MaskState.Disgust => disgustMaskSprite,
            _ => happinessMaskSprite
        };
    }

    private System.Collections.IEnumerator EnableColliderAfterDelay()
    {
        yield return new WaitForSeconds(spawnProtectionTime);
        if (!isCollected)
        {
            GetComponent<Collider>().enabled = true;
        }
    }

    private void Update()
    {
        // Rotating animation
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Bobbing animation
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isCollected) return;  // Already collected, ignore

        // Mark as collected and disable collider immediately
        isCollected = true;
        GetComponent<Collider>().enabled = false;

        if (maskManager != null)
        {
            maskManager.CollectMask();
        }

        // Play collection effect
        StartCoroutine(CollectAnimation());
    }

    private System.Collections.IEnumerator CollectAnimation()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 originalScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * 3f, t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
