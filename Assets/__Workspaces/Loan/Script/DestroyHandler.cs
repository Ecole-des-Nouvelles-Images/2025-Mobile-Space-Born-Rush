using System;
using UnityEngine;

public class DestroyHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        EventBus.OnPlayerDead += PlayerDead;
        EventBus.OnGameStart += GameStart;
        
    }

    private void OnDisable()
    {
        EventBus.OnPlayerDead -= PlayerDead;
        EventBus.OnGameStart -= GameStart;
    }

    private void GameStart()
    {
        this.gameObject.SetActive(true);
    }

    private void PlayerDead()
    {
        this.gameObject.SetActive(false);
    }
}
