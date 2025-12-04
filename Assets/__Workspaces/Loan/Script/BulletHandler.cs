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
    
    private LayerMask _hitMask;
    private float _radius = 0.2f;
    [SerializeField] private LayerMask _obstacleMask;
    

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Setup(BulletData data,LayerMask enemyMask)
    {
        _bulletData = data;
        _hitMask = enemyMask;
        _rb.linearVelocity = _bulletData.BulletSpeed;
        _lifeTime =  _bulletData.BulletLifeTime;
        _lastPosition = transform.position;
    }


    private void Update()
    {
        _lifeTime -= Time.deltaTime;
        if (_lifeTime <= 0f)
        {
            Destroy(this.gameObject);
            return;
        }
        
        Vector3 currentPos =  transform.position;
        Vector3 direction = currentPos - _lastPosition;
        float distance = direction.magnitude;

        if (distance > 0.0001f)
        {
            RaycastHit hit;

            if (Physics.SphereCast(_lastPosition,_radius,direction.normalized, out hit,distance,_hitMask))
            {
                if (_bulletData.IsDestroyedObstacle)
                {
                    if (hit.collider.TryGetComponent<EnemyHandler>(out var enemy))
                    {
                        _bulletData.OnHitEnemy(gameObject, enemy, enemy);
                        Destroy(gameObject);
                    }
                    else if (hit.collider.TryGetComponent<PlayerController>(out var player))
                    {
                        _bulletData.OnHitEnemy(gameObject, player, null);
                        Destroy(gameObject);
                    }
                }
                else
                {
                    if (hit.collider.TryGetComponent<EnemyHandler>(out var enemy))
                    {
                        _bulletData.OnHitEnemy(gameObject, enemy, enemy);
                        Destroy(gameObject);
                    }
                    else if (hit.collider.TryGetComponent<PlayerController>(out var player))
                    {
                        _bulletData.OnHitEnemy(gameObject, player, null);
                        Destroy(gameObject);
                    }
                    else if (hit.collider.TryGetComponent<ObstacleHandler>(out var obstacle))
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
        _lastPosition = currentPos;
        
        // if (_lifeTime > 0)
        // {
        //     Vector3 direction = transform.position - _lastPosition;
        //     float distance = direction.magnitude;
        //
        //     if (_bulletData.IsDestroyedObstacle)
        //     {
        //         if (Physics.Raycast(_lastPosition, direction.normalized, out RaycastHit hit, distance, _enemyMask))
        //         {
        //             if (hit.collider.TryGetComponent<EnemyHandler>(out var enemy))
        //             {
        //                 _bulletData.OnHitEnemy(gameObject,enemy, enemy);
        //             }
        //         }
        //         else if (Physics.Raycast(_lastPosition, direction.normalized, out RaycastHit other, distance, _enemyMask))
        //         {
        //             if (other.collider.TryGetComponent<PlayerController>(out var enemy))
        //             {
        //                 _bulletData.OnHitEnemy(gameObject,enemy, null);
        //             }
        //         }
        //     }
        //     else
        //     {
        //         if (Physics.Raycast(_lastPosition, direction.normalized, out RaycastHit hitObstacle, distance, _obstacleMask))
        //         {
        //             if (hitObstacle.collider.TryGetComponent<ObstacleHandler>(out var enemy))
        //             {
        //                 Destroy(gameObject);
        //             }
        //         }
        //         
        //         if (Physics.Raycast(_lastPosition, direction.normalized, out RaycastHit hit, distance, _enemyMask))
        //         {
        //             if (hit.collider.TryGetComponent<EnemyHandler>(out var enemy))
        //             {
        //                 _bulletData.OnHitEnemy(gameObject,enemy, enemy);
        //             }
        //         }
        //         else if (Physics.Raycast(_lastPosition, direction.normalized, out RaycastHit hitPlayer, distance, _enemyMask))
        //         {
        //             if (hitPlayer.collider.TryGetComponent<PlayerController>(out var enemy))
        //             {
        //                 _bulletData.OnHitEnemy(gameObject,enemy, null);
        //             }
        //         }
        //         
        //     }
        //
        //     
        //     _lastPosition = transform.position;
        // }
        // else
        // {
        //     Destroy(gameObject);
        // }
        //
        // _lifeTime -= Time.deltaTime;
        
    }
    
}
