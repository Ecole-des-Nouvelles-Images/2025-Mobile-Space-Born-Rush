using System;
using UnityEngine;

public class EnemyTest : MonoBehaviour, IDamageable
{
    public Vector3 Speed;
    public Transform EnemyTransform;
    private Vector3 _knockbackVelocity;
    private float _knockbackTimer = 3f;
    private float _health = 10;
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        EnemyTransform = GetComponent<Transform>();
    }

    public void TakeDamage(float damage)
    {
        if (_health > 0)
        {
            _health -= damage;
            // Debug.Log(_health);
        }
        else
        {
            Debug.Log("Die !");
        }
    }

    private void FixedUpdate()
    {
        _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, _knockbackTimer * Time.fixedDeltaTime);
        
        _rb.linearVelocity = Speed + _knockbackVelocity;
    }
    public void ApplyKnockback(Vector3 force)
    {
        _knockbackVelocity += force;
    }
}
