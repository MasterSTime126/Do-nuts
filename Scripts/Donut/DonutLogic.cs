using UnityEngine;

public class DonutLogic : MonoBehaviour
{
    private MaskManager maskManager;
    private GameObject player;
    private float speed = 2f;

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
                {
                    Vector3 moveTo = player.transform.position;
                    moveTo.y = 0f;
                    transform.position = Vector3.MoveTowards(transform.position, moveTo, speed * Time.deltaTime);
                }
                break;
            case 2:
                if (player != null)
                {
                    GetComponent<Collider>().isTrigger = false;
                    Vector3 directionAway = (transform.position - player.transform.position).normalized;
                    directionAway.y = 0f;
                    transform.position += directionAway * speed * Time.deltaTime;
                }
                break;
            case 3:
                if (player != null)
                {
                    if (Vector3.Distance(transform.position, player.transform.position) > 3f)
                    {
                        Vector3 moveTo = player.transform.position;
                        moveTo.y = 0f;
                        transform.position = Vector3.MoveTowards(transform.position, moveTo, speed * Time.deltaTime);
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
