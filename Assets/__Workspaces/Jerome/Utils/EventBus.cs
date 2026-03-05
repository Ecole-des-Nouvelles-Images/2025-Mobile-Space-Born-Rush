using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus
{
    // ===== GAME EVENTS =====
    public static Action OnGameStart;
    public static Action OnGamePause;
    public static Action OnGameResume;
    public static Action OnGameOver;
    public static Action<int> OnHighScore;
    /// <summary>
    /// Multiplicative value, duration in seconds
    /// </summary>
    public static Action<float, float> OnSpeedChanged;
    public static Action<bool> OnInternetConnection;
    
    // ===== PLAYER EVENTS =====
    public static Action OnPlayerSpawn;
    public static Action OnPlayerDead;
    public static Action OnPlayerRevive;
    public static Action<float> OnPlayerTakeDamage;
    public static Action<float> OnPlayerTakeStun;
    // public static Action<PowerUpType> OnPowerUpCollected;
    /// <summary>
    /// Current health + value passed
    /// </summary>
    public static Action<float> OnPlayerCurrentHealthChange;
    /// <summary>
    /// Max health + value passed
    /// </summary>
    public static Action<float> OnPlayerMaxHealthChange;
    /// <summary>
    /// Current XP + value passed
    /// </summary>
    public static Action<float> OnPlayerCurrentXPChange;
    /// <summary>
    /// Max xp + value passed
    /// </summary>
    public static Action<float> OnPlayerXPToLevelUpChange;
    public static Action<int> OnLaneChanged;
    /// <summary>
    /// Duration in seconds
    /// </summary>
    public static Action<float> OnMagnetActivated;
    
    // ===== UPGRADE SYSTEM EVENTS =====
    public static Action<List<UpgradeData>> OnPlayerLevelUp;
    public static Action<UpgradeType> OnPlayerChooseAnItem;
    public static Action<UpgradeData> OnUpgradeApplied;
    public static Action<UpgradeType, float> OnStatUpgrade;
    public static Action<UpgradeType, float> OnWeaponUpgrade;
    public static Action OnWeaponSwap;
    public static Action<UpgradeType, float> OnGameSystemUpgrade;

    // ===== COMBAT EVENTS =====
    // public static Action<WeaponType> OnWeaponChanged;
    public static Action OnObstacleHit;

    // ===== ENEMY EVENTS =====
    // public static Action<EnemyType> OnEnemySpawn;
    /// <summary>
    /// Returns the enemy gameobject in case it needs to be called somewhere
    /// </summary>
    public static Action<GameObject> OnEnemyDead;
    public static Action<float> OnEnemyTakeDamage;
    public static Action OnEnemyGotPastPlayer;
    public static Action<int, int> OnDifficultyChange;

    // ===== ENVIRONMENT EVENTS =====
    public static Action<Terrain> GenerationTerrainFinished;
}