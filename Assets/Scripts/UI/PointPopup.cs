using UnityEngine;
using TMPro;

public class PointPopup : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float moveUpSpeed = 50f;
    public float duration = 1f;

    private float timer = 0f;

    public void Init(string pointText)
    {
        text.text = pointText;
    }

    void Update()
    {
        transform.Translate(Vector3.up * moveUpSpeed * Time.deltaTime);
        timer += Time.deltaTime;

        if (timer > duration)
            Destroy(gameObject);
            Debug.Log("Cần pooling");
    }
}
