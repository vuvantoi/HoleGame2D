using UnityEngine;

[CreateAssetMenu(fileName = "HoleLevelData", menuName = "Game/Hole Level")]
public class HoleLevelData : ScriptableObject
{
    public int level;
    public float requiredScore;
    public float size;
}
