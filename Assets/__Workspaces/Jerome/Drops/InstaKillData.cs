using UnityEngine;

[CreateAssetMenu(fileName = "InstaKillData", menuName = "Drops/InstaKillData")]
public class InstaKillData : DropsData
{
    private DropsData _dropsData;
    
    public override void ApplyEffect(float value, float duration)
    {
        EventBus.OnEnemyTakeDamage?.Invoke(value);
    }
}