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

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.linearVelocity = _speedMove;
        _lastPosition = transform.position;
    }


    private void Update()
    {
        Vector3 direction = transform.position - _lastPosition;
        float distance = direction.magnitude;

        if (Physics.Raycast(_lastPosition, direction.normalized, out RaycastHit hit, distance, _enemyMask))
        {
            if (hit.collider.TryGetComponent<EnemyTest>(out var enemy))
            {
                EventBus.OnEnemyTakeDamage?.Invoke(Damage);
                Debug.Log("damage enemy!");
                Destroy(gameObject);
            }
        }

        _lastPosition = transform.position;
    }
    
}
