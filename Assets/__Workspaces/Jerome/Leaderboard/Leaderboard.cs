using System.Collections.Generic;
using UnityEngine;
using Dan.Main;
using Dan.Models;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject _scorePrefab; //Children are in order Rank, Name, Score
    [SerializeField] private Transform _scoreParent;
    
    // Cache the player's personal best from server
    private int _cachedPersonalBest = 0;
    private bool _isPersonalBestLoaded = false;
    
    
    private void OnEnable()
    {
        //If there is no username saved then don't subscribe to the event otherwise it'll cause errors in uploading.
        if (IsUser())
        {
            EventBus.OnHighScore += OnHighScore;
            
            // // Try to upload any pending scores when coming online
            // if (HasInternetConnection())
            // {
            //     UploadPendingScores();
            // }
        }
    }

    private void OnHighScore(int newHighscore)
    {
        UploadScore(PlayerPrefs.GetString("Username"), newHighscore);
    }

    private void OnDisable()
    {
        if (IsUser())
        {
            EventBus.OnHighScore -= OnHighScore;
        }
    }
    #region Personal Best Management
    
    /// <summary>
    /// Load the player's personal best from server and cache it
    /// </summary>
    private void LoadPersonalBest(System.Action<int> onComplete = null)
    {
        Leaderboards.SpaceBornRush.GetPersonalEntry(entry =>
        {
            _cachedPersonalBest = entry.Score;
            _isPersonalBestLoaded = true;
            Debug.Log($"Loaded personal best: {_cachedPersonalBest}");
            onComplete?.Invoke(_cachedPersonalBest);
        }, error =>
        {
            Debug.LogWarning($"Could not load personal best: {error}. Using cached value.");
            onComplete?.Invoke(_cachedPersonalBest);
        });
    }
    
    /// <summary>
    /// Check if a score is a new high score
    /// </summary>
    public bool IsHighScore(int score)
    {
        // If not loaded yet, assume it's a high score to be safe
        if (!_isPersonalBestLoaded) return true;
        return score > _cachedPersonalBest;
    }
    
    /// <summary>
    /// Get the current high score (cached value)
    /// </summary>
    public int GetHighScore()
    {
        return _cachedPersonalBest;
    }
    
    /// <summary>
    /// Submit score if it's a new high score
    /// </summary>
    public void SubmitScoreIfHighScore(int score)
    {
        if (!IsUser())
        {
            Debug.LogWarning("Cannot submit score: No username set");
            return;
        }
        
        if (!IsHighScore(score))
        {
            Debug.Log($"Score {score} is not higher than personal best {_cachedPersonalBest}. Not uploading.");
            return;
        }
        
        string username = PlayerPrefs.GetString("Username");
        UploadScore(username, score, (success) =>
        {
            if (success)
            {
                _cachedPersonalBest = score;
                Debug.Log($"New personal best set: {score}");
            }
        });
    }
    
    /// <summary>
    /// Force upload a score regardless of high score check
    /// </summary>
    public void ForceUploadScore(int score)
    {
        if (!IsUser()) return;
        
        string username = PlayerPrefs.GetString("Username");
        UploadScore(username, score, (success) =>
        {
            if (success) _cachedPersonalBest = Mathf.Max(_cachedPersonalBest, score);
        });
    }
    
    #endregion
    #region ========== Private Methods ==========
    // Modified ClaimUsername to accept callback
    void ClaimUsername(string username, System.Action<bool, string> resultCallback = null)
    {
        Leaderboards.SpaceBornRush.UploadNewEntry(username, 0, success =>
        {
            if (success)
            {
                PlayerPrefs.SetString("Username", username);
                PlayerPrefs.SetInt("UsernameLocked", 1);
                PlayerPrefs.Save();
            
                // Subscribe to score events now
                EventBus.OnHighScore += OnHighScore;
            
                string message = $"Username '{username}' permanently claimed.";
                Debug.Log(message);
                resultCallback?.Invoke(true, message);
            }
            else
            {
                string message = "Failed to claim username. Try again.";
                Debug.LogError(message);
                resultCallback?.Invoke(false, message);
            }
        }, error =>
        {
            string message = $"[LeaderboardCreator] {error}";
            Debug.LogError(message);
            resultCallback?.Invoke(false, message);
        });
    }
    
    private List<OfflineScore> LoadOfflineScores()
    {
        string json = PlayerPrefs.GetString("OfflineScores", "[]");
        return JsonUtility.FromJson<OfflineScoreList>(json).scores;
    }

    private void SaveOfflineScores(List<OfflineScore> scores)
    {
        OfflineScoreList wrapper = new OfflineScoreList { scores = scores };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("OfflineScores", json);
        PlayerPrefs.Save();
    }

    private void ClearOfflineScores()
    {
        PlayerPrefs.DeleteKey("OfflineScores");
        PlayerPrefs.Save();
    }

    private bool HasInternetConnection()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    // Wrapper class for JSON serialization
    [System.Serializable]
    private class OfflineScoreList
    {
        public List<OfflineScore> scores;
    }

    #endregion
    
    #region ========== Public Methods ==========
    /// <summary>
    /// This function will get all the scores from the leaderboard and put them into the score section.
    /// </summary>
    public void GetScores()
    {
        // Clear any existing scores
        foreach (Transform child in _scoreParent)
        {
            Destroy(child.gameObject);
        }
    
        Leaderboards.SpaceBornRush.GetEntries(callback =>
        {
            for (int i = 0; i < callback.Length; i++)
            {
                GameObject ScoreObj = Instantiate(_scorePrefab, _scoreParent);
                
                TMP_Text[] textComponents = ScoreObj.GetComponentsInChildren<TMP_Text>();
                
                textComponents[0].text = callback[i].Rank.ToString();
                textComponents[1].text = callback[i].Username;
                textComponents[2].text = callback[i].Score.ToString("N0"); // Formatted with commas
            
                // 4. Optional: Highlight player's own score
                // Note: callback[i].IsMine() would need access to Entry struct
                // We'll implement this differently below
            }
        });
    }
    /// <summary>
    /// Function will upload the score passed.
    /// </summary>
    /// <param name="username">String</param>
    /// <param name="score">Int</param>
    public void UploadScore(string username, int score, System.Action<bool> callback = null)
    {
        if (HasInternetConnection())
        {
            Leaderboards.SpaceBornRush.UploadNewEntry(username, score, success =>
            {
                if (success)
                {
                    Debug.Log($"Score {score} uploaded successfully for {username}");
                    // Update cache if this score is higher
                    if (score > _cachedPersonalBest)
                    {
                        _cachedPersonalBest = score;
                    }
                    GetScores(); // Refresh leaderboard display
                }
                callback?.Invoke(success);
            }, error =>
            {
                Debug.LogError($"Upload failed: {error}");
                // Save offline for later
                SaveScoreForLaterUpload(username, score);
                callback?.Invoke(false);
            });
        }
        else
        {
            Debug.Log("No internet connection. Saving score offline.");
            SaveScoreForLaterUpload(username, score);
            callback?.Invoke(false);
        }
    }

    public void SubmitUsername(string username, System.Action<bool, string> resultCallback = null)
    {
        // SAFETY CHECK: Prevent multiple submissions
        if (PlayerPrefs.GetInt("UsernameLocked", 0) == 1)
        {
            string currentName = PlayerPrefs.GetString("Username", "Unknown");
            string message = $"Username '{currentName}' is already locked. Cannot change.";
            Debug.LogWarning(message);
            resultCallback?.Invoke(false, message);
            return;
        }
    
        // Original validation
        if (string.IsNullOrEmpty(username) || username.Length < 3)
        {
            string message = "Username must be at least 3 characters!";
            Debug.LogError(message);
            resultCallback?.Invoke(false, message);
            return;
        }
    
        // Check if username exists
        var query = LeaderboardSearchQuery.ByUsername(username);
    
        Leaderboards.SpaceBornRush.GetEntries(query, entries =>
        {
            if (entries != null && entries.Length > 0)
            {
                string message = $"Username '{username}' is already taken!";
                Debug.LogError(message);
                resultCallback?.Invoke(false, message);
            }
            else
            {
                ClaimUsername(username, resultCallback);
            }
        }, error =>
        {
            string message = $"Failed to check username: {error}";
            Debug.LogError(message);
            resultCallback?.Invoke(false, message);
        });
    }

    
    /// <summary>
    /// Gets the player's personal best score from server
    /// </summary>
    public void GetPersonalBest(System.Action<int> callback)
    {
        Leaderboards.SpaceBornRush.GetPersonalEntry(entry =>
        {
            callback?.Invoke(entry.Score);
        }, error =>
        {
            Debug.LogError($"Failed to get personal best: {error}");
            callback?.Invoke(0);
        });
    }

    /// <summary>
    /// Will return true if in the player prefs there is a username or not.
    /// </summary>
    /// <returns></returns>
    public bool IsUser()
    {
        return PlayerPrefs.GetInt("UsernameLocked", 0) == 1 && 
               !string.IsNullOrEmpty(PlayerPrefs.GetString("Username"));
    }
    [System.Serializable]
    public class OfflineScore
    {
        public string username;
        public int score;
        public long timestamp;
    }

// Save score locally when offline
    public void SaveScoreForLaterUpload(string username, int score)
    {
        List<OfflineScore> offlineScores = LoadOfflineScores();
    
        offlineScores.Add(new OfflineScore
        {
            username = username,
            score = score,
            timestamp = System.DateTime.Now.Ticks
        });
    
        SaveOfflineScores(offlineScores);
        Debug.Log($"Score saved offline: {username} - {score}");
    }

// Upload pending scores when internet returns
    public void UploadPendingScores()
    {
        if (!HasInternetConnection()) return;
    
        List<OfflineScore> offlineScores = LoadOfflineScores();
    
        if (offlineScores.Count == 0) return;
    
        Debug.Log($"Uploading {offlineScores.Count} pending scores...");
    
        foreach (OfflineScore score in offlineScores)
        {
            UploadScore(score.username, score.score);
        }
    
        // Clear after uploading
        ClearOfflineScores();
    }
    #endregion
}


//Thank you danqzq for the leaderboard