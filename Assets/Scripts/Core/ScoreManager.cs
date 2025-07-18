using System.Linq;
using UnityEngine;

public class ScoreManager : SingletonBase<ScoreManager>
{
    [Header("Level Data")]
    public HoleLevelData[] levels;

    [SerializeField] private CameraZoomController cameraZoomController;
    [SerializeField] private int startLevelIndex = 0;
    private int lastStartLevelIndex = -1;

    [SerializeField] public float totalScore = 0f;
    public int currentLevelIndex = 0;

    private HoleSize holeSize;

    private void Start()
    {
        holeSize = FindAnyObjectByType<HoleSize>();

        currentLevelIndex = Mathf.Clamp(startLevelIndex, 0, levels.Length - 1);
        totalScore = levels[currentLevelIndex].requiredScore;

        ApplyLevel(currentLevelIndex); // cấp đầu đợi xíu
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
        Debug.Log($"[SCORE] Current Score: {totalScore}");
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
        holeSize.SetSize(newSize);
        // Gọi zoom camera theo kích thước hố mới
        if (cameraZoomController != null)
        {
            cameraZoomController.SetZoomByHoleSize(newSize);
        }
        Debug.Log($"[LEVEL UP] Level: {levels[index].level} → Size: {newSize}");
    }

    public int GetCurrentLevel()
    {
        return levels[currentLevelIndex].level; // Trả về level hiển thị đúng với người chơi
    }

    public float GetScore()
    {
        return totalScore;
    }

}
