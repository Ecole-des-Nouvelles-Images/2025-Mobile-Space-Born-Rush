using UnityEngine;

[CreateAssetMenu(fileName = "XpData", menuName = "Scriptable Objects/Drops/XpData")]
public class XpData : DropsData
{
    [SerializeField] private Collider _collider;
    private DropsData _dropsData;
    
    public override void ApplyEffect(float value, float duration)
    {
        EventBus.OnPlayerCurrentXPChange?.Invoke(value);
    }

    private void OnEnable()
    {
        EventBus.OnMagnetActivated += OnMagnetActivated;
    }
    private void OnMagnetActivated(bool arg1, float arg2)
    {
        
    }
    private void OnDisable()
    {
        EventBus.OnMagnetActivated -= OnMagnetActivated;
    }
    
    void Magnatize()
    {
        var player = FindObjectOfType<PlayerController>();
        
        _collider.transform.position = Vector3.Lerp(_collider.transform.position,  player.transform.position, 0.1f);
    }

    void Demagnetize()
    {
        //Go back to moving forwards 
    }
}