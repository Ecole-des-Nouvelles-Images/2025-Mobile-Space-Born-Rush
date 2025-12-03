using System.Collections;
using UnityEngine;

public class DropsMovement : MonoBehaviour
{
    private GameUpgradeController _gameUpgradeController;
    private XpData _xpData;
    
    private PlayerController _playerController;
    private Coroutine _magnetCoroutine = null;
    private bool _magnetized = false;
    private void OnEnable()
    {
        EventBus.OnMagnetActivated += OnMagnetActivated;
    }

    private void OnMagnetActivated(float time)
    {
        if (!_xpData) return;
        if (_magnetCoroutine != null)
        {
            StopCoroutine(_magnetCoroutine);
        }

        _magnetCoroutine = StartCoroutine(MagnetCoroutine(time));

    }

    private void OnDisable()
    {
        EventBus.OnMagnetActivated -= OnMagnetActivated;
    }

    private void Start()
    {
        _gameUpgradeController = FindAnyObjectByType<GameUpgradeController>();
        _xpData = FindObjectOfType<XpData>();
        _playerController = FindObjectOfType<PlayerController>();
    }

    void Update()
    {

        if (_magnetized)
        {
            transform.position = Vector3.Lerp(transform.position, _playerController.transform.position,
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
