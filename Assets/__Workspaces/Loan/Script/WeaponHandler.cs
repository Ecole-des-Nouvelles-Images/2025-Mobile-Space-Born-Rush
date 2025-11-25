using System;
using UnityEngine;
using UnityEngine.Serialization;

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
    [SerializeField] private LayerMask _enemyMask;
    
    [SerializeField] private bool _isRaycastDraw;
    

    private void Start()
    {
        _currentWeapon = _weaponData;
        _ammoNumber = _currentWeapon.MagSize;
        _firePoint.GetComponent<Transform>();
        _weaponRaycast = new Ray(_firePoint.position, _firePoint.forward);
        
        _weaponUpgrades = FindAnyObjectByType<WeaponUpgradeController>();
    }

    private void Update()
    {
        if (_currentFireRate <= 0 && _currentCooldown <= 0 )
        {
            _weaponRaycast = new Ray(_firePoint.position, _firePoint.forward);
            if (Physics.Raycast(_weaponRaycast,out RaycastHit hit,_currentWeapon.FireDistance,_currentWeapon.EnemyMask))
            {
                if (hit.collider.TryGetComponent<EnemyHandler>(out var enemy))
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
            _currentFireRate = _currentWeapon.FireRate;
            currentAmmo.GetComponent<BulletHandler>().Setup(_currentWeapon.BulletData);
            _currentFireRate = _currentWeapon.FireRate - _weaponUpgrades.FireRate;
            _ammoNumber--;
        }
    }
    
}
