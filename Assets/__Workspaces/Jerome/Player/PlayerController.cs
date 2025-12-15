using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField]private bool _isJumping = false;
    [SerializeField] private bool _isSliding;
    private bool _isPaused;
    
    [Header("Lane Movement")]
    [SerializeField] private float _laneChangeSpeed = 10f; // How fast we move between lanes
    private bool _isChangingLane = false;
    private Coroutine _laneChangeCoroutine = null; // Reference to our running movement coroutine
    
    private bool IsInNeutralState => !_isJumping && !_isSliding && !_isChangingLane;    
    
    public Vector3 JumpForce = Vector3.up;
    public int SlideAmount;
    
    [SerializeField] private Animator _animator;
    
    #region subscriptions
    private void OnEnable()
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

    private void OnSpeedChanged(float val, float duration)
    {
        //Will need to speed up all the players animations with this
        
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
        Vector3 targetPos = _laneProfile.GetLanePosition(_currentLaneIndex);
        var position = transform.position;
        position.x = targetPos.x;
        transform.position = position;
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
        if (_animator)
        {
            _animator.SetTrigger("isDie");
        }

        if (_isSliding)
        {
            StopSlide();
        }
        
        _isPaused = true;
    }

    private void OnGamePause()
    {
        _isPaused =  true;
    }

    private void OnGameStart()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<BoxCollider>();
        EnableGameplayInput();

        if (_animator)
        {
            _animator = GetComponent<Animator>();
        }
        // Animator animator = GetComponent<Animator>();
        // animator.speed = 2;

        _isJumping = true;
        _isPaused = false;
        _currentLaneIndex = 1;
        
        Vector3 targetPos = _laneProfile.GetLanePosition(_currentLaneIndex);
        var position = transform.position;
        position.x = targetPos.x;
        transform.position = position;
    }

    private void OnDisable()
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
    
    //This method is called at the start and end of any animation to stop the player from being able to double jump or slide
    public bool MovementStateChange(bool state)
    {
        return !state;
    }
    private void Jump()
    {
        if (_animator)
        {
            _animator.SetTrigger("isJump");
        }
        _rb.AddForce(JumpForce, ForceMode.Impulse);
    }
    
    //Sliding will basically be just the players hitbox getting smaller
    //And when the end of the animation plays it should call for the collider to resize
    private void Slide()
    {
        if (_animator)
        {
            _animator.SetTrigger("isSlide");
        }
        _isSliding = MovementStateChange(_isSliding);
        _col.size /= SlideAmount;
        _col.center /= SlideAmount;
        
        _rb.AddForce(-JumpForce, ForceMode.Impulse);
    }

    public void StopSlide()
    {
        _isSliding = MovementStateChange(_isSliding);
        _col.size *= SlideAmount;
        _col.center *= SlideAmount;
    }
    
    #region Input System
    private void EnableGameplayInput()
    {
        _inputActions.Player.Enable();
        
        // Subscribe to touch events
        _inputActions.Player.TouchContact.started += OnTouchStarted;
        _inputActions.Player.TouchContact.canceled += OnTouchEnded;
        
        // CRITICAL: This gives us real-time touch position updates while finger is down
        _inputActions.Player.TouchPosition.performed += OnTouchPositionPerformed;
    }

    private void DisableGameplayInput()
    {
        // Unsubscribe from all events
        _inputActions.Player.TouchContact.started -= OnTouchStarted;
        _inputActions.Player.TouchContact.canceled -= OnTouchEnded;
        _inputActions.Player.TouchPosition.performed -= OnTouchPositionPerformed;
        
        _inputActions.Player.Disable();
    }

    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        if (_isPaused) return;
        
        // Record where the touch began
        _touchStartPosition = _inputActions.Player.TouchPosition.ReadValue<Vector2>();
        _fingerDown = true;
    }

    private void OnTouchPositionPerformed(InputAction.CallbackContext context)
    {
        // Only process if we're actively touching and game isn't paused
        if (_isPaused || !_fingerDown) return;
        
        Vector2 currentTouchPosition = context.ReadValue<Vector2>();
        Vector2 currentDelta = currentTouchPosition - _touchStartPosition;
        
        // Have we moved far enough to qualify as a swipe?
        if (currentDelta.magnitude >= PixelDistToDetect)
        {
            // YES! Process the swipe immediately (no waiting for finger lift)
            ProcessSwipe(_touchStartPosition, currentTouchPosition);
            
            // IMPORTANT: Prevent same swipe from triggering multiple times
            // Option 1: Reset start position (allows continuous swiping with same finger)
            // _touchStartPosition = currentTouchPosition;
            
            // Option 2: Require finger lift between swipes (recommended for runners)
            _fingerDown = false;
        }
    }

    private void OnTouchEnded(InputAction.CallbackContext context)
    {
        if (_isPaused || !_fingerDown) return;
        
        Vector2 touchEndPosition = _inputActions.Player.TouchPosition.ReadValue<Vector2>();
        Vector2 swipeDelta = touchEndPosition - _touchStartPosition;
        
        // Check if this was just a tap (short movement)
        if (swipeDelta.magnitude < PixelDistToDetect)
        {
            // Could implement tap-to-jump here if desired
            // Example: if (!_isJumping) Jump();
            return;
        }
        
        // If we haven't already processed this as a real-time swipe, process it now
        // (This is a backup for edge cases)
        ProcessSwipe(_touchStartPosition, touchEndPosition);
        
        _fingerDown = false;
    }

    private void ProcessSwipe(Vector2 startPos, Vector2 endPos)
    {
        Vector2 swipeDelta = endPos - startPos;
        
        // Determine primary swipe direction
        bool isHorizontalSwipe = Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y);
        
        if (isHorizontalSwipe)
        {
            // Calculate target lane
            int targetLane = _currentLaneIndex + (swipeDelta.x > 0 ? 1 : -1);
            targetLane = _laneProfile.GetLaneIndex(targetLane); // Clamp/validate
            
            // Don't move if we're already on that lane
            if (targetLane == _currentLaneIndex) return;
            
            // RULE 2: Allow lane changes during slide, but no animation
            // RULE 3: Only play animation from neutral state
            if (IsInNeutralState && _animator)
            {
                _animator.SetTrigger(swipeDelta.x > 0 ? "isMoveRight" : "isMoveLeft");
            }
            
            // Update current lane immediately
            _currentLaneIndex = targetLane;
            
            // Stop any existing movement coroutine
            if (_laneChangeCoroutine != null)
            {
                StopCoroutine(_laneChangeCoroutine);
            }
            
            // Start smooth movement (works whether sliding or not)
            _laneChangeCoroutine = StartCoroutine(MoveToLane(_currentLaneIndex));
        }
        else // Vertical swipe
        {
            if (swipeDelta.y > 0 && !_isJumping) // Upward swipe
            {
                Jump();
                
                // If sliding, stop the slide when jumping
                if (_isSliding) StopSlide();
            }
            else if (swipeDelta.y < 0 && !_isSliding) // Downward swipe
            {
                Slide();
            }
        }
    }
    #endregion

    private IEnumerator MoveToLane(int targetLaneIndex)
    {
        _isChangingLane = true;
    
        // Get the starting and target X positions
        float startX = transform.position.x;
        float targetX = _laneProfile.GetLanePosition(targetLaneIndex).x;
    
        float elapsedTime = 0f;
        // Calculate how long the movement should take based on distance and speed
        float distance = Mathf.Abs(targetX - startX);
        float duration = distance / _laneChangeSpeed;
    
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
        
            // Use a smoother interpolation curve (easing)
            float smoothedT = Mathf.SmoothStep(0f, 1f, t);
        
            // Calculate new X position
            float newX = Mathf.Lerp(startX, targetX, smoothedT);
        
            // Apply only to X axis
            Vector3 newPos = transform.position;
            newPos.x = newX;
            transform.position = newPos;
        
            yield return null; // Wait until next frame
        }
    
        // Ensure we snap to the exact final position
        Vector3 finalPos = transform.position;
        finalPos.x = targetX;
        transform.position = finalPos;
    
        _isChangingLane = false;
        _laneChangeCoroutine = null;
    }
    
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
        if (_animator)
        {
            _animator.SetTrigger("isHit");
        }
        EventBus.OnPlayerTakeDamage?.Invoke(damage);
    }
}
