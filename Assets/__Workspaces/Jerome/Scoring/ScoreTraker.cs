using TMPro;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScoringManager _scoringManager;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _currentScoreText;
    
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
        
        if (!_scoringManager)
            _scoringManager = FindAnyObjectByType<ScoringManager>();
        
        // Initialize display
        _currentDisplayScore = 0;
        
        UpdateCurrentScoreText();
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
    }
    
    private void UpdateCurrentScoreText()
    {
        if (_currentScoreText)
        {
            string formattedScore = _currentDisplayScore.ToString("N0");
            _currentScoreText.text = string.Format(_currentScoreFormat, formattedScore);
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
}