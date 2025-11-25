using UnityEngine;

[CreateAssetMenu(fileName = "BulletData", menuName = "Scriptable Objects/BulletData/PistolBulletData")]
public class PistolBulletData : BulletData
{
    public override void OnHitEnemy(GameObject bullet,IDamageable damageable, EnemyHandler enemy)
    {
        damageable.TakeDamage(BulletDamage);
        Destroy(bullet);
    }
}
