using System;
using UnityEngine;

public class GenerateGrid : MonoBehaviour
{
    [SerializeField] private GameObject _floor;
    [SerializeField] private int _cellX;
    [SerializeField] private int _cellZ;
    [SerializeField] private float _cellSize = 1.5f;

    [SerializeField] private MeshRenderer[] _lanesBounds;
    
    
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

        Debug.Log($"Grille générée : {_cellX} x {_cellZ}");
    }
    
    public Vector3 GetCellWorldPosition(int x, int z)
    {
        return Grid[x, z].Position;
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
