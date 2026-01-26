using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private float currentSpeed = 5f;
    private float moveSpeed = 5f;
    private float sprintSpeed = 7.5f;


   private Vector2 movementInput;
   private InputActionAsset inputActions;
   private InputActionMap playerActionMap;
   private InputAction moveAction;
   private InputAction sprintAction;

   private void OnEnable()
   {
       inputActions = GetComponent<PlayerInput>().actions;
       //Just in case, you find these actions in InputSystem_Actions file in Assets.
       playerActionMap = inputActions.FindActionMap("Player");
       moveAction = playerActionMap.FindAction("Move");
       sprintAction = playerActionMap.FindAction("Sprint");

       moveAction.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
       moveAction.canceled += ctx => movementInput = Vector2.zero;
       playerActionMap.Enable();
   }

   private void Update()
   {
       currentSpeed = sprintAction.IsPressed() ? sprintSpeed : moveSpeed;
       Vector3 move = new Vector3(movementInput.x, 0, movementInput.y) * currentSpeed * Time.deltaTime;
       transform.Translate(move);
   }

}