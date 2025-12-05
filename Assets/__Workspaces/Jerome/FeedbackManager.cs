using Unity.Cinemachine;
using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance {get; private set;}
    
    [SerializeField] private CinemachineImpulseSource _impulseSource;
    private static CinemachineImpulseSource _localImpulseSource;


    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        _localImpulseSource = _impulseSource;
    }
    
    
    [ContextMenu("SmallShake")]
    public static void SmallShake()
    {
        _localImpulseSource.GenerateImpulseWithForce(0.2f);
    }
}
