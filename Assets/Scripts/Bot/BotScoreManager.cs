using UnityEngine;

public class BotScoreManager : MonoBehaviour
{
    [Header("Level Data")]
    public HoleLevelData[] levels;

    [SerializeField] private int startLevelIndex = 0; 
    [SerializeField] private float totalScore = 0f;
    [SerializeField] private int debugLevelIndex;

    private int currentLevelIndex = 0;
    private int lastStartLevelIndex = -1;

    private BotSize botSize;

    private void Start()
    {
        botSize = GetComponent<BotSize>();
        if (botSize == null) Debug.LogError($"{gameObject.name} thiếu BotSize!");
        currentLevelIndex = Mathf.Clamp(startLevelIndex, 0, levels.Length - 1);
        totalScore = levels[currentLevelIndex].requiredScore;

        ApplyLevel(currentLevelIndex);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return; // Chỉ làm trong khi đang chơi game (Play Mode)

        if (startLevelIndex != lastStartLevelIndex)
        {
            // Cập nhật level ngay khi chỉ số thay đổi
            currentLevelIndex = Mathf.Clamp(startLevelIndex, 0, levels.Length - 1);
            totalScore = levels[currentLevelIndex].requiredScore;
            ApplyLevel(currentLevelIndex);

            lastStartLevelIndex = startLevelIndex;
            Debug.Log($"[DEBUG] Updated to level {startLevelIndex} at runtime via Inspector.");
        }
#endif
    }

    public void AddScore(float value)
    {
        totalScore += value;
        Debug.Log($"[BOT SCORE] {gameObject.name} Score: {totalScore}");
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        if (currentLevelIndex + 1 >= levels.Length) return;

        var nextLevel = levels[currentLevelIndex + 1];
        if (totalScore >= nextLevel.requiredScore)
        {
            currentLevelIndex++;
            ApplyLevel(currentLevelIndex);
        }
    }

    private void ApplyLevel(int index)
    {
        float newSize = levels[index].size;
        botSize.SetSize(newSize);
        debugLevelIndex = currentLevelIndex;

        Debug.Log($"[BOT LEVEL UP] {gameObject.name} → Level {levels[index].level}, Size: {newSize}");
    }

    public float GetScore() => totalScore;
    public int GetCurrentLevel() => levels[currentLevelIndex].level;
}
