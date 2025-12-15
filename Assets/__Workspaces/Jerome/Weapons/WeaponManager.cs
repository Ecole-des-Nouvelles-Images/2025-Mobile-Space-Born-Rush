using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private WeaponUpgradeController _weaponUpgrades;
    
    [SerializeField] private int _currentWeaponIndex;
    [SerializeField] private int _swapWeaponIndex;

    // Drag all weapon data assets here
    public List<GameObject> AllWeaponPrefabs;

    private void Awake()
    {
        EventBus.OnWeaponUpgrade += OnWeaponUpgrade;
        EventBus.OnGameStart += OnGameStart;
        SwapWeapon(AllWeaponPrefabs[0]);
    }

    private void OnWeaponUpgrade(UpgradeType upgrade, float value)
    {
        if (upgrade == UpgradeType.WeaponSwap)
        {
            _swapWeaponIndex = (int)value;
            SwapWeapon(AllWeaponPrefabs[_swapWeaponIndex]);
        }
    }

    private void OnDestroy()
    {
        EventBus.OnWeaponUpgrade -= OnWeaponUpgrade;
        EventBus.OnGameStart -= OnGameStart;
    }

    void Start()
    {
        _weaponUpgrades = FindAnyObjectByType<WeaponUpgradeController>();
        
        // Equip default weapon
        SwapWeapon(AllWeaponPrefabs[0]);
    }

    private void OnGameStart()
    {
        _swapWeaponIndex = 0;
        SwapWeapon(AllWeaponPrefabs[0]);
    }

    public void SwapWeapon(GameObject newWeaponData)
    {
        if (!newWeaponData) return;

        // Deactivate current
        AllWeaponPrefabs[_currentWeaponIndex].SetActive(false);


        // Activate new
        AllWeaponPrefabs[_swapWeaponIndex].SetActive(true);
        _currentWeaponIndex = _swapWeaponIndex;
        Debug.Log($"Swapped to {newWeaponData.name}"); // Use the asset name for logging
    }

    public GameObject GetCurrentWeaponData()
    {
        return AllWeaponPrefabs[_currentWeaponIndex];
    }
}