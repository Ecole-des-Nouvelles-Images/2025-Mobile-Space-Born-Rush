using UnityEngine;

[CreateAssetMenu(fileName = "HealthData", menuName = "Drops/HealthData")]
public class HealthData : DropsData
{
    private DropsData _dropsData;
    
    public override void ApplyEffect()
    {
        EventBus.OnPlayerCurrentHealthChange?.Invoke(this.Value);
    }
}