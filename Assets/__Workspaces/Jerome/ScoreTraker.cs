using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScoreTraker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private ScoringManager _scoringManager;
    
    // Optional: Formatting and animation variables
    [SerializeField] private string _scoreFormat = "SCORE: {0}";
    [SerializeField] private float _animationDuration = 0.3f;
    
    private float _currentDisplayScore;
    private float _targetScore;
    private float _animationTimer;

    private void Start()
    {
        // If not assigned in inspector, try to find them
        if (_scoreText == null)
            _scoreText = GetComponent<TextMeshProUGUI>();
        
        if (_scoringManager == null)
            _scoringManager = FindObjectOfType<ScoringManager>();
        
        // Initialize display
        _currentDisplayScore = 0;
        UpdateScoreText();
    }

    private void Update()
    {
        // Handle continuous score updates from ScoringManager's Update method
        if (_scoringManager)
        {
            _targetScore = _scoringManager.Score;
            
            // Smooth animation for score changes
            if (Mathf.Abs(_currentDisplayScore - _targetScore) > 0.1f)
            {
                _currentDisplayScore = Mathf.Lerp(_currentDisplayScore, _targetScore, _animationDuration * Time.deltaTime * 10f);
                UpdateScoreText();
            }
            else if (_currentDisplayScore != _targetScore)
            {
                _currentDisplayScore = _targetScore;
                UpdateScoreText();
            }
        }
    }

    private void OnScoreChanged(GameObject obj = null)
    {
        // This forces an immediate update when big score changes happen
        if (_scoringManager)
        {
            _currentDisplayScore = _scoringManager.Score;
            UpdateScoreText();
        }
    }

    private void UpdateScoreText()
    {
        if (_scoreText)
        {
            // Format the score (0 decimal places, with commas for thousands)
            string formattedScore = _currentDisplayScore.ToString("N0");
            _scoreText.text = string.Format(_scoreFormat, formattedScore);
        }
    }

    // Optional: Add visual feedback for score changes
    public void PulseText()
    {
        // You could add a simple scale animation here
        // or change color temporarily when score increases/decreases
    }
}


    
