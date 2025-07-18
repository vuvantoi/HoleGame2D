using UnityEngine;
using TMPro;
public class UpdateText : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private TextMeshProUGUI scoreView;
    private TextMeshProUGUI levelView;
    void Start()
    {
        scoreView = GameObject.Find("ScoreView").GetComponent<TextMeshProUGUI>();
        levelView = GameObject.Find("LevelView").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        scoreView.text = $"Score: {ScoreManager.Instance.totalScore}";
        levelView.text = $"Level: {ScoreManager.Instance.currentLevelIndex}";
    }
}
