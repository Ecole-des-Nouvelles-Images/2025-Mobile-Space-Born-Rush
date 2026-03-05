using System;
using UnityEngine;
using UnityEngine.UI;

public class MagazineDisplay : MonoBehaviour
{
    [SerializeField] private Image _magImgL;
    [SerializeField] private Image _magImgR;
    [SerializeField] private WeaponHandler _weaponManager;
    [SerializeField] private GameObject _player;

    private float _fillLevel;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        EventBus.OnPlayerSpawn += OnPlayerSpawn;
        EventBus.OnWeaponSwap += SwapWeapon;
    }

    private void OnPlayerSpawn()
    {
        if (!_weaponManager)
            _weaponManager = FindAnyObjectByType<WeaponHandler>();
        
        _player = FindObjectOfType<PlayerController>().gameObject;
    }

    private void SwapWeapon()
    {
        _weaponManager = _player.GetComponentInChildren<WeaponHandler>();
    }

    private void OnDisable()
    {
        EventBus.OnPlayerSpawn -= OnPlayerSpawn;
        EventBus.OnWeaponSwap -= SwapWeapon;
    }
    
    // Update is called once per frame
    void Update()
    {
        _fillLevel = (float)_weaponManager.AmmoNumber / (float)_weaponManager.CurrentWeapon.MagSize;
        _magImgL.fillAmount = _fillLevel;
        _magImgR.fillAmount = _fillLevel;
    }
}
