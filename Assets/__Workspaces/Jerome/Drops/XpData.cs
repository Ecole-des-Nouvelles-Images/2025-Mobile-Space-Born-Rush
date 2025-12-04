using UnityEngine;

[CreateAssetMenu(fileName = "XpData", menuName = "Drops/XpData")]
public class XpData : DropsData
{
    private DropsData _dropsData;
    
    public override void ApplyEffect()
    {
        EventBus.OnPlayerCurrentXPChange?.Invoke(this.Value);
    }
}