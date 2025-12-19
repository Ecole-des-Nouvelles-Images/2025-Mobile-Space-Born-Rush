using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;

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
    
    [Header("=====Buttons=====")]
    [SerializeField] private GameObject _homeButton;
    [SerializeField] private GameObject _settingsButton;
    [SerializeField] private GameObject _scoreButton; 
    [SerializeField] private GameObject _creditButton;

    [Header("=====Username Panel Components=====")]
    [SerializeField] private TMP_InputField _usernameInputField;
    [SerializeField] private Button _usernameSubmitButton;
    [SerializeField] private Button _offlineButton;
    [SerializeField] private TMP_Text _usernameStatusText;

    [Header("=====Game Settings=====")] 
    [SerializeField] private GameObject _gameEmpty;
    [SerializeField] private GameObject _menuEmpty;

    [Header("=====Dependencies=====")]
    [SerializeField] private LeaderboardManager _leaderboardManager;

    [Header("=====Online Settings=====")]
    [SerializeField] private bool _requireInternetForUsername = true;
    [SerializeField] private float _internetCheckTimeout = 3f;
    [SerializeField] private bool _allowOfflinePlay = true;
    [SerializeField] private bool _isActive;
    [SerializeField] private bool _isCheckingUsername = false;
    [SerializeField] private bool _hasInternetConnection = false;
    [SerializeField] private bool _isCheckingInternet = false;
    
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
        // Wait one frame for everything to initialize
        yield return null;
        
        // Check internet connection first (async, won't block)
        yield return StartCoroutine(CheckInternetConnection());
        
        // Initialize all UI based on internet and username status
        InitializeUI();
    }

    private void OnEnable()
    {
        // Set up event listeners
        EventBus.OnPlayerDead += GameLose;
    }

    private void OnDestroy()
    {
        EventBus.OnPlayerDead -= GameLose;
        
        // Clean up button listeners
        if (_usernameSubmitButton != null)
            _usernameSubmitButton.onClick.RemoveAllListeners();
            
        if (_offlineButton != null)
            _offlineButton.onClick.RemoveAllListeners();
            
        if (_usernameInputField != null)
            _usernameInputField.onSubmit.RemoveAllListeners();
    }
    private void Update()
    {
        if (!_isCountdownActive) return;
        
        _countdownTime -= Time.deltaTime;
        
        if (_countdownTime > 0)
        {
            _countdownText.text = Mathf.CeilToInt(_countdownTime).ToString();
        }
        else
        {
            CompleteCountdown();
        }
    }

    #region ==================== Countdown ====================
    private void StartCountdown()
    {
        // Start countdown
        _countdownTime = 3f;
        _isCountdownActive = true;
        _countdownPanel.SetActive(true);
    }
    private void CompleteCountdown()
    {
        _isCountdownActive = false;
        _countdownPanel.SetActive(false);
        
        // Actually resume the game
        EventBus.OnGameResume?.Invoke();
    }
    #endregion

    // ==================== INTERNET HANDLING ====================
    private IEnumerator CheckInternetConnection()
    {
        _isCheckingInternet = true;
        
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            _hasInternetConnection = false;
            _isCheckingInternet = false;
            yield break;
        }
        _isCheckingInternet = false;
        _hasInternetConnection = true;
    }

    #region ==================== UI INITIALIZATION ====================
    private void InitializeUI()
    {
        // Set defaults
        _menuEmpty.SetActive(true);
        _gameEmpty.SetActive(false);
        
        // Check if player already has a username (online or offline)
        bool hasUsername = _leaderboardManager && _leaderboardManager.IsUser();
        
        // Determine if we should show username panel
        bool shouldShowUsernamePanel = ShouldShowUsernamePanel(hasUsername);
        
        // Setup username panel if needed
        if (shouldShowUsernamePanel)
        {
            OpenUsernamePanel();
            SetupUsernamePanel();
        }
        else
        {
            OpenGamePanel();
        }
        
    }
    
    private bool ShouldShowUsernamePanel(bool hasUsername)
    {
        // If player already has a proper username, never show panel
        if (hasUsername) return false;
        
        // If player is offline, don't show panel
        if (!_hasInternetConnection && _allowOfflinePlay) return false;
        
        // If we require internet but don't have it, AND we allow offline play, don't show panel
        if (_requireInternetForUsername && !_hasInternetConnection && _allowOfflinePlay) return false;
        
        // Otherwise, show the panel
        return true;
    }
    
    private void SetupUsernamePanel()
    {
        
        // Set up button listeners
        _usernameSubmitButton.onClick.RemoveAllListeners();
        _usernameSubmitButton.onClick.AddListener(OnUsernameSubmitClicked);
        
        _offlineButton.onClick.RemoveAllListeners();
        _offlineButton.onClick.AddListener(OnOfflineButtonClicked);
        
        _usernameInputField.onSubmit.RemoveAllListeners();
        _usernameInputField.onSubmit.AddListener(_ => OnUsernameSubmitClicked());
        
        // Configure based on internet status
        if (!_hasInternetConnection)
        {
            if (_requireInternetForUsername)
            {
                // No internet and we require it
                SetUsernameStatus("No internet connection", Color.yellow);
                _usernameSubmitButton.interactable = false;
                
                // Show offline button if allowed
                _offlineButton.gameObject.SetActive(_allowOfflinePlay);
                
                if (_allowOfflinePlay)
                {
                    SetUsernameStatus("No internet - you can play offline", Color.yellow);
                }
            }
            else
            {
                // No internet but we don't require it
                SetUsernameStatus("No internet - using local username", Color.yellow);
                _usernameSubmitButton.interactable = true;
                _offlineButton.gameObject.SetActive(false);
            }
        }
        else
        {
            // We have internet
            SetUsernameStatus("Choose a username (cannot be changed later!)", Color.white);
            _usernameSubmitButton.interactable = true;
            _offlineButton.gameObject.SetActive(false);
        }
        
        // Reset input field
        _usernameInputField.text = "";
        _usernameInputField.interactable = true;
        
        // Auto-focus if we have internet
        if (_hasInternetConnection)
        {
            _usernameInputField.Select();
            _usernameInputField.ActivateInputField();
        }
    }
    #endregion

    #region ==================== USERNAME SUBMISSION ====================
    private void OnUsernameSubmitClicked()
    {
        if (_isCheckingUsername) return;
        
        // Check internet if required
        if (_requireInternetForUsername && !_hasInternetConnection)
        {
            SetUsernameStatus("No internet connection!", Color.red);
            StartCoroutine(ShowTemporaryStatus("Connect to internet to save username", 2f, Color.red));
            return;
        }
        
        string username = _usernameInputField.text.Trim();
        
        // Client-side validation
        if (!ValidateUsername(username)) return;
        
        // Disable UI while processing
        SetUsernameUIState(false);
        _isCheckingUsername = true;
        
        if (_hasInternetConnection)
        {
            // Online submission
            SetUsernameStatus("Checking availability...", Color.yellow);
            _leaderboardManager.SubmitUsername(username, OnUsernameSubmissionResult);
        }
        else
        {
            // Offline submission (if allowed)
            StartCoroutine(ProcessOfflineUsername(username));
        }
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
        
        // Optional: Character validation
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
    
    private IEnumerator ProcessOfflineUsername(string username)
    {
        SetUsernameStatus("Setting up offline play...", Color.yellow);
        
        // Simulate processing time
        yield return new WaitForSeconds(0.5f);
        
        // Save as offline username
        PlayerPrefs.SetString("OfflineUsername", username);
        PlayerPrefs.Save();
        
        // Success
        SetUsernameStatus($"Welcome, {username} (offline)!", Color.green);
        
        // Transition to main menu
        yield return new WaitForSeconds(1.5f);
        CompleteUsernameSetup(true);
    }
    
    private void OnUsernameSubmissionResult(bool success, string message)
    {
        _isCheckingUsername = false;
        
        if (success)
        {
            SetUsernameStatus($"Welcome, {_usernameInputField.text}!", Color.green);
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

    #region ==================== OFFLINE PLAY HANDLING ====================
    private void OnOfflineButtonClicked()
    {
        if (!_allowOfflinePlay) return;
        
        // Generate a unique offline username
        string offlineUsername = "Player_" + System.DateTime.Now.ToString("HHmmss");
        
        SetUsernameStatus("Setting up offline play...", Color.yellow);
        SetUsernameUIState(false);
        
        StartCoroutine(ProcessOfflineUsername(offlineUsername));
    }
    
    private IEnumerator CompleteUsernameSetupDelayed(bool success, float delay)
    {
        yield return new WaitForSeconds(delay);
        CompleteUsernameSetup(success);
    }
    
    private void CompleteUsernameSetup(bool success)
    {
        OpenGamePanel();
        
        // Reset panel for next time
        ResetUsernamePanel();
        
        if (!_hasInternetConnection && success)
        {
            ShowTemporaryMainMenuMessage("Playing offline - scores saved locally", 3f);
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
            
        if (_offlineButton)
            _offlineButton.interactable = interactive;
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
    
    private void ShowTemporaryMainMenuMessage(string message, float duration)
    {
        // You could implement this by showing a temporary text on the main menu
        // For now, just log it
        Debug.Log($"Main menu message: {message}");
        
    }
    #endregion

    #region ==================== GAME METHODS ====================

    private void OpenUsernamePanel()
    {
        // Hide all other panels
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
        
        _audioSource.PlayOneShot(_audioClip);
        _menuEmpty.SetActive(true);
        _gamePanel.SetActive(true);
    }

    public void OpenGamePanel()
    {
        _optionPanel.SetActive(false);
        _scorePanel.SetActive(false);
        _creditPanel.SetActive(false);
        _usernamePanel.SetActive(false);
        _creditPanel.SetActive(false);
        
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
        
        _audioSource.PlayOneShot(_audioClip);
        _optionPanel.SetActive(true);
    }
    
    public void OpenScorePanel()
    {
        _gamePanel.SetActive(false);
        _optionPanel.SetActive(false);
        _creditPanel.SetActive(false);
        _usernamePanel.SetActive(false);
        
        _audioSource.PlayOneShot(_audioClip);
        _scorePanel.SetActive(true);
        _scoreboardPanel.SetActive(true);
        
        _leaderboardManager.GetScores();
    }
    
    public void OpenCreditPanel()
    {
        _gamePanel.SetActive(false);
        _optionPanel.SetActive(false);
        _scorePanel.SetActive(false);
        _usernamePanel.SetActive(false);
        
        _audioSource.PlayOneShot(_audioClip);
        _creditPanel.SetActive(true);
    }
    
    public void ChangeLangue(int langueID)
    {
        if (_isActive)
            return;
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
    

    // ==================== DEBUG & TESTING ====================
    [ContextMenu("Test: Force Show Username Panel")]
    public void DebugForceShowUsernamePanel()
    {
        // Clear any existing username data
        PlayerPrefs.DeleteKey("Username");
        PlayerPrefs.DeleteKey("UsernameLocked");
        PlayerPrefs.DeleteKey("OfflineUsername");
        PlayerPrefs.Save();
        
        // Force internet check and show panel
        StartCoroutine(DebugResetAndShowPanel());
    }
    
    private IEnumerator DebugResetAndShowPanel()
    {
        yield return StartCoroutine(CheckInternetConnection());
        
        _usernamePanel.SetActive(true);
        _gamePanel.SetActive(false);
        SetupUsernamePanel();
        
        Debug.Log($"Debug: Forcing username panel. Internet: {_hasInternetConnection}");
    }
    
    [ContextMenu("Test: Simulate No Internet")]
    public void DebugSimulateNoInternet()
    {
        _hasInternetConnection = false;
        
        if (_usernamePanel.activeSelf)
        {
            SetupUsernamePanel();
        }
        
        Debug.Log("Debug: Simulating no internet connection");
    }
    
    [ContextMenu("Test: Clear All Usernames")]
    public void DebugClearAllUsernames()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        Debug.Log("Debug: Cleared all username data");
    }
}