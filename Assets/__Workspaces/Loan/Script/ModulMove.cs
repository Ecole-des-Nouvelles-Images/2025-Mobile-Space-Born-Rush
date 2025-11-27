using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ModulMove : MonoBehaviour
{
    [Header("<color=cyan>=====Modul Settings=====</color>")]
    [SerializeField] private GameObject _modulPrefabs;
    [SerializeField] private GameUpgradeController _gameUpgrades;
    
    [Header("<color=green>=====Grid Settings=====</color>")] 
    [SerializeField] private GridManager _gridManager;

    [Header("<color=red>=====Enemy Settings=====</color>")]
    [SerializeField] private List<GameObject> _enemies;
    [SerializeField] private Transform _parentsTransform; 
    public int EnemyNumber;
    [SerializeField] private bool _isEnemy;
    
    [Header("<color=yellow>=====Obstacle Settings=====</color>")]
    [SerializeField] private List<ObstacleData> _obstaclesPrefabs = new List<ObstacleData>();
    [SerializeField] private bool _isObstacle;

    public List<Vector3> EnemyPos = new List<Vector3>();
    
    private float _width = 22f;
    private void Start()
    {
        _gameUpgrades = FindAnyObjectByType<GameUpgradeController>();
        
        if (_isObstacle)
        {
            SpawnObstacles();
        }

        if (_isEnemy)
        {
            SpawnEnemy();
        }
    }

    private void SpawnObstacles()
    {
        for (int i = 0; i < _gridManager.Grid.GetLength(0); i++)
        {
            int numberObstacle = 0;
            for (int j = 0; j < _gridManager.Grid.GetLength(1); j++)
            {

                if (numberObstacle == 2)
                {
                    break;
                }
                float chance = Random.Range(0f, 1f);

                if (chance > 0.75) 
                {
                    Vector3 offset = new Vector3(0, 0.5f, 0);
                    Vector3 pos = _gridManager.GetCellWorldPosition(i,j);
                    _gridManager.SetCellEmpty(i,j,false);
                    int randomObstacle = Random.Range(0, _obstaclesPrefabs.Count);
                    ObstacleData currentObstacle = _obstaclesPrefabs[randomObstacle];
                    GameObject obstacle = Instantiate(currentObstacle.Prefab,offset + pos,Quaternion.identity,transform);
                    obstacle.GetComponent<ObstacleHandler>().ObstacleData =  currentObstacle;
                    numberObstacle++;
                }
            }
        }
    }

    //PROBLEME IL NE SPAWN PAS AU BON ENDROIT !!!!!
    private void SpawnEnemy()
    {
        for (int i = 0; i < _gridManager.Grid.GetLength(0); i++)
        {
            for (int j = 0; j < _gridManager.Grid.GetLength(1); j++)
            {
                float chance = Random.Range(0f, 1f);
                if (chance > 0.8f && EnemyNumber > 0 && _gridManager.Grid[i,j].IsEmpty)
                {
                    int random =  Random.Range(0, _enemies.Count);
                    Vector3 offset = new Vector3(0, 0.5f, 0);
                    Vector3 pos = _gridManager.GetCellWorldPosition(i,j);
                    GameObject enemy = Instantiate(_enemies[random],offset + pos ,Quaternion.identity);
                    EnemyPos.Add(offset + pos);
                    EnemyNumber--;
                }
                else if (EnemyNumber == 0)
                {
                    return;
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
