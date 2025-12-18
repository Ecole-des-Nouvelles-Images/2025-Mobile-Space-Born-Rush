using System;
using UnityEngine;

public class CameraOffset : MonoBehaviour
{
  [SerializeField] private Transform _player;

  private void OnEnable()
  {
    EventBus.OnPlayerSpawn += OnPlayerSpawn;
  }

  private void OnPlayerSpawn()
  {
    _player = GameObject.FindGameObjectWithTag("Player").transform;
  }

  private void Update()
  {
    Vector3 pos = new Vector3(_player.position.x, transform.position.y, _player.position.z);
    transform.position = pos;
  }
}
