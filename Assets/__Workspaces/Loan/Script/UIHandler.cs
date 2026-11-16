using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;
using TaloGameServices;

public class UIHandler : MonoBehaviour
{
    [Header("=====Panel Settings=====")]
    [SerializeField] private GameObject _gamePanel;
    [SerializeField] private GameObject _optionPanel;
    [SerializeField] private GameObject _scorePanel;
    [SerializeField] private GameObject _loosePanel;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _hudPanel;
    [SerializeField] private GameObject _scoreboardPanel;
    [SerializeField] private GameObject _usernamePanel;
    [SerializeField] private GameObject _creditPanel;
    [SerializeField] private GameObject _controlPanel;

    [Header("=====Buttons=====")]
    [SerializeField] private GameObject _buttonParent;
    [SerializeField] private GameObject _homeButton;
    [SerializeField] private GameObject _settingsButton;
    [SerializeField] private GameObject _scoreButton;
    [SerializeField] private GameObject _creditButton;

    [Header("=====Username Panel Components=====")]
    [SerializeField] private TMP_InputField _usernameInputField;
    [SerializeField] private Button _usernameSubmitButton;
    [SerializeField] private Button _usernameOfflineButton;
    [SerializeField] private TMP_Text _usernameStatusText;

    [Header("=====Game Settings=====")]
    [SerializeField] private GameObject _gameEmpty;
    [SerializeField] private GameObject _menuEmpty;

    [Header("=====Dependencies=====")]
    [SerializeField] private LeaderboardManager _leaderboardManager;
    [SerializeField] private bool _blackNut;

    [Header("=====Online Settings=====")]
    [SerializeField] private bool _isActive;
    [SerializeField] private bool _isCheckingUsername = false;
    [SerializeField] private bool _hasInternetConnection = false;

    [Header("=====Countdown Settings=====")]
    [SerializeField] private GameObject _countdownPanel;
    [SerializeField] private TextMeshProUGUI _countdownText;
    private float _countdownTime;
    private bool _isCountdownActive;

    [Header("<color=orange>======Audio Settings=====</color>")]
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _audioClip;

    /// <summary>Max time the menu waits for the Talo auto-login before showing up anyway.</summary>
    private const float AutoLoginTimeout = 5f;

    private IEnumerator Start()
    {
        ApplySavedVolumes();
        yield return null;

        // BlackNut has no online account: show the menu right away instead of waiting for Talo.
        if (!_blackNut)
        {
            yield return StartCoroutine(CheckInternetConnection());

            if (_hasInternetConnection && _leaderboardManager != null)
                yield return StartCoroutine(AwaitTask(_leaderboardManager.TryAutoLogin(), AutoLoginTimeout));
        }

        InitializeUI();
    }

    private IEnumerator AwaitTask(Task task, float timeout)
    {
        float end = Time.realtimeSinceStartup + timeout;
        while (!task.IsCompleted)
        {
            // A slow / stuck server must never leave the player without a menu.
            if (Time.realtimeSinceStartup > end)
            {
                Debug.LogWarning("[UIHandler] Auto-login timed out, opening the menu anyway.");
                yield break;
            }
            yield return null;
        }
        if (task.IsFaulted) Debug.LogException(task.Exception);
    }

    private void OnEnable()
    {
        EventBus.OnPlayerDead += GameLose;
        Talo.OnConnectionRestored += HandleConnectionRestored;
    }

    // Unsubscribe in OnDisable (not OnDestroy): a disable/enable cycle would otherwise subscribe twice.
    private void OnDisable()
    {
        EventBus.OnPlayerDead -= GameLose;
        Talo.OnConnectionRestored -= HandleConnectionRestored;
    }

    private void OnDestroy()
    {
        if (_usernameSubmitButton != null)
            _usernameSubmitButton.onClick.RemoveAllListeners();

        if (_usernameOfflineButton != null)
            _usernameOfflineButton.onClick.RemoveAllListeners();

        if (_usernameInputField != null)
            _usernameInputField.onSubmit.RemoveAllListeners();
    }

    // Volumes / language are in PlayerPrefs: make sure they reach the disk even if the app is killed.
    private void OnApplicationPause(bool paused)
    {
        if (paused) PlayerPrefs.Save();
    }

