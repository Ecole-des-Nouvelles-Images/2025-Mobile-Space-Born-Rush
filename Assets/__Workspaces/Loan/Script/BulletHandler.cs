using UnityEngine;

public class BulletHandler : MonoBehaviour
{
    private Rigidbody _rb;
    
    [SerializeField] private Vector3 _speedMove;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.linearVelocity = _speedMove;
    }
}
