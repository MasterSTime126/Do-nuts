using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown = 0.75f;
    [SerializeField] private float attackPauseDuration = 0.3f;  // Pause for attack animation
    [SerializeField] private float cleanPauseDuration = 0.6f;   // Pause for cleaning animation
    [SerializeField] private float attackRange = 2f;  // Range for attack detection
    [SerializeField] private LayerMask donutLayerMask;  // Optional: set to Donut layer for better performance

    private float currentCooldown = 0f;
    private InputActionAsset inputActions;
    private InputAction attackAction;
    private InputAction interactAction;  // AIUANAT - For cleaning traces
    private List<GameObject> donutsInRange = new List<GameObject>();
    private List<GameObject> tracesInRange = new List<GameObject>();  // AIUANAT - Track traces in range

    private MaskManager maskManager;
    private PlayerMovement playerMovement;
    private PlayerAnimator playerAnimator;
    private PlayerAudio playerAudio;

    private void Awake()
    {
        inputActions = GetComponent<PlayerInput>().actions;
        attackAction = inputActions.FindAction("Attack");
        interactAction = inputActions.FindAction("Interact");  // AIUANAT - Get interact action
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        playerAudio = GetComponent<PlayerAudio>();
    }

    private void OnEnable()
    {
        attackAction.performed += OnAttackPerformed;
        attackAction.Enable();
        
        // AIUANAT - Subscribe to interact for cleaning
        interactAction.performed += OnInteractPerformed;
        interactAction.Enable();
    }

    private void OnDisable()
    {
        attackAction.performed -= OnAttackPerformed;
        attackAction.Disable();
        
        // AIUANAT - Unsubscribe from interact
        interactAction.performed -= OnInteractPerformed;
        interactAction.Disable();
    }

    private void Start()
    {
        maskManager = MaskManager.Instance;
    }

    private void Update()
    {
        // Only allow attacking in Anger mask (state 3)
        if (maskManager == null) return;

        if (maskManager.GetMaskState() == MaskManager.MaskState.Anger)
        {
            if (currentCooldown > 0)
            {
                currentCooldown -= Time.deltaTime;
            }
        }
        else
        {
            // Can't attack in other states
            currentCooldown = attackCooldown;
        }

        // Clean up destroyed objects from list
        donutsInRange.RemoveAll(obj => obj == null);
    }

    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        if (maskManager == null) return;

        // Only attack in Anger state
        if (maskManager.GetMaskState() != MaskManager.MaskState.Anger) return;

        if (currentCooldown > 0) return;

        Debug.Log($"PlayerAttack: Attack performed! Donuts in range: {donutsInRange.Count}");

        // Pause player movement for attack animation
        if (playerMovement != null)
        {
            playerMovement.PauseMovement(attackPauseDuration);
        }
        
        // Trigger attack animation
        if (playerAnimator != null)
        {
            playerAnimator.TriggerAttack();
        }

        // Play attack sound
        if (playerAudio != null)
        {
            playerAudio.PlayAttack();
        }

        // Find all donuts in attack range using OverlapSphere
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        int donutsHit = 0;
        
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Donut"))
            {
                Debug.Log($"PlayerAttack: Found donut in range: {hitCollider.gameObject.name}");
                DonutLogic logic = hitCollider.GetComponent<DonutLogic>();
                if (logic != null)
                {
                    logic.BeforeDestroy();
                    maskManager.OnDonutKilled();
                    donutsHit++;
                }
            }
        }
        
        Debug.Log($"PlayerAttack: Hit {donutsHit} donuts");

        donutsInRange.Clear();
        currentCooldown = attackCooldown;
    }

    // AIUANAT - Clean trace when interacting
    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (maskManager == null) return;

        // Only clean traces in Disgust state
        if (maskManager.GetMaskState() != MaskManager.MaskState.Disgust) return;

        // Clean up null references
        tracesInRange.RemoveAll(obj => obj == null);

        if (tracesInRange.Count == 0) return;

        // Pause player movement for cleaning animation
        if (playerMovement != null)
        {
            playerMovement.PauseMovement(cleanPauseDuration);
        }
        
        // Trigger clean animation
        if (playerAnimator != null)
        {
            playerAnimator.TriggerClean();
        }

        // Play clean sound
        if (playerAudio != null)
        {
            playerAudio.PlayClean();
        }

        // Clean the first trace in range
        GameObject trace = tracesInRange[0];
        if (trace != null)
        {
            TraceLogic traceLogic = trace.GetComponent<TraceLogic>();
            if (traceLogic != null)
            {
                traceLogic.CleanTrace();  // AIUANAT - This calls the clean animation
            }
        }

        tracesInRange.RemoveAt(0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Donut"))
        {
            Debug.Log($"PlayerAttack: Donut entered range: {other.gameObject.name}");
            if (!donutsInRange.Contains(other.gameObject))
            {
                donutsInRange.Add(other.gameObject);
            }
        }
        if(other.CompareTag("Projectile"))
        {
            maskManager.TakeDamage(5f);
            Destroy(other.gameObject);
            return;
        }

        // AIUANAT - Track traces in range
        if (other.CompareTag("Trace"))
        {
            if (!tracesInRange.Contains(other.gameObject))
            {
                tracesInRange.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Donut"))
        {
            Debug.Log($"PlayerAttack: Donut exited range: {other.gameObject.name}");
            donutsInRange.Remove(other.gameObject);
        }
        
        // AIUANAT - Remove traces from range
        if (other.CompareTag("Trace"))
        {
            tracesInRange.Remove(other.gameObject);
        }
    }
}
