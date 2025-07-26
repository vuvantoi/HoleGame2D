using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
public class BotMovement : MonoBehaviour
{
    [Header("Bot Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float targetRefreshInterval = 1f;

    private Rigidbody2D rb;
    private float refreshTimer = 1f;
    private Transform currentTarget;
    private Transform closestDangerousTarget = null;
    private BotSize botSize;
    private bool isDangerous = false;
    private float minDangerousDistance = 7f; // Khoảng cách tối thieeu voi ng choi/bot lon hon
    private bool isTargetDangerous = false; // check xem target co phai ng choi/bot ko
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        botSize = GetComponent<BotSize>();
        if (botSize == null)
            Debug.LogError($"{gameObject.name} thiếu BotSize!");
    }

    private void Update()
    {
        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f || currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            FindNearestTarget();
            refreshTimer = targetRefreshInterval;
        }

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

    private void FindNearestTarget()
    {
        // Combine all potential targets and filter out self and inactive objects
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Object")
            .Concat(GameObject.FindGameObjectsWithTag("Bot"))
            .Concat(GameObject.FindGameObjectsWithTag("Player"))
            .Where(go => go != gameObject && go.activeInHierarchy)
            .ToArray();

        float botCurrentSize = botSize.GetSize();
        float minDangerousDist = float.MaxValue;
        closestDangerousTarget = null; // Reset closest dangerous target for each refresh

        // 1. Check for dangerous targets first (bigger bots/players)
        foreach (var target in targets)
        {
            bool isBot = (target.tag == "Bot");
            bool isPlayer = (target.tag == "Player");

            float targetSize = 0f;
            if (isPlayer) targetSize = target.GetComponent<HoleSize>().CurrentSize;
            else if (isBot) targetSize = target.GetComponent<BotSize>().CurrentSize;
            else continue; // Not a bot or player, so not a dangerous target in this context

            isTargetDangerous = (targetSize >= botCurrentSize);

            if (isTargetDangerous)
            {
                float dist = Vector2.Distance(transform.position, target.transform.position);
                if (dist < minDangerousDist)
                {
                    minDangerousDist = dist;
                    closestDangerousTarget = target.transform;
                }
            }
        }
    

        // 2. Decide whether to flee or hunt. Priority is to flee from danger.
        if (closestDangerousTarget != null && minDangerousDist < minDangerousDistance)
        {
            isDangerous = true;
            currentTarget = closestDangerousTarget;
            return; // Fleeing, so we don't need to look for food.
        }

        // 3. If not fleeing, find the nearest food (smaller objects).
        Debug.Log("Eating");
        Transform nearestFood = null;
        isDangerous = false;
        float minFoodDist = float.MaxValue;

        foreach (var target in targets)
        {
            var absorbable = target.GetComponent<AbsorbableObject>();
            if (absorbable == null || absorbable.GetSize() >= botCurrentSize) continue;

            float dist = Vector2.Distance(transform.position, target.transform.position);
            if (dist < minFoodDist)
            {
                minFoodDist = dist;
                nearestFood = target.transform;
            }
        }

        currentTarget = nearestFood;
    }


    private void MoveTowardsTarget()
    {
        if (currentTarget == null) return;

        Vector2 direction = ((Vector2)currentTarget.position - rb.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }
    private void MoveAwayFromTarget()
    {
        if (currentTarget == null) return;

        Vector2 direction = (rb.position - (Vector2)currentTarget.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    private void OnDisable()
    {
        rb.linearVelocity = Vector2.zero;
    }
}
