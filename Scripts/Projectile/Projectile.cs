using UnityEngine;
using UnityEngine.UIElements;

public class Projectile : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float range = 15f;
    private float lifeTimer;
    void Start()
    {
        transform.LookAt(GameObject.FindWithTag("Player").transform);
        transform.Rotate(0f, Random.Range(-range,range), 0f);
    }

    // Update is called once per frame
    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer < lifetime)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            lifeTimer += Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    

}
