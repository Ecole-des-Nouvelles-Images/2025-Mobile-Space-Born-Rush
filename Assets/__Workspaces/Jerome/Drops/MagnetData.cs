using UnityEngine;

[CreateAssetMenu(fileName = "MagnetData", menuName = "Drops/MagnetData")]
public class MagnetData : DropsData
{
    private DropsData _dropsData;
    
    public override void ApplyEffect(float value, float duration)
    {
        EventBus.OnMagnetActivated?.Invoke(duration);
    }
}