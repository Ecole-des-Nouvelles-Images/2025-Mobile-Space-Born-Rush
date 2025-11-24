using System;
using UnityEngine;

public class CameraOffset : MonoBehaviour
{
  [SerializeField] private Transform _player;

  private void Start()
  {
    _player = _player.GetComponent<Transform>();
  }

  private void Update()
  {
    Vector3 pos = new Vector3(_player.position.x, transform.position.y, _player.position.z);
    transform.position = pos;
  }
}
