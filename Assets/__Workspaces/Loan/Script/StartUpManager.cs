using UnityEngine;

public class StartUpManager : MonoBehaviour
{

    [SerializeField] private GameObject[] _modulPrefb;
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
        float pos = 0;
        if (_playerPrefb != null)
        {
            GameObject player = Instantiate(_playerPrefb,new Vector3(0,0,pos),Quaternion.identity,_parentTransform);
        }
        
        while (pos <= 60)
        {
            int rand = Random.Range(0, _modulPrefb.Length);
            GameObject modul = Instantiate(_modulPrefb[rand],new Vector3(0,0,pos),Quaternion.identity,_parentTransform.transform);
            modul.GetComponent<ModulMove>().isEnemy = false;
            modul.GetComponent<ModulMove>().isObstacle = false;
            pos += 19.5f;
        }
    }
}
