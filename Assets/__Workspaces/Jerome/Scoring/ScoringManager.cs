using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoringManager : MonoBehaviour
{
    private GameUpgradeController _gameUpgrades;
    
    private float _score, _highScore;
    private bool _isGamePaused = true;
    [SerializeField]private int _killsNumber = 0;
    [SerializeField]private float _timer = 0;
    [Header("=====Text Settings=====")]
    [SerializeField]private TextMeshProUGUI _timerText;
    [SerializeField]private TextMeshProUGUI _scoreText;
    [SerializeField]private TextMeshProUGUI _killsText;

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
        _killsNumber++;
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
        _killsNumber = 0;
        _timer = 0;
        _score = 0;
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
        _gameUpgrades =  FindAnyObjectByType<GameUpgradeController>();
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: Uncomment when the game UI is implemented
        //if (_isGamePaused) return;
        if (_isGamePaused)
            return;
        _timer += Time.deltaTime;
        
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
        int minutes = Mathf.FloorToInt(_timer / 60);
        int seconds = Mathf.FloorToInt(_timer % 60);

        _timerText.text = $"Timer: {minutes:00}:{seconds:00}";
        _scoreText.text = "Score: " + (int)_score;
        _killsText.text = "Kills: " + _killsNumber;
    }

    public float Score => _score;
    public float GetHighScore()
    {
        return _highScore;
    }
}
