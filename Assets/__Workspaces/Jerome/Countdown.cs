using TMPro;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    [SerializeField] private GameObject _countdownPanel;
    [SerializeField] private TextMeshProUGUI _countdownText;
    
    private float _time;
    private bool _isCountdownActive;

    private void OnEnable()
    {
       EventBus.OnGamePause += OnGamePause;
       EventBus.OnGameResume += OnGameResume;
    }

    private void OnGameResume()
    {
        _time = 3;
        _isCountdownActive = true;
        _countdownPanel.SetActive(true);
    }

    private void OnGamePause()
    {
        _isCountdownActive = false;
    }

    private void OnDisable()
    {
        _time = 0;
    }

    private void Update()
    {
        if (!_isCountdownActive) return;
        
        if (_time > 0)
        {
            _time -= Time.deltaTime;

            _countdownText.text = ((int)_time).ToString();
        }
        else
        {
            EventBus.OnGameResume?.Invoke();
            _countdownPanel.SetActive(false);
        }
    }
}
