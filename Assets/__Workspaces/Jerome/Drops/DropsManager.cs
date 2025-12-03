using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DropsManager : MonoBehaviour
{

    [SerializeField] private List<DropsData> _dropsData = new List<DropsData>();
    
    private Dictionary<DropType, DropsData> _dropTypeMap;
    private Dictionary<DropType, Vector2> _dropChanceRanges;

    #region MyRegion
        private void OnEnable()
        {
            EventBus.OnEnemyDead += OnEnemyDead;
        }

        private void OnEnemyDead(GameObject obj)
        {
            SpawnDrop(obj.transform);
        }

        private void OnDisable()
        {
            EventBus.OnEnemyDead -= OnEnemyDead;
        }
        #endregion

    private void Start()
    {
        ValidateDropData();
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
        // XP is always first in list
        DropsData xpData = _dropsData[0];
        
        // TODO replace with pooling
        GameObject xp = Instantiate(xpData.Prefab);
        xp.transform.position = position;
    }
    
    void SpawnRandomPowerUps(Vector3 basePosition)
    {
        // Check each power-up
        for (int i = 0; i < _dropsData.Count; i++)
        {
            DropsData drop = _dropsData[i];
            
            // Independent chance check for this power-up
            // Random.Range(0, 101) gives 0-100, so < DropChance works correctly
            if (Random.Range(0, 101) < drop.DropChance)
            {
                SpawnPowerUp(drop, basePosition, i);
            }
        }
    }
    
    void SpawnPowerUp(DropsData data, Vector3 basePosition, int index)
    {
        // Calculate position with offset to avoid overlap
        Vector3 offset = CalculateSpawnOffset(index);
        Vector3 spawnPosition = basePosition + offset;
        
        // TODO replace with pooling
        GameObject drop = Instantiate(data.Prefab);
        drop.transform.position = spawnPosition;
        
        Debug.Log($"Spawned {data.Name} at {spawnPosition}");
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
