using System;
using UnityEngine;

public class SectionTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _modulPrefabs;
    private float _rotate = -90f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject modul = Instantiate(_modulPrefabs, new Vector3(0,0,60), Quaternion.identity);
        }
    }
}
