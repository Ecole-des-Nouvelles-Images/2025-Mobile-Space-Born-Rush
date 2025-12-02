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

    private void Awake()
    {
        BuildDropTables();
    }
    
    void BuildDropTables()
    {
        _dropTypeMap = new Dictionary<DropType, DropsData>();
        _dropChanceRanges = new Dictionary<DropType, Vector2>();
        
        float currentMax = 0f;
        
        // First pass: Find XP (should be first, but let's be safe)
        DropsData xpData = _dropsData.Find(d => d.Type == DropType.XP);
        if (xpData == null)
        {
            Debug.LogError("No XP drop data found in list!");
            return;
        }
        
        // Build chance ranges for NON-XP items (power-ups only)
        foreach (DropsData drop in _dropsData)
        {
            _dropTypeMap[drop.Type] = drop; // Map type to data
            
            if (drop.Type == DropType.XP) continue; // Skip XP for chance table
            
            float chance = drop.DropChance / 100f;
            _dropChanceRanges[drop.Type] = new Vector2(currentMax, currentMax + chance);
            currentMax += chance;
        }
    }
    
    void SpawnDrop(Transform enemyTransform)
    {
        Vector3 spawnPos = enemyTransform.position;
        
        // 1. ALWAYS drop XP (find by enum)
        if (_dropTypeMap.TryGetValue(DropType.XP, out DropsData xpData))
        {
            GameObject xp = Instantiate(xpData.Prefab);
            xp.transform.position = spawnPos;
        }
        
        // 2. Roll for power-ups (using your range system)
        if (_dropsData.Count > 1) // If we have more than just XP
        {
            float roll = Random.Range(0f, 1f);
            
            foreach (var kvp in _dropChanceRanges)
            {
                DropType type = kvp.Key;
                Vector2 range = kvp.Value;
                
                if (roll >= range.x && roll < range.y)
                {
                    // Spawn this power-up by looking up its data
                    if (_dropTypeMap.TryGetValue(type, out DropsData powerUpData))
                    {
                        GameObject drop = Instantiate(powerUpData.Prefab);
                        drop.transform.position = spawnPos;
                    }
                    break; // Only one power-up
                }
            }
        }
    }
}
