using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatController : MonoBehaviour
{
    //Data variables
    [SerializeField] private PlayerData _playerData;
    
    
    //Player Variables
    [SerializeField] private float _maxHealth, _currentHealth, _hpDropMultiplier, _currentXP, _xpToLevelUp, _xpMultiplier;

    #region subscriptions
    private void Awake()
    {
        // Player Specific Events
        EventBus.OnPlayerRevive += OnPlayerRevive;
        EventBus.OnPlayerTakeDamage += OnPlayerTakeDamage;
        EventBus.OnPlayerCurrentHealthChange += OnPlayerCurrentHealthChange;
        EventBus.OnPlayerMaxHealthChange += OnPlayerMaxHealthChange;
        EventBus.OnPlayerCurrentXPChange += OnPlayerCurrentXPChange;
        EventBus.OnPlayerXPToLevelUpChange += OnPlayerXPToLevelUpChange;
        EventBus.OnPlayerLevelUp += OnPlayerLevelUp;
        EventBus.OnPlayerChooseAnItem += OnPlayerChooseAnItem;
        
        // Main event
        EventBus.OnStatUpgrade += OnStatUpgrade;
        
    }

    private void OnStatUpgrade(UpgradeType upgrade, float value)
    {
        switch (upgrade)
        {
            case UpgradeType.MaxHP:
                EventBus.OnPlayerMaxHealthChange?.Invoke(value);
                break;
            case UpgradeType.HPMultiplier:
                _hpDropMultiplier += value;
                break;
            case UpgradeType.XPMultiplier:
                _xpMultiplier += value;
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
    private void OnPlayerMaxHealthChange(float val)
    {
        //Depends on how the value is sent but this is the basic logic
        _maxHealth += val;
        EventBus.OnPlayerCurrentHealthChange(val);
    }
    private void OnPlayerCurrentHealthChange(float val)
    {
        _currentHealth += val;
        
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
    }

    private void OnPlayerCurrentXPChange(float val)
    {
        _currentXP += val;

        if (_currentXP >= _xpToLevelUp)
        {
            // Get upgrades and trigger level up UI
            var upgradeGenerator = FindObjectOfType<UpgradeGenerator>();
            var choices = upgradeGenerator.GenerateUpgradeChoices();
            EventBus.OnPlayerLevelUp?.Invoke(choices);
            
            // Reset XP for next level
            _currentXP = 0;
            EventBus.OnPlayerXPToLevelUpChange?.Invoke(_xpToLevelUp * 1.2f);
        }
    }
    private void OnPlayerXPToLevelUpChange(float val)
    {
        _xpToLevelUp = val;
    }
    private void OnPlayerRevive()
    {
        _currentHealth = _maxHealth;
    }
    private void OnPlayerTakeDamage(float val)
    {
        _currentHealth -= val;
        
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        if (_currentHealth <= 0)
        {
            Invoke("OnPlayerDead", 0.1f);
        }
    }
    private void OnDestroy()
    {
        // Player Specific Events
        EventBus.OnPlayerRevive -= OnPlayerRevive;
        EventBus.OnPlayerTakeDamage -= OnPlayerTakeDamage;
        EventBus.OnPlayerCurrentHealthChange -= OnPlayerCurrentHealthChange;
        EventBus.OnPlayerMaxHealthChange -= OnPlayerMaxHealthChange;
        EventBus.OnPlayerCurrentXPChange -= OnPlayerCurrentXPChange;
        EventBus.OnPlayerXPToLevelUpChange -= OnPlayerXPToLevelUpChange;
        EventBus.OnPlayerLevelUp -= OnPlayerLevelUp;
        EventBus.OnPlayerChooseAnItem -= OnPlayerChooseAnItem;
        
        // Main event
        EventBus.OnStatUpgrade -= OnStatUpgrade;
    }

    #endregion
    
    public float HPDropMultiplier => _hpDropMultiplier;
    public float XPMultiplier => _xpMultiplier;
    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public float CurrentXp => _currentXP;
    public float XpToLevelUp => _xpToLevelUp;
    
    void Start()
    {
        _maxHealth = _playerData.BaseMaxHealth;
        _currentHealth = _playerData.BaseMaxHealth;
        _hpDropMultiplier = _playerData.BaseHPDropMultiplier;
        _currentXP = 0;
        _xpToLevelUp = _playerData.BaseXPToLevelUp;
        _xpMultiplier = _playerData.BaseXPMultiplier;
    }
}