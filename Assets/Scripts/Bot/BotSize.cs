using UnityEngine;

public class BotSize : MonoBehaviour
{
    public float CurrentSize { get; private set; } = 1f;

    public float GetSize() => CurrentSize;
    public void SetSize(float size)
    {
        CurrentSize = size;

        transform.localScale = Vector3.one * CurrentSize;
    }
}
