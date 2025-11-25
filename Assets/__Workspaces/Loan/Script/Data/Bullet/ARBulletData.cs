using UnityEngine;

[CreateAssetMenu(fileName = "BulletData", menuName = "Scriptable Objects/BulletData/ARBulletData")]
public class ARBulletData : BulletData
{
    
    private WeaponUpgradeController _weaponUpgrades;

    public void Start()
    {
        _weaponUpgrades = FindAnyObjectByType<WeaponUpgradeController>();
    }
    public override void OnHitEnemy(GameObject bullet, IDamageable damageable, EnemyHandler enemy)
    {
        if(!_weaponUpgrades) Start();
        damageable.TakeDamage(BulletDamage + _weaponUpgrades.Damage);
        Destroy(bullet);
    }
}