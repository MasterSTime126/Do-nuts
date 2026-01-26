using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHP : MonoBehaviour
{
    private float playerHP = 25f;
    private float maxPlayerHP = 100f;

    private int maskState = 0;

    private InputActionAsset inputActions;
    private InputAction interactAction;

    private GameObject donut;

    public void UpdateTheMaskState(int value)
    {
        maskState = value;
    }

    private void Update()
    {
        inputActions = GetComponent<PlayerInput>().actions;
        interactAction = inputActions.FindAction("Interact");

        interactAction.performed += ctx =>
        {
            Debug.Log("Interact pressed");
            if(donut != null)
            {
                EatDonut();
                Destroy(donut);
                donut = null;
                Debug.Log("Player HP: " + playerHP);
            }
        };
    }

    public void EatDonut()
    {
        switch (maskState)
        {
            case 0:
                playerHP = Mathf.Clamp(playerHP + 10f, 0f, maxPlayerHP);
                break;
            case 1:
                playerHP = Mathf.Clamp(playerHP - 10f, 0f, maxPlayerHP);
                break;
            case 2:
                playerHP = Mathf.Clamp(playerHP - 20f, 0f, maxPlayerHP);
                break;
            case 3:
                playerHP = Mathf.Clamp(playerHP - 30f, 0f, maxPlayerHP);
                break;
            case 4:
                playerHP = Mathf.Clamp(playerHP - 40f, 0f, maxPlayerHP);
                break;
            default:
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.CompareTag("Donut"))
        {
            donut = collider.gameObject;
            Debug.Log("Donut in range");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(donut == collision.gameObject)
        {
            donut = null;
            Debug.Log("Donut out of range");
        }
    }
}