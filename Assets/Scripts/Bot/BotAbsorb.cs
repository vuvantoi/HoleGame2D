using UnityEngine;

[RequireComponent(typeof(BotSize))]
public class BotAbsorb : MonoBehaviour
{
    private BotSize botSize;

    private void Awake()
    {
        botSize = GetComponent<BotSize>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        AbsorbableObject absorbable = collision.GetComponent<AbsorbableObject>();
        if (absorbable == null) return;

        if (CanAbsorb(absorbable))
        {
            Absorb(absorbable);
        }
    }

    private bool CanAbsorb(AbsorbableObject target)
    {
        return target.GetSize() < botSize.CurrentSize;
    }

    private void Absorb(AbsorbableObject target)
    {
        float score = target.GetScore();

        // Cộng điểm 
        BotScoreManager.Instance.AddScore(score);

        // Bắt đầu hút mà KHÔNG phát âm thanh
        StartCoroutine(AbsorbRoutine(target.transform));

        // Debug hoặc tuỳ chỉnh khác nếu muốn
        Debug.Log($"[BOT ABSORB] Absorbed: {target.name}, Score: {score}");
    }

    private System.Collections.IEnumerator AbsorbRoutine(Transform target)
    {
        if (target == null) yield break;

        Vector3 startScale = target.localScale;
        float duration = 0.7f;
        float time = 0f;

        while (time < duration)
        {
            if (target == null) yield break;

            time += Time.deltaTime;
            float t = time / duration;

            Vector3 currentTargetPos = transform.position;

            target.position = Vector3.Lerp(target.position, currentTargetPos, t);
            target.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            yield return null;
        }

        yield return null;

        if (target != null && target.gameObject != null)
        {
            AbsorbableObject absorbable = target.GetComponent<AbsorbableObject>();
            if (absorbable != null)
            {
                absorbable.ReturnToPool();
            }
        }
    }
}