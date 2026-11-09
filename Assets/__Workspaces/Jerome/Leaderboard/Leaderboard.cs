using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using TaloGameServices;

public class LeaderboardManager : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject _scorePrefab;
    [SerializeField] private Transform _scoreParent;

    [Header("Talo")]
    [Tooltip("Internal name of the leaderboard as configured in the Talo dashboard.")]
    [SerializeField] private string _leaderboardInternalName = "space-born-rush";

    // Personal best cache
    private int _cachedPersonalBest = 0;
    private bool _isPersonalBestLoaded = false;

    // Background loading control
    private CancellationTokenSource _paginationCts;
    
    // Duplicate post safeguard
    private bool _uploadInFlight = false;

    private void OnEnable()
    {
        EventBus.OnHighScore += OnHighScore;
        if (IsUser()) _ = LoadPersonalBest();
    }

    private void OnDisable()
    {
        EventBus.OnHighScore -= OnHighScore;
        CancelBackgroundLoading();
    }

    private void OnHighScore(int newHighscore)
    {
        _ = SubmitScoreIfHighScore(newHighscore);
    }

    #region ========== Identity ==========
    public async Task<bool> IsUsernameAvailable(string username)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            return false;

        try
        {
            var searchPage = await Talo.Players.Search(username);
            foreach (var player in searchPage.players)
            {
                var alias = player.GetAlias();
                if (alias != null &&
                    string.Equals(alias.identifier, username, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Talo] Username availability check failed: {ex.Message}");
            return false;
        }
    }

    public async Task<(bool success, string message)> SubmitUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            return (false, "Username must be at least 3 characters!");

        try
        {
            Talo.IdentityCheck();
            return (false, $"Already identified as '{Talo.CurrentAlias.identifier}'.");
        }
        catch { /* Not identified yet — proceed. */ }

        try
        {
            await Talo.Players.Identify("username", username);
            PlayerPrefs.SetString("Username", username);
            PlayerPrefs.Save();

            _ = LoadPersonalBest();
            return (true, $"Welcome, {Talo.CurrentAlias.identifier}!");
        }
        catch (IdentifyException ex)
        {
            string message = ex.ErrorCode switch
            {
                IdentifyErrorCode.IDENTIFIER_TAKEN => $"Username '{username}' is already taken!",
                IdentifyErrorCode.IDENTIFIER_PROFANITY => "That username is not allowed.",
                _ => ex.Message
            };
            Debug.LogError($"[Talo] Identify failed: {message}");
            return (false, message);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Talo] Identify failed: {ex.Message}");
            return (false, "Failed to claim username. Try again.");
        }
    }

    public bool IsUser()
    {
        try { Talo.IdentityCheck(); return true; }
        catch { return false; }
    }

    /// <summary>
    /// Attempts to re-identify a previously-claimed username at startup.
    /// Returns true if the player is identified after this call.
    /// </summary>
    public async Task<bool> TryAutoLogin()
    {
        if (IsUser()) return true;

        var saved = PlayerPrefs.GetString("Username", "");
        if (string.IsNullOrEmpty(saved)) return false;

        try
        {
            await Talo.Players.Identify("username", saved);
            
            _ = LoadPersonalBest();
            Debug.Log($"[Talo] Auto-logged in as '{saved}'.");
            return true;
        }
        catch (IdentifyException ex) when (ex.ErrorCode == IdentifyErrorCode.IDENTIFIER_TAKEN)
        {
            // The name was claimed by someone else while we were away.
            // Clear the stale local copy and let the player pick again.
            Debug.LogWarning($"[Talo] Saved username '{saved}' is taken. Clearing.");
           PlayerPrefs.DeleteKey("Username");
            PlayerPrefs.Save();
            return false;
        }
        catch (Exception ex)
        {
            // Network error, server error, etc. Don't block the player.
            Debug.LogWarning($"[Talo] Auto-login failed: {ex.Message}");
            return false;
        }
    }
    #endregion

    #region ========== Personal Best ==========
    private async Task LoadPersonalBest()
    {
        try
        {
            var options = new GetEntriesOptions { aliasId = Talo.CurrentAlias.id };
            var result = await Talo.Leaderboards.GetEntries(_leaderboardInternalName, options);
            if (result.entries.Length > 0)
                _cachedPersonalBest = (int)result.entries[0].score;
            _isPersonalBestLoaded = true;
            Debug.Log($"Loaded personal best: {_cachedPersonalBest}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Talo] Could not load personal best: {ex.Message}. Using cached value.");
            _isPersonalBestLoaded = false;
        }
    }

    public bool IsHighScore(int score)
    {
        if (!_isPersonalBestLoaded) return true;
        return score > _cachedPersonalBest;
    }

    public int GetHighScore() => _cachedPersonalBest;

    public async Task SubmitScoreIfHighScore(int score)
    {
        if (!IsUser())
        {
            Debug.LogWarning("Cannot submit score: No player identified.");
            return;
        }
        if (!IsHighScore(score))
        {
            Debug.Log($"Score {score} is not higher than PB {_cachedPersonalBest}. Skipping.");
            return;
        }
        await UploadScore(score, (success) =>
        {
            if (success)
            {
                _cachedPersonalBest = score;
                Debug.Log($"New personal best set: {score}");
            }
        });
    }

    public async Task ForceUploadScore(int score)
    {
        if (!IsUser()) return;
        await UploadScore(score, (success) =>
        {
            if (success) _cachedPersonalBest = Mathf.Max(_cachedPersonalBest, score);
        });
    }
    #endregion

    #region ========== Leaderboard Pagination ==========

    /// <summary>
    /// Clears the current UI and loads the first page (page 0) synchronously.
    /// Then kicks off background loading for any remaining pages.
    /// </summary>
    public async Task LoadLeaderboardInitial()
    {
        // Cancel any previous background loading before starting a new load
        CancelBackgroundLoading();
        _paginationCts = new CancellationTokenSource();

        // Clear existing UI
        foreach (Transform child in _scoreParent)
            Destroy(child.gameObject);

        try
        {
            var options = new GetEntriesOptions { page = 0 };
            var result = await Talo.Leaderboards.GetEntries(
                _leaderboardInternalName, options);

            PopulateUI(result.entries);

            // If there are more pages, load them in the background
            if (!result.isLastPage)
            {
                _ = LoadRemainingPagesInBackground(1, _paginationCts.Token);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Talo] Failed to load first page: {ex.Message}");
        }
    }

    /// <summary>
    /// Loops through pages sequentially in the background, appending entries
    /// to the UI as they arrive. Stops when the last page is reached or cancelled.
    /// </summary>
    private async Task LoadRemainingPagesInBackground(int startPage, CancellationToken token)
    {
        int page = startPage;

        while (!token.IsCancellationRequested)
        {
            try
            {
                var options = new GetEntriesOptions { page = page };
                var result = await Talo.Leaderboards.GetEntries(
                    _leaderboardInternalName, options);

                if (token.IsCancellationRequested)
                    break;

                PopulateUI(result.entries);

                if (result.isLastPage)
                    break;

                page++;
            }
            catch (Exception ex)
            {
                // Log but don't spam; background loading can fail silently if
                // the player navigates away or loses connection.
                Debug.LogWarning($"[Talo] Background page {page} failed: {ex.Message}");
                break;
            }
        }
    }

    /// <summary>
    /// Instantiates UI prefabs for the given entries and appends them to _scoreParent.
    /// </summary>
    private void PopulateUI(LeaderboardEntry[] entries)
    {
        foreach (var entry in entries)
        {
            GameObject scoreObj = Instantiate(_scorePrefab, _scoreParent);
            TMP_Text[] texts = scoreObj.GetComponentsInChildren<TMP_Text>();

            texts[0].text = (entry.position + 1).ToString();
            texts[1].text = entry.playerAlias.identifier;
            texts[2].text = ((int)entry.score).ToString("N0");
        }
    }

    /// <summary>
    /// Cancels any ongoing background pagination.
    /// Call this when the leaderboard is closed or refreshed.
    /// </summary>
    public void CancelBackgroundLoading()
    {
        if (_paginationCts != null)
        {
            _paginationCts.Cancel();
            _paginationCts.Dispose();
            _paginationCts = null;
        }
    }

    #endregion

    #region ========== Leaderboard Writes ==========
    public async Task UploadScore(int score, Action<bool> callback = null)
    {
        if (!IsUser())
        {
            Debug.LogWarning("Cannot upload score: No player identified.");
            callback?.Invoke(false);
            return;
        }

        if (_uploadInFlight)
        {
            Debug.LogWarning($"[Talo] Upload already in flight, skipping duplicate for score {score}.");
            callback?.Invoke(false);
            return;
        }

        _uploadInFlight = true;
        try
        {
            var result = await Talo.Leaderboards.AddEntry(_leaderboardInternalName, score);

            if (!result.Success)
            {
                Debug.LogWarning($"[Talo] Upload rejected by server. RejectedProps: {result.RejectedProps?.Length ?? 0}");
                callback?.Invoke(false);
                return;
            }

            Debug.Log($"Score {score} uploaded. Updated: {result.Updated}");

            if (result.Updated && score > _cachedPersonalBest)
                _cachedPersonalBest = score;

            // Refresh the leaderboard UI from the top
            _ = LoadLeaderboardInitial();

            callback?.Invoke(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Talo] Upload failed: {ex.Message}");
            callback?.Invoke(false);
        }
        finally
        {
            _uploadInFlight = false;
        }
    }

    public async Task GetPersonalBest(Action<int> callback)
    {
        await LoadPersonalBest();
        callback?.Invoke(_cachedPersonalBest);
    }

    #endregion
}