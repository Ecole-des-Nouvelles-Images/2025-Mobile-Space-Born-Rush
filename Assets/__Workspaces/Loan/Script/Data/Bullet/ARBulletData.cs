using UnityEngine;

[CreateAssetMenu(fileName = "BulletData", menuName = "Scriptable Objects/BulletData/ARBulletData")]
public class ARBulletData : BulletData
{
    private WeaponUpgradeController _weaponUpgrade;
    public override void OnHitEnemy(GameObject bullet, IDamageable damageable, GameObject ennemy)
    {
        damageable.TakeDamage(BulletDamage + _weaponUpgrade.Damage);
        Destroy(bullet);
    }
}
