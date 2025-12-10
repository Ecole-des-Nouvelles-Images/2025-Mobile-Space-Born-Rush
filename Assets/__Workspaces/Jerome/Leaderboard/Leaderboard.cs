using UnityEngine;
using Dan.Main;
using Dan.Models;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject _scorePrefab; //Children are in order Rank, Name, Score
    
    [Header("Parent")]
    [SerializeField] private Transform _scoreParent;
    
    private string _publicLeaderboardKey = "5c3c902af5a8e494aab05ef56ebcd46b03e3ea04c7848ea0482a6a154304c6dc";
    
    
    private void OnEnable()
    {
        //If there is no username saved then don't subscribe to the event otherwise it'll cause errors in uploading.
        if (IsUser())
        {
            EventBus.OnHighScore += OnHighScore;
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

    #region Private Methods
    /// <summary>
    /// This function will get all the scores from the leaderboard and put them into the score section.
    /// TODO have it set it into the UI using a prefab
    /// </summary>
    [ContextMenu("Get Scores")]
    void GetScores()
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
    // Function claims a spot for the user with the given username
    void ClaimUsername(string username)
    {
        Leaderboards.SpaceBornRush.UploadNewEntry(username, 0, success =>
        {
            if (success)
            {
                // Save username AND lock it permanently
                PlayerPrefs.SetString("Username", username);
                PlayerPrefs.SetInt("UsernameLocked", 1); // Lock flag
                PlayerPrefs.Save();
                
                Debug.Log($"Username '{username}' permanently claimed.");
            }
            else
            {
                Debug.LogError("Failed to claim username. Try again.");
            }
        }, error =>
        {
            Debug.LogError($"[LeaderboardCreator] {error}");
        });
    }

    #endregion
    
    #region Public Methods
    /// <summary>
    /// Function will upload the score passed.
    /// </summary>
    /// <param name="username">String</param>
    /// <param name="score">Int</param>
    public void UploadScore(string username, int score)
    {
        Leaderboards.SpaceBornRush.UploadNewEntry(username, score, callback =>
        {
            if (callback) GetScores();
        }, failed =>
        {
            //Could add a popup to say this.
            Debug.LogError($"[LeaderboardCreator] {failed}");
        });
    }

    public void SubmitUsername(string username)
    {
        // SAFETY CHECK: Prevent multiple submissions
        if (PlayerPrefs.GetInt("UsernameLocked", 0) == 1)
        {
            string currentName = PlayerPrefs.GetString("Username", "Unknown");
            Debug.LogWarning($"Username '{currentName}' is already locked. Cannot change.");
            return; // Exit quietly - no error needed for player
        }
    
        // Additional safety: Check if already subscribed to avoid duplicates
        if (EventBus.OnHighScore != null)
        {
            var subscribers = EventBus.OnHighScore.GetInvocationList();
            foreach (var sub in subscribers)
            {
                if (sub.Target == this && sub.Method.Name == "OnHighScore")
                {
                    Debug.LogWarning("Already listening for high scores. Username likely processing.");
                    return;
                }
            }
        }
    
        // Original validation
        if (string.IsNullOrEmpty(username) || username.Length < 3)
        {
            Debug.LogError("Username must be at least 3 characters!");
            // Optional: Add callback/event for UI feedback
            return;
        }
    
        // Check if username exists (your existing code)
        var query = LeaderboardSearchQuery.ByUsername(username);
    
        Leaderboards.SpaceBornRush.GetEntries(query, entries =>
        {
            if (entries != null && entries.Length > 0)
            {
                Debug.LogError($"Username '{username}' is already taken!");
                // Optional: Add callback for UI
            }
            else
            {
                ClaimUsername(username);
            }
        }, error =>
        {
            Debug.LogError($"Failed to check username: {error}");
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
    
    #endregion
}


//Thank you danqzq for the leaderboard