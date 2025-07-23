using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BotMovement : MonoBehaviour
{
    [Header("Bot Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float targetRefreshInterval = 1f;

    private Rigidbody2D rb;
    private Transform currentTarget;
    private float refreshTimer = 0f;

    private BotSize botSize;

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
        if (refreshTimer <= 0f || currentTarget == null)
        {
            FindNearestTarget();
            refreshTimer = targetRefreshInterval;
        }

        MoveTowardsTarget();
    }

    private void FindNearestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Object");
        float minDistance = float.MaxValue;
        Transform nearest = null;
        float botCurrentSize = botSize.GetSize();

        foreach (var obj in targets)
        {
            if (!obj.activeInHierarchy) continue;

            AbsorbableObject absorbable = obj.GetComponent<AbsorbableObject>();
            if (absorbable == null) continue;

            if (absorbable.GetSize() >= botCurrentSize) continue; // Bỏ qua nếu lớn hơn hoặc bằng

            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = obj.transform;
            }
        }

        currentTarget = nearest;
    }

    private void MoveTowardsTarget()
    {
        if (currentTarget == null) return;

        Vector2 direction = ((Vector2)currentTarget.position - rb.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    private void OnDisable()
    {
        rb.linearVelocity = Vector2.zero;
    }
}

