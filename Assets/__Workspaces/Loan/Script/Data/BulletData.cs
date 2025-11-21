using UnityEngine;

[CreateAssetMenu(fileName = "BulletData", menuName = "Scriptable Objects/BulletData")]
public abstract class BulletData : ScriptableObject
{
    public GameObject BulletPrefab;
    public float BulletSpeed;
    public float BulletLifeTime;
    public float BulletDamage;

    public abstract void OnHitEnemy(IDamageable damageable, GameObject ennemy);
}
