using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerRotation : MonoBehaviour
{
        [SerializeField] private Transform rotatingChild;
        [SerializeField] private Camera sceneCamera;

    private void Awake()
    {
        if (sceneCamera == null) sceneCamera = Camera.main;
        if (rotatingChild == null && transform.childCount > 0) rotatingChild = transform.GetChild(0);
    }

    private void Update()
    {
        if (rotatingChild == null || sceneCamera == null) return;

        // Raycast from mouse position to ground plane (y=0)
        Ray ray = sceneCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float enter = 0f;
        if (groundPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Debug.DrawLine(sceneCamera.transform.position, hitPoint, Color.green);

            // Calculate direction from rotatingChild to hitPoint
            Vector3 direction = hitPoint - rotatingChild.position;
            direction.y = 0f; // Ignore vertical difference
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                rotatingChild.rotation = targetRotation;
            }
        }
    }
}
