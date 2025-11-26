using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BulletData", menuName = "Scriptable Objects/BulletData/ShotGunBulletData")]
public class ShotGunBulletData : BulletData
{
    [FormerlySerializedAs("_knockbackDirections")] [SerializeField] private List<Vector3> _knockbackMidDirections = new List<Vector3>();
    [SerializeField] private List<Vector3> _knockbackRightDirections = new List<Vector3>();
    [SerializeField] private List<Vector3> _knockbackLeftDirections = new List<Vector3>();
    [SerializeField] private float _knockbackForce;
    
    private WeaponUpgradeController _weaponUpgrades;

    public void Start()
    {
        _weaponUpgrades = FindAnyObjectByType<WeaponUpgradeController>();
    }
    public override void OnHitEnemy(GameObject bullet, IDamageable damageable, EnemyHandler enemy)
    {
        if(!_weaponUpgrades) Start();
        damageable.TakeDamage(BulletDamage + _weaponUpgrades.Damage);
        if (enemy.Transform.position.x == 0)
        {
            Vector3 dir = _knockbackMidDirections[Random.Range(0, _knockbackMidDirections.Count)];
            enemy.ApplyKnockback(dir * _knockbackForce);
        }
        else
        {
            if (enemy.Transform.position.x > 1 )
            {
                Vector3 dir = _knockbackRightDirections[Random.Range(0, _knockbackMidDirections.Count)];
                enemy.ApplyKnockback(dir * _knockbackForce);
            }
            else if (enemy.Transform.position.x < -1 )
            {
                Vector3 dir = _knockbackLeftDirections[Random.Range(0, _knockbackMidDirections.Count)];
                enemy.ApplyKnockback(dir * _knockbackForce);
            }
        }
        
        Destroy(bullet);
    }
}