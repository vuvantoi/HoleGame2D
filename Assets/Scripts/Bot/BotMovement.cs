using UnityEngine;
using System.Linq;

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
    [SerializeField] private float targetRefreshInterval = 0.5f;
    [SerializeField] private float dangerDetectionRange = 7f;
    [SerializeField] private float chaseStopRange = 14f;

    private Rigidbody2D rb;
    private BotSize botSize;

    private float refreshTimer;

    private Transform currentTarget;
    private bool isDangerous = false;
    private bool isChasing = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        botSize = GetComponent<BotSize>();

        if (botSize == null)
            Debug.LogError($"{gameObject.name} is missing BotSize component!");
    }

    private void Update()
    {
        dangerDetectionRange = botSize.GetSize() * 3f;
        chaseStopRange = dangerDetectionRange;

        HandleStamina();
        DetectImmediateDanger(); // << Detect danger every frame
        HandleTargetLogic();
        HandleMovement();
    }

    private void HandleStamina()
    {
        if (curStamina < maxStamina)
        {
            curStamina += staminaRecoveryRate * Time.deltaTime;
            curStamina = Mathf.Min(curStamina, maxStamina);
        }

        if ((isChasing || isDangerous) && curStamina > minStamina)
        {
            curStamina -= staminaDrainRate * Time.deltaTime;
            curStamina = Mathf.Max(curStamina, minStamina);
        }
    }

    private void HandleTargetLogic()
    {
        refreshTimer -= Time.deltaTime;

        if (isDangerous)
            return; // Don't override danger target

        if (isChasing && currentTarget != null)
        {
            float dist = Vector2.Distance(transform.position, currentTarget.position);
            if (dist > chaseStopRange)
            {
                isChasing = false;
                currentTarget = null;
            }
        }

        if (refreshTimer <= 0f || currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            FindChaseOrFoodTarget();
            refreshTimer = targetRefreshInterval;
        }
    }

    private void HandleMovement()
    {
        if (currentTarget == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isDangerous)
        {
            MoveAwayFromTarget();
        }
        else
        {
            MoveTowardsTarget();
        }
    }

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
                targetSize = sizeComp.CurrentSize;
            }
            else if (target.CompareTag("Player"))
            {
                var sizeComp = target.GetComponent<HoleSize>();
                if (sizeComp == null) continue;
                targetSize = sizeComp.CurrentSize;
            }

            float dist = Vector2.Distance(transform.position, target.transform.position);
            if (targetSize >= mySize && dist < dangerDetectionRange && dist < closestDist)
            {
                closestDist = dist;
                danger = target.transform;
            }
        }

        if (danger != null)
        {
            isDangerous = true;
            isChasing = false;
            currentTarget = danger;
        }
        else if (isDangerous)
        {
            // Reset danger if threat gone
            isDangerous = false;
            currentTarget = null;
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
                targetSize = sizeComp.CurrentSize;
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

            // Chase logic
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
        float speed = (curStamina > minStamina && isChasing) ? sprintSpeed : walkSpeed;
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
}
