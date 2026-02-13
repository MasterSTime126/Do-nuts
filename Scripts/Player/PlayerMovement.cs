using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7.5f;

    private Vector2 movementInput;
    private InputActionAsset inputActions;
    private InputActionMap playerActionMap;
    private InputAction moveAction;
    private Rigidbody rb;

    // Movement pause for animations
    private bool isMovementPaused = false;
    private float pauseTimer = 0f;

    // Reference to animator
    private PlayerAnimator playerAnimator;
    private PlayerAudio playerAudio;

    public void PauseMovement(float duration)
    {
        isMovementPaused = true;
        pauseTimer = duration;
    }

    public void TeleportTo(Vector3 targetPosition)
    {
        if (rb != null)
        {
            rb.MovePosition(targetPosition);
        }
    }

   private void OnEnable()
   {
       inputActions = GetComponent<PlayerInput>().actions;
       //Just in case, you find these actions in InputSystem_Actions file in Assets.
       playerActionMap = inputActions.FindActionMap("Player");
       moveAction = playerActionMap.FindAction("Move");

       moveAction.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
       moveAction.canceled += ctx => movementInput = Vector2.zero;
       playerActionMap.Enable();
   }

   private void Start()
   {
       
       playerAnimator = GetComponentInChildren<PlayerAnimator>();
       playerAudio = GetComponent<PlayerAudio>();
       rb = GetComponent<Rigidbody>();
       
       if (rb == null)
       {
           Debug.LogError("PlayerMovement: Rigidbody component required for wall collision!");
       }
       AchievementManager.Instance?.UnlockAchievement("Starting the game!");
   }

   private void Update()
   {
        
       if (pauseTimer > 0)
       {
           pauseTimer -= Time.deltaTime;
           if (pauseTimer <= 0)
           {
               isMovementPaused = false;
           }
       }

       // Update animator walking state
       bool isMoving = movementInput.sqrMagnitude > 0.01f && !isMovementPaused;
       
       if (playerAnimator != null)
       {
           playerAnimator.SetWalking(isMoving);
           
           // Flip sprite based on horizontal movement
           if (Mathf.Abs(movementInput.x) > 0.01f)
           {
               playerAnimator.SetFacingDirection(movementInput.x);
           }
       }

       // Update audio walking state
       if (playerAudio != null)
       {
           playerAudio.SetWalking(isMoving);
       }
   }

   private void FixedUpdate()
   {
       if (isMovementPaused || rb == null)
       {
           if (rb != null) rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
           return;
       }
       
       Vector3 inputDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;
       Vector3 targetVelocity = inputDirection * moveSpeed;
       

       
       // Set horizontal velocity while preserving vertical (gravity)
       rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
   }

}