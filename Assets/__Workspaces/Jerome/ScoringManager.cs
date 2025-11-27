using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoringManager : MonoBehaviour
{
    private GameUpgradeController _gameUpgrades;
    
    private float _score, _highScore;
    private bool _isGamePaused = true;

    #region subscriptions
    private void Awake()
    {
        //Pauses the Score
        EventBus.OnGamePause += OnGamePause;
        EventBus.OnPlayerLevelUp += OnPlayerLevelUp;
        EventBus.OnGameOver += OnGameOver;
        
        //Starts the score
        EventBus.OnUpgradeApplied += OnUpgradeApplied;
        EventBus.OnGameResume += OnGameResume;
        EventBus.OnGameStart += OnGameStart;
        
        //Increases the score
        EventBus.OnEnemyDead += OnEnemyDead;
        
        //Decreases the score
        EventBus.OnEnemyGotPastPlayer += OnEnemyGotPastPlayer;
        EventBus.OnObstacleHit += OnObstacleHit;
    }

    private void OnObstacleHit()
    {
        DecreaseScore(10f);
    }

    private void OnEnemyGotPastPlayer()
    {
        //Need to add VFX to show the hit.
        DecreaseScore(100f);
    }

    private void OnEnemyDead(GameObject obj)
    {
        //Maybe we could take the enemy type and decrease the score more or less.
        IncreaseScore(100f);
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
        
        //Increases the score
        EventBus.OnEnemyDead -= OnEnemyDead;
        
        //Decreases the score
        EventBus.OnEnemyGotPastPlayer -= OnEnemyGotPastPlayer;
        EventBus.OnObstacleHit -= OnObstacleHit;
    }

    #endregion
    void Start()
    {
        _gameUpgrades =  FindObjectOfType<GameUpgradeController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isGamePaused) return;
        
        _score += _gameUpgrades.GameSpeed *  Time.deltaTime;
    }

    private void DecreaseScore(float value)
    {
        _score -= value;
        //Do something to make it look pretty when the player looses points
    }    
    private void IncreaseScore(float value)
    {
        _score += value;
        //Do something to make it look pretty when the player looses points
    }
    private void GameOver()
    {
        Debug.Log(_score);
    }

    public float Score => _score;
    public float GetHighScore()
    {
        return _highScore;
    }
}
