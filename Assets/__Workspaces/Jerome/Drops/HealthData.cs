using UnityEngine;

[CreateAssetMenu(fileName = "HealthData", menuName = "Scriptable Objects/Drops/HealthData")]
public class HealthData : DropsData
{
    private DropsData _dropsData;
    
    public override void ApplyEffect(float value, float duration)
    {
        EventBus.OnPlayerCurrentHealthChange?.Invoke(value);
    }
}