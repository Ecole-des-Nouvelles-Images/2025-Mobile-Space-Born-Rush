using UnityEngine;

[CreateAssetMenu(fileName = "BulletData", menuName = "Scriptable Objects/BulletData")]
public abstract class BulletData : ScriptableObject
{
    public float BulletDamage;

    public abstract void OnHitEnemy(IDamageable damageable, EnemyHandler enemy);
}
