using System.Collections.Generic;
using UnityEngine;

public class TutoManager : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private List<GameObject> _tutoModuls;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _parentTransform;
    public Transform _parentModulTransform;
    public GameObject CurrentModulSpawn;

    [Header("Verification")] 
    [SerializeField] private bool _isPhase1;
    [SerializeField] private bool _isPhase2;
    [SerializeField] private bool _isPhase3;
    [SerializeField] private bool _isPhase4;
    [SerializeField] private bool _isPhase5;
    [SerializeField] private bool _isPhase6;
    [SerializeField] private bool _isFinish;

    private float _timePhase5 = 10f;
    private float _timePhase6 = 10f;
    
    private void Start()
    {
        float pos = 0;
        _parentModulTransform =  GameObject.FindGameObjectWithTag("ParentsModul").transform;
        CurrentModulSpawn = _tutoModuls[0];
        
        while (pos <= 60)
        {
            GameObject modul = Instantiate(CurrentModulSpawn,new Vector3(0,0,pos),Quaternion.identity,_parentModulTransform.transform);
            modul.GetComponent<TutoModulMove>().TutoManager = this;
            pos += 19.5f;
        }

        _isPhase1 = true;
    }

    private void Update()
    {
        if (_isFinish)
        {
            Debug.Log("Finish Tuto ! ");
        }
        
        if (_isPhase1 && _playerTransform.position.z == 1.5f)
        {
            _isPhase1 = false;
            _isPhase2 = true;
            return;
        }

        if (_isPhase2 && _playerTransform.position.z == 0f )
        {
            _isPhase2 = false;
            _isPhase3 = true;
            return;
        }

        if (_isPhase3 && _playerTransform.position.y > 0.3f)
        {
            _isPhase3 = false;
            _isPhase4 = true;
            return;
        }

        if (_isPhase4 && _playerTransform.position.y < 0.2f)
        {
            _isPhase4 = false;
            _isPhase5 = true;
            CurrentModulSpawn = _tutoModuls[1];
            return;
        }

        if (_isPhase5)
        {
            if (_timePhase5 <= 0)
            {
                _isPhase5 = false;
                _isPhase6 = true;
                CurrentModulSpawn = _tutoModuls[2];
                return;
            }

            _timePhase5 -= Time.deltaTime;
        }
        
        if (_isPhase6)
        {
            if (_timePhase6 <= 0)
            {
                _isPhase6 = false;
                _isFinish = true;
                return;
            }

            _timePhase6 -= Time.deltaTime;
        }
    }
}