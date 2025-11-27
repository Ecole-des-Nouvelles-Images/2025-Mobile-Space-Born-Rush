using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoringManager : MonoBehaviour
{
    private GameUpgradeController _gameUpgrades;
    
    private float _score;
    private float _highScore;
    private bool _isGamePaused = true;

    #region subscriptions
    private void Awake()
    {
        EventBus.OnGamePause += OnGamePause;
        EventBus.OnPlayerLevelUp += OnPlayerLevelUp;
        EventBus.OnGameOver += OnGameOver;
        
        EventBus.OnUpgradeApplied += OnUpgradeApplied;
        EventBus.OnGameResume += OnGameResume;
        EventBus.OnGameStart += OnGameStart;
        
    }

    private void OnGamePause()
    {
        _isGamePaused = true;
    }
    private void OnPlayerLevelUp(List<UpgradeData> obj)
    {
        _isGamePaused = true;
    }

    private void OnGameOver()
    {
        _isGamePaused = true;
        GameOver();
    }
    
    private void OnUpgradeApplied(UpgradeData obj)
    {
        _isGamePaused = false;
    }
    private void OnGameResume()
    {
        _isGamePaused = false;
    }


    private void OnGameStart()
    {
        _isGamePaused = false;
    }

    private void OnDestroy()
    {
        EventBus.OnGamePause -= OnGamePause;
        EventBus.OnPlayerLevelUp -= OnPlayerLevelUp;
        EventBus.OnGameOver -= OnGameOver;
        
        EventBus.OnUpgradeApplied -= OnUpgradeApplied;
        EventBus.OnGameResume -= OnGameResume;
        EventBus.OnGameStart -= OnGameStart;
    }

    #endregion
    void Start()
    {
        _gameUpgrades =  FindObjectOfType<GameUpgradeController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void GameOver()
    {
        throw new NotImplementedException();
    }
}
