using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework;
using UnityEngine.EventSystems;
public class UpdateText : SingletonBase<UpdateText>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private TextMeshProUGUI scoreView;
    private TextMeshProUGUI levelView;
    private static TextMeshProUGUI staminaView;
    public bool isSprintButtonPressed = false;
    void Start()
    {

        scoreView = GameObject.Find("UI_ScoreView").GetComponent<TextMeshProUGUI>();
        levelView = GameObject.Find("UI_LevelView").GetComponent<TextMeshProUGUI>();
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
        staminaView.text = $"Stamina: {stamina:F0}"; // Display stamina with no decimal places
    }
    

}  

