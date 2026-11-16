using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerController : MonoBehaviour, IDamageable
{
    // Data variables
    private Rigidbody _rb;
    private BoxCollider _col;

    [Header("IK Movement")]
    [SerializeField] private GameObject _weapons;
    [SerializeField] private Rig _rHand;
    [SerializeField] private Rig _lHand;

    [Header("Tweaks")]
    [SerializeField] private int _pixelDistToDetect = 20;
    [SerializeField] private int _colliderSize;
    [SerializeField] private Vector3 _jumpForce = Vector3.up;

    // Collider variables
    private Vector3 _originalColliderSize;
    private Vector3 _originalColliderCenter;

    [Header("State")]
    [SerializeField] private bool _isJumping;
    [SerializeField] private bool _isSliding;
    [SerializeField] private bool _isPaused;

    public bool IsPaused => _isPaused;

    [Header("Lane Movement")]
    [SerializeField] private float _laneChangeSpeed = 10f;
    private bool _isChangingLane;
    private Coroutine _laneChangeCoroutine;

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

        // SAFE NULL CHECKS
        if (_gameUpgradeController != null)
        {
            _currentBlendSpeed = _gameUpgradeController.GameSpeed;
        }

        if (_animator != null)
        {
            _animator.SetFloat("Blend", _currentBlendSpeed);
        }
    }
    

    private void OnPlayerDead()
    {
        _audioSource.PlayOneShot(_deathSound);
        Destroy(gameObject);
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

        _originalColliderSize = _col.size;
        _originalColliderCenter = _col.center;

        if (_animator) _animator = GetComponent<Animator>();

        _isJumping = true;
        _isPaused = false;
        _currentLaneIndex = 1;

        Vector3 targetPos = _laneProfile.GetLanePosition(_currentLaneIndex);
        Vector3 position = transform.position;
        position.x = targetPos.x;
        transform.position = position;
    }

    private void OnGameResume() => _isPaused = false;
    private void OnGamePause() => _isPaused = true;

    private void OnGameOver()
    {
        if (_isSliding) StopSlide();
        ResetCollider();
        _isPaused = true;
        Destroy(this.gameObject);
    }

    private void OnGameStart() { }

    private void OnDisable()
    {
        EventBus.OnGameStart -= OnGameStart;
        EventBus.OnGamePause -= OnGamePause;
        EventBus.OnGameResume -= OnGameResume;
        EventBus.OnGameOver -= OnGameOver;

        EventBus.OnPlayerSpawn -= OnPlayerSpawn;
        EventBus.OnPlayerTakeDamage -= OnPlayerTakeDamage;
        EventBus.OnPlayerCurrentXPChange -= OnPlayerCurrentXpChange;
        EventBus.OnPlayerXPToLevelUpChange -= OnPlayerXpToLevelUpChange;
        EventBus.OnUpgradeApplied -= OnUpgradeApplied;
        EventBus.OnPlayerDead -= OnPlayerDead;
    }
    #endregion

    // ============ Extracted Gameplay Actions ============

    public void TryMoveLane(int direction)
    {
        int targetLane = _currentLaneIndex + direction;
        targetLane = _laneProfile.GetLaneIndex(targetLane);

        if (targetLane == _currentLaneIndex) return;

        if (IsInNeutralState && _animator)
        {
            _animator.SetTrigger(direction > 0 ? "isMoveRight" : "isMoveLeft");
        }

        _currentLaneIndex = targetLane;

        if (_laneChangeCoroutine != null) StopCoroutine(_laneChangeCoroutine);
        _laneChangeCoroutine = StartCoroutine(MoveToLane(_currentLaneIndex));
    }

    public void TryJump()
    {
        if (_isJumping) return;
        if (_isSliding) StopSlide();
        Jump();
    }

    public void TrySlide()
    {
        // Quirk #2: Slide is allowed while airborne, so no _isJumping check.
        if (_isSliding) return;
        Slide();
    }

    // ====================================================

    private void ShrinkCollider()
    {
        _col.size = _originalColliderSize / _colliderSize;
        _col.center = _originalColliderCenter / _colliderSize;
    }

    private void ResetCollider()
    {
        _col.size = _originalColliderSize;
        _col.center = _originalColliderCenter;
    }

    private void HideIK()
    {
        Renderer[] allRenderers = _weapons.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allRenderers) r.enabled = false;
        _rHand.weight = 0f;
        _lHand.weight = 0f;
    }

    private void ShowIK()
    {
        Renderer[] allRenderers = _weapons.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allRenderers) r.enabled = true;
        _rHand.weight = 1f;
        _lHand.weight = 1f;
    }

    private void Jump()
    {
        if (_animator) _animator.SetTrigger("isJump");
        _rb.AddForce(_jumpForce, ForceMode.Impulse);
        HideIK();
    }

    private void Slide()
    {
        if (_animator) _animator.SetTrigger("isSlide");
        _isSliding = true;
        ShrinkCollider();
        _rb.AddForce(-_jumpForce, ForceMode.Impulse);
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
        if (!_isSliding) return;
        _isSliding = false;
        ResetCollider();
        if (!_isJumping && !_isPaused) ShowIK();
    }

    private void Update()
    {
        if (_animator != null)
        {
            _animator.SetBool("isNeutral", IsInNeutralState);
        }

        if (_gameUpgradeController == null) return;

        if (_currentBlendSpeed == _gameUpgradeController.GameSpeed) return;

        _currentBlendSpeed = _gameUpgradeController.GameSpeed;
        
        if (_animator != null)
        {
            _animator.SetFloat("Blend", _currentBlendSpeed);
        }
    }

    private IEnumerator MoveToLane(int targetLaneIndex)
    {
        _isChangingLane = true;
        float startX = transform.position.x;
        float targetX = _laneProfile.GetLanePosition(targetLaneIndex).x;
        float elapsedTime = 0f;
        float distance = Mathf.Abs(targetX - startX);
        float duration = distance / _laneChangeSpeed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float smoothedT = Mathf.SmoothStep(0f, 1f, t);
            float newX = Mathf.Lerp(startX, targetX, smoothedT);

            Vector3 newPos = transform.position;
            newPos.x = newX;
            transform.position = newPos;
            yield return null;
        }

        Vector3 finalPos = transform.position;
        finalPos.x = targetX;
        transform.position = finalPos;

        _isChangingLane = false;
        _laneChangeCoroutine = null;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            _isJumping = false;
            if (!_isSliding) ShowIK();
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            _isJumping = true;
        }
    }

    public void TakeDamage(float damage)
    {
       EventBus.OnPlayerTakeDamage?.Invoke(damage);
    }
}