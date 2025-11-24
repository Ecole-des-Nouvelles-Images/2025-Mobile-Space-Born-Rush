using UnityEngine;

[CreateAssetMenu(fileName = "BulletData", menuName = "Scriptable Objects/BulletData/ARBulletData")]
public class ARBulletData : BulletData
{
    public override void OnHitEnemy(GameObject bullet, IDamageable damageable, EnemyHandler enemy)
    {
        damageable.TakeDamage(BulletDamage);
        Destroy(bullet);
    }
}