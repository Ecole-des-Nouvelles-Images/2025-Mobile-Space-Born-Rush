using UnityEngine;

[CreateAssetMenu(fileName = "SpeedBoostData", menuName = "Scriptable Objects/Drops/SpeedBoostData")]
public class SpeedBoostData : DropsData
{
    public override void ApplyEffect(float value, float duration)
    {
        EventBus.OnSpeedChanged?.Invoke(value, duration);
    }
}