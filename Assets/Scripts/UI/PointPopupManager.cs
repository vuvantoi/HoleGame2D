using UnityEngine;

public class PointPopupManager : SingletonBase<PointPopupManager>
{
    [Header("Popup Settings")]
    public GameObject popupPrefab;
    public Transform canvasTransform;

    public void ShowPoint(Vector3 worldPosition, int point)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        GameObject popup = Instantiate(popupPrefab, screenPos, Quaternion.identity, canvasTransform);
        popup.GetComponent<PointPopup>().Init("+" + point.ToString());
    }
}
