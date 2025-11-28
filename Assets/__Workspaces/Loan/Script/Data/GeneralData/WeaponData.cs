using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public GameObject Prefab;
    public BulletData BulletData; 
    public string Name;
    public float Cooldown;
    public float FireRate;
    public float FireDistance;
    public int MagSize;
    public LayerMask EnemyMask;
}
