using System;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyHandler : MonoBehaviour, IDamageable
{
    public Vector3 Speed;
    public Transform Transform;
    private Vector3 _knockbackVelocity;
    private float _knockbackTimer = 3f;
    private float _health = 10;
    private Rigidbody _rb;
    private float _knockbackForceMax = 8f;
    private float[] _lanes = new[] { -1.5f, 0f, 1.5f };
    private bool _isKnocked = false;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        Transform = GetComponent<Transform>();
    }

    private void FixedUpdate()
    {
        _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, _knockbackTimer * Time.fixedDeltaTime);
        
        _rb.linearVelocity = Speed + _knockbackVelocity;
        
        if (!_isKnocked && _knockbackVelocity.magnitude < 0.05f)
        {
            SnapToLanes();
            _isKnocked = true;
        }
        
        if (_knockbackVelocity.magnitude > 0.1f)
        {
            _isKnocked = false;
        }
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
    
    public void ApplyKnockback(Vector3 force)
    {
        _knockbackVelocity += force;
        
        _knockbackVelocity = Vector3.ClampMagnitude(_knockbackVelocity, _knockbackForceMax);
    }

    private void SnapToLanes()
    {
        float currentX = transform.position.x;
        float closeLane = _lanes[0];
        float betterDistance = Mathf.Abs(currentX - _lanes[0]);

        for (int i = 1; i < _lanes.Length; i++)
        {
            float diff =  Mathf.Abs(currentX - _lanes[i]);

            if (diff < betterDistance)
            {
                betterDistance = diff;
                closeLane = _lanes[i];
            }
        }
        transform.position = new Vector3(closeLane, transform.position.y, transform.position.z);
    }
}
