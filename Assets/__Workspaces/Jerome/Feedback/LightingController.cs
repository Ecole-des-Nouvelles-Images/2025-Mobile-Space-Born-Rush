using UnityEngine;

public class LightingController : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerStatController _playerStats;

    [Header("Light Reference")]
    [SerializeField] private Light _targetLight; // Direct reference to the Light component
    [SerializeField] private Light _targetLight2; // Direct reference to the other Light component

    [Header("Health Light Settings")]
    [SerializeField] private Color _fullHealthColor = Color.white;
    [SerializeField] private Color _lowHealthColor = Color.red;
    [SerializeField, Range(0.1f, 5f)] private float _colorChangeSpeed = 1f;

    [Header("Intensity Settings (Optional)")]
    [SerializeField] private bool _changeIntensity = false;
    [SerializeField] private float _fullHealthIntensity = 1f;
    [SerializeField] private float _lowHealthIntensity = 0.5f;

    [Header("Health Threshold")]
    [SerializeField, Range(0f, 1f)] private float _lowHealthThreshold = 0.5f;

    // Cached values for optimization
    private Color _currentTargetColor;
    private Color _currentColor;
    private float _lastKnownHealth = -1f;

    void Start()
    {
        // === VALIDATION ===
        if (!_targetLight)
        {
            // Try to get it from this GameObject if not set
            _targetLight = GetComponentInChildren<Light>();
            
            if (!_targetLight)
            {
                Debug.LogError("LightingController: No Light component assigned or found!", this);
                enabled = false;
                return;
            }
        }

        if (!_playerStats)
        {
            _playerStats = FindObjectOfType<PlayerStatController>();
        }

        // Initialize with current light color
        _currentColor = _targetLight.color;
        _currentTargetColor = _currentColor;
    }

    void Update()
    {
        if (!_playerStats || !_targetLight) return;

        float currentHealth = _playerStats.CurrentHealth;

        // === OPTIMIZATION: Skip if health hasn't changed ===
        if (SmallHealthDiff(currentHealth) && _currentColor == _currentTargetColor) return;

        _lastKnownHealth = currentHealth;

        // === Determine target color based on health ===
        if (currentHealth > currentHealth*_lowHealthThreshold)
        {
            _currentTargetColor = _fullHealthColor;
        }
        else
        {
            _currentTargetColor = _lowHealthColor;
        }

        // === Smooth color transition ===
        _currentColor = Color.Lerp(_currentColor, _currentTargetColor, 
                                   Time.deltaTime * _colorChangeSpeed);
        
        _targetLight.color = _currentColor;
        _targetLight2.color = _currentColor;

        // === Optional: Change intensity too ===
        if (_changeIntensity)
        {
            float healthPercent = Mathf.Clamp01(currentHealth / 100f);
            float targetIntensity = Mathf.Lerp(_lowHealthIntensity, _fullHealthIntensity, healthPercent);
            _targetLight.intensity = Mathf.Lerp(_targetLight.intensity, targetIntensity, 
                                                Time.deltaTime * _colorChangeSpeed);
        }
    }

    bool SmallHealthDiff(float currentHealth)
    {
        if (currentHealth >= _lastKnownHealth - 5 && currentHealth <= _lastKnownHealth + 5) return true;

        _lastKnownHealth = currentHealth;
        return false;
        
    }
    
    // === Helper method for instant changes (e.g., healing potion) ===
    public void ForceUpdateLight()
    {
        _lastKnownHealth = -1f; // This will force an update on next frame
    }
}