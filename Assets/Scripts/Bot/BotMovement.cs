using UnityEngine;
using System.Linq; // Make sure this is included for Concat and Where

[RequireComponent(typeof(Rigidbody2D))]
public class BotMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 7f;

    [Header("Stamina Settings")]
    [SerializeField] private float curStamina = 100f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float minStamina = 0f;
    [SerializeField] private float staminaRecoveryRate = 5f;
    [SerializeField] private float staminaDrainRate = 25f;

    [Header("AI Settings")]
    [SerializeField] private float targetRefreshInterval = 0.3f; // How often to search for new targets
    [SerializeField] private float dangerDetectionRange = 7f;
    [SerializeField] private float chaseStopRange = 14f; // Distance to stop chasing if target gets too far
    [SerializeField] private float dangerClearanceRangeOffset = 2f; // Added for hysteresis when clearing danger
    [SerializeField] private float postActionDelay = 0.2f; // Delay after eating/losing target/fleeing

    private Rigidbody2D rb;
    private BotSize botSize;

    private float refreshTimer; // No longer initialized to 0, will be set on start
    private float actionDelayTimer = 0f; // Used for post-action delays

    private Transform currentTarget;
    private bool isDangerous = false; // Flag set by DetectImmediateDanger
    private bool isChasing = false; // Flag set by FindChaseOrFoodTarget

    // NEW: Define Bot States
    public enum BotState
    {
        Idle,
        SeekingFood,
        ChasingPrey,
        FleeingDanger,
        Absorbing // New state to handle absorption without movement
    }

    private BotState currentState = BotState.Idle; // Bot starts idle, will transition to seeking food

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        botSize = GetComponent<BotSize>();

        if (botSize == null)
            Debug.LogError($"{gameObject.name} is missing BotSize component!");
    }
    
    private void Start()
    {
        targetRefreshInterval = Random.Range(0.1f, 0.5f); // Ensure it's not too low
        // Start the refresh timer so the bot doesn't immediately become Idle.
        // This ensures the bot starts looking for a target immediately.
        refreshTimer = targetRefreshInterval;
    }

    private void Update()
    {
        // Dynamic range calculation should always happen
        dangerDetectionRange = botSize.GetSize() * 3f;
        chaseStopRange = dangerDetectionRange; // Keeping this linked for now as per your original logic

        // *** REVISED TIMER LOGIC: Decrement and clamp timers in one place. ***
        // This is a cleaner, more reliable way to manage them.
        refreshTimer -= Time.deltaTime;
        refreshTimer = Mathf.Max(0f, refreshTimer);

        actionDelayTimer -= Time.deltaTime;
        actionDelayTimer = Mathf.Max(0f, actionDelayTimer);
        // *** END OF REVISED TIMER LOGIC ***

        HandleStamina();

        // Always detect immediate danger, as it's the highest priority behavior.
        // This will set 'isDangerous' and 'currentTarget' if a danger is found.
        DetectImmediateDanger();

        // State machine logic
        switch (currentState)
        {
            case BotState.FleeingDanger:
                if (!isDangerous) // Only check for transition if no longer in danger
                {
                    // Wait for action delay after fleeing before transitioning
                    if (actionDelayTimer <= 0f)
                    {
                        currentState = BotState.SeekingFood; // Default to seeking food after fleeing
                        currentTarget = null; // Clear fleeing target
                        actionDelayTimer = postActionDelay; // Short delay before seeking
                        refreshTimer = 0f; // Force an immediate refresh to find a new target
                    }
                }
                break;

            case BotState.SeekingFood:
            case BotState.ChasingPrey:
                // If danger is detected, override current behavior and flee.
                if (isDangerous)
                {
                    currentState = BotState.FleeingDanger;
                    isChasing = false; // Stop chasing if fleeing
                    // currentTarget is already set by DetectImmediateDanger to the danger
                }
                else // If no danger, proceed with seeking/chasing logic
                {
                    HandleTargetLogic(); // This will find/manage food or chase targets
                }
                break;

            case BotState.Idle:
                // Only try to find a new target if the refresh timer has expired
                if (refreshTimer <= 0f)
                {
                    FindChaseOrFoodTarget();
                    refreshTimer = targetRefreshInterval; // Reset refresh timer
                    
                    if (currentTarget != null) // If a target was found, transition to appropriate state
                    {
                        currentState = isChasing ? BotState.ChasingPrey : BotState.SeekingFood;
                    }
                    // If no target found, it remains in Idle and will try again after refreshTimer
                }
                break;
            
            // NEW: The bot does nothing while in the absorbing state.
            case BotState.Absorbing:
                // The absorb script is responsible for transitioning out of this state
                break;
        }

        // Handle Movement based on the determined state
        HandleMovement();
    }

    // NEW: Public method to be called by your absorb script
    public void SetStateToAbsorbing()
    {
        currentState = BotState.Absorbing;
        rb.linearVelocity = Vector2.zero; // Immediately stop all movement
        //wait for 0.5 second then set state to idle
    }
    public void SetStateToSeekingFood()
    {
        currentState = BotState.SeekingFood;
        currentTarget = null; // Clear target when going idle
        actionDelayTimer = postActionDelay; // Reset action delay timer
        refreshTimer = targetRefreshInterval; // Reset refresh timer for next target search
    }

    private void HandleStamina()
    {
        if (curStamina < maxStamina)
        {
            curStamina += staminaRecoveryRate * Time.deltaTime;
            curStamina = Mathf.Min(curStamina, maxStamina);
        }

        // Only drain stamina if actively chasing prey or fleeing danger
        if ((currentState == BotState.ChasingPrey || currentState == BotState.FleeingDanger) && curStamina > minStamina)
        {
            curStamina -= staminaDrainRate * Time.deltaTime;
            curStamina = Mathf.Max(curStamina, minStamina);
        }
    }

    // Handles finding and managing food/chase targets when not in FleeingDanger state
    private void HandleTargetLogic()
    {
        // Determine if the current target is no longer valid (e.g., eaten, disabled, or too far for chasing)
        bool currentTargetLost = currentTarget == null || !currentTarget.gameObject.activeInHierarchy;

        // If currently chasing and the target is still valid, check if it went out of chase range
        if (isChasing && !currentTargetLost)
        {
            float dist = Vector2.Distance(transform.position, currentTarget.position);
            if (dist > chaseStopRange)
            {
                isChasing = false;
                currentTarget = null;
                currentTargetLost = true; // Mark as lost due to distance
            }
        }

        // Find a new target only if:
        // 1. The regular refresh interval is met, OR we just lost our current target.
        // 2. AND the action delay timer has run out (prevents immediate search after losing a target).
        if ((refreshTimer <= 0f || currentTargetLost) && actionDelayTimer <= 0f)
        {
            FindChaseOrFoodTarget();
            refreshTimer = targetRefreshInterval; // Reset timer regardless of whether a target was found
        }
        
        // --- REVISED: Update state based on currentTarget at the end of the logic ---
        if (currentTarget != null)
        {
            if (isChasing)
                currentState = BotState.ChasingPrey;
            else // It's a food target
                currentState = BotState.SeekingFood;
        }
        else
        {
            currentState = BotState.Idle; // No target found, go idle
        }
    }

    private void HandleMovement()
    {
        // *** REVISED: Do not move if the bot is in the Absorbing state. ***
        if (currentState == BotState.Absorbing)
        {
            return;
        }
        
        // If no current target or in Idle state, stop movement
        if (currentTarget == null || currentState == BotState.Idle)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Move based on the current state
        if (currentState == BotState.FleeingDanger)
        {
            MoveAwayFromTarget();
        }
        else if (currentState == BotState.SeekingFood || currentState == BotState.ChasingPrey)
        {
            MoveTowardsTarget();
        }
    }

    // Detects larger entities (Bots or Players) that are a danger
    private void DetectImmediateDanger()
    {
        GameObject[] potentialThreats = GameObject.FindGameObjectsWithTag("Bot")
            .Concat(GameObject.FindGameObjectsWithTag("Player"))
            .Where(go => go != gameObject && go.activeInHierarchy)
            .ToArray();

        float mySize = botSize.GetSize();
        float closestDist = float.MaxValue;
        Transform danger = null;

        foreach (var target in potentialThreats)
        {
            float targetSize = 0f;

            if (target.CompareTag("Bot"))
            {
                var sizeComp = target.GetComponent<BotSize>();
                if (sizeComp == null) continue;
                targetSize = sizeComp.GetSize();
            }
            else if (target.CompareTag("Player"))
            {
                var sizeComp = target.GetComponent<HoleSize>();
                if (sizeComp == null) continue;
                targetSize = sizeComp.CurrentSize;
            }

            // A danger is a target that is larger than us AND within our danger detection range
            float dist = Vector2.Distance(transform.position, target.transform.position);
            if (targetSize > mySize && dist < dangerDetectionRange && dist < closestDist)
            {
                closestDist = dist;
                danger = target.transform;
            }
        }

        if (danger != null)
        {
            isDangerous = true;
            currentTarget = danger;
            currentState = BotState.FleeingDanger;
        }
        else if (isDangerous) // If we were previously dangerous but now no immediate threat
        {
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy || Vector2.Distance(transform.position, currentTarget.position) > (dangerDetectionRange + dangerClearanceRangeOffset))
            {
                isDangerous = false;
            }
        }
    }

    private void FindChaseOrFoodTarget()
    {
        GameObject[] allTargets = GameObject.FindGameObjectsWithTag("Object")
            .Concat(GameObject.FindGameObjectsWithTag("Bot"))
            .Concat(GameObject.FindGameObjectsWithTag("Player"))
            .Where(go => go != gameObject && go.activeInHierarchy)
            .ToArray();

        float mySize = botSize.GetSize();
        float minChaseDist = float.MaxValue;
        float minFoodDist = float.MaxValue;

        Transform chaseTarget = null;
        Transform foodTarget = null;

        foreach (var target in allTargets)
        {
            float targetSize = 0f;

            if (target.CompareTag("Bot"))
            {
                var sizeComp = target.GetComponent<BotSize>();
                if (sizeComp == null) continue;
                targetSize = sizeComp.GetSize();
            }
            else if (target.CompareTag("Player"))
            {
                var sizeComp = target.GetComponent<HoleSize>();
                if (sizeComp == null) continue;
                targetSize = sizeComp.CurrentSize;
            }
            else if (target.CompareTag("Object"))
            {
                var food = target.GetComponent<AbsorbableObject>();
                if (food == null || food.GetSize() >= mySize) continue;

                float dist = Vector2.Distance(transform.position, target.transform.position);
                if (dist < minFoodDist)
                {
                    foodTarget = target.transform;
                    minFoodDist = dist;
                }
                continue;
            }

            float chaseDist = Vector2.Distance(transform.position, target.transform.position);
            if (targetSize < mySize && chaseDist < chaseStopRange && chaseDist < minChaseDist)
            {
                chaseTarget = target.transform;
                minChaseDist = chaseDist;
            }
        }

        if (chaseTarget != null)
        {
            isChasing = true;
            currentTarget = chaseTarget;
        }
        else if (foodTarget != null)
        {
            isChasing = false;
            currentTarget = foodTarget;
        }
        else
        {
            isChasing = false;
            currentTarget = null;
        }
    }

    private void MoveTowardsTarget()
    {
        if (currentTarget == null) return;

        Vector2 direction = ((Vector2)currentTarget.position - rb.position).normalized;
        float speed = (currentState == BotState.ChasingPrey && curStamina > minStamina) ? sprintSpeed : walkSpeed;
        rb.linearVelocity = direction * speed;
    }

    private void MoveAwayFromTarget()
    {
        if (currentTarget == null) return;

        Vector2 direction = (rb.position - (Vector2)currentTarget.position).normalized;
        rb.linearVelocity = direction * walkSpeed;
    }

    private void OnDisable()
    {
        rb.linearVelocity = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (rb == null || botSize == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dangerDetectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseStopRange);

        if (currentTarget != null)
        {
            switch (currentState)
            {
                case BotState.FleeingDanger:
                    Gizmos.color = Color.magenta;
                    break;
                case BotState.ChasingPrey:
                    Gizmos.color = Color.blue;
                    break;
                case BotState.SeekingFood:
                    Gizmos.color = Color.green;
                    break;
                default:
                    Gizmos.color = Color.gray;
                    break;
            }
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }

        #if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, $"State: {currentState.ToString()}", style);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f, $"Delay: {actionDelayTimer:F2}", style);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.4f, $"Refresh: {refreshTimer:F2}", style);
        #endif
    }
}