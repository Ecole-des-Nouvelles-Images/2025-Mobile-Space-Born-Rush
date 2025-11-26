using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerStatController : MonoBehaviour
{
    //Data variables
    [SerializeField] private PlayerData _playerData;
    
    
    //Player Variables
    [FormerlySerializedAs("_maxHealth")]public  float MaxHealth;
    [SerializeField] private float _currentHealth, _hpDropMultiplier, _currentXP, _xpToLevelUp, _xpMultiplier;

    #region subscriptions
    private void Awake()
    {
        // Player Specific Events
        EventBus.OnPlayerRevive += OnPlayerRevive;
        EventBus.OnPlayerTakeDamage += OnPlayerTakeDamage;
        EventBus.OnPlayerDead += OnPlayerDead;
        EventBus.OnPlayerCurrentHealthChange += OnPlayerCurrentHealthChange;
        EventBus.OnPlayerMaxHealthChange += OnPlayerMaxHealthChange;
        EventBus.OnPlayerCurrentXPChange += OnPlayerCurrentXPChange;
        EventBus.OnPlayerXPToLevelUpChange += OnPlayerXPToLevelUpChange;
        
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
    }
    private void OnPlayerMaxHealthChange(float val)
    {
        //Depends on how the value is sent but this is the basic logic
        MaxHealth += val;
        EventBus.OnPlayerCurrentHealthChange(val);
    }
    private void OnPlayerCurrentHealthChange(float val)
    {
        _currentHealth += val;
        
        _currentHealth = Mathf.Clamp(_currentHealth, 0, MaxHealth);
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
        _currentHealth = MaxHealth;
    }
    private void OnPlayerTakeDamage(float val)
    {
        _currentHealth -= val;
        
        _currentHealth = Mathf.Clamp(_currentHealth, 0, MaxHealth);

        if (_currentHealth <= 0)
        {
            EventBus.OnPlayerDead?.Invoke();
        }
    }
    private void OnPlayerDead()
    {
        //Play death animations
        //
        
        //Play death screen
        //
        
        //Then die
        Destroy(this);
        
        //Invoke GameOver
        EventBus.OnGameOver?.Invoke();
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
        
        // Main event
        EventBus.OnStatUpgrade -= OnStatUpgrade;
    }

    #endregion
    
    //If I ever decide to decouple this class into a data class and a damage handler or smth
    // public float HPDropMultiplier => _hpDropMultiplier;
    // public float XPMultiplier => _xpMultiplier;
    // public float MaxHealth => _maxHealth;
    // public float CurrentHealth => _currentHealth;
    // public float CurrentXp => _currentXP;
    // public float XpToLevelUp => _xpToLevelUp;
    
    void Start()
    {
        MaxHealth = _playerData.BaseMaxHealth;
        _currentHealth = _playerData.BaseMaxHealth;
        _hpDropMultiplier = _playerData.BaseHPDropMultiplier;
        _currentXP = 0;
        _xpToLevelUp = _playerData.BaseXPToLevelUp;
        _xpMultiplier = _playerData.BaseXPMultiplier;
    }
}