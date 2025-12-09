using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleData", menuName = "Scriptable Objects/ObstacleData")]
public class ObstacleData : ScriptableObject
{
    public GameObject Prefab;
    public int LanesTaken;
    public float Damage;
    public bool IsStunning = false;
    public float StunTime;
    public bool IsRotate;
}