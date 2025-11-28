using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public float BaseMaxHealth;
    public float BaseXPToLevelUp;
    public float BaseXPMultiplier;
    public float BaseHPDropMultiplier;
}
