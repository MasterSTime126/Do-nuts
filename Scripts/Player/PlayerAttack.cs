using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown = 0.75f;

    private float currentCooldown = 0f;
    private InputActionAsset inputActions;
    private InputAction attackAction;
    private List<GameObject> donutsInRange = new List<GameObject>();

    private MaskManager maskManager;

    private void Awake()
    {
        inputActions = GetComponent<PlayerInput>().actions;
        attackAction = inputActions.FindAction("Attack");
    }

    private void OnEnable()
    {
        attackAction.performed += OnAttackPerformed;
        attackAction.Enable();
    }

    private void OnDisable()
    {
        attackAction.performed -= OnAttackPerformed;
        attackAction.Disable();
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Donut"))
        {
            if (!donutsInRange.Contains(other.gameObject))
            {
                donutsInRange.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Donut"))
        {
            donutsInRange.Remove(other.gameObject);
        }
    }
}
