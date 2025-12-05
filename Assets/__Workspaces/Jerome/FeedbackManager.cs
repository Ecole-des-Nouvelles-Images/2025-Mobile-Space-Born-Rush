using Unity.Cinemachine;
using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource _impulseSource;
    
    [ContextMenu("SmallShake")]
    public void SmallShake()
    {
        // This is it! One line.
        _impulseSource.GenerateImpulseWithForce(0.2f);
    }
}
