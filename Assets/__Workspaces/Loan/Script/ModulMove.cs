using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ModulMove : MonoBehaviour
{
    [SerializeField] private GameObject _modulPrefabs;
    [SerializeField]private Vector3 _speedMovement = new Vector3(0f, 0f, -5f);
    [SerializeField] private GameUpgradeController _gameUpgrades;
    private Rigidbody _rb;
    private float _width = 22f;
    
    [SerializeField] private List<ObstacleData> _obstaclesPrefabs = new List<ObstacleData>();
    [SerializeField] private List<Transform> _obstaclesPos = new List<Transform>();
    
    [SerializeField] private bool _isObstacle;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _gameUpgrades = FindAnyObjectByType<GameUpgradeController>();
        
        if (_isObstacle)
        {
            SpawnObstacles();
        }
    }
    private void SpawnObstacles()
    {
        foreach (var pos in _obstaclesPos)
        {
            float chance = Random.Range(0f, 1f);

            if (chance > 0.75) 
            {
                int randomObstacle = Random.Range(0, _obstaclesPrefabs.Count);
                ObstacleData currentObstacle = _obstaclesPrefabs[randomObstacle];
                GameObject obstacle = Instantiate(currentObstacle.Prefab,pos.position,Quaternion.identity,transform);
                obstacle.GetComponent<ObstacleHandler>().ObstacleData =  currentObstacle;
            }
        }
        _obstaclesPos.Clear();
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
