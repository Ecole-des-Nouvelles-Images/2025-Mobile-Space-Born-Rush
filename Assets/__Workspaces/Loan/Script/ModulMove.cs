using System;
using UnityEngine;

public class ModulMove : MonoBehaviour
{
    private Vector3 _speedMovement = new Vector3(0f, 0f, -5f);
    private Rigidbody _rb;
    private float _width = 23f;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
        _rb.linearVelocity = _speedMovement;
    }

    void Update()
    {
        //Si utilisation de pull
        if (transform.position.z < -_width)
        {
            Destroy(gameObject);
        }
    }
}
