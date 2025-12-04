using UnityEngine;

[CreateAssetMenu(fileName = "MagnetData", menuName = "Drops/MagnetData")]
public class MagnetData : DropsData
{
    private DropsData _dropsData;
    
    public override void ApplyEffect()
    {
        EventBus.OnMagnetActivated?.Invoke(this.Duration);
    }
}