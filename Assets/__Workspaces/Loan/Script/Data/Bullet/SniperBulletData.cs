using UnityEngine;

[CreateAssetMenu(fileName = "BulletData", menuName = "Scriptable Objects/BulletData/SniperBulletData")]
public class SniperBulletData : BulletData
{
    public override void OnHitEnemy(GameObject bullet, IDamageable damageable, EnemyHandler enemy)
    {
        damageable.TakeDamage(BulletDamage);
    }
}