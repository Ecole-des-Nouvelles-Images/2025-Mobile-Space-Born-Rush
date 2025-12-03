using System.Collections;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [SerializeField]private WeaponData _weaponData;
    [SerializeField]private WeaponUpgradeController _weaponUpgrades;
    [SerializeField]private Transform _firePoint ;
    private WeaponData _currentWeapon ;
    private Ray _weaponRaycast;
    [SerializeField]private float _currentFireRate;
    [SerializeField]private float _currentCooldown;
    [SerializeField] private int _ammoNumber;
    
    [SerializeField] private bool _isRaycastDraw;
    
    private Coroutine _stunCoroutine = null;
    private bool _isStunned;
    [SerializeField]private LayerMask _layerMask;

    private void Awake()
    {
        EventBus.OnPlayerTakeStun += OnPlayerTakeStun;
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
    }

    private void Start()
    {
        _currentWeapon = _weaponData;
        _ammoNumber = _currentWeapon.MagSize;
        _layerMask = _currentWeapon.EnemyMask;
        _firePoint.GetComponent<Transform>();
        _weaponRaycast = new Ray(_firePoint.position, _firePoint.forward);
        
        _weaponUpgrades = FindAnyObjectByType<WeaponUpgradeController>();
    }

    private void Update()
    {
        if (_isStunned) return;
        if (_currentFireRate <= 0 && _currentCooldown <= 0 )
        {
            _weaponRaycast = new Ray(_firePoint.position, _firePoint.forward);
            
            
            if (Physics.Raycast(_weaponRaycast,out RaycastHit hit,_currentWeapon.FireDistance,_layerMask))
            { 
                if (hit.collider.TryGetComponent<EnemyHandler>(out var enemy)) 
                { 
                    Fire();
                }
            }
            else if (Physics.Raycast(_weaponRaycast,out RaycastHit other ,_currentWeapon.FireDistance,_layerMask))
            {
                if (other.collider.TryGetComponent<PlayerController>(out var enemy)) 
                { 
                    Fire();
                }
            }
            
            
            
        }
            
        if (_currentFireRate > 0)
        {
            _currentFireRate -= Time.deltaTime;
        }

        if (_currentCooldown > 0)
        {
            _currentCooldown -= Time.deltaTime;
        }

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
    
    public void Fire()
    {
        if (_ammoNumber <= 0)
        {
            _ammoNumber = (int)(_currentWeapon.MagSize + _weaponUpgrades.MagSize);
            _currentCooldown = _currentWeapon.Cooldown - _weaponUpgrades.Cooldown;
        }
        else
        {
            GameObject currentAmmo = Instantiate(_currentWeapon.BulletData.BulletPrefab,_firePoint.position,Quaternion.identity);
            currentAmmo.GetComponent<BulletHandler>().Setup(_currentWeapon.BulletData,_currentWeapon.EnemyMask);
            _currentFireRate = _currentWeapon.FireRate - _weaponUpgrades.FireRate;
            _ammoNumber--;
        }
    }

    
    
}
