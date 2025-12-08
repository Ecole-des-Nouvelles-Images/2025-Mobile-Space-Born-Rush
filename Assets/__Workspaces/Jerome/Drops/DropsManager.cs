using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DropsManager : MonoBehaviour
{

    [SerializeField] private List<DropsData> _dropsData = new List<DropsData>();
    [SerializeField] private Transform _dropsParent;
    
    private Dictionary<DropType, DropsData> _dropTypeMap;
    private Dictionary<DropType, Vector2> _dropChanceRanges;

    #region MyRegion
        private void OnEnable()
        {
            EventBus.OnEnemyDead += OnEnemyDead;
            EventBus.OnGameOver += GameOver;
        }

        private void OnEnemyDead(GameObject obj)
        {
            SpawnDrop(obj.transform);
        }

        private void OnDisable()
        {
            EventBus.OnEnemyDead -= OnEnemyDead;
            EventBus.OnGameOver -= GameOver;
        }
        #endregion
        
    private void Start()
    {
        ValidateDropData();
    }

    private void GameOver()
    {
        foreach (Transform child in _dropsParent)
        {
            Destroy(child.gameObject);
        }
            
    }

    private void ValidateDropData()
    {
        if (_dropsData.Count == 0)
        {
            Debug.LogError("DropsManager: No drop data configured!");
            return;
        }
        
        // Check if first item is XP (recommended convention)
        if (_dropsData[0].Type != DropType.XP)
        {
            Debug.LogWarning($"DropsManager: First item is {_dropsData[0].Type}, not XP. Make sure XP is first in the list.");
        }
        
        // XP should have 0% drop chance since it always drops
        if (_dropsData[0].DropChance != 0)
        {
            Debug.LogWarning($"DropsManager: XP has DropChance = {_dropsData[0].DropChance}%. XP should have 0% chance.");
        }
    }
    
    void SpawnDrop(Transform enemyTransform)
    {
        Vector3 spawnPos = enemyTransform.position;
        
        // 1. ALWAYS drop XP (guaranteed, first in list)
        SpawnXP(spawnPos);
        
        // 2. Check EACH power-up independently (skip index 0 = XP)
        SpawnRandomPowerUps(spawnPos);
    }
    
    void SpawnXP(Vector3 position)
    {
        DropsData xpData = _dropsData[0];
        GameObject xp = Instantiate(xpData.Prefab, position, Quaternion.identity, _dropsParent.transform);
        // xp.transform.position = position;
        
        // CRITICAL: Pass the data to the drop
        DropsMovement movement = xp.GetComponent<DropsMovement>();
        if (movement)
        {
            movement.Initialize(xpData);
            xp.SetActive(true);
        }
    }
    
    void SpawnRandomPowerUps(Vector3 basePosition)
    {
        // Check each power-up
        for (int i = 1; i < _dropsData.Count; i++)
        {
            DropsData drop = _dropsData[i];
            
            // Independent chance check for this power-up
            // Random.Range(0, 101) gives 0-100, so < DropChance works correctly
            if (Random.Range(0, 101) < drop.DropChance)
            {
                SpawnPowerUp(drop, basePosition, i);
                return;
            }
        }
    }
    
    void SpawnPowerUp(DropsData data, Vector3 basePosition, int index)
    {
        Vector3 offset = CalculateSpawnOffset(index);
        Vector3 spawnPosition = basePosition + offset;
        
        GameObject drop = Instantiate(data.Prefab, spawnPosition, Quaternion.identity, _dropsParent.transform);
        drop.transform.position = spawnPosition;
        
        // CRITICAL: Pass the data to the drop
        DropsMovement movement = drop.GetComponent<DropsMovement>();
        if (movement)
        {
            movement.Initialize(data);
            drop.SetActive(true);
        }
        
    }
    
    Vector3 CalculateSpawnOffset(int powerUpIndex)
    {
        // Simple circular offset pattern
        
        int adjustedIndex = powerUpIndex - 1; // Convert to 0-based for power-ups
        
        float angle = adjustedIndex * 90f; // 90 degrees apart
        float radius = 0.3f;
        
        float x = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
        float z = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
        
        return new Vector3(x, 0.2f, z); // Slightly above ground
    }
}
