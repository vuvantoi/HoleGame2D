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
    [SerializeField] private float targetRefreshInterval = 0.5f; // How often to search for new targets
    [SerializeField] private float dangerDetectionRange = 7f;
    [SerializeField] private float chaseStopRange = 14f; // Distance to stop chasing if target gets too far
    [SerializeField] private float dangerClearanceRangeOffset = 2f; // Added for hysteresis when clearing danger
    [SerializeField] private float postActionDelay = 0.2f; // Delay after eating/losing target/fleeing

    private Rigidbody2D rb;
    private BotSize botSize;

    private float refreshTimer = 0f; // Initialize to 0 so it searches immediately on start
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
        FleeingDanger
    }

    private BotState currentState = BotState.Idle; // Bot starts idle, will transition to seeking food

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        botSize = GetComponent<BotSize>();

        if (botSize == null)
            Debug.LogError($"{gameObject.name} is missing BotSize component!");
    }

    private void Update()
    {
        // Dynamic range calculation should always happen
        if (refreshTimer < 0) refreshTimer = 1f; // Ensure timer doesn't go negative
        dangerDetectionRange = botSize.GetSize() * 3f;
        chaseStopRange = dangerDetectionRange; // Keeping this linked for now as per your original logic

        HandleStamina();
        actionDelayTimer -= Time.deltaTime; // Decrement action delay timer

        // Always detect immediate danger, as it's the highest priority behavior.
        // This will set 'isDangerous' and 'currentTarget' if a danger is found.
        DetectImmediateDanger();

        // State machine logic
        switch (currentState)
        {
            case BotState.FleeingDanger:
                // If we were fleeing but no longer detect danger, transition out of Fleeing.
                // The 'isDangerous' flag is set by DetectImmediateDanger.
                if (!isDangerous && actionDelayTimer <= 0f)
                {
                    currentState = BotState.SeekingFood; // Default to seeking food after fleeing
                    currentTarget = null; // Clear fleeing target
                    actionDelayTimer = postActionDelay; // Short delay before seeking
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
                // If no target and not dangerous, try to find a target to start seeking food/chasing
                if (!isDangerous && (refreshTimer <= 0f || currentTarget == null || !currentTarget.gameObject.activeInHierarchy) && actionDelayTimer <= 0f)
                {
                    FindChaseOrFoodTarget(); // Attempt to find an initial target
                    refreshTimer = targetRefreshInterval; // Reset refresh timer

                    if (currentTarget != null) // If a target was found, transition to appropriate state
                    {
                        currentState = isChasing ? BotState.ChasingPrey : BotState.SeekingFood;
                    }
                    // If no target found, remains Idle and will try again after refreshTimer
                }
                break;
        }

        // Handle Movement based on the determined state
        HandleMovement();
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
        refreshTimer -= Time.deltaTime;

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

        // If a target was just lost, activate the action delay timer to prevent immediate re-targeting
        if (currentTargetLost && actionDelayTimer <= 0f)
        {
            actionDelayTimer = postActionDelay; // Start the delay
        }

        // Find a new target only if:
        // 1. The regular refresh interval is met, OR we just lost our current target.
        // 2. AND the action delay timer has run out (prevents immediate search after losing a target).
        // This ensures the bot waits a moment before finding a new target after eating or losing one.
        if ((refreshTimer <= 0f || currentTargetLost) && actionDelayTimer <= 0f)
        {
            FindChaseOrFoodTarget();
            refreshTimer = targetRefreshInterval; // Reset timer regardless of whether a target was found
        }

        // Update current state based on what FindChaseOrFoodTarget found
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
        // Optimization: For performance, consider using Physics2D.OverlapCircleAll with LayerMasks
        // if your game has many objects. For now, keeping your FindGameObjectsWithTag approach.
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
                var sizeComp = target.GetComponent<HoleSize>(); // Assuming player has HoleSize
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

        // If a danger is found
        if (danger != null)
        {
            isDangerous = true;
            currentTarget = danger; // Set danger as current target for movement
            currentState = BotState.FleeingDanger; // Explicitly set state to fleeing
        }
        else if (isDangerous) // If we were previously dangerous but now no immediate threat
        {
            // Apply hysteresis: only clear danger if the target is sufficiently far away
            // This prevents rapid flickering between fleeing and seeking if a threat is at the edge of the range
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy || Vector2.Distance(transform.position, currentTarget.position) > (dangerDetectionRange + dangerClearanceRangeOffset))
            {
                isDangerous = false; // Clear danger status
                // The state transition (e.g., to SeekingFood) will be handled in the Update loop
            }
        }
    }

    // Finds the closest absorbable food object or a smaller bot/player to chase
    private void FindChaseOrFoodTarget()
    {
        // Optimization: Consider Physics2D.OverlapCircleAll if performance is an issue
        GameObject[] allTargets = GameObject.FindGameObjectsWithTag("Object") // Absorbable food
            .Concat(GameObject.FindGameObjectsWithTag("Bot")) // Other bots
            .Concat(GameObject.FindGameObjectsWithTag("Player")) // Player
            .Where(go => go != gameObject && go.activeInHierarchy) // Exclude self and inactive objects
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
            else if (target.CompareTag("Object")) // It's an absorbable food object
            {
                var food = target.GetComponent<AbsorbableObject>();
                // Bot can only eat objects smaller than itself
                if (food == null || food.GetSize() >= mySize) continue;

                float dist = Vector2.Distance(transform.position, target.transform.position);
                if (dist < minFoodDist)
                {
                    foodTarget = target.transform;
                    minFoodDist = dist;
                }
                continue; // Move to the next target after processing this object
            }

            // Logic for chasing other bots or players (must be smaller than us)
            float chaseDist = Vector2.Distance(transform.position, target.transform.position);
            if (targetSize < mySize && chaseDist < chaseStopRange && chaseDist < minChaseDist)
            {
                chaseTarget = target.transform;
                minChaseDist = chaseDist;
            }
        }

        // Prioritize chasing a smaller bot/player over eating food
        if (chaseTarget != null)
        {
            isChasing = true;
            currentTarget = chaseTarget;
        }
        else if (foodTarget != null)
        {
            isChasing = false; // Not "chasing" a bot/player, but seeking food
            currentTarget = foodTarget;
        }
        else // No valid chase or food target found
        {
            isChasing = false;
            currentTarget = null;
        }
    }

    private void MoveTowardsTarget()
    {
        if (currentTarget == null) return;

        Vector2 direction = ((Vector2)currentTarget.position - rb.position).normalized;
        // Sprint only if in ChasingPrey state and has enough stamina
        float speed = (currentState == BotState.ChasingPrey && curStamina > minStamina) ? sprintSpeed : walkSpeed;
        rb.linearVelocity = direction * speed;
    }

    private void MoveAwayFromTarget()
    {
        if (currentTarget == null) return;

        Vector2 direction = (rb.position - (Vector2)currentTarget.position).normalized;
        // Fleeing typically uses walk speed to conserve stamina
        rb.linearVelocity = direction * walkSpeed;
    }

    private void OnDisable()
    {
        rb.linearVelocity = Vector2.zero;
    }

    // Optional: Visual debugging in editor (requires UnityEditor for Handles.Label)
    private void OnDrawGizmosSelected()
    {
        if (rb == null || botSize == null) return;

        // Draw Danger Detection Range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dangerDetectionRange);

        // Draw Chase/Food Search Range (if different, currently linked)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseStopRange);

        // Draw currentTarget line
        if (currentTarget != null)
        {
            switch (currentState)
            {
                case BotState.FleeingDanger:
                    Gizmos.color = Color.magenta; // Fleeing
                    break;
                case BotState.ChasingPrey:
                    Gizmos.color = Color.blue;   // Chasing
                    break;
                case BotState.SeekingFood:
                    Gizmos.color = Color.green;  // Seeking food
                    break;
                default:
                    Gizmos.color = Color.gray; // Idle or unhandled state
                    break;
            }
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }

        // Display current state and timer in editor for debugging
        #if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, $"State: {currentState.ToString()}", style);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f, $"Delay: {actionDelayTimer:F2}", style);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.4f, $"Refresh: {refreshTimer:F2}", style);
        #endif
    }
}