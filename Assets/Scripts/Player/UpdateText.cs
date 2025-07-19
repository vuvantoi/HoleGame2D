using UnityEngine;
using TMPro;
public class UpdateText : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private TextMeshProUGUI scoreView;
    private TextMeshProUGUI levelView;

    private static TextMeshProUGUI staminaView;
    void Start()
    {
        scoreView = GameObject.Find("ScoreView").GetComponent<TextMeshProUGUI>();
        levelView = GameObject.Find("LevelView").GetComponent<TextMeshProUGUI>();
        staminaView = GameObject.Find("StaminaView").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        scoreView.text = $"Score: {ScoreManager.Instance.GetScore()}";
        levelView.text = $"Level: {ScoreManager.Instance.GetCurrentLevel()}";
    }
    public static void UpdateStamina(float stamina)
    {
        staminaView.text = $"Stamina: {stamina:F1}"; // Display stamina with one decimal place
    }
}   

