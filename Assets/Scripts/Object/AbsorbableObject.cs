using System.Collections;
using UnityEngine;

public class AbsorbableObject : MonoBehaviour
{
    public AbsorbableObjectData data;

    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector3 originalPosition;
    private Vector3 originalScale;

    private void Awake()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;
    }

    private void Start()
    {
        ApplyData();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyData();
    }
#endif

    private void ApplyData()
    {
        if (data != null)
        {
            if (spriteRenderer != null && data.sprite != null)
                spriteRenderer.sprite = data.sprite;

            transform.localScale = Vector3.one * data.objectSize;
        }
    }

    public float GetSize() => data?.objectSize ?? 0.5f;
    public float GetScore() => data?.scoreValue ?? 1f;

    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        RespawnManager.Instance.RespawnAfterDelay(this, data.respawnDelay); // ✅ Sử dụng manager chạy coroutine
    }

    public void Respawn()
    {
        transform.position = originalPosition;
        transform.localScale = originalScale;
        gameObject.SetActive(true);
    }
}
