using UnityEngine;

[CreateAssetMenu(fileName = "MagnetData", menuName = "Scriptable Objects/Drops/MagnetData")]
public class MagnetData : DropsData
{
    private DropsData _dropsData;
    
    public override void ApplyEffect(float value, float duration)
    {
        EventBus.OnMagnetActivated?.Invoke(true, duration);
    }
}