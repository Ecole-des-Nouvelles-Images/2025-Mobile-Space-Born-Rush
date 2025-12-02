using UnityEngine;

[CreateAssetMenu(fileName = "DropsData", menuName = "Scriptable Objects/DropData")]
public class DropsData : ScriptableObject
{
    public DropType Type;
    public GameObject Prefab;
    
    public string Name;
    public string Description;

    public float Value;
    [Header("In seconds")]
    public float Duration;
    [Header("0 to 100")]
    [Range(0, 100)] public int DropChance;
}

public enum DropType
{
    XP,
    Health,
    InstaFill,
    Magnet,
    SpeedBoost
}