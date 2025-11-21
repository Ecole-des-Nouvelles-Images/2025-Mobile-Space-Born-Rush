using UnityEngine;

public class PistolBulletData : BulletData
{
    public override void OnHitEnemy(IDamageable damageable, GameObject ennemy)
    {
        damageable.TakeDamage(BulletDamage);
    }
}
