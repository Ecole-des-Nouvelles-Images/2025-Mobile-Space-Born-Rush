using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour,IDamageable
{
    //Data variables
    private Rigidbody _rb;
    private BoxCollider _col;
    
    [Header("IK Movment")]
    [SerializeField] private GameObject _weapons;
    [SerializeField] private Rig _RHand;
    [SerializeField] private Rig _LHand;
    
    //Variables for swipe detection
    private TouchControls _inputActions;
    private Vector2 _touchStartPosition;
    private bool _fingerDown;
    
    [Header("Tweaks")]
    [SerializeField] private int _pixelDistToDetect = 20;
    [SerializeField] private int _colliderSize;
    [SerializeField] private Vector3 JumpForce = Vector3.up;
    
    // Collider variables
    private Vector3 _originalColliderSize;
    private Vector3 _originalColliderCenter;
    
    [Header("state")]
    [SerializeField] private bool _isJumping;
    [SerializeField] private bool _isSliding;
    [SerializeField] private bool _isPaused;
    
    [Header("Lane Movement")]
    [SerializeField] private float _laneChangeSpeed = 10f; // How fast we move between lanes
    private bool _isChangingLane;
    private Coroutine _laneChangeCoroutine; // Reference to our running movement coroutine
    
    private bool IsInNeutralState => !_isJumping && !_isSliding && !_isChangingLane;    
    
    [Header("Player Movement")]
    [SerializeField] private LaneProfile _laneProfile;
    [SerializeField] private int _currentLaneIndex;
    
    [Header("Animator")]
    [SerializeField] private Animator _animator;
    
    [Header("Sound")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private AudioClip _deathSound;
    [SerializeField] private AudioClip _xpSound;
    [SerializeField] private AudioClip _lvlUpSound;
    [SerializeField] private AudioClip _choiceSound;
    
    [SerializeField] private ParticleSystem _xpParticles;
    
    private GameUpgradeController _gameUpgradeController;
    private float _currentBlendSpeed;
    
    #region ========== Subscriptions ==========
    private void OnEnable()
    {
        _inputActions = new TouchControls();
        _gameUpgradeController = FindAnyObjectByType<GameUpgradeController>();
        
        // Game State Events
        EventBus.OnGameStart += OnGameStart;
        EventBus.OnGamePause += OnGamePause;
        EventBus.OnGameResume += OnGameResume;
        EventBus.OnGameOver += OnGameOver;
        
        // Player Specific Events
        EventBus.OnPlayerSpawn += OnPlayerSpawn;
        EventBus.OnPlayerTakeDamage += OnPlayerTakeDamage;
        EventBus.OnPlayerCurrentXPChange += OnPlayerCurrentXpChange;
        EventBus.OnPlayerXPToLevelUpChange += OnPlayerXpToLevelUpChange;
        EventBus.OnUpgradeApplied += OnUpgradeApplied;
        EventBus.OnPlayerDead += OnPlayerDead;
        
        _currentBlendSpeed = _gameUpgradeController.GameSpeed;
        _animator.SetFloat("Blend", _currentBlendSpeed);
    }

    private void OnPlayerDead()
    {
        _audioSource.PlayOneShot(_deathSound);
        Destroy(gameObject);
        // if (_animator) _animator.SetTrigger("isDie");
    }

    private void OnUpgradeApplied(UpgradeData obj)
    {
        _audioSource.PlayOneShot(_choiceSound);
    }

    private void OnPlayerXpToLevelUpChange(float obj)
    {
        _audioSource.PlayOneShot(_lvlUpSound);
    }

    private void OnPlayerCurrentXpChange(float obj)
    {
        _audioSource.PlayOneShot(_xpSound);
        _xpParticles.Play();
    }

    private void OnPlayerTakeDamage(float obj)
    {
        _audioSource.PlayOneShot(_hitSound);
        if (_animator) _animator.SetTrigger("isHit");
        StopSlide();
    }
    private void OnPlayerSpawn()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<BoxCollider>();
        EnableGameplayInput();
        
        _originalColliderSize = _col.size;
        _originalColliderCenter = _col.center;

        if (_animator)
        {
            _animator = GetComponent<Animator>();
        }

        _isJumping = true; // Because the player is spawned in the air and land on the platform reversing this frame 1
        _isPaused = false;
        _currentLaneIndex = 1;
        
        Vector3 targetPos = _laneProfile.GetLanePosition(_currentLaneIndex);
        Vector3 position = transform.position;
        position.x = targetPos.x;
        transform.position = position;
    }

    private void OnGameResume()
    {
        _isPaused  = false;
    }

    private void OnGameOver()
    {
        if (_isSliding) StopSlide();
        
        ResetCollider();
        
        _isPaused = true;
        
        Destroy(this.gameObject);
    }

    private void OnGamePause()
    {
        _isPaused =  true;
    }

    private void OnGameStart()
    {

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
        
        // Player Specific Events
        EventBus.OnPlayerSpawn -= OnPlayerSpawn;
        EventBus.OnPlayerTakeDamage -= OnPlayerTakeDamage;
        EventBus.OnPlayerCurrentXPChange -= OnPlayerCurrentXpChange;
        EventBus.OnPlayerXPToLevelUpChange -= OnPlayerXpToLevelUpChange;
        EventBus.OnUpgradeApplied -= OnUpgradeApplied;
        EventBus.OnPlayerDead -= OnPlayerDead;
        
    }

    #endregion
    
    private void ShrinkCollider()
    {
        _col.size = _originalColliderSize/_colliderSize;
        _col.center = _originalColliderCenter/_colliderSize;
    }

    private void ResetCollider()
    {
        _col.size = _originalColliderSize;
        _col.center = _originalColliderCenter;
    }

    private void HideIK()
    {
        Renderer[] allRenderers = _weapons.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allRenderers)
        {
            r.enabled = false;
        }

        _RHand.weight = 0f;
        _LHand.weight = 0f;
    }

    private void ShowIK()
    {
        Renderer[] allRenderers = _weapons.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allRenderers)
        {
            r.enabled = true;
        }
        
        _RHand.weight = 1f;
        _LHand.weight = 1f;
    }
    private void Jump()
    {
        if (_animator)
        {
            _animator.SetTrigger("isJump");
        }
        
        _rb.AddForce(JumpForce, ForceMode.Impulse);

        HideIK();
    }
    
    //Sliding will basically be just the players hitbox getting smaller
    //And when the end of the animation plays it should call for the collider to resize
    private void Slide()
    {
        if (_animator) _animator.SetTrigger("isSlide");
        
        _isSliding = true;  // Direct assignment
        
        // Scale down from original
        ShrinkCollider();
        
        _rb.AddForce(-JumpForce, ForceMode.Impulse);
        HideIK();
    }
    
    public void StopSlide()
    {
        _isSliding = false;
        
        ResetCollider();
        ShowIK();
    }
    public void OnSlideAnimationComplete()
    {
        // This gets called whether slide finished normally OR was interrupted
        if (!_isSliding) return;
        
        _isSliding = false;
        ResetCollider();
        
        // Only show weapons if we're grounded and alive
        if (!_isJumping && !_isPaused) ShowIK();
    }

    private void Update()
    {
        _animator.SetBool("isNeutral", IsInNeutralState);

        if (_currentBlendSpeed == _gameUpgradeController.GameSpeed) return;
        
        _currentBlendSpeed = _gameUpgradeController.GameSpeed;
        _animator.SetFloat("Blend", _currentBlendSpeed);
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
        if (currentDelta.magnitude >= _pixelDistToDetect)
        {
            ProcessSwipe(_touchStartPosition, currentTouchPosition);
            
            _fingerDown = false;
        }
    }

    private void OnTouchEnded(InputAction.CallbackContext context)
    {
        if (_isPaused || !_fingerDown) return;
        
        Vector2 touchEndPosition = _inputActions.Player.TouchPosition.ReadValue<Vector2>();
        Vector2 swipeDelta = touchEndPosition - _touchStartPosition;
        
        // Check if this was just a tap (short movement)
        if (swipeDelta.magnitude < _pixelDistToDetect) return;
        
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
                // If sliding, stop the slide when jumping
                if (_isSliding) StopSlide();
                
                Jump();
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
                _isJumping = false;
                if(!_isSliding) ShowIK();
                break;
        }
    }
    private void OnCollisionExit(Collision other)
    {
        switch (other.gameObject.tag)
        {
            case "Platform":
                _isJumping = true;
                break;
        }
    }
    public void TakeDamage(float damage)
    {
       EventBus.OnPlayerTakeDamage?.Invoke(damage);
    }
}
