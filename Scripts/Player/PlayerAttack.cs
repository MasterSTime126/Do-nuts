using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown = 0.75f;
    [SerializeField] private float attackPauseDuration = 0.3f;  // Pause for attack animation
    [SerializeField] private float cleanPauseDuration = 0.6f;   // Pause for cleaning animation

    private float currentCooldown = 0f;
    private InputActionAsset inputActions;
    private InputAction attackAction;
    private InputAction interactAction;  // AIUANAT - For cleaning traces
    private List<GameObject> donutsInRange = new List<GameObject>();
    private List<GameObject> tracesInRange = new List<GameObject>();  // AIUANAT - Track traces in range

    private MaskManager maskManager;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        inputActions = GetComponent<PlayerInput>().actions;
        attackAction = inputActions.FindAction("Attack");
        interactAction = inputActions.FindAction("Interact");  // AIUANAT - Get interact action
        playerMovement = GetComponent<PlayerMovement>();
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

        // Pause player movement for attack animation
        if (playerMovement != null)
        {
            playerMovement.PauseMovement(attackPauseDuration);
        }

        // Attack all donuts in range
        foreach (GameObject donut in donutsInRange.ToArray())
        {
            if (donut != null)
            {
                DonutLogic logic = donut.GetComponent<DonutLogic>();
                if (logic != null)
                {
                    logic.BeforeDestroy();
                    maskManager.OnDonutKilled();
                }
            }
        }

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
            if (!donutsInRange.Contains(other.gameObject))
            {
                donutsInRange.Add(other.gameObject);
            }
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
            donutsInRange.Remove(other.gameObject);
        }
        
        // AIUANAT - Remove traces from range
        if (other.CompareTag("Trace"))
        {
            tracesInRange.Remove(other.gameObject);
        }
    }
}
