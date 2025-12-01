using UnityEngine;

[CreateAssetMenu(fileName = "HighScoreData", menuName = "Game/HighScore Data")]
public class HighScoreData : ScriptableObject
{
    [SerializeField] private int _highScore;
    
    public float HighScore
    {
        get => _highScore;
        set
        {
            if (value > _highScore)
            {
                _highScore = (int)value;
                SaveHighScore();
            }
        }
    }
    
    private void SaveHighScore()
    {
        // Also save to PlayerPrefs for backup/cross-platform
        PlayerPrefs.SetFloat("HighScore", _highScore);
        PlayerPrefs.Save();
        
        // Optional: Trigger UI update
        EventBus.OnHighScore?.Invoke(_highScore);
    }
    
    public void LoadHighScore()
    {
        _highScore = (int)PlayerPrefs.GetFloat("HighScore", 0f);
    }
    
    public void ResetHighScore()
    {
        _highScore = 0;
        PlayerPrefs.DeleteKey("HighScore");
    }
}