using System.Collections;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [SerializeField] private WeaponData _weaponData;
    [SerializeField] private WeaponUpgradeController _weaponUpgrades;
    [SerializeField] private Transform _firePoint ;
    private WeaponData _currentWeapon ;
    private BulletData _currentBullet ;
    private Ray _weaponRaycast;
    [SerializeField] private float _currentFireRate;
    [SerializeField] private float _currentCooldown;
    [SerializeField] private float _overheatCooldown;
    [SerializeField] private bool _isOverheating;
    [SerializeField] private int _ammoNumber;
    
    [Header("Enemy Charge Settings")]
    [SerializeField] private bool _isCharging = false;
    [SerializeField] private ParticleSystem _chargeParticles; //If enemy has different effects and sounds 
    [SerializeField] private AudioSource _chargeSound;
    
    [SerializeField] private bool _isRaycastDraw;
    
    [SerializeField] private ParticleSystem _fireParticles;
    private Coroutine _stunCoroutine = null;
    private bool _isStunned;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _audioClip;
    private bool _isAim;
    private Material _material;
    private float _toRougeValue;
    private bool _isPause;
    
    private void Awake()
    {
        _material = GetComponent<Renderer>().material;
    }

    private void Start()
    {
        _animator = GetComponentInParent<Animator>();
        _firePoint.GetComponent<Transform>();
    }

    private void OnEnable()
    {
        //Subs need to be OnEnable since we Enable and disable weapons throughout the game
        EventBus.OnPlayerTakeStun += OnPlayerTakeStun;
        EventBus.OnGameStart += GameStart;
        EventBus.OnGamePause += GamePause;
        EventBus.OnGameResume += GameResume;
        
        _currentWeapon = _weaponData;
        _currentBullet = _currentWeapon.BulletData;
        _currentCooldown = _currentWeapon.Cooldown;
        _ammoNumber = _currentWeapon.MagSize;
        _layerMask = _currentWeapon.EnemyMask;
        _overheatCooldown = 0;
        _isOverheating = false;
        _weaponRaycast = new Ray(_firePoint.position, _firePoint.forward);
        
        _weaponUpgrades = FindAnyObjectByType<WeaponUpgradeController>();
    }

    private void GameStart()
    {
        _isPause = false;
        _currentWeapon = _weaponData;
        _currentBullet = _currentWeapon.BulletData;
        _currentCooldown = _currentWeapon.Cooldown;
        _ammoNumber = _currentWeapon.MagSize;
        _layerMask = _currentWeapon.EnemyMask;
        _weaponRaycast = new Ray(_firePoint.position, _firePoint.forward);
        
        _weaponUpgrades = FindAnyObjectByType<WeaponUpgradeController>();
    }

    private void GamePause()
    {
        _isPause = true;
    }

    private void GameResume()
    {
        _isPause = false;
    }

    private void OnPlayerTakeStun(float time)
    {
        if (_stunCoroutine != null)
        {
            StopCoroutine(_stunCoroutine);
        }

        _stunCoroutine = StartCoroutine(AsyncStun(time));
    }

    private void OnDisable()
    {
        EventBus.OnPlayerTakeStun -= OnPlayerTakeStun;
        EventBus.OnGameStart -= GameStart;
        EventBus.OnGamePause -= GamePause;
        EventBus.OnGameResume -= GameResume;
    }

    private void Update()
    {
        if (_isRaycastDraw) Debug.DrawRay(_weaponRaycast.origin,
            _weaponRaycast.direction * _currentWeapon.FireDistance, Color.red);
        
        if (_isPause)
            return;
        
        _isAim = false;
        if (_isStunned) return;
        
        if (_currentFireRate <= 0 && !_isOverheating) // We can fire
        {
            _weaponRaycast = new Ray(_firePoint.position, _firePoint.forward);
            
            if (Physics.Raycast(_weaponRaycast,out RaycastHit hit,_currentWeapon.FireDistance,_layerMask)) // If hit
            {
                if (_currentWeapon.IsPlayer)
                {
                    if (hit.collider.TryGetComponent<EnemyHandler>(out var enemy)) 
                    {
                        if (_animator != null)
                        {
                            _animator.SetTrigger("isFire");
                            _isAim = true;
                        }
                        PlayerFire(enemy);
                    }
                }
                else // Enemy Fire
                {
                    if (hit.collider.TryGetComponent<EnemyHandler>(out var enemy)) return; 
                    
                    if (hit.collider.TryGetComponent<PlayerController>(out var player)) 
                    {
                        if (_isCharging && _currentFireRate >= _currentWeapon.FireRate) EnemyFire(player);
                        if (_isCharging && _currentFireRate < _currentWeapon.FireRate) _currentFireRate -= Time.deltaTime;
                        if (!_isCharging && _currentFireRate < _currentWeapon.FireRate) _isCharging = true;
                    }
                }
                
            }
            else
            {
                if (_ammoNumber <= 0 && !_isOverheating) Overheat();
                if (_currentCooldown > 0 && !_isOverheating && 
                    _ammoNumber > 0 && 
                    _ammoNumber < (_currentWeapon.MagSize + _weaponUpgrades.MagSize)) _currentCooldown -= Time.deltaTime;
                if (_currentCooldown <= 0 && !_isOverheating) Reload();
            }
        }
        else
        {
            _currentFireRate -= Time.deltaTime;
            if (_ammoNumber <= 0) _overheatCooldown -= Time.deltaTime;
            if (_ammoNumber <= 0 && _isOverheating && _overheatCooldown <= 0)
            {
                Reload((int)(_currentWeapon.MagSize + _weaponUpgrades.MagSize));
                _isOverheating = false;
            }
        }
    }
    private IEnumerator AsyncStun(float time)
    {
        _isStunned = true;
        
        yield return new WaitForSeconds(time);
        _isStunned = false;
        
        _stunCoroutine = null;
    }
    private void PlayerFire(EnemyHandler enemy)
    {
        // Player firing at nothing
        if (!enemy) return;
    
        // Apply bullet damage/effect to enemy
        _currentBullet.OnHitEnemy(enemy, enemy);
    
        // Visual and audio feedback
        _fireParticles.Play();
        if (_audioSource && _audioClip) _audioSource.PlayOneShot(_audioClip);
    
        // Animation
        if (_animator)
        {
            _animator.SetBool("isFire", true);
            _isAim = true;
        }
    
        // Weapon cooldown and ammo
        _currentFireRate = _currentWeapon.FireRate - _weaponUpgrades.FireRate;
        _ammoNumber--;
    }

    private void EnemyFire(PlayerController player)
    {
        // Enemy firing at nothing
        if (!player) return;
    
        // Apply bullet damage/effect to player
        _currentBullet.OnHitEnemy(player);
    
        // Visual and audio feedback
        _fireParticles.Play();
        if (_audioSource && _audioClip) _audioSource.PlayOneShot(_audioClip);
        if (_animator)  _animator.SetTrigger("isFire");
    
        // Reset charge state after firing
        _isCharging = false;
    
        // Weapon cooldown and ammo
        _currentFireRate = _currentWeapon.FireRate;
    }

    private void Reload(int amm = 1)
    {
        _currentCooldown = _currentWeapon.Cooldown - _weaponUpgrades.Cooldown;
        _ammoNumber += amm;
        
        _toRougeValue = 0;
        _material.SetFloat("_ToRouge", _toRougeValue);
    }

    private void Overheat()
    {
        // If its positive then the weapon is Overheating
        _overheatCooldown = (_currentWeapon.Cooldown - _weaponUpgrades.Cooldown) *
                            (_currentWeapon.MagSize + _weaponUpgrades.MagSize)/2;
        _isOverheating = true;
        _toRougeValue = 1;
        _material.SetFloat("_ToRouge", _toRougeValue);
    }
    
    
}
