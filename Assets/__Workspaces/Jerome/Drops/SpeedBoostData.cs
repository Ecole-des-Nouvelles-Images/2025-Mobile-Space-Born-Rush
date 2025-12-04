using UnityEngine;

[CreateAssetMenu(fileName = "SpeedBoostData", menuName = "Drops/SpeedBoostData")]
public class SpeedBoostData : DropsData
{
    public override void ApplyEffect()
    {
        EventBus.OnSpeedChanged?.Invoke(this.Value, this.Duration);
    }
}