    private void OnApplicationQuit() => PlayerPrefs.Save();

    private void Update()
    {
        if (!_isCountdownActive) return;

        _countdownTime -= Time.deltaTime;
        if (_countdownTime > 0)
            _countdownText.text = Mathf.CeilToInt(_countdownTime).ToString();
        else
            CompleteCountdown();
    }

    #region ==================== Countdown ====================
    private void StartCountdown()
    {
        _countdownTime = 3f;
        _isCountdownActive = true;
        _countdownPanel.SetActive(true);
    }

    private void CompleteCountdown()
    {
        _isCountdownActive = false;
        _countdownPanel.SetActive(false);
        EventBus.OnGameResume?.Invoke();
    }
    #endregion

    #region ==================== INTERNET HANDLING ====================
    private IEnumerator CheckInternetConnection()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            _hasInternetConnection = false;
            yield break;
        }

        _hasInternetConnection = true;
    }
    
    /// <summary>
    /// Fires when Talo detects the connection was lost and has since been restored.
    /// If the player was offline at launch and never had a chance to auto-login,
    /// attempt it silently now so the username panel doesn't appear mid-session.
    /// </summary>
    private void HandleConnectionRestored()
    {
        if (_leaderboardManager == null) return;
        if (_leaderboardManager.IsUser()) return;

        _ = _leaderboardManager.TryAutoLogin();
    }
    #endregion
    
    #region ==================== UI INITIALIZATION ====================
    private void InitializeUI()
    {
        _menuEmpty.SetActive(true);
        _gameEmpty.SetActive(false);

        if(_blackNut)
        {
            SetupBlacknutUI();
            return;
        }
        
        bool hasUsername = _leaderboardManager && _leaderboardManager.IsUser();

        // Offline: play as guest, do not prompt for a username.
        if (!_hasInternetConnection)
        {
            OpenGamePanel();
            return;
        }

        if (hasUsername)
        {
            OpenGamePanel();
        }
        else
        {
            OpenUsernamePanel();
            SetupUsernamePanel();
        }
    }

    /// <summary>
    /// Specific UI setup for the blacknut build. Will remove everything to do with 
    /// </summary>
    private void SetupBlacknutUI()
    {
        _scoreButton.SetActive(false);
        _usernamePanel.SetActive(false);
        if (_buttonParent && _creditButton.transform.parent != _buttonParent.transform)
            _creditButton.transform.SetParent(_buttonParent.transform, false);
    }

    /// <summary>
    /// Re-checks internet and shows the username panel if the player is
    /// online, unidentified, and the panel isn't already visible.
    /// </summary>
    private void CheckAndShowUsernamePanelIfNeeded()
    {
        if (_blackNut) return; // BlackNut build: no online username at all
        if (_playingAsGuest) return; // the player chose "offline" this session: don't nag

        _hasInternetConnection = Application.internetReachability != NetworkReachability.NotReachable;
        if (!_hasInternetConnection) return;
        if (_leaderboardManager != null && _leaderboardManager.IsUser()) return;

        OpenUsernamePanel();
        SetupUsernamePanel();
    }

    private void SetupUsernamePanel()
    {
        _usernameSubmitButton.onClick.RemoveAllListeners();
        _usernameSubmitButton.onClick.AddListener(OnUsernameSubmitClicked);

        _usernameInputField.onSubmit.RemoveAllListeners();
        _usernameInputField.onSubmit.AddListener(_ => OnUsernameSubmitClicked());

        // "Offline" button had no action at all: it now lets the player continue as a guest.
        if (_usernameOfflineButton != null)
        {
            _usernameOfflineButton.onClick.RemoveAllListeners();
            _usernameOfflineButton.onClick.AddListener(PlayAsGuest);
        }

        if (!_hasInternetConnection)
        {
            SetUsernameStatus("No internet connection — connect to choose a username", Color.yellow);
            _usernameSubmitButton.interactable = false;
        }
        else
        {
            SetUsernameStatus("Choose a username (cannot be changed later!)", Color.white);
            _usernameSubmitButton.interactable = true;
        }

        _usernameInputField.text = "";
        _usernameInputField.interactable = _hasInternetConnection;

        if (_hasInternetConnection)
        {
            _usernameInputField.Select();
            _usernameInputField.ActivateInputField();
        }
    }

    private bool _playingAsGuest;

    private void PlayAsGuest()
    {
        if (_isCheckingUsername) return;
        _playingAsGuest = true;
        ResetUsernamePanel();
        OpenGamePanel();
    }
    #endregion

    #region ==================== USERNAME SUBMISSION ====================
    private async void OnUsernameSubmitClicked()
    {
        if (_isCheckingUsername) return;

        if (!_hasInternetConnection)
        {
            SetUsernameStatus("No internet connection!", Color.red);
            StartCoroutine(ShowTemporaryStatus("Connect to internet to save username", 2f, Color.red));
            return;
        }

        string username = _usernameInputField.text.Trim();
        if (!ValidateUsername(username)) return;

        SetUsernameUIState(false);
        _isCheckingUsername = true;
        SetUsernameStatus("Checking availability...", Color.yellow);

        var (success, message) = await _leaderboardManager.SubmitUsername(username);
        OnUsernameSubmissionResult(success, message);
    }

    private bool ValidateUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            SetUsernameStatus("Username cannot be empty!", Color.red);
            return false;
        }

        if (username.Length < 3)
        {
            SetUsernameStatus("Must be at least 3 characters!", Color.red);
            return false;
        }

        if (username.Length > 20)
        {
            SetUsernameStatus("Maximum 20 characters!", Color.red);
            return false;
        }

        foreach (char c in username)
        {
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
            {
                SetUsernameStatus("Only letters, numbers, _ and - allowed", Color.red);
                return false;
            }
        }

        return true;
    }

    private void OnUsernameSubmissionResult(bool success, string message)
    {
        _isCheckingUsername = false;

        if (success)
        {
            SetUsernameStatus(message, Color.green);
            StartCoroutine(CompleteUsernameSetupDelayed(true, 1.5f));
        }
        else
        {
            SetUsernameStatus(message, Color.red);
            SetUsernameUIState(true);
            _usernameInputField.Select();
            _usernameInputField.ActivateInputField();
        }
    }
    #endregion

    #region ==================== HELPER METHODS ====================
    private void SetUsernameStatus(string message, Color color)
    {
        if (_usernameStatusText)
        {
            _usernameStatusText.text = message;
            _usernameStatusText.color = color;
        }
    }

    private void SetUsernameUIState(bool interactive)
    {
        if (_usernameSubmitButton)
            _usernameSubmitButton.interactable = interactive;

        if (_usernameInputField)
            _usernameInputField.interactable = interactive;
    }

    private void ResetUsernamePanel()
    {
        if (_usernameInputField)
            _usernameInputField.text = "";

        SetUsernameStatus("", Color.white);
        SetUsernameUIState(true);
        _isCheckingUsername = false;
    }

    private IEnumerator ShowTemporaryStatus(string message, float duration, Color color)
    {
        string originalStatus = _usernameStatusText.text;
        Color originalColor = _usernameStatusText.color;

        SetUsernameStatus(message, color);
        yield return new WaitForSeconds(duration);
        SetUsernameStatus(originalStatus, originalColor);
    }

    private IEnumerator CompleteUsernameSetupDelayed(bool success, float delay)
    {
        yield return new WaitForSeconds(delay);
        CompleteUsernameSetup(success);
    }

    private void CompleteUsernameSetup(bool success)
    {
        OpenGamePanel();
        ResetUsernamePanel();
    }
    #endregion

    #region ==================== GAME METHODS ====================

    private void OpenUsernamePanel()
    {
        _optionPanel.SetActive(false);
        _loosePanel.SetActive(false);
        _pausePanel.SetActive(false);
        _hudPanel.SetActive(false);
        _creditPanel.SetActive(false);
        _homeButton.SetActive(false);
        _scoreButton.SetActive(false);
        _settingsButton.SetActive(false);
        _scorePanel.SetActive(false);
        _gamePanel.SetActive(false);
        _creditButton.gameObject.SetActive(false);
        _controlPanel.SetActive(false);
        _gameEmpty.SetActive(false);
        _menuEmpty.SetActive(true); // P_Username is a child of the menu: it must stay visible

        _usernamePanel.SetActive(true);
    }

    private void GameLose()
    {
        if (_loosePanel.activeSelf) return; // death reported twice

        StopCountdown();
        _pausePanel.SetActive(false);
        _hudPanel.SetActive(false);
        _loosePanel.SetActive(true);
    }

    public void GamePause()
    {
        // Already paused / not in a run (double press, HUD button + Start in the same frame, ...).
        if (_pausePanel.activeSelf || _loosePanel.activeSelf || !_gameEmpty.activeInHierarchy) return;

        StopCountdown();
        _hudPanel.SetActive(false);
        _pausePanel.SetActive(true);
        _audioSource.PlayOneShot(_audioClip);
        EventBus.OnGamePause?.Invoke();
    }

    public void GameResume()
    {
        if (!_pausePanel.activeSelf) return; // B + Resume button in the same frame → only one countdown

        _pausePanel.SetActive(false);
        _hudPanel.SetActive(true);
        _audioSource.PlayOneShot(_audioClip);
        StartCountdown();
    }

    private void StopCountdown()
    {
        _isCountdownActive = false;
        if (_countdownPanel) _countdownPanel.SetActive(false);
    }

    /// <summary>
    /// Pause / resume from a gamepad Start button or Escape (see UINavigationManager).
    /// Ignored outside of gameplay, during the resume countdown and on the lose screen.
    /// </summary>
    public void TogglePause()
    {
        if (_isCountdownActive || !_gameEmpty.activeInHierarchy || _loosePanel.activeSelf) return;
        if (Time.timeScale == 0f && !_pausePanel.activeSelf) return; // upgrade choice on screen

        if (_pausePanel.activeSelf) GameResume();
        else if (_hudPanel.activeSelf) GamePause();
    }

    public void GameStart()
    {
        // A double click / A spam on "Start" would fire OnGameStart twice (two players spawned).
        if (_gameEmpty.activeSelf && _hudPanel.activeSelf) return;

        Time.timeScale = 1f;
        StopCountdown();
        _menuEmpty.SetActive(false);
        _pausePanel.SetActive(false);
        _loosePanel.SetActive(false);

        _audioSource.PlayOneShot(_audioClip);
        _gameEmpty.SetActive(true);
        _hudPanel.SetActive(true);
        EventBus.OnGameStart?.Invoke();
    }

    public void ReturnMenu()
    {
        if (!_gameEmpty.activeSelf) return; // already back in the menu (double click)

        EventBus.OnGameOver?.Invoke();

        Time.timeScale = 1f;
        StopCountdown();
        _hudPanel.SetActive(false);
        _loosePanel.SetActive(false);
        _pausePanel.SetActive(false);
        _gameEmpty.SetActive(false);
        _usernamePanel.SetActive(false);

        // The username panel lives under the menu: show the menu first, otherwise nothing is visible.
        _menuEmpty.SetActive(true);

        // If the player went online while playing, prompt for a username now.
        CheckAndShowUsernamePanelIfNeeded();
        if (_usernamePanel.activeSelf) return;

        OpenGamePanel();
    }

    public void OpenGamePanel()
    {
        // Cancel any background leaderboard pagination still running.
        if (_leaderboardManager != null)
            _leaderboardManager.CancelBackgroundLoading();

        // Leaving the settings: persist volumes / language now.
        if (_optionPanel.activeSelf) PlayerPrefs.Save();

        _optionPanel.SetActive(false);
        _scorePanel.SetActive(false);
        _creditPanel.SetActive(false);
        _usernamePanel.SetActive(false);
        _controlPanel.SetActive(false);

        _audioSource.PlayOneShot(_audioClip);
        _creditButton.gameObject.SetActive(true);
        _gamePanel.SetActive(true);
        _homeButton.SetActive(true);
        // _scoreButton.SetActive(true);
        _settingsButton.SetActive(true);
    }

    public void OpenOptionPanel()
    {
        _gamePanel.SetActive(false);
        _scorePanel.SetActive(false);
        _creditPanel.SetActive(false);
        _usernamePanel.SetActive(false);
        _controlPanel.SetActive(false);

        _audioSource.PlayOneShot(_audioClip);
        _optionPanel.SetActive(true);
    }

    public void OpenScorePanel()
    {
        if (_blackNut || _leaderboardManager == null) return; // no online leaderboard in this build

        // If the player is online but unidentified, show the username panel first.
        CheckAndShowUsernamePanelIfNeeded();
        if (_usernamePanel.activeSelf) return;

        _gamePanel.SetActive(false);
        _optionPanel.SetActive(false);
        _creditPanel.SetActive(false);
        _usernamePanel.SetActive(false);
        _controlPanel.SetActive(false);

        _audioSource.PlayOneShot(_audioClip);
        _scorePanel.SetActive(true);
        _scoreboardPanel.SetActive(true);

        _ = _leaderboardManager.LoadLeaderboardInitial();
    }

    public void OpenCreditPanel()
    {
        _gamePanel.SetActive(false);
        _optionPanel.SetActive(false);
        _scorePanel.SetActive(false);
        _usernamePanel.SetActive(false);
        _controlPanel.SetActive(false);

        _audioSource.PlayOneShot(_audioClip);
        _creditPanel.SetActive(true);
    }

    public void OpenControlPanel()
    {
        _gamePanel.SetActive(false);
        _optionPanel.SetActive(false);
        _scorePanel.SetActive(false);
        _usernamePanel.SetActive(false);
        _creditPanel.SetActive(false);

        _audioSource.PlayOneShot(_audioClip);
        _controlPanel.SetActive(true);
    }

    public void ChangeLangue(int langueID)
    {
        if (_isActive) return;
        _audioSource.PlayOneShot(_audioClip);
        StartCoroutine(SetLangue(langueID));
    }

    IEnumerator SetLangue(int langueID)
    {
        _isActive = true;
        yield return LocalizationSettings.InitializationOperation;

        // A wrong index used to throw here and leave _isActive stuck at true: language locked forever.
        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (langueID >= 0 && langueID < locales.Count)
            LocalizationSettings.SelectedLocale = locales[langueID];
        else
            Debug.LogWarning($"[UIHandler] No locale at index {langueID} ({locales.Count} available).");

        _isActive = false;
    }

    #endregion

    #region ==================== AudioMixer METHODS ====================
    /// <summary>Re-applies the volumes saved in PlayerPrefs (the mixer forgets them between sessions).</summary>
    private void ApplySavedVolumes()
    {
        if (!_audioMixer) return;
        if (PlayerPrefs.HasKey("MasterVolume")) _audioMixer.SetFloat("MasterVol", PlayerPrefs.GetFloat("MasterVolume"));
        if (PlayerPrefs.HasKey("SFXVolume")) _audioMixer.SetFloat("SFXVol", PlayerPrefs.GetFloat("SFXVolume"));
        if (PlayerPrefs.HasKey("MusicVolume")) _audioMixer.SetFloat("MusicVol", PlayerPrefs.GetFloat("MusicVolume"));
        if (PlayerPrefs.HasKey("AmbianceVolume")) _audioMixer.SetFloat("AmbianceVol", PlayerPrefs.GetFloat("AmbianceVolume"));
    }

    public void SetMasterVolume(float volume)
    {
        _audioMixer.SetFloat("MasterVol", volume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        _audioMixer.SetFloat("SFXVol", volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        _audioMixer.SetFloat("MusicVol", volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetAmbianceVolume(float volume)
    {
        _audioMixer.SetFloat("AmbianceVol", volume);
        PlayerPrefs.SetFloat("AmbianceVolume", volume);
    }
    #endregion

    #region ==================== DEBUG & TESTING ====================
    [ContextMenu("Test: Force Show Username Panel")]
    public void DebugForceShowUsernamePanel()
    {
        StartCoroutine(DebugResetAndShowPanel());
    }

    private IEnumerator DebugResetAndShowPanel()
    {
        // Clear Talo identity so the panel is guaranteed to show.
        _ = Talo.Players.ClearIdentity();

        yield return StartCoroutine(CheckInternetConnection());

        _usernamePanel.SetActive(true);
        _gamePanel.SetActive(false);
        SetupUsernamePanel();

        Debug.Log($"Debug: Forced username panel. Internet: {_hasInternetConnection}");
    }

    [ContextMenu("Test: Clear All Usernames")]
    public async void DebugClearAllUsernames()
    {
        try
        {
            await Talo.Players.ClearIdentity();
            Debug.Log("Debug: Cleared Talo identity");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Debug clear failed: {ex.Message}");
        }
    }
    #endregion
}