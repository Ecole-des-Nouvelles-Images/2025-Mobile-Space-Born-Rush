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
    public static Action OnGameRestart;
    public static Action<float> OnDistanceRun;
    public static Action<int> OnScoreChanged;
    public static Action<float> OnSpeedChanged;
    
    // ===== PLAYER EVENTS =====
    public static Action OnPlayerSpawn;
    public static Action OnPlayerDead;
    public static Action OnPlayerRevive;
    public static Action<float> OnPlayerTakeDamage;
    public static Action<float> OnPlayerTakeStun;
    // public static Action<PowerUpType> OnPowerUpCollected;
    public static Action<float> OnPlayerCurrentHealthChange;
    public static Action<float> OnPlayerMaxHealthChange;
    public static Action<float> OnPlayerCurrentXPChange;
    public static Action<float> OnPlayerXPToLevelUpChange;
    public static Action<int> OnLaneChanged;
    
    // ===== UPGRADE SYSTEM EVENTS =====
    public static Action<List<UpgradeData>> OnPlayerLevelUp;
    public static Action<UpgradeType> OnPlayerChooseAnItem;
    public static Action<UpgradeData> OnUpgradeApplied;
    public static Action<UpgradeType, float> OnStatUpgrade;
    public static Action<UpgradeType, float> OnWeaponUpgrade;
    public static Action<UpgradeType, float> OnGameSystemUpgrade;

    // ===== COMBAT EVENTS =====
    // public static Action<WeaponType> OnWeaponChanged;
    public static Action OnObstacleHit;
    /// <summary>
    /// Damage the obstacle inflicts
    /// </summary>
    public static Action OnObstacleDodged;

    // ===== ENEMY EVENTS =====
    // public static Action<EnemyType> OnEnemySpawn;
    public static Action<GameObject> OnEnemyDead;
    public static Action<float> OnEnemyTakeDamage;
    public static Action OnEnemyGotPastPlayer;

    // ===== ENVIRONMENT EVENTS =====
    public static Action<Terrain> GenerationTerrainFinished;
}