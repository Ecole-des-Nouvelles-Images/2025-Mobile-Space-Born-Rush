using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int _cellX;
    [SerializeField] private int _cellZ;
    [SerializeField] private float _cellSize = 1.5f;

    [SerializeField] private MeshRenderer[] _lanesBounds;
    
    private List<int> _neightbornUpDown =  new List<int>(){2,1,-1,-2};
    private List<int> _neightbornEnemy = new List<int>() {1,2};
    
    private Bounds _planeBounds;

    public Cell[,] Grid;

    private void Awake()
    {
        Generate();
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
                foreach (var pos in _neightbornUpDown)
                {
                    int neightborn = z + pos;
                    if ( neightborn < 0 || neightborn >= _cellZ)
                        continue;

                    if (!Grid[x, neightborn].IsEmpty)
                        return false;
                    SetCellEmpty(x,neightborn, false);
                }
                return true;
            }
            else if (obs.LanesTaken == 3)
            {
                for (int laneX = 0; laneX < _cellX; laneX++)
                {
                    if (!Grid[laneX, z].IsEmpty)
                        return false;
                }
                
                foreach (var pos in _neightbornUpDown)
                {
                    int neightbornZ = z + pos;
                    if (neightbornZ < 0 || neightbornZ >= _cellZ)
                        continue;

                    for (int laneX = 0; laneX < _cellX; laneX++)
                    {
                        if (!Grid[laneX, neightbornZ].IsEmpty)
                            return false;
                        SetCellEmpty(laneX, neightbornZ, false);
                    }
                }
            }
            return true;
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
    
    private void OnDrawGizmos()
    {
        if (Grid == null || Grid.Length == 0)
            return;

        Gizmos.color = Color.green;

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
