using UnityEngine;

public class HoleSize : MonoBehaviour
{
    public float CurrentSize { get; private set; } = 1f;

    public void SetSize(float size)
    {
        CurrentSize = size;

        transform.localScale = Vector3.one * CurrentSize;
    }
}
