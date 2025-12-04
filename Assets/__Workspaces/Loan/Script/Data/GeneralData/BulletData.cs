using UnityEngine;

[CreateAssetMenu(fileName = "BulletData", menuName = "Scriptable Objects/BulletData")]
public abstract class BulletData : ScriptableObject
{
    public float BulletDamage;
    public bool IsDestroyedObstacle = false;

    public abstract void OnHitEnemy(IDamageable damageable, EnemyHandler enemy);
}
