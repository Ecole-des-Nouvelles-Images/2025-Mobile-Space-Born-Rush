using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Data variables
    private Rigidbody _rb;
    private BoxCollider _col;
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private LaneProfile _laneProfile;
    
    //Variables for swipe detection
    private Vector2 _pos1;
    private bool _fingerDown;
    
    public int PixelDistToDetect = 200;
    
    //Player Variables
    [SerializeField] private float _maxHealth, _currentHealth, _hpDropMultiplier, _currentXP, _xpToLevelUp, _xpMultiplier;
    [SerializeField] private int _currentLaneIndex;
    
    //Variables for methods
    private bool _isJumping = true;
    private bool _isSliding = true;
    
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
        EventBus.OnPlayerLevelUp += OnPlayerLevelUp;
        EventBus.OnPlayerChooseAnItem += OnPlayerChooseAnItem;
        
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

    private void OnPlayerChooseAnItem(UpgradeType obj)
    {
        throw new NotImplementedException();
    }

    private void OnPlayerLevelUp(List<UpgradeData> obj)
    {
        Debug.Log("PlayerLevelUp");
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
            // Get upgrades and trigger level up UI
            var upgradeGenerator = FindObjectOfType<UpgradeGenerator>();
            var choices = upgradeGenerator.GenerateUpgradeChoices();
            EventBus.OnPlayerLevelUp?.Invoke(choices);
            
            // Reset XP for next level
            _currentXP = 0;
            EventBus.OnPlayerXPToLevelUpChange?.Invoke(_xpToLevelUp * 1.2f);
        }
    }
    private void OnPlayerXPToLevelUpChange(float val)
    {
        _xpToLevelUp = val;
    }

    private void OnPlayerCurrentHealthChange(float val)
    {
        _currentHealth += val;
        
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
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
        
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

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
        _col = GetComponent<BoxCollider>();

        // Animator animator = GetComponent<Animator>();
        // animator.speed = 2;

        _maxHealth = _playerData.BaseMaxHealth;
        _currentHealth = _playerData.BaseMaxHealth;
        _hpDropMultiplier = _playerData.BaseHPDropMultiplier;
        _currentXP = 0;
        _xpToLevelUp = _playerData.BaseXPToLevelUp;
        _xpMultiplier = _playerData.BaseXPMultiplier;

        _currentLaneIndex = 1;
    }
    
    //This method is called at the start and end of any animation to stop the player from being able to double jump or slide
    public bool MovementStateChange(bool state)
    {
        return state = !state;
    }
    private void Jump()
    {
        _rb.AddForce(JumpForce, ForceMode.Impulse);
        
    }
    
    //Sliding will basically be just the players hitbox getting smaller
    //And when the end of the animation plays it should call for the collider to resize
    private void Slide()
    {
        MovementStateChange(_isSliding);
        _col.size /= SlideAmount;
        _col.center /= SlideAmount;
    }

    public void StopSlide()
    {
        MovementStateChange(_isSliding);
        _col.size *= SlideAmount;
        _col.center *= SlideAmount;
    }

    private void Update()
    {
        if (!_fingerDown && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            _pos1 = Input.touches[0].position;
            _fingerDown = true;
        }

        if (_fingerDown)
        {
            if (!_isJumping) 
            {
                if (Input.touches[0].position.y >= _pos1.y + PixelDistToDetect)
                {
                    Jump();
                    Debug.Log("Swipe Up");
                    _fingerDown = false;
                }
            }
            else if(!_isSliding)
            {
                if (Input.touches[0].position.y <= _pos1.y - PixelDistToDetect)
                {
                    Slide();
                    Debug.Log("Swipe Down");
                    _fingerDown = false;
                }
            }
            else
            {
                _fingerDown = false;
            }

            if (Input.touches[0].position.x >= _pos1.x + PixelDistToDetect)
            {
                _currentLaneIndex = _laneProfile.GetLaneIndex(_currentLaneIndex + 1);
                Vector3 targetPos =_laneProfile.GetLanePosition(_currentLaneIndex);
                var vector3 = transform.position;
                vector3.x = targetPos.x;
                transform.position = vector3;
                Debug.Log("Swipe Right");   
                _fingerDown = false;
            }

            if (Input.touches[0].position.x <= _pos1.x - PixelDistToDetect)
            {
                _currentLaneIndex = _laneProfile.GetLaneIndex(_currentLaneIndex - 1);
                Vector3 targetPos =_laneProfile.GetLanePosition(_currentLaneIndex);
                var vector3 = transform.position;
                vector3.x = targetPos.x;
                transform.position = vector3;
                Debug.Log("Swipe Left");
                _fingerDown = false;
            }
            
            
        }

        //TODO Delete
        if (Input.GetButtonDown("Fire2"))
        {
            StopSlide();
        }

        if (Input.GetButtonDown("Jump"))
        {
            EventBus.OnPlayerCurrentXPChange?.Invoke(100);
        }
    }

    
    //The logic to stop the player from double jumping or sliding while jumping
    private void OnCollisionEnter(Collision other)
    {
        if(other.collider.tag == "Platform")
        {
            MovementStateChange(_isJumping);
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if(other.collider.tag == "Platform")
        {
            MovementStateChange(_isJumping);
        }
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     throw new NotImplementedException();
    // }
}
