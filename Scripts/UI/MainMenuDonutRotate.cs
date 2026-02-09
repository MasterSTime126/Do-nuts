using UnityEngine;

public class MainMenuDonutRotate : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.back, Time.deltaTime * 45);
    }
}
