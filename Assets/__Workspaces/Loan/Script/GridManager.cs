using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    [Header("===== Cells Settings =====")]
    [SerializeField] private int _cellX;
    [SerializeField] private int _cellZ;
    [SerializeField] private float _cellSize = 1.5f;

    [Header("===== Lanes List =====")]
    [SerializeField] private MeshRenderer[] _lanesBounds;
    
    private List<int> _neightbornUpDown =  new List<int>(){2,1,-1,-2};
    private List<int> _neightbornEnemy = new List<int>() {1,2};
    private List<int> _neightbornAll = new List<int>() { 1,-1 };
    
    private Bounds _planeBounds;
    private Color _gizmoColor;

    public Cell[,] Grid;

    private void Awake()
    {
        Generate();
        _gizmoColor = Random.ColorHSV();
    }

    private void Generate()
    {
        _planeBounds = _lanesBounds[0].bounds;
        for (int i = 1; i < _lanesBounds.Length; i++)
        {
            _planeBounds.Encapsulate(_lanesBounds[i].bounds);
        }
        
        Vector3 size = _planeBounds.size;
        _cellX = Mathf.RoundToInt(size.x / _cellSize);
        _cellZ = Mathf.RoundToInt(size.z / _cellSize);

        Grid = new Cell[_cellX, _cellZ];
        
        Vector3 origin = new Vector3(_planeBounds.min.x, _planeBounds.center.y, _planeBounds.min.z);

        for (int x = 0; x < _cellX; x++)
        {
            for (int z = 0; z < _cellZ; z++)
            {
                Vector3 cellPos = origin + new Vector3(
                    (x + 0.5f) * _cellSize,
                    0f,
                    (z + 0.5f) * _cellSize
                );

                Grid[x, z] = new Cell
                {
                    IndexX = x,
                    IndexZ = z,
                    Position = cellPos,
                    ParentObject = null,
                    IsEmpty = true
                };
            }
        }
    }
    
    public Vector3 GetCellWorldPosition(int x, int z)
    {
        return Grid[x, z].Position;
    }

    public void SetCellEmpty(int x, int z, bool empty)
    {
        Cell cell = Grid[x, z];
        cell.IsEmpty = empty;
        Grid[x, z] = cell;
    }

    public bool IsNeightbornEmpty(int x, int z, ObstacleData obs, bool IsObstacle = false)
    {
        if (IsObstacle)
        {
            if (!Grid[x, z].IsEmpty)
                return false;
            
            if (obs.LanesTaken == 1)
            {
                if (obs.IsRotate)
                {
                   return CheckAndReserveUpDownSafe(x, z);
                }
                else
                {
                    return CheckAndReserveAllNeighborsSafe(x, z);
                }
                
            }
            else if (obs.LanesTaken == 3)
            {
                return CheckAndReserveLanesSafe(z);
            }
        }
        else
        {
            if (!Grid[x, z].IsEmpty)
                return false;

            foreach (var pos in _neightbornEnemy)
            {
                int neightborn = z - pos;
                if ( neightborn < 0 || neightborn >= _cellZ)
                    continue;

                if (!Grid[x, neightborn].IsEmpty)
                    return false;
            }
            return true;
        }
        
        return false;
    }

    #region CheckRegion
    
    bool CheckAndReserveAllNeighborsSafe(int x, int z)
    {
        List<(int x, int z)> toReserve = new List<(int x, int z)>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) 
                    continue; 

                int nx = x + dx;
                int nz = z + dz;
                
                if (nx < 0 || nx >= _cellX || nz < 0 || nz >= _cellZ)
                    continue;
                        
                if (!Grid[nx, nz].IsEmpty)
                    return false;

                toReserve.Add((nx, nz));
            }
        }
        
        foreach (var cell in toReserve)
            SetCellEmpty(cell.x, cell.z, false);

        return true;
    }
    
    bool CheckAndReserveUpDownSafe(int x, int z)
    {
        List<int> toReserveZ = new List<int>();

        foreach (var pos in _neightbornUpDown)
        {
            int nz = z + pos;

            if (nz < 0 || nz >= _cellZ)
                continue;

            if (!Grid[x, nz].IsEmpty)
                return false;

            toReserveZ.Add(nz);
        }
        
        foreach (int nz in toReserveZ)
            SetCellEmpty(x, nz, false);

        return true;
    }
    
    bool CheckAndReserveLanesSafe(int z)
    {
        for (int laneX = 0; laneX < _cellX; laneX++)
        {
            if (!Grid[laneX, z].IsEmpty)
                return false;
        }
        
        List<(int x, int z)> toReserve = new List<(int x, int z)>();

        foreach (var pos in _neightbornUpDown)
        {
            int neightbornZ = z + pos;
            if (neightbornZ < 0 || neightbornZ >= _cellZ)
                continue;

            for (int laneX = 0; laneX < _cellX; laneX++)
            {
                if (!Grid[laneX, neightbornZ].IsEmpty)
                    return false;

                toReserve.Add((laneX, neightbornZ));
            }
        }
        
        foreach (var p in toReserve)
            SetCellEmpty(p.x, p.z, false);

        return true;
    }

    #endregion
    
    
    private void OnDrawGizmos()
    {
        if (Grid == null || Grid.Length == 0)
            return;

        Gizmos.color = _gizmoColor;;

        for (int x = 0; x < _cellX; x++)
        {
            for (int z = 0; z < _cellZ; z++)
            {
                Vector3 pos = Grid[x, z].Position;
                Vector3 size = new Vector3(_cellSize, 0.01f, _cellSize);

                Gizmos.DrawWireCube(pos, size);
            }
        }
    }
}

[System.Serializable]
public struct Cell
{
    public int IndexX;
    public int IndexZ;
    public Vector3 Position;
    public GameObject ParentObject;
    public bool IsEmpty;
}
