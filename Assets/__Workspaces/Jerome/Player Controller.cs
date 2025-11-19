using System;
using System.Threading;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class PlayerController : MonoBehaviour
{
    private Rigidbody _rb;
    private CapsuleCollider _col;
    private PlayerData _playerData;
    private bool _isMoving = false;
    
    private float _maxHealth, _currentHealth, _hpDropMultiplier, _currentXP, _xpToLevelUp, _xpMultiplier;
    
    public Vector3 JumpForce = Vector3.up;
    public int SlideAmount;
    
    #region subscriptions
    private void Awake()
    {
        // Game State Events
        EventBus.OnGameStart += OnGameStart;
        EventBus.OnGamePause += OnGamePause;
        EventBus.OnGameResume += OnGameResume;
        EventBus.OnGameOver += OnGameOver;
        EventBus.OnGameRestart += OnGameRestart;
        
        // Player Specific Events
        EventBus.OnPlayerSpawn += OnPlayerSpawn;
        EventBus.OnPlayerRevive += OnPlayerRevive;
        EventBus.OnPlayerTakeDamage += OnPlayerTakeDamage;
        EventBus.OnPlayerCurrentHealthChange += OnPlayerCurrentHealthChange;
        EventBus.OnPlayerMaxHealthChange += OnPlayerMaxHealthChange;
        EventBus.OnPlayerCurrentXPChange += OnPlayerCurrentXPChange;
        EventBus.OnPlayerXPToLevelUpChange += OnPlayerXPToLevelUpChange;
        // EventBus.OnPlayerLevelUp += OnPlayerLevelUp;
        // EventBus.OnPlayerChooseAnItem += OnPlayerChooseAnItem;
        
        // Power-up & Items
        // EventBus.OnPowerUpCollected += OnPowerUpCollected;
        
        // Combat Events
        // EventBus.OnWeaponChanged += OnWeaponChanged;
        EventBus.OnObstacleHit += OnObstacleHit;
        EventBus.OnObstacleDodged += OnObstacleDodged;
        
        // Movement & Environment
        EventBus.OnLaneChanged += OnLaneChanged;
        EventBus.OnSpeedChanged += OnSpeedChanged;
        
    }

    private void OnPlayerXPToLevelUpChange(float val)
    {
        _xpToLevelUp = val;
    }

    private void OnPlayerMaxHealthChange(float val)
    {
        //Depends on how the value is sent but this is the basic logic
        _maxHealth = val;
    }

    private void OnSpeedChanged(float val)
    {
        //Will need to speed up all the players animations with this
        throw new NotImplementedException();
    }

    private void OnLaneChanged(int val)
    {
        throw new NotImplementedException();
    }

    private void OnObstacleDodged()
    {
        throw new NotImplementedException();
    }

    private void OnObstacleHit(float val)
    {
        //The value should be how the speed is effected and for how long Unless we do a flat speed reduction
        //TODO ask Loan if its flat or a percentage of the current speed, will need to change the event based on his ans.
    }

    private void OnPlayerCurrentXPChange(float val)
    {
        _currentXP += val;

        if (_currentXP >= _xpToLevelUp)
        {
            //EventBus.OnPlayerLevelUp Invoke this
        }
    }

    private void OnPlayerCurrentHealthChange(float val)
    {
        _currentHealth += val;
        
        Mathf.Clamp(_currentHealth, 0, _maxHealth);
    }

    private void OnPlayerRevive()
    {
        _currentHealth = _maxHealth;
    }

    private void OnPlayerSpawn()
    {
        throw new NotImplementedException();
    }

    private void OnPlayerTakeDamage(float val)
    {
        _currentHealth -= val;
        
        Mathf.Clamp(_currentHealth, 0, _maxHealth);

        if (_currentHealth <= 0)
        {
            Invoke("OnPlayerDead", 0.1f);
        }
    }

    private void OnGameResume()
    {
        throw new NotImplementedException();
    }

    private void OnGameRestart()
    {
        throw new NotImplementedException();
    }

    private void OnGameOver()
    {
        throw new NotImplementedException();
    }

    private void OnGamePause()
    {
        throw new NotImplementedException();
    }

    private void OnGameStart()
    {
        throw new NotImplementedException();
    }

    private void OnDestroy()
    {
        // Game State Events
        EventBus.OnGameStart -= OnGameStart;
        EventBus.OnGamePause -= OnGamePause;
        EventBus.OnGameResume -= OnGameResume;
        EventBus.OnGameOver -= OnGameOver;
        EventBus.OnGameRestart -= OnGameRestart;
        
        // Player Specific Events
        EventBus.OnPlayerSpawn -= OnPlayerSpawn;
        EventBus.OnPlayerRevive -= OnPlayerRevive;
        EventBus.OnPlayerTakeDamage -= OnPlayerTakeDamage;
        EventBus.OnPlayerCurrentHealthChange -= OnPlayerCurrentHealthChange;
        EventBus.OnPlayerMaxHealthChange -= OnPlayerMaxHealthChange;
        EventBus.OnPlayerCurrentXPChange -= OnPlayerCurrentXPChange;
        EventBus.OnPlayerXPToLevelUpChange -= OnPlayerXPToLevelUpChange;
        // EventBus.OnPlayerLevelUp -= OnPlayerLevelUp;
        // EventBus.OnPlayerChooseAnItem -= OnPlayerChooseAnItem;
        
        // Power-up & Items
        // EventBus.OnPowerUpCollected -= OnPowerUpCollected;
        
        // Combat Events
        // EventBus.OnWeaponChanged -= OnWeaponChanged;
        EventBus.OnObstacleHit -= OnObstacleHit;
        EventBus.OnObstacleDodged -= OnObstacleDodged;
        
        // Movement & Environment
        EventBus.OnLaneChanged -= OnLaneChanged;
        EventBus.OnSpeedChanged -= OnSpeedChanged;
    }

    #endregion

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<CapsuleCollider>();

        // Animator animator = GetComponent<Animator>();
        // animator.speed = 2;

        _maxHealth = _playerData.BaseMaxHealth;
        _currentHealth = _playerData.BaseMaxHealth;
        _hpDropMultiplier = _playerData.BaseHPDropMultiplier;
        _currentXP = 0;
        _xpToLevelUp = _playerData.BaseXPToLevelUp;
        _xpMultiplier = _playerData.BaseXPMultiplier;
    }
    
    //This method is called at the start and end of any animation to stop the player from being able to double jump or slide
    public void MovementStateChange()
    {
        _isMoving = !_isMoving;
    }
    private void Jump()
    {
        _rb.AddForce(JumpForce, ForceMode.Impulse);
        
    }
    
    //Sliding will basically be just the players hitbox getting smaller
    //And when the end of the animation plays it should call for the collider to resize
    private void Slide()
    {
        _col.radius /= SlideAmount;
        _col.center /= SlideAmount;
    }

    public void StopSlide()
    {
        _col.radius *= SlideAmount;
        _col.center *= SlideAmount;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        if (Input.GetButtonDown("Fire1"))
        {
            Slide();
        }

        if (Input.GetButtonDown("Fire2"))
        {
            StopSlide();
        }
    }

}
