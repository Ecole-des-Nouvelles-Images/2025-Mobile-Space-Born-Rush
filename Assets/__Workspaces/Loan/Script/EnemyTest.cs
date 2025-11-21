using System;
using UnityEngine;

public class EnemyTest : MonoBehaviour, IDamageable
{
    public Vector3 Speed;
    private float _health = 10;
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.linearVelocity = Speed;
    }

    public void TakeDamage(float damage)
    {
        if (_health > 0)
        {
            _health -= damage;
            Debug.Log(_health);
        }
        else
        {
            Debug.Log("Die !");
        }
    }
}
