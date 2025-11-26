using System;
using UnityEngine;
using UnityEngine.Serialization;

public class BulletHandler : MonoBehaviour
{
    private Rigidbody _rb;
    private Vector3  _lastPosition;
    private Ray _bulletRaycast;
    private float _lifeTime;
    private BulletData _bulletData;
    private Vector3 _speedMove;
    
    private LayerMask _enemyMask;
    [SerializeField] private LayerMask _obstacleMask;
    

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Setup(BulletData data,LayerMask enemyMask)
    {
        _bulletData = data;
        _enemyMask = enemyMask;
        _rb.linearVelocity = _bulletData.BulletSpeed;
        _lifeTime =  _bulletData.BulletLifeTime;
        _lastPosition = transform.position;
    }


    private void Update()
    {
        if (_lifeTime > 0)
        {
            Vector3 direction = transform.position - _lastPosition;
            float distance = direction.magnitude;

            if (_bulletData.IsDestroyedObstacle)
            {
                if (_enemyMask == LayerMask.NameToLayer("Enemy"))
                {
                    if (Physics.Raycast(_lastPosition, direction.normalized, out RaycastHit hit, distance, _enemyMask))
                    {
                        if (hit.collider.TryGetComponent<EnemyHandler>(out var enemy))
                        {
                            _bulletData.OnHitEnemy(gameObject,enemy, enemy);
                        }
                    }
                }
                else
                {
                    if (Physics.Raycast(_lastPosition, direction.normalized, out RaycastHit hit, distance, _enemyMask))
                    {
                        if (hit.collider.TryGetComponent<PlayerController>(out var enemy))
                        {
                            _bulletData.OnHitEnemy(gameObject,enemy, null);
                        }
                    }
                }
                
            }
            else
            {
                if (_enemyMask == LayerMask.NameToLayer("Enemy"))
                {
                    if (Physics.Raycast(_lastPosition, direction.normalized, out RaycastHit hitObstacle, distance, _obstacleMask))
                    {
                        if (hitObstacle.collider.TryGetComponent<ObstacleHandler>(out var enemy))
                        {
                            Destroy(gameObject);
                        }
                    }
                    else if (Physics.Raycast(_lastPosition, direction.normalized, out RaycastHit hit, distance, _enemyMask))
                    {
                        if (hit.collider.TryGetComponent<EnemyHandler>(out var enemy))
                        {
                            _bulletData.OnHitEnemy(gameObject,enemy, enemy);
                        }
                    }
                }
                else
                {
                    if (Physics.Raycast(_lastPosition, direction.normalized, out RaycastHit hitObstacle, distance, _obstacleMask))
                    {
                        if (hitObstacle.collider.TryGetComponent<ObstacleHandler>(out var enemy))
                        {
                            Destroy(gameObject);
                        }
                    }
                    else if (Physics.Raycast(_lastPosition, direction.normalized, out RaycastHit hit, distance, _enemyMask))
                    {
                        if (hit.collider.TryGetComponent<PlayerController>(out var enemy))
                        {
                            _bulletData.OnHitEnemy(gameObject,enemy, null);
                        }
                    }
                }
                
            }

            
            _lastPosition = transform.position;
        }
        else
        {
            Destroy(gameObject);
        }
        
        _lifeTime -= Time.deltaTime;
        
    }
    
}
