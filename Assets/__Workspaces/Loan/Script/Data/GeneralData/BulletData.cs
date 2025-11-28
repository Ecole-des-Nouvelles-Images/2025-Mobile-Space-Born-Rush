using UnityEngine;

[CreateAssetMenu(fileName = "BulletData", menuName = "Scriptable Objects/BulletData")]
public abstract class BulletData : ScriptableObject
{
    public GameObject BulletPrefab;
    public Vector3 BulletSpeed;
    public float BulletLifeTime;
    public float BulletDamage;
    public bool IsDestroyedObstacle = false;

    public abstract void OnHitEnemy(GameObject bullet,IDamageable damageable, EnemyHandler enemy);
}
