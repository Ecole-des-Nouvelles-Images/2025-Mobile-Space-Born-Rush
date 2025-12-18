using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class GameDirector : MonoBehaviour
{

    //The game director should handle the logic for the game to become harder and harder as the player is playing.
    //I would say that the director should periodically increase the speed by a little at a time.
    private Timer _speedIncrementTimer;
    
    [Header("In ms 1000ms = 1s")]
    [SerializeField] private int _speedIncrement = 30000;
    //It should also increase the enemies health, maybe on player level up
    private int _playerLevel = 1; // Could tweak how often it happens ex: every 2,5 or 10 levels
    [SerializeField] private int _difficultyPerLevel = 5;
    //Finally it should also increase the spawn rate of enemies and obstacles, but it should be very slow to do that.
    //Maybe mix player level with a new timer?
    private Timer _difficultyIncrementTimer;

    [Header("In ms 1000ms = 1s")]
    [SerializeField] private int _difficultyIncrement = 60000;
    [Header("Current boost they have")]
    [SerializeField] private int _difficultyHealth;
    [SerializeField] private int _difficultyDamage;
    [Header("Increment per level up and timer")]
    [SerializeField] private int _healthIncrement = 10;
    [SerializeField] private int _damageIncrement = 2;
    //It needs a reference to all the upgrade controllers and then the enemy handler
    [SerializeField] private GameUpgradeController _gameUpgrades;
    
    [Header("Sound")]
    [SerializeField] private AudioSource _audio1Source;
    [SerializeField] private AudioSource _audio2Source;

    private void OnEnable()
    {
        EventBus.OnGameStart += OnGameStart;
        EventBus.OnPlayerLevelUp += OnPlayerLevelUp;
        EventBus.OnGameOver += OnGameOver;
        float random = Random.Range(0f, 1f);

        if (random < 0.5f)
        {
            _audio1Source.Play();
        }
        else
        {
            _audio2Source.Play();
        }
    }

    private void OnGameOver()
    {
        //Turn timers off
        _speedIncrementTimer.Enabled = false;
        _speedIncrementTimer.Elapsed -= SpeedBoost;
        _difficultyIncrementTimer.Enabled = false;
        _difficultyIncrementTimer.Elapsed -= DifficultyBoost;
        
        //Reset the health and difficulty
        _difficultyHealth = 0;
        _difficultyDamage = 0;
    }

    private void OnPlayerLevelUp(List<UpgradeData> obj)
    {
        _playerLevel++;

        if (_playerLevel >= _difficultyPerLevel)
        {
            _playerLevel = 1;

            _difficultyHealth += _healthIncrement;
            _difficultyDamage += _damageIncrement;
        }
    }

    private void OnGameStart()
    {
        // Timer in ms 1000ms = 1s
        _speedIncrementTimer = new Timer(_speedIncrement);
        _speedIncrementTimer.Elapsed += SpeedBoost;
        _speedIncrementTimer.AutoReset = true;
        _speedIncrementTimer.Enabled = true;
        
        _difficultyIncrementTimer = new Timer(_difficultyIncrement);
        _difficultyIncrementTimer.Elapsed += DifficultyBoost;
        _difficultyIncrementTimer.AutoReset = true;
        _difficultyIncrementTimer.Enabled = true;
    }



    private void OnDisable()
    {
        EventBus.OnGameStart -= OnGameStart;
        EventBus.OnPlayerLevelUp -= OnPlayerLevelUp;
        EventBus.OnGameOver -= OnGameOver;

        if (_speedIncrementTimer == null && _difficultyIncrementTimer == null) return;
        
        //Turn off timers
        _speedIncrementTimer.Enabled = false;
        _speedIncrementTimer.Elapsed -= SpeedBoost;
        _difficultyIncrementTimer.Enabled = false;
        _difficultyIncrementTimer.Elapsed -= DifficultyBoost;
    }
    
    private void SpeedBoost(object sender, ElapsedEventArgs e)
    {
        _gameUpgrades.GameSpeed += 0.2f;
    }
    private void DifficultyBoost(object sender, ElapsedEventArgs e)
    {
        _difficultyHealth += _healthIncrement;
        _difficultyDamage += _damageIncrement;
    }

    public int DifficultyHealth => _difficultyHealth;
    public int DifficultyDamage => _difficultyDamage;
}
