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

    private IEnumerator Start()
    {
        yield return null;
        yield return StartCoroutine(CheckInternetConnection());

        if (_hasInternetConnection && _leaderboardManager != null)
            yield return StartCoroutine(AwaitTask(_leaderboardManager.TryAutoLogin()));

        InitializeUI();
    }

    private IEnumerator AwaitTask(Task task)
    {
        while (!task.IsCompleted) yield return null;
        if (task.IsFaulted) Debug.LogException(task.Exception);
    }

    private void OnEnable()
    {
        EventBus.OnPlayerDead += GameLose;
        Talo.OnConnectionRestored += HandleConnectionRestored;
    }

    private void OnDestroy()
    {
        EventBus.OnPlayerDead -= GameLose;
        Talo.OnConnectionRestored -= HandleConnectionRestored;

        if (_usernameSubmitButton != null)
            _usernameSubmitButton.onClick.RemoveAllListeners();

        if (_usernameInputField != null)
            _usernameInputField.onSubmit.RemoveAllListeners();
    }

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
        _creditButton.transform.SetParent(_buttonParent.transform, false);

    }

    /// <summary>
    /// Re-checks internet and shows the username panel if the player is
    /// online, unidentified, and the panel isn't already visible.
    /// </summary>
    private void CheckAndShowUsernamePanelIfNeeded()
    {
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
        _menuEmpty.SetActive(false);
        _gameEmpty.SetActive(false);

        _usernamePanel.SetActive(true);
    }

    private void GameLose()
    {
        _hudPanel.SetActive(false);
        _loosePanel.SetActive(true);
    }

    public void GamePause()
    {
        _hudPanel.SetActive(false);
        _pausePanel.SetActive(true);
        _audioSource.PlayOneShot(_audioClip);
        EventBus.OnGamePause?.Invoke();
    }

    public void GameResume()
    {
        _pausePanel.SetActive(false);
        _hudPanel.SetActive(true);
        _audioSource.PlayOneShot(_audioClip);
        StartCountdown();
    }

    public void GameStart()
    {
        _menuEmpty.SetActive(false);
        _pausePanel.SetActive(false);
        _countdownPanel.SetActive(false);

        _audioSource.PlayOneShot(_audioClip);
        _gameEmpty.SetActive(true);
        _hudPanel.SetActive(true);
        EventBus.OnGameStart?.Invoke();
    }

    public void ReturnMenu()
    {
        EventBus.OnGameOver?.Invoke();

        _loosePanel.SetActive(false);
        _pausePanel.SetActive(false);
        _gameEmpty.SetActive(false);
        _usernamePanel.SetActive(false);

        // If the player went online while playing, prompt for a username now.
        CheckAndShowUsernamePanelIfNeeded();
        if (_usernamePanel.activeSelf) return;

        _audioSource.PlayOneShot(_audioClip);
        _menuEmpty.SetActive(true);
        _gamePanel.SetActive(true);
    }

    public void OpenGamePanel()
    {
        // Cancel any background leaderboard pagination still running.
        if (_leaderboardManager != null)
            _leaderboardManager.CancelBackgroundLoading();

        _optionPanel.SetActive(false);
        _scorePanel.SetActive(false);
        _creditPanel.SetActive(false);
        _usernamePanel.SetActive(false);
        _controlPanel.SetActive(false);

        _audioSource.PlayOneShot(_audioClip);
        _creditButton.gameObject.SetActive(true);
        _gamePanel.SetActive(true);
        _homeButton.SetActive(true);
        _scoreButton.SetActive(true);
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
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[langueID];
        _isActive = false;
    }

    #endregion

    #region ==================== AudioMixer METHODS ====================
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