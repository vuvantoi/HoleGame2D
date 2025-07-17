using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [Header("Zoom Settings")]
    [SerializeField] private float baseSize = 4.44f;
    [SerializeField] private float zoomMultiplier = 1.56f;
    [SerializeField] private float zoomSpeed = 2f;

    private Coroutine zoomCoroutine;


    public void SetZoomByHoleSize(float holeSize)
    {
        // Clamp zoom nếu muốn
        float targetSize = baseSize + holeSize * zoomMultiplier;
        targetSize = Mathf.Clamp(targetSize, 5f, 20f); // Tùy mức lớn nhất bạn muốn

        SetZoom(targetSize);
    }

    private void SetZoom(float targetSize)
    {
        if (virtualCamera == null) return;

        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);

        zoomCoroutine = StartCoroutine(ZoomRoutine(targetSize));
    }

    private IEnumerator ZoomRoutine(float targetSize)
    {
        float startSize = virtualCamera.Lens.OrthographicSize;
        float time = 0f;
        float duration = Mathf.Abs(targetSize - startSize) / zoomSpeed;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            virtualCamera.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }

        virtualCamera.Lens.OrthographicSize = targetSize;
    }
}
