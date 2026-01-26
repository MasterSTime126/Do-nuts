using UnityEngine;

public class DonutLogic : MonoBehaviour
{
    private MaskManager maskManager;
    private GameObject player;
    private float speed = 5f;

    [System.Obsolete]
    private void Start()
    {
        maskManager = FindObjectOfType<MaskManager>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        if (maskManager == null) return;

        int maskState = maskManager.GetMaskState();
        switch (maskState)
        {
            case 0:
                // nothing happens
                break;
            case 1:
                if (player != null)
                    transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
                break;
            case 2:
                if (player != null)
                {
                    Vector3 directionAway = (transform.position - player.transform.position).normalized;
                    transform.position += directionAway * speed * Time.deltaTime;
                }
                break;
            case 3:
                if (player != null)
                {
                    if (Vector3.Distance(transform.position, player.transform.position) > 3f)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
                    }
                    else
                    {
                        // Prepare for the dash, maybe IEnumerator, in this phase
                    }
                }
                break;
            case 4:
                if (player != null)
                {
                    // IDK what happens here
                }
                break;
            default:
                // Unknown state
                break;
        }
    }
}
