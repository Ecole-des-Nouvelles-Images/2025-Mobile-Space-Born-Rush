using System;
using UnityEngine;

public class DestroyHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        EventBus.OnPlayerDead += PlayerDead;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerDead -= PlayerDead;
    }

    private void PlayerDead()
    {
        Destroy(gameObject);
    }
}
