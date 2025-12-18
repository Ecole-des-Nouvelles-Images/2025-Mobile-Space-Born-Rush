using UnityEngine;
using Random = UnityEngine.Random;

public class StartUpManager : MonoBehaviour
{

    [SerializeField] private GameObject[] _modulPrefb;
    [SerializeField] private GameObject _playerPrefb;
    [SerializeField] private Transform _parentTransform;
    [SerializeField] private Transform _parentModulTransform;

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
        float pos = 0;
        if (_playerPrefb != null)
        {
            GameObject player = Instantiate(_playerPrefb,new Vector3(0,0.1f,pos),Quaternion.identity,_parentTransform);
            EventBus.OnPlayerSpawn?.Invoke();
        }
        _parentModulTransform =  GameObject.FindGameObjectWithTag("ParentsModul").transform;
        
        while (pos <= 60)
        {
            int rand = Random.Range(0, _modulPrefb.Length);
            GameObject modul = Instantiate(_modulPrefb[rand],new Vector3(0,0,pos),Quaternion.identity,_parentModulTransform.transform);
            modul.GetComponent<ModulMove>().isEnemy = false;
            modul.GetComponent<ModulMove>().isObstacle = false;
            pos += 19.5f;
        }
    }
}
