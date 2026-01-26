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
        if(maskState == 0)
        {
            playerHP += 10f;
            if(playerHP > maxPlayerHP)
            {
                playerHP = maxPlayerHP;
            }
        }
        else if(maskState == 1)
        {
            playerHP -= 10f;
            if(playerHP < 0)
            {
                playerHP = 0;
            }
        }
        else if(maskState == 2)
        {
            playerHP -= 20f;
            if(playerHP < 0)
            {
                playerHP = 0;
            }
        }
        else if(maskState == 3)
        {
            playerHP -= 30f;
            if(playerHP < 0)
            {
                playerHP = 0;
            }
        }
        else if(maskState == 4)
        {
            playerHP -= 40f;
            if(playerHP < 0)
            {
                playerHP = 0;
            }
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