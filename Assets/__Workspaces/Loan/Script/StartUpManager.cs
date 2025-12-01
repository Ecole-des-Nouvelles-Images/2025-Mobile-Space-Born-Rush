using System;
using UnityEngine;

public class StartUpManager : MonoBehaviour
{

    [SerializeField] private GameObject _modulPrefb;
    [SerializeField] private GameObject _playerPrefb;
    [SerializeField] private Transform _parentTransform;

    private void Awake()
    {
        EventBus.OnGameStart += StartGame; 
    }

    private void OnDestroy()
    {
        EventBus.OnGameStart -= StartGame;
        
    }

    private void StartGame()
    {
        Debug.Log("Hello World!");
        int pos = 0;
        GameObject player = Instantiate(_playerPrefb,new Vector3(0,0,pos),Quaternion.identity,_parentTransform);

        while (pos <= 60)
        {
            GameObject modul = Instantiate(_modulPrefb,new Vector3(0,0,pos),Quaternion.identity,_parentTransform);
            var mooveModul = modul.GetComponent<ModulMove>();
            mooveModul.isObstacle = false;
            mooveModul.isEnemy = false;
            pos += 20;
        }
    }
}
