using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public GameObject Prefab;
    public GameObject AmmoPrefab; 
    public string Name;
    public float Damage;
    public float Cooldown;
    public float FireRate;
    public int MagSize;
}
