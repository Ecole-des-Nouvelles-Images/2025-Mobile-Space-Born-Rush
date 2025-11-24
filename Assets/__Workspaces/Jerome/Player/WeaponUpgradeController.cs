using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponUpgradeController : MonoBehaviour
{
    [SerializeField] private float _damage, _fireRate, _cooldown, _magSize, _weapon;

          #region subscriptions
    private void Awake()
    {
        // Game State Events
        EventBus.OnGameResume += OnGameResume;
        EventBus.OnGameRestart += OnGameRestart;
        
        // Player Specific Events
        EventBus.OnPlayerLevelUp += OnPlayerLevelUp;
        EventBus.OnPlayerChooseAnItem += OnPlayerChooseAnItem;
        
        // Main event
        EventBus.OnWeaponUpgrade += OnWeaponUpgrade;
        
    }

    private void OnWeaponUpgrade(UpgradeType upgrade, float value)
    {
        switch (upgrade)
        {
            case UpgradeType.Damage:
                _damage  += value;
                break;
            
            case UpgradeType.FireRate:
                _fireRate += value;
                break;
            
            case UpgradeType.Cooldown:
                _cooldown += value;
                break;
            
            case UpgradeType.MagSize:
                _magSize += value;
                break;
            
            case UpgradeType.WeaponSwap :
                _weapon = value;
                break;
                
        }
        Debug.Log("Stat upgrade: " + upgrade);
    }

    private void OnPlayerChooseAnItem(UpgradeType obj)
    {
        throw new NotImplementedException();
    }

    private void OnPlayerLevelUp(List<UpgradeData> obj)
    {
        Debug.Log("PlayerLevelUp");
    }
    private void OnGameResume()
    {
        throw new NotImplementedException();
    }
    private void OnGameRestart()
    {
        throw new NotImplementedException();
    }
    private void OnDestroy()
    {
        // Game State Events
        EventBus.OnGameResume -= OnGameResume;
        EventBus.OnGameRestart -= OnGameRestart;
        
        // Player Specific Events
        EventBus.OnPlayerLevelUp -= OnPlayerLevelUp;
        EventBus.OnPlayerChooseAnItem -= OnPlayerChooseAnItem;
        
        // Main event
        EventBus.OnWeaponUpgrade -= OnWeaponUpgrade;
    }

    #endregion
    
    public float Damage => _damage;
    public float FireRate => _fireRate;
    public float Cooldown => _cooldown;
    public float MagSize => _magSize;
    
    void Start()
    {
        _damage = 0;
        _fireRate = 0;
        _cooldown = 0;
        _magSize = 0;
        _weapon = 0;
    }

    public void ResetUpgrades()
    {
        _damage = 0;
        _fireRate = 0;
        _cooldown = 0;
        _magSize = 0;
    }
}