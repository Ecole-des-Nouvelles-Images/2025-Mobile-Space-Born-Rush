using System;
using UnityEngine;

public class KillTriggerHandler : MonoBehaviour
{
    [SerializeField] private float _percent;
    private PlayerStatController _playerStat;

    private void Start()
    {
        _playerStat = FindAnyObjectByType<PlayerStatController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            float damage = _playerStat.MaxHealth * _percent;
            EventBus.OnPlayerTakeDamage?.Invoke(damage);
            Destroy(other.gameObject);
        }
    }
}
