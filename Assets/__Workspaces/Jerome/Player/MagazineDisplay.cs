using System;
using UnityEngine;
using UnityEngine.UI;

public class MagazineDisplay : MonoBehaviour
{
    [SerializeField] private Image _magImg;
    [SerializeField] private WeaponHandler _weaponManager;

    private float _fillLevel;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        EventBus.OnPlayerSpawn += OnPlayerSpawn;
    }

    private void OnPlayerSpawn()
    {
        if (!_weaponManager)
            _weaponManager = FindAnyObjectByType<WeaponHandler>();
    }

    private void OnDisable()
    {
        EventBus.OnPlayerSpawn -= OnPlayerSpawn;
    }
    
    // Update is called once per frame
    void Update()
    {
        _fillLevel = (float)_weaponManager.AmmoNumber / (float)_weaponManager.CurrentWeapon.MagSize;
        _magImg.fillAmount = _fillLevel;
    }
}
