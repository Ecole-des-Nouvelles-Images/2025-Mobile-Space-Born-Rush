using UnityEngine;

[CreateAssetMenu(fileName = "LaneProfile", menuName = "Scriptable Objects/LaneProfile")]
public class LaneProfile : ScriptableObject
{
    [Header("Lane Positions")]
    [Tooltip("The position of the lane you want to switch to.")]
    
    public Vector3[] LanePositions;
    
    // A public property to safely get the number of lanes.
    public int LaneCount => LanePositions != null ? LanePositions.Length : 0;

    public int GetRandomLane()
    {
        return Random.Range(0, LaneCount);
    }

    public int GetLaneIndex(int laneIndex)
    {
        if (laneIndex < 0) return 0;
        if (laneIndex >= LanePositions.Length) return LaneCount - 1;

        return laneIndex;
    }
    // Public method to safely get a lane's world position.
    public Vector3 GetLanePosition(int laneIndex)
    {
        if (LanePositions == null || LanePositions.Length == 0)
        {
            Debug.LogError("LaneData: No lanes are defined!", this);
            return Vector3.zero;
        }


        if (laneIndex < 0) return LanePositions[0];
        if (laneIndex >= LanePositions.Length) return LanePositions[LaneCount - 1];
        
        
        return LanePositions[laneIndex];
    }

    
}
