using UnityEngine;

[RequireComponent(typeof(HoleSize))]
public class HoleAbsorb : MonoBehaviour
{
    private HoleSize holeSize;
    // define audio clip
    public AudioClip absorbSound;
    private AudioSource audioSource;
    private void Awake()
    {
        holeSize = GetComponent<HoleSize>();
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        AbsorbableObject absorbable = collision.GetComponent<AbsorbableObject>();
        if (absorbable == null && collision.tag != "Bot" && collision.tag != "Player") return;

        if (collision.tag == "Object" && CanAbsorb(absorbable))
        {
            Absorb(absorbable);
        }
        else if ((collision.tag == "Bot" || collision.tag == "Player") && CanAbsorbPlayerOrBotTarget(collision))
        {
            AbsorbPlayerOrBot(collision.transform);
        }
    }

    private bool CanAbsorb(AbsorbableObject target)
    {
        return target.GetSize() < holeSize.CurrentSize;
    }
    private void Absorb(AbsorbableObject target)
    {
        float score = target.GetScore();
       
        // Cộng điểm và hiện popup
        ScoreManager.Instance.AddScore(score);

        // Bắt đầu hút
        StartCoroutine(AbsorbRoutine(target.transform));
        // check if if sound is being played
        if (absorbSound != null)
            audioSource.PlayOneShot(absorbSound);
        Debug.Log($"[ABSORB] Absorbed: {target.name}, Score: {score}");

        PointPopupManager.Instance.ShowPoint(transform.position, (int)score);
    }

    private bool CanAbsorbPlayerOrBotTarget(Collider2D target)
    {
        if (target == null) return false;
        GameObject obj = target.gameObject;
        if (obj.tag == "Bot" && obj.activeInHierarchy) //check if target is active in hierarchy
        {
            BotSize botSize = obj.GetComponent<BotSize>();
            if (botSize == null) return false;

            return botSize.GetSize() < this.holeSize.CurrentSize;
        }
        if (!obj.activeInHierarchy) { return false; }
        HoleSize holesize = obj.GetComponent<HoleSize>();
        if (holesize == null) return false;

        return holesize.CurrentSize < holesize.CurrentSize;
    }

    private void AbsorbPlayerOrBot(Transform target)
    {
        if (target == null) return;

        // Bắt đầu hút đối tượng
        StartCoroutine(AbsorbRoutine(target));
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

            // Cập nhật lại mỗi frame để vật hút về vị trí mới nhất của Player
            Vector3 currentTargetPos = transform.position;

            // Di chuyển & scale dần về 0
            target.position = Vector3.Lerp(target.position, currentTargetPos, t);
            target.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            yield return null;
        }

        yield return null;

        if (target != null && target.gameObject != null)
        {
            AbsorbableObject absorbable = target.GetComponent<AbsorbableObject>();
            if (absorbable != null && target.gameObject.tag == "Object")
            {
                absorbable.ReturnToPool();
            }
            else if (target.gameObject.tag == "Bot")
            {
                EnemyPoolManager.Instance.ReturnEnemyToPool(target.gameObject);
                Debug.Log($"[PLAYER ABSORB] Absorbed Bot: {target.name}");
            }
            else if (target.gameObject.tag == "Player")
            {
                target.gameObject.SetActive(false);
                Debug.Log($"[PLAYER ABSORB] Absorbed Player: {target.name}");
            }
        }
    }

}
