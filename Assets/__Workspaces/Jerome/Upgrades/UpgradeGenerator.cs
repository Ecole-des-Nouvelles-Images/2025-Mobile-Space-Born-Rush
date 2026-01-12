using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpgradeGenerator : MonoBehaviour
{
    private GameUpgradeController _gameUpgrades;
    [SerializeField] private List<UpgradeData> _allPossibleUpgrades;
    private Dictionary<UpgradeData, int> _timesSelected = new();
    
    // Weighted probabilities for different upgrade categories
    [SerializeField] private float _weaponWeight;
    [SerializeField] private float _playerStatWeight;
    [SerializeField] private float _gameSystemWeight;

    [SerializeField] private List<UpgradeData> _availableUpgrades;

    private void OnEnable()
    {
        // Start with all upgrades available
        _availableUpgrades = new List<UpgradeData>(_allPossibleUpgrades);
        
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
        foreach (UpgradeData upgrade in _allPossibleUpgrades)
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
        List<UpgradeData> choices = new();
        List<UpgradeData> tempPool = new(_availableUpgrades);

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
        List<UpgradeData> weightedPool = new();
        
        foreach (UpgradeData upgrade in pool)
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
            UpgradeCategory.WeaponStat => Mathf.RoundToInt(_weaponWeight * 10),
            UpgradeCategory.PlayerStat => Mathf.RoundToInt(_playerStatWeight * 10),
            UpgradeCategory.GameSystem => Mathf.RoundToInt(_gameSystemWeight * 10),
            _ => 1
        };
    }


    private void RegenerateUpgradePool()
    {
        _availableUpgrades = new List<UpgradeData>(_allPossibleUpgrades);
    }
    
    // Call this when the upgrade should no longer be available
    // like when there are the max amount of choices or the speed is too high
    private void RemoveUpgradeFromPool(UpgradeData selectedUpgrade)
    {
        _availableUpgrades.Remove(selectedUpgrade);
        
    }

    /// <summary>
    //The point of this function is to increment the amount of times we have selected an upgrade
    //If it has a limit then it''s call a local function to remove it from the choice pool.
    /// </summary>
    /// <param name="selectedUpgrade"></param>
    public void SelectedIncrement(UpgradeData selectedUpgrade)
    {
        if (selectedUpgrade.maxTimesSelectable == 0) return;

        _timesSelected.TryAdd(selectedUpgrade, 0);

        _timesSelected[selectedUpgrade]++;

        if (_timesSelected[selectedUpgrade] >= selectedUpgrade.maxTimesSelectable)
        {
            RemoveUpgradeFromPool(selectedUpgrade);
        }
 
    }
}