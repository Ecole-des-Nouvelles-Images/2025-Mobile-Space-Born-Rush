using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerStatController : MonoBehaviour
{
    //Data variables
    [SerializeField] private PlayerData _playerData;
    
    
    //Player Variables
    [SerializeField] private float _maxHealth, _currentHealth, _hpDropMultiplier, _currentXP, _xpToLevelUp, _xpMultiplier;

    [SerializeField] private Image _healthBar;
    [SerializeField] private ParticleSystem _takeXpParticles;
    private bool _isStart = false;
    
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
        EventBus.OnGameStart += GameStart;

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
        _maxHealth += val;
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
        _takeXpParticles.Play();

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
        FeedbackManager.SmallShake();
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
        
        
        //Invoke GameOver
        EventBus.OnGameOver?.Invoke();
        _isStart = false;
        
        //Then die
        // Destroy(this);
        
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
        EventBus.OnGameStart -= GameStart;
    }

    #endregion
    
    //If I ever decide to decouple this class into a data class and a damage handler or smth
    public float HPDropMultiplier => _hpDropMultiplier;
    public float XPMultiplier => _xpMultiplier;
    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public float CurrentXp => _currentXP;
    public float XpToLevelUp => _xpToLevelUp;
    
    private void GameStart()
    {
        _maxHealth = _playerData.BaseMaxHealth;
        _currentHealth = _playerData.BaseMaxHealth;
        _hpDropMultiplier = _playerData.BaseHPDropMultiplier;
        _currentXP = 0;
        _xpToLevelUp = _playerData.BaseXPToLevelUp;
        _xpMultiplier = _playerData.BaseXPMultiplier;
        _isStart = true;
    }

    private void Update()
    {
        if (_isStart)
        {
            _healthBar.fillAmount = _currentHealth / MaxHealth;
        }
    }
}