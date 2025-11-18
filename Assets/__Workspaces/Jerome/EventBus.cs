using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus
{
    // Game
    public static Action OnGameStart;
    public static Action OnGamePause;
    public static Action OnGameResume;
    public static Action OnGameOver;
    public static Action OnGameRestart;
    public static Action<float> OnDistanceRun;
    public static Action<int> OnScoreChanged;
    public static Action<float> OnSpeedChanged;
    
    // Player
    public static Action OnPlayerSpawn;
    public static Action OnPlayerDead;
    public static Action OnPlayerRevive;
    public static Action<float, bool> OnPlayerTakeDamage;
    // public static Action<PowerUpType> OnPowerUpCollected;
    // public static Action<List<Item>> OnPlayerLevelUp;
    // public static Action<Item> OnPlayerChooseAnItem;
    public static Action<float> OnPlayerCurrentHealthChange;
    public static Action<float> OnPlayerMaxHealthChange;
    public static Action<float> OnPlayerCurrentXPChange;
    public static Action<float> OnPlayerXPToLevelUpChange;
    public static Action<int> OnLaneChanged;

    // Combat
    // public static Action<WeaponType> OnWeaponChanged;
    public static Action<float> OnObstacleHit;
    /// <summary>
    /// Damage the obstacle inflicts
    /// </summary>
    public static Action OnObstacleDodged;

    // Enemy
    // public static Action<EnemyType> OnEnemySpawn;
    // public static Action<GameObject, EnemyType> OnEnemyDead;
    public static Action<float> OnEnemyTakeDamage;

    // Environment
    public static Action<Terrain> GenerationTerrainFinished;
}