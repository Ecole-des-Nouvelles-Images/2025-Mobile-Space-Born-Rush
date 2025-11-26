using System;
using System.Collections.Generic;
using UnityEngine;

public class GameUpgradeController : MonoBehaviour
{
    [SerializeField] private float _gameSpeed, _numberOfUpgrades;

    #region subscriptions
    private void Awake()
    {
        // Player Specific Events
        EventBus.OnPlayerChooseAnItem += OnPlayerChooseAnItem;
        
        // Main event
        EventBus.OnGameSystemUpgrade += OnGameSystemUpgrade;
        
    }

    private void OnGameSystemUpgrade(UpgradeType upgrade, float value)
    {
        switch (upgrade)
        {
            case UpgradeType.CardChoiceCount:
                _numberOfUpgrades  += value;
                break;
            
            case UpgradeType.Speed:
                _gameSpeed += value;
                break;
                
        }
    }

    private void OnPlayerChooseAnItem(UpgradeType obj)
    {
        throw new NotImplementedException();
    }
    private void OnDestroy()
    {
        // Player Specific Events
        EventBus.OnPlayerChooseAnItem -= OnPlayerChooseAnItem;
        
        // Main event
        EventBus.OnGameSystemUpgrade -= OnGameSystemUpgrade;
    }

    #endregion
    
    public float GameSpeed => _gameSpeed;
    public float NumberOfUpgrades => _numberOfUpgrades;
    
    void Start()
    {
        _gameSpeed = 1;
        _numberOfUpgrades = 2;
    }

    public void ResetUpgrades()
    {
        _gameSpeed = 1;
        _numberOfUpgrades = 2;
    }
}
