using System.Collections.Generic;
using UnityEngine;

public class UpgradeGenerator : MonoBehaviour
{
    private GameUpgradeController _gameUpgrades;
    [SerializeField] private List<UpgradeData> allPossibleUpgrades;
    [Header("Dynamic Choices")]
    
    // Weighted probabilities for different upgrade categories
    [SerializeField] private float weaponWeight = 0.4f;
    [SerializeField] private float playerStatWeight = 0.4f;
    [SerializeField] private float gameSystemWeight = 0.2f;

    private List<UpgradeData> availableUpgrades;

    private void OnEnable()
    {
        // Start with all upgrades available
        availableUpgrades = new List<UpgradeData>(allPossibleUpgrades);
        
        _gameUpgrades = FindAnyObjectByType<GameUpgradeController>();
        
        EventBus.OnWeaponUpgrade += OnWeaponUpgrade; 
    }

    private void OnWeaponUpgrade(UpgradeType weapon, float weaponIndex)
    {
        RegenerateUpgradePool();
        //RemoveUpgradeFromPool(weapon.);
    }

    // Called when player levels up
    public List<UpgradeData> GenerateUpgradeChoices()
    {
        if (availableUpgrades.Count == 0)
        {
            Debug.LogWarning("No upgrades available! Regenerating pool.");
            RegenerateUpgradePool();
        }

        List<UpgradeData> choices = new List<UpgradeData>();
        List<UpgradeData> tempPool = new List<UpgradeData>(availableUpgrades);

        // Use dynamic choice count instead of fixed
        for (int i = 0; i < _gameUpgrades.NumberOfUpgrades; i++)
        {
            if (tempPool.Count == 0) break;

            // Get weighted random upgrade
            UpgradeData selected = GetWeightedRandomUpgrade(tempPool);
            choices.Add(selected);
            tempPool.Remove(selected);
        }

        return choices;
    }

    private UpgradeData GetWeightedRandomUpgrade(List<UpgradeData> pool)
    {
        // Create weighted list based on category weights
        List<UpgradeData> weightedPool = new List<UpgradeData>();
        
        foreach (var upgrade in pool)
        {
            int copies = GetWeightCopies(upgrade.category);
            for (int i = 0; i < copies; i++)
            {
                weightedPool.Add(upgrade);
            }
        }

        // If weighted pool is empty, fall back to random
        if (weightedPool.Count == 0) 
            return pool[Random.Range(0, pool.Count)];

        return weightedPool[Random.Range(0, weightedPool.Count)];
    }

    private int GetWeightCopies(UpgradeCategory category)
    {
        return category switch
        {
            UpgradeCategory.WeaponStat => Mathf.RoundToInt(weaponWeight * 10),
            UpgradeCategory.PlayerStat => Mathf.RoundToInt(playerStatWeight * 10),
            UpgradeCategory.GameSystem => Mathf.RoundToInt(gameSystemWeight * 10),
            _ => 1
        };
    }


    private void RegenerateUpgradePool()
    {
        availableUpgrades = new List<UpgradeData>(allPossibleUpgrades);
        
        // Optional: Increase weights for rare upgrades on regeneration
        gameSystemWeight += 0.1f; // Make system upgrades more common in late game
    }
    
    // Call this when the upgrade should no longer be available
    // like when there are the max amount of choices or the speed is too high
    public void RemoveUpgradeFromPool(UpgradeData selectedUpgrade)
    {
        availableUpgrades.Remove(selectedUpgrade);
        
    }
}