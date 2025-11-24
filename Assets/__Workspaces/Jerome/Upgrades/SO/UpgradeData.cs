using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Scriptable Objects/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    [Header("Basic Info")]
    public string Name, Description;
    public Sprite Icon;
    
    [Header("Gameplay Effects")]
    public UpgradeCategory category;
    public UpgradeType Type;
    public float Value;
    
    [Header("Generation Settings")]
    public int maxTimesSelectable; // 0 = infinite
    public int timesSelected = 0;
    public bool isExclusive = false; // If true, cannot appear with other exclusive upgrades
    
    [Header("Progression")]
    public float baseWeight = 1f; // Base probability weight
}

public enum UpgradeCategory
{
    PlayerStat, // Max HP, HPMultiplier, XPMultiplier
    WeaponStat, // Damage, FireRate, MagSize, Cooldown, WeaponSwap
    GameSystem // Speed, CardChoice
}