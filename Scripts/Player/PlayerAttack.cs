using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] MaskManager maskManager;

    private float attackTime = 0.75f;
    private float attackCooldown = 0f;

    private InputActionAsset inputActions;
    private InputAction attackAction;
    private List<GameObject> gameObjects = new List<GameObject>();

    private void Awake()
    {
        inputActions = GetComponent<PlayerInput>().actions;
        attackAction = inputActions.FindAction("Attack");

        attackAction.performed += ctx =>
        {
            if(attackCooldown > 0)
            {
                return;
            }
            foreach (GameObject obj in gameObjects)
            {
                Destroy(obj);
            }
            // Implement attack logic here
        };
    }

    private void Update()
    {
        if(maskManager.GetMaskState() == 2)
        {
            attackCooldown -= attackCooldown > 0 ? Time.deltaTime : 0;
        }
        //CHECKPOINT
        else
        {
            attackCooldown = 1f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Donut"))
        {
            Debug.Log("Enemy detected");
            gameObjects.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Donut"))
        {
            Debug.Log("Enemy exited");
            gameObjects.Remove(other.gameObject);
        }
    }
}
