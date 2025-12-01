using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.TouchPhase;

public class PlayerController : MonoBehaviour,IDamageable
{
    //Data variables
    private Rigidbody _rb;
    private BoxCollider _col;
    [SerializeField] private LaneProfile _laneProfile;
    private WeaponData _currentweaponData;
    
    //Variables for swipe detection
    private TouchControls _inputActions;
    private Vector2 _touchStartPosition;
    private bool _fingerDown;
    //Legacy    
    private Vector2 _pos1;
    
    public int PixelDistToDetect = 200;
    
    //Player Variables
    [SerializeField] private int _currentLaneIndex;
    
    //Variables for methods
    private bool _isJumping = true;
    [SerializeField] private bool _isSliding;
    private bool _isPaused;
    
    public Vector3 JumpForce = Vector3.up;
    public int SlideAmount;
    
    #region subscriptions
    private void Awake()
    {
        _inputActions = new TouchControls();
        
        // Game State Events
        EventBus.OnGameStart += OnGameStart;
        EventBus.OnGamePause += OnGamePause;
        EventBus.OnGameResume += OnGameResume;
        EventBus.OnGameOver += OnGameOver;
        EventBus.OnGameRestart += OnGameRestart;
        
        // Player Specific Events
        EventBus.OnPlayerSpawn += OnPlayerSpawn;
        EventBus.OnPlayerRevive += OnPlayerRevive;
        
        // Movement & Environment
        EventBus.OnLaneChanged += OnLaneChanged;
        EventBus.OnSpeedChanged += OnSpeedChanged;
        
    }

    private void OnPlayerRevive()
    {
        throw new NotImplementedException();
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
    private void OnPlayerSpawn()
    {
        throw new NotImplementedException();
    }

    private void OnGameResume()
    {
        _isPaused  = false;
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
        _isPaused =  true;
    }

    private void OnGameStart()
    {
        throw new NotImplementedException();
    }

    private void OnDestroy()
    {
        DisableGameplayInput();
        _inputActions?.Dispose();
        
        // Game State Events
        EventBus.OnGameStart -= OnGameStart;
        EventBus.OnGamePause -= OnGamePause;
        EventBus.OnGameResume -= OnGameResume;
        EventBus.OnGameOver -= OnGameOver;
        EventBus.OnGameRestart -= OnGameRestart;
        
        // Player Specific Events
        EventBus.OnPlayerSpawn -= OnPlayerSpawn;
        EventBus.OnPlayerRevive -= OnPlayerRevive;
        
        // Movement & Environment
        EventBus.OnLaneChanged -= OnLaneChanged;
        EventBus.OnSpeedChanged -= OnSpeedChanged;
    }

    #endregion
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<BoxCollider>();
        EnableGameplayInput();

        // Animator animator = GetComponent<Animator>();
        // animator.speed = 2;

        _currentLaneIndex = 1;
    }
    
    //This method is called at the start and end of any animation to stop the player from being able to double jump or slide
    public bool MovementStateChange(bool state)
    {
        return !state;
    }
    private void Jump()
    {
        _rb.AddForce(JumpForce, ForceMode.Impulse);
        
    }
    
    //Sliding will basically be just the players hitbox getting smaller
    //And when the end of the animation plays it should call for the collider to resize
    private void Slide()
    {
        _isSliding = MovementStateChange(_isSliding);
        _col.size /= SlideAmount;
        _col.center /= SlideAmount;
    }

    public void StopSlide()
    {
        _isSliding = MovementStateChange(_isSliding);
        _col.size *= SlideAmount;
        _col.center *= SlideAmount;
    }
    
    #region Inputs
      private void EnableGameplayInput()
    {
        _inputActions.Player.Enable();
        
        _inputActions.Player.TouchContact.started += OnTouchStarted;
        _inputActions.Player.TouchContact.canceled += OnTouchEnded;
    }

    private void DisableGameplayInput()
    {
        _inputActions.Player.Disable();
        
        _inputActions.Player.TouchContact.started -= OnTouchStarted;
        _inputActions.Player.TouchContact.canceled -= OnTouchEnded;
    }

    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        if (_isPaused) return;
        
        _touchStartPosition = _inputActions.Player.TouchPosition.ReadValue<Vector2>();
        _fingerDown = true;
    }

    private void OnTouchEnded(InputAction.CallbackContext context)
    {
        if (_isPaused || !_fingerDown) return;
        
        Vector2 touchEndPosition = _inputActions.Player.TouchPosition.ReadValue<Vector2>();
        ProcessSwipe(_touchStartPosition, touchEndPosition);
        
        _fingerDown = false;
    }

    private void ProcessSwipe(Vector2 startPos, Vector2 endPos)
    {
        Vector2 swipeDelta = endPos - startPos;
        
        // Your existing swipe processing logic goes here
        if (swipeDelta.magnitude < PixelDistToDetect) return;
        
        // Rest of your swipe logic from before...
        bool isHorizontalSwipe = Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y);
        
        if (isHorizontalSwipe)
        {
            // Your lane switching code
            if (swipeDelta.x > 0)
            {
                _currentLaneIndex = _laneProfile.GetLaneIndex(_currentLaneIndex + 1);
            }
            else
            {
                _currentLaneIndex = _laneProfile.GetLaneIndex(_currentLaneIndex - 1);
            }
            
            Vector3 targetPos = _laneProfile.GetLanePosition(_currentLaneIndex);
            var position = transform.position;
            position.x = targetPos.x;
            transform.position = position;
        }
        else
        {
            // Your jump/slide code
            if (swipeDelta.y > 0 && !_isJumping)
            {
                Jump();
            }
            else if (swipeDelta.y < 0 && !_isSliding)
            {
                Slide();
            }
        }
    }
    #endregion
    
    //The logic to stop the player from double jumping or sliding while jumping
    private void OnCollisionEnter(Collision other)
    {
        switch (other.gameObject.tag)
        {
            case "Platform":
                _isJumping = MovementStateChange(_isJumping);
                break;
        }
    }
    private void OnCollisionExit(Collision other)
    {
        switch (other.gameObject.tag)
        {
            case "Platform":
                _isJumping = MovementStateChange(_isJumping);
                break;
        }
    }
    public void TakeDamage(float damage)
    {
        EventBus.OnPlayerTakeDamage?.Invoke(damage);
    }
}
