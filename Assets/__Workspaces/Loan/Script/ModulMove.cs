using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ModulMove : MonoBehaviour
{
    [Header("<color=cyan>=====Modul Settings=====</color>")]
    [SerializeField] private GameObject _modulPrefabs;
    [SerializeField] private GameUpgradeController _gameUpgrades;

    [Header("<color=green>=====Grid Settings=====</color>")] 
    [SerializeField] private GenerateGrid _grid;
    
    [Header("<color=yellow>=====Obstacle Settings=====</color>")]
    [SerializeField] private List<ObstacleData> _obstaclesPrefabs = new List<ObstacleData>();
    [SerializeField] private bool _isObstacle;
    
    [SerializeField] private List<Transform> _obstaclesPos = new List<Transform>();
    
    private float _width = 22f;
    private void Start()
    {
        _gameUpgrades = FindAnyObjectByType<GameUpgradeController>();
        
        if (_isObstacle)
        {
            SpawnObstacles();
        }
    }

    private void SpawnObstacles()
    {
        //First method
        // foreach (var pos in _obstaclesPos)
        // {
        //     float chance = Random.Range(0f, 1f);
        //
        //     if (chance > 0.75) 
        //     {
        //         int randomObstacle = Random.Range(0, _obstaclesPrefabs.Count);
        //         ObstacleData currentObstacle = _obstaclesPrefabs[randomObstacle];
        //         GameObject obstacle = Instantiate(currentObstacle.Prefab,pos.position,Quaternion.identity,transform);
        //         obstacle.GetComponent<ObstacleHandler>().ObstacleData =  currentObstacle;
        //     }
        // }
        // _obstaclesPos.Clear();
        
        //Grid 

        for (int i = 0; i < _grid.Grid.GetLength(0); i++)
        {
            int numberObstacle = 0;
            for (int j = 0; j < _grid.Grid.GetLength(1); j++)
            {

                if (numberObstacle == 2)
                {
                    break;
                }
                float chance = Random.Range(0f, 1f);

                if (chance > 0.75) 
                {
                    Vector3 offset = new Vector3(0, 0.5f, 0);
                    Vector3 pos = _grid.GetCellWorldPosition(i,j);
                    int randomObstacle = Random.Range(0, _obstaclesPrefabs.Count);
                    ObstacleData currentObstacle = _obstaclesPrefabs[randomObstacle];
                    GameObject obstacle = Instantiate(currentObstacle.Prefab,offset + pos,Quaternion.identity,transform);
                    obstacle.GetComponent<ObstacleHandler>().ObstacleData =  currentObstacle;
                    numberObstacle++;
                }
            }
        }
    }

    void Update()
    {
        var vector3 = transform.position;
        vector3.z = vector3.z - _gameUpgrades.GameSpeed * Time.deltaTime;
        transform.position = vector3;
        if (transform.position.z < -_width)
        {
            Instantiate(_modulPrefabs, new Vector3(0,0,58), Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
