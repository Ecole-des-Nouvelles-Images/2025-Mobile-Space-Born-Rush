using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ModulMove : MonoBehaviour
{
    [Header("<color=cyan>=====Modul Settings=====</color>")]
    [SerializeField] private List<GameObject> _modulPrefabs = new List<GameObject>();
    [SerializeField] private GameUpgradeController _gameUpgrades;
    
    [Header("<color=green>=====Grid Settings=====</color>")] 
    [SerializeField] private GridManager _gridManager;

    [Header("<color=red>=====Enemy Settings=====</color>")]
    [SerializeField] private List<GameObject> _enemies;
    [SerializeField] private Transform _parentsTransform; 
    public int EnemyNumber;
    [FormerlySerializedAs("_isEnemy")] public bool isEnemy;
    
    [Header("<color=yellow>=====Obstacle Settings=====</color>")]
    [SerializeField] private List<ObstacleData> _obstaclesPrefabs = new List<ObstacleData>();
    [FormerlySerializedAs("_isObstacle")] public bool isObstacle;
    
    
    private float _width = 25f;
    private void Start()
    {
        _gameUpgrades = FindAnyObjectByType<GameUpgradeController>();
        _parentsTransform = GameObject.FindGameObjectWithTag("Parents").transform;
        EventBus.OnGameOver += GameOver;
        
        if (isObstacle)
        {
            SpawnObstacles();
        }

        if (isEnemy)
        {
            SpawnEnemy();
        }
    }

    private void OnDestroy()
    {
        EventBus.OnGameOver -= GameOver;
    }

    private void SpawnObstacles()
    {
        // for (int i = 2; i < _gridManager.Grid.GetLength(0); i++)
        // {
        //     for (int j = 0; j < _gridManager.Grid.GetLength(1); j++)
        //     {
        //         float chance = Random.Range(0f, 1f);
        //
        //         if (chance > 0.75) 
        //         {
        //             int randomObstacle = Random.Range(0, _obstaclesPrefabs.Count);
        //             ObstacleData currentObstacle = _obstaclesPrefabs[randomObstacle];
        //             
        //             if (_gridManager.IsNeightbornEmpty(i,j,currentObstacle,true))
        //             {
        //                 Vector3 pos = _gridManager.GetCellWorldPosition(i,j);
        //                 Quaternion rotate = Quaternion.identity;
        //                 
        //                 if (currentObstacle.LanesTaken == 3)
        //                 {
        //                     pos.x = 0;
        //                 }
        //
        //                 if (currentObstacle.LanesTaken == 1)
        //                 {
        //                      rotate = Quaternion.Euler(0,Random.Range(0,360),0);
        //                 }
        //                 GameObject obstacle = Instantiate(currentObstacle.Prefab,pos,rotate,transform);
        //                 obstacle.GetComponent<ObstacleHandler>().ObstacleData =  currentObstacle;
        //
        //                 if (currentObstacle.LanesTaken == 1)
        //                 {
        //                     _gridManager.SetCellEmpty(i,j,false);
        //                 }
        //                 else if (currentObstacle.LanesTaken == 3)
        //                 {
        //                     for (int laneX = 0; laneX < _gridManager.Grid.GetLength(0); laneX++)
        //                     {
        //                         _gridManager.SetCellEmpty(laneX, j, false);
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }
        
        int width = _gridManager.Grid.GetLength(0);
        int height = _gridManager.Grid.GetLength(1);
            
        List<Vector2Int> candidateCells = new List<Vector2Int>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 1; j < height; j++) 
            { 
                candidateCells.Add(new Vector2Int(i, j));
            }
        }
        
        // int obstaclesToSpawn = ObstacleNumber; 

        while (candidateCells.Count > 0)
        {
            int randomIndex = Random.Range(0, candidateCells.Count);
            Vector2Int cell = candidateCells[randomIndex];
            candidateCells.RemoveAt(randomIndex);
            
            float chance = Random.Range(0f, 1f);
            if (chance <= 0.75f)
                continue;
            
            int randomObstacle = Random.Range(0, _obstaclesPrefabs.Count);
            ObstacleData currentObstacle = _obstaclesPrefabs[randomObstacle];
            
            if (!_gridManager.IsNeightbornEmpty(cell.x, cell.y, currentObstacle, true))
                continue;
            
            Vector3 pos = _gridManager.GetCellWorldPosition(cell.x, cell.y);
            Quaternion rotate = Quaternion.identity;
            
            if (currentObstacle.LanesTaken == 3)
            {
                pos.x = 0;
            }

            if (currentObstacle.LanesTaken == 1)
            {
                rotate = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }
            
            GameObject obstacle = Instantiate(currentObstacle.Prefab, pos, rotate, transform);
            obstacle.GetComponent<ObstacleHandler>().ObstacleData = currentObstacle;
            
            if (currentObstacle.LanesTaken == 1)
            {
                _gridManager.SetCellEmpty(cell.x, cell.y, false);
            }
            else if (currentObstacle.LanesTaken == 3)
            {
                for (int laneX = 0; laneX < width; laneX++)
                {
                    _gridManager.SetCellEmpty(laneX, cell.y, false);
                }
            }

            // obstaclesToSpawn--;
        }
    }

    
    private void SpawnEnemy()
    {
        //First Version !  
        // for (int z = 0; z < _gridManager.Grid.GetLength(1); z++)
        // {
        //     for (int x = 0; x < _gridManager.Grid.GetLength(0); x++)
        //     {
        //         float chance = Random.Range(0f, 1f);
        //         if (chance > 0.8f && EnemyNumber > 0 && _gridManager.IsNeightbornEmpty(x,z,null, false))
        //         {
        //             int random =  Random.Range(0, _enemies.Count);
        //             Vector3 offset = new Vector3(0, 1f, 0);
        //             Vector3 pos = _gridManager.GetCellWorldPosition(x,z);
        //             GameObject enemy = Instantiate(_enemies[random],offset + pos ,Quaternion.Euler(0f, 180f, 0f),_parentsTransform.transform);
        //             _gridManager.SetCellEmpty(x,z,false);
        //             EnemyNumber--;
        //         }
        //         else if (EnemyNumber <= 0)
        //         {
        //             return;
        //         }
        //     }
        // }
        
        //Second Version !
        
        int width = _gridManager.Grid.GetLength(0);
        int height = _gridManager.Grid.GetLength(1);
        
        List<Vector2Int> emptyCells = new List<Vector2Int>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (_gridManager.IsNeightbornEmpty(i,j,null,false))
                {
                    emptyCells.Add(new Vector2Int(i, j));
                }
            }
        }

        while (EnemyNumber > 0 && emptyCells.Count > 0)
        {
            int random = Random.Range(0, emptyCells.Count);
            Vector2Int cell = emptyCells[random];
            emptyCells.RemoveAt(random);
            
            float chance = Random.Range(0f, 1f);
            if (chance <= 0.8f)
                continue;
            
            int randomEnemy = Random.Range(0, _enemies.Count);
            Vector3 offset = new Vector3(0, 1f, 0);
            Vector3 pos = _gridManager.GetCellWorldPosition(cell.x, cell.y);
            
            Instantiate(_enemies[randomEnemy], offset + pos, Quaternion.identity, _parentsTransform.transform);
            EnemyNumber--;
        }
    }

    void Update()
    {
        var vector3 = transform.position;
        vector3.z = vector3.z - _gameUpgrades.GameSpeed * Time.deltaTime;
        transform.position = vector3;
        if (transform.position.z < -_width)
        {
            var random =  Random.Range(0, _modulPrefabs.Count);
            Instantiate(_modulPrefabs[random], new Vector3(0,0,53), Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private void GameOver()
    {
        Destroy(this.gameObject);
    }
}
