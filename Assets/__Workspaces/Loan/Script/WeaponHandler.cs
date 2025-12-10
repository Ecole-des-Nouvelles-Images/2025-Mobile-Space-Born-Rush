using System;
using System.Collections;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [SerializeField]private WeaponData _weaponData;
    [SerializeField]private WeaponUpgradeController _weaponUpgrades;
    [SerializeField]private Transform _firePoint ;
    private WeaponData _currentWeapon ;
    private BulletData _currentBullet ;
    private Ray _weaponRaycast;
    [SerializeField]private float _currentFireRate;
    [SerializeField]private float _currentCooldown;
    [SerializeField] private int _ammoNumber;
    
    [SerializeField] private bool _isRaycastDraw;
    
    [SerializeField] private ParticleSystem _fireParticles;
    
    private Coroutine _stunCoroutine = null;
    private bool _isStunned;
    [SerializeField]private LayerMask _layerMask;
    private Animator _animator;
    private bool _isAim;
    private Material _material;
    private float _toRougeValue;

    private void Awake()
    {
        EventBus.OnPlayerTakeStun += OnPlayerTakeStun;
        EventBus.OnGameStart += GameStart;
        _material = GetComponent<Renderer>().material;
    }

    private void Start()
    {
        _animator = GetComponentInParent<Animator>();
        _firePoint.GetComponent<Transform>();
    }

    private void OnEnable()
    {
        _currentWeapon = _weaponData;
        _currentBullet = _currentWeapon.BulletData;
        _ammoNumber = _currentWeapon.MagSize;
        _layerMask = _currentWeapon.EnemyMask;
        _weaponRaycast = new Ray(_firePoint.position, _firePoint.forward);
        
        _weaponUpgrades = FindAnyObjectByType<WeaponUpgradeController>();
    }

    private void GameStart()
    {
        _currentWeapon = _weaponData;
        _currentBullet = _currentWeapon.BulletData;
        _ammoNumber = _currentWeapon.MagSize;
        _layerMask = _currentWeapon.EnemyMask;
        _weaponRaycast = new Ray(_firePoint.position, _firePoint.forward);
        
        _weaponUpgrades = FindAnyObjectByType<WeaponUpgradeController>();
    }

    private void OnPlayerTakeStun(float time)
    {
        if (_stunCoroutine != null)
        {
            StopCoroutine(_stunCoroutine);
        }

        _stunCoroutine = StartCoroutine(AsyncStun(time));
    }

    private void OnDestroy()
    {
        EventBus.OnPlayerTakeStun -= OnPlayerTakeStun;
        EventBus.OnGameStart -= GameStart;
    }

    private void Update()
    {
        _isAim = false;
        if (_isStunned) return;
        
        if (_currentFireRate <= 0 && _currentCooldown <= 0 )
        {
            _weaponRaycast = new Ray(_firePoint.position, _firePoint.forward);

            if (_currentBullet.IsDestroyedObstacle)
            {
                if (Physics.Raycast(_weaponRaycast,out RaycastHit hit,_currentWeapon.FireDistance,_layerMask))
                {
                    if (_currentWeapon.IsPlayer)
                    {
                        if (hit.collider.TryGetComponent<EnemyHandler>(out var enemy)) 
                        {
                            // if (_animator != null)
                            // {
                            //     _animator.SetBool("isAim",true);
                            //     _isAim = true;
                            // }
                            Fire(enemy,null);
                        }
                    }
                    else
                    {
                        if (hit.collider.TryGetComponent<EnemyHandler>(out var enemy))
                            return; 
                        if (hit.collider.TryGetComponent<PlayerController>(out var player)) 
                        { 
                            // if (_animator != null)
                            // {
                            //     _animator.SetBool("isAim",true);
                            //     _isAim = true;
                            // }
                            Fire(null,player);
                        }
                    }
                    
                }
            }
            else
            {
                if (Physics.Raycast(_weaponRaycast,out RaycastHit hit,_currentWeapon.FireDistance,_layerMask))
                { 
                    if (hit.collider.TryGetComponent<ObstacleHandler>(out var obstacle)) 
                        return;

                    if (_currentWeapon.IsPlayer)
                    {
                        if (hit.collider.TryGetComponent<EnemyHandler>(out var enemy)) 
                        {
                            // if (_animator != null)
                            // {
                            //     _animator.SetBool("isAim",true);
                            //     _isAim = true;
                            // }
                            Fire(enemy,null);
                        }
                    }
                    else
                    {
                        if (hit.collider.TryGetComponent<EnemyHandler>(out var enemy)) 
                            return;
                        if (hit.collider.TryGetComponent<PlayerController>(out var player)) 
                        { 
                            // if (_animator != null)
                            // {
                            //     _animator.SetBool("isAim",true);
                            //     _isAim = true;
                            // }
                            Fire(null,player);
                        }
                    }
                    
                }
            }
        }
        
        if (_ammoNumber <= 0)
        {
            _ammoNumber = (int)(_currentWeapon.MagSize + _weaponUpgrades.MagSize);
            _currentCooldown = _currentWeapon.Cooldown - _weaponUpgrades.Cooldown;
            _toRougeValue = 1;
        }
            
        if (_currentFireRate > 0)
        {
            _currentFireRate -= Time.deltaTime;
        }

        if (_currentCooldown > 0)
        {
            _currentCooldown -= Time.deltaTime;
            _isAim = false;
        }
        else
        {
            _toRougeValue = 0;
        }
        
        // if (_animator != null && !_isAim)
        // {
        //     _animator.SetBool("isAim",false);
        // }
        _material.SetFloat("_ToRouge", _toRougeValue);
        
        if (_isRaycastDraw)
        {
            Debug.DrawRay(_weaponRaycast.origin, _weaponRaycast.direction * _currentWeapon.FireDistance, Color.red);
        }
    }
    private IEnumerator AsyncStun(float time)
    {
        _isStunned = true;
        
        yield return new WaitForSeconds(time);
        _isStunned = false;
        
        _stunCoroutine = null;
    }
    
    public void Fire(EnemyHandler enemy, PlayerController player)
    {
        if (_ammoNumber <= 0)
        {
            _ammoNumber = (int)(_currentWeapon.MagSize + _weaponUpgrades.MagSize);
            _currentCooldown = _currentWeapon.Cooldown - _weaponUpgrades.Cooldown;
            _toRougeValue = 1;
        }
        else
        {
            // if (_animator != null)
            // {
            //     _animator.SetTrigger("isFire");
            // }

            if (enemy != null)
            {
                _currentBullet.OnHitEnemy(enemy,enemy);
                _fireParticles.Play();
            }
            else
            {
                _currentBullet.OnHitEnemy(player,null);
                _fireParticles.Play();
            }
            
            _currentFireRate = _currentWeapon.FireRate - _weaponUpgrades.FireRate;
            _ammoNumber--;
        }
    }

    
    
}
