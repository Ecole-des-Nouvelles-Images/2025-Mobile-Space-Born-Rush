using TMPro;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScoringManager _scoringManager;
    [SerializeField] private HighScoreData _highScoreData;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _currentScoreText;
    [SerializeField] private TextMeshProUGUI _highScoreText;
    
    [Header("Formatting")]
    [SerializeField] private string _currentScoreFormat;
    [SerializeField] private string _highScoreFormat;
    
    [Header("Animation")]
    [SerializeField] private float _animationDuration = 0.3f;
    
    private float _currentDisplayScore;
    private float _targetScore;
    private float _currentHighScore;
    
    private void Start()
    {
        // Find references if not assigned
        if (!_currentScoreText)
            Debug.LogWarning("Current Score Text not assigned to ScoreTracker!");
        
        if (!_highScoreText)
            Debug.LogWarning("High Score Text not assigned to ScoreTracker!");
        
        if (!_scoringManager)
            _scoringManager = FindObjectOfType<ScoringManager>();
        
        if (!_highScoreData)
            Debug.LogWarning("HighScoreData not assigned to ScoreTracker!");
        else
            _highScoreData.LoadHighScore(); // Load saved high score
        
        // Initialize display
        _currentDisplayScore = 0;
        _currentHighScore = _highScoreData?.HighScore ?? 0;
        
        UpdateCurrentScoreText();
        UpdateHighScoreText();
    }
    
    private void Update()
    {
        if (!_scoringManager) return;
        
        // Update current score with smooth animation
        _targetScore = _scoringManager.Score;
        
        if (Mathf.Abs(_currentDisplayScore - _targetScore) > 0.1f)
        {
            _currentDisplayScore = Mathf.Lerp(_currentDisplayScore, _targetScore, 
                _animationDuration * Time.deltaTime * 10f);
            UpdateCurrentScoreText();
        }
        else if (_currentDisplayScore != _targetScore)
        {
            _currentDisplayScore = _targetScore;
            UpdateCurrentScoreText();
        }
        
        // Check and update high score if needed
        if (_highScoreData && _targetScore > _currentHighScore)
        {
            _currentHighScore = _targetScore;
            _highScoreData.HighScore = _currentHighScore; // This will trigger save and event
            UpdateHighScoreText();
        }
        
        
    }
    
    private void OnScoreChanged(GameObject obj = null)
    {
        // Force immediate update for big score changes
        if (_scoringManager)
        {
            _currentDisplayScore = _scoringManager.Score;
            UpdateCurrentScoreText();
        }
    }
    
    private void OnHighScoreChanged(float newHighScore)
    {
        _currentHighScore = newHighScore;
        UpdateHighScoreText();
    }
    
    private void UpdateCurrentScoreText()
    {
        if (_currentScoreText)
        {
            string formattedScore = _currentDisplayScore.ToString("N0");
            _currentScoreText.text = string.Format(_currentScoreFormat, formattedScore);
        }
    }
    
    private void UpdateHighScoreText()
    {
        if (_highScoreText)
        {
            string formattedScore = _currentHighScore.ToString("N0");
            _highScoreText.text = string.Format(_highScoreFormat, formattedScore);
        }
    }
    
    // Optional visual effects
    public void PulseCurrentScore()
    {
        // Add scale animation or color change for current score
        if (_currentScoreText)
        {
            // Example: _currentScoreText.transform.localScale = Vector3.one * 1.2f;
            // Use LeanTween or DOTween for smooth animation
        }
    }
    
    public void PulseHighScore()
    {
        // Add visual effect when high score is broken
        if (_highScoreText)
        {
            // Example: Change color to gold temporarily
        }
    }
}