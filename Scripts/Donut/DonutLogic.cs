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
        if(maskManager.GetMaskState() == 0)
        {
            //nothing happens
        }
        else if(maskManager.GetMaskState() == 1)
        {
            if (player != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
            }
        }
        else if(maskManager.GetMaskState() == 2)
        {
            if (player != null)
            {
                Vector3 directionAway = (transform.position - player.transform.position).normalized;
                transform.position += directionAway * speed * Time.deltaTime;
            }
        }
        else if(maskManager.GetMaskState() == 3)
        {
            if (player != null && Vector3.Distance(transform.position, player.transform.position) > 3f)
            {
                transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
            }
            else
            {
                //Prepare for the dash, mb IEnumerator, in this phase
            }
        }
        else if(maskManager.GetMaskState() == 4)
        {
            if (player != null)
            {
                //IDK what happens here
            }
        }
    }
}
