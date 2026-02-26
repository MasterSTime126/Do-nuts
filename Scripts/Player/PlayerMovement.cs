using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7.5f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    private Vector2 movementInput;
    private InputActionAsset inputActions;
    private InputActionMap playerActionMap;
    private InputAction moveAction;
    private InputAction dashAction;
    private Rigidbody rb;

    // Movement pause for animations
    private bool isMovementPaused = false;
    private float pauseTimer = 0f;

    // Dash state
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;

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
       dashAction = playerActionMap.FindAction("Dash");

       moveAction.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
       moveAction.canceled += ctx => movementInput = Vector2.zero;
       
       if (dashAction != null)
       {
           dashAction.performed += ctx => TryDash();
       }
       
       playerActionMap.Enable();
   }
   
   private void OnDisable()
   {
       if (dashAction != null)
       {
           dashAction.performed -= ctx => TryDash();
       }
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

       // Dash cooldown
       if (dashCooldownTimer > 0)
       {
           dashCooldownTimer -= Time.deltaTime;
       }

       // Dash duration
       if (isDashing)
       {
           dashTimer -= Time.deltaTime;
           if (dashTimer <= 0)
           {
               isDashing = false;
               Debug.Log("[PlayerMovement] Dash ended");
           }
       }

       // Update animator walking state
       bool isMoving = movementInput.sqrMagnitude > 0.01f && !isMovementPaused && !isDashing;
       
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

   private void TryDash()
   {
       // Check if can dash
       if (isDashing || dashCooldownTimer > 0 || isMovementPaused) return;
       
       // Get dash direction (current movement or facing direction)
       if (movementInput.sqrMagnitude > 0.01f)
       {
           dashDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;
       }
       else
       {
           // Dash forward if not moving
           dashDirection = playerAnimator != null && playerAnimator.IsFacingRight() 
               ? Vector3.right 
               : Vector3.left;
       }
       
       // Start dash
       isDashing = true;
       dashTimer = dashDuration;
       dashCooldownTimer = dashCooldown;
       
       Debug.Log($"[PlayerMovement] Dash started! Direction: {dashDirection}");
   }

   private void FixedUpdate()
   {
       if (rb == null) return;
       
       // During dash, use dash velocity
       if (isDashing)
       {
           rb.linearVelocity = new Vector3(dashDirection.x * dashSpeed, rb.linearVelocity.y, dashDirection.z * dashSpeed);
           return;
       }
       
       if (isMovementPaused)
       {
           rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
           return;
       }
       
       Vector3 inputDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;
       Vector3 targetVelocity = inputDirection * moveSpeed;
       
       // Set horizontal velocity while preserving vertical (gravity)
       rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
   }

}