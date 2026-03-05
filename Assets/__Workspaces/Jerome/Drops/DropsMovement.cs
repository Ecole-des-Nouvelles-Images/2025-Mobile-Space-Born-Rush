using System.Collections;
using TMPro;
using UnityEngine;

public class DropsMovement : MonoBehaviour
{
    public DropsData DropData { get; private set; }
    
    private GameUpgradeController _gameUpgradeController;
    private PlayerController _playerController;
    private Coroutine _magnetCoroutine = null;
    private bool _magnetized = false;
    
    private TextMeshProUGUI _textMesh;

    private readonly float _width = 5;
    
    public void Initialize(DropsData data)
    {
        DropData = data;
        MyEnable();
    }
    private void MyEnable()
    {
        // Only listen to magnet if this is XP
        if (DropData.Type == DropType.XP) EventBus.OnMagnetActivated += OnMagnetActivated;
        
        EventBus.OnPlayerDead += OnPlayerDead;
    }

    private void OnPlayerDead()
    {
        if (this) Destroy(this.gameObject);
    }

    private void OnMagnetActivated(float time)
    {
        if (!DropData || DropData.Type != DropType.XP) return;
        
        if (_magnetCoroutine != null)
        {
            StopCoroutine(_magnetCoroutine);
        }
        _magnetCoroutine = StartCoroutine(MagnetCoroutine(time));
    }
    
    private void OnDisable()
    {
        if (DropData.Type == DropType.XP) EventBus.OnMagnetActivated -= OnMagnetActivated;
        
        EventBus.OnPlayerDead += OnPlayerDead;
    }

    private void Start()
    {
        _gameUpgradeController = FindAnyObjectByType<GameUpgradeController>();
        _playerController = FindAnyObjectByType<PlayerController>();
        _textMesh = GameObject.FindWithTag("DropText").GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {

        if (transform.position.z < -_width)
        {
            Destroy(gameObject);
        }
        
        if (_magnetized)
        {
            //The Vector up is so that the xp doesn't go into the floor. 
            transform.position = Vector3.Lerp(transform.position, _playerController.transform.position + Vector3.up,
                _gameUpgradeController.GameSpeed * Time.deltaTime);
            return;
        }
        // Move backward at constant speed (world moves forward)
        transform.position += Vector3.back * (_gameUpgradeController.GameSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (DropData)
            {
                DropData.ApplyEffect();
                if (DropData.Type != DropType.XP)
                {
                    _textMesh.text = DropData.Type.ToString();
                    _textMesh.transform.localScale = Vector3.zero;
                    _textMesh.gameObject.GetComponent<DropTextHandler>().IsTakeDrops =  true;
                }
                
            }
            
            Destroy(gameObject);
        }
    }

    private IEnumerator MagnetCoroutine(float time)
    {
        _magnetized = true;
        yield return new WaitForSeconds(time);
        _magnetized = false;
    }
}
