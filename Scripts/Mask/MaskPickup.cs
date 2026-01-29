using UnityEngine;

public class MaskPickup : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    private Vector3 startPosition;
    private MaskManager maskManager;

    private void OnDestroy()
    {
        if (maskManager != null)
        {
            Debug.Log("MaskPickup destroyed " + maskManager.GetMaskState());
        }
    }

    private void Start()
    {
        startPosition = transform.position;
        maskManager = MaskManager.Instance;
        
        // Start inactive, MaskManager will activate when conditions are met
        //gameObject.SetActive(false);
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
            transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.5f, t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
