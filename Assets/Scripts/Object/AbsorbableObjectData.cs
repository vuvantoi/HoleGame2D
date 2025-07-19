using UnityEngine;

[CreateAssetMenu(fileName = "NewAbsorbableObjectData", menuName = "Game/Absorbable Object")]
public class AbsorbableObjectData : ScriptableObject
{
    public string objectName;
    public float objectSize = 1f;
    public float scoreValue = 1f;
    public float respawnDelay = 5f; 
    public Sprite sprite;
}
