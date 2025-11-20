using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ModulMove : MonoBehaviour
{
    [SerializeField]private Vector3 _speedMovement = new Vector3(0f, 0f, -5f);
    private Rigidbody _rb;
    private float _width = 22f;
    
    [SerializeField] private List<GameObject> _obstaclesPrefabs = new List<GameObject>();
    [SerializeField] private List<Transform> _obstaclesPos = new List<Transform>();
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
        
        _rb.linearVelocity = _speedMovement;
         // SpawnObstacles();
    }

    private void SpawnObstacles()
    {
        foreach (var pos in _obstaclesPos)
        {
            float chance = Random.Range(0f, 1f);

            if (chance > 0.75) 
            {
                int randomObstacle = Random.Range(0, _obstaclesPrefabs.Count);
                Instantiate(_obstaclesPrefabs[randomObstacle],pos.position,Quaternion.identity,transform);
            }
        }
        _obstaclesPos.Clear();
    }

    void Update()
    {
        if (transform.position.z < -_width)
        {
            Destroy(gameObject);
        }
    }
}
