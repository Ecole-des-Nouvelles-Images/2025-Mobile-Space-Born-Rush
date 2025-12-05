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

/// <summary>
/// PlayerStat <br/>
/// Max HP, HPMultiplier, XPMultiplier <br/><br/>
/// WeaponStat <br/>
/// Damage, FireRate, MagSize, Cooldown, WeaponSwap <br/><br/>
/// GameSystem <br/>
/// Speed, CardChoice
/// </summary>
public enum UpgradeCategory
{
    /// <summary>
    /// Max HP, HPMultiplier, XPMultiplier
    /// </summary>
    PlayerStat,
    /// <summary>
    /// Damage, FireRate, MagSize, Cooldown, WeaponSwap
    /// </summary>
    WeaponStat,
    /// <summary>
    /// Speed, CardChoice
    /// </summary>
    GameSystem
}