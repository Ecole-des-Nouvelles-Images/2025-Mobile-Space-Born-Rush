using UnityEngine;

[CreateAssetMenu(fileName = "DropsData", menuName = "Scriptable Objects/DropData")]
public class DropsData : ScriptableObject
{
    public DropType Type;
    public GameObject Prefab;
    
    public string Name;
    public string Description;

    public float Value;
    public float Duration;
    public float DropChance;
}

public enum DropType
{
    Health,
    XP,
    InstaFill,
    Magnet,
    SpeedBoost
}