using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpgradeGenerator : MonoBehaviour
{
    private GameUpgradeController _gameUpgrades;
    [SerializeField] private List<UpgradeData> allPossibleUpgrades;
    private Dictionary<UpgradeData, int> timesSelected = new();
    
    // Weighted probabilities for different upgrade categories
    [SerializeField] private float weaponWeight;
    [SerializeField] private float playerStatWeight;
    [SerializeField] private float gameSystemWeight;

    [SerializeField] private List<UpgradeData> availableUpgrades;

    private void OnEnable()
    {
        // Start with all upgrades available
        availableUpgrades = new List<UpgradeData>(allPossibleUpgrades);
        
        _gameUpgrades = FindAnyObjectByType<GameUpgradeController>();
        
        EventBus.OnWeaponUpgrade += OnWeaponUpgrade;
        EventBus.OnGameStart += () => RegenerateUpgradePool();
    }

    private void OnWeaponUpgrade(UpgradeType weapon, float weaponIndex)
    {
        if (weapon != UpgradeType.WeaponSwap) return;
        //Basically when the weapon swap happens we should remove that weapon from the pool of choices
        //We regenerate it so that when you swap again that older weapon can be picked again
        RegenerateUpgradePool();
        RemoveUpgradeFromPool(FindWeapon(weaponIndex));
    }

    private void OnDisable()
    {
        EventBus.OnWeaponUpgrade -= OnWeaponUpgrade;
        EventBus.OnGameStart -= () => RegenerateUpgradePool();
    }

    private UpgradeData FindWeapon(float weaponIndex)
    {
        foreach (var upgrade in allPossibleUpgrades)
        {
            if (upgrade.Type == UpgradeType.WeaponSwap)
            {
                if(upgrade.Value == weaponIndex) return upgrade;
            }
        }
        return null;
    }

    // Called when player levels up
    public List<UpgradeData> GenerateUpgradeChoices()
    {
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
    }
    
    // Call this when the upgrade should no longer be available
    // like when there are the max amount of choices or the speed is too high
    private void RemoveUpgradeFromPool(UpgradeData selectedUpgrade)
    {
        availableUpgrades.Remove(selectedUpgrade);
        
    }

    /// <summary>
    //The point of this function is to increment the amount fo times we have selected an upgrade
    //If it has a limit then it''s call a local function to remove it from the choice pool.
    /// </summary>
    /// <param name="selectedUpgrade"></param>
    public void UpgradeSelected(UpgradeData selectedUpgrade)
    {
        if (selectedUpgrade.maxTimesSelectable == 0) return;
        
    }
}