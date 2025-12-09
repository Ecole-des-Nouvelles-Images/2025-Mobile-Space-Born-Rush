using System;
using System.Collections;
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
        EventBus.OnGameStart +=  GameStart;
        EventBus.OnSpeedChanged += OnSpeedChanged;
        
    }

    private void OnSpeedChanged(float speed, float time)
    {
        StartCoroutine(SpeedBoost(speed, time));
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
        EventBus.OnGameStart -= GameStart;
    }

    #endregion
    
    public float GameSpeed
    {
        get => _gameSpeed;
        set => _gameSpeed = value;
    }

    public float NumberOfUpgrades => _numberOfUpgrades;

    IEnumerator SpeedBoost(float speedBoost, float time)
    {
        _gameSpeed *= speedBoost;
        yield return new WaitForSeconds(time);
        _gameSpeed/= speedBoost;
    }
    private void GameStart()
    {
        _gameSpeed = 8;
        _numberOfUpgrades = 2;
    }

    public void ResetUpgrades()
    {
        _gameSpeed = 1;
        _numberOfUpgrades = 2;
    }
}
