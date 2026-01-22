using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class StartUpManager : MonoBehaviour
{

    [SerializeField] private GameObject[] _modulPrefb;
    [SerializeField] private GameObject _playerPrefb;
    [SerializeField] private Transform _parentTransform;
    [SerializeField] private Transform _parentModulTransform;

    private void OnEnable()
    {
        EventBus.OnGameStart += StartGame; 
    }

    private void OnDestroy()
    {
        EventBus.OnGameStart -= StartGame;
        
    }

    private void StartGame()
    {
        float pos = 0;
        if (_playerPrefb)
        {
            GameObject player = Instantiate(_playerPrefb,new Vector3(0,0.1f,pos),Quaternion.identity,_parentTransform);
            EventBus.OnPlayerSpawn?.Invoke();
        }
        _parentModulTransform =  GameObject.FindGameObjectWithTag("ParentsModul").transform;
        
        while (pos <= 60)
        {
            //TODO Add a portion for when its the tutorial
            //Tutorial will be luch longer It should follow this logic
            //1. Swipe left right
            //2. Jump and Slide
            //3. Shoot (Force 3 enemies in front of the player and show text to explain auto shooting)
            //4. Show amo count
            //5. Explain Overload (Force it on first shot)
            //6. XP explain the xp bar
            //7. Stun and Damage (Force 3 barriers side by side)
            //8. Explain Level up
            int rand = Random.Range(0, _modulPrefb.Length);
            GameObject modul = Instantiate(_modulPrefb[rand],new Vector3(0,0,pos),Quaternion.identity,_parentModulTransform.transform);
            modul.GetComponent<ModulMove>().isEnemy = false;
            modul.GetComponent<ModulMove>().isObstacle = false;
            pos += 19.5f;
        }
    }
}