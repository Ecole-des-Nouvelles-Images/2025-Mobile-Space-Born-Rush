using System;
using UnityEngine;

public class BulletHandler : MonoBehaviour
{
    private Rigidbody _rb;
    private Vector3  _lastPosition;
    private Ray _bulletRaycast;
    
    [SerializeField] private Vector3 _speedMove;
    [SerializeField] private LayerMask _enemyMask;

    public float Damage;
    [SerializeField]private bool _isRaycastDraw;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.linearVelocity = _speedMove;
        _lastPosition = transform.position;
    }


    private void Update()
    {
        float currentRaycastDistance =  _lastPosition.z - transform.position.z;

        if (Physics.Raycast(_bulletRaycast,out RaycastHit hit, currentRaycastDistance,_enemyMask))
        {
            if (hit.collider.TryGetComponent<EnemyTest>(out var enemy))
            {
                EventBus.OnEnemyTakeDamage?.Invoke(Damage);
                Debug.Log("damage enemy!");
                Destroy(gameObject);
            }
        }
        _lastPosition = transform.position;
        
        if (_isRaycastDraw)
        {
            Debug.DrawRay(_bulletRaycast.origin, _bulletRaycast.direction * currentRaycastDistance, Color.red);
        }
    }
    
}
