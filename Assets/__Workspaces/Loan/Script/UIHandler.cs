using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class UIHandler : MonoBehaviour
{
    [Header("=====Panel Settings=====")]
    [SerializeField] private GameObject _gamePanel;
    [SerializeField] private GameObject _optionPanel;
    [SerializeField] private GameObject _scorePanel;
    [SerializeField] private GameObject _creditPanel;
    [SerializeField] private GameObject _loosePanel;

    [Header("=====Game Settings=====")] 
    [SerializeField] private GameObject _gameEmpty;
    [SerializeField] private GameObject _menuEmpty;

    private bool _isActive;
    private void Start()
    {
        _menuEmpty.SetActive(true);
        _gamePanel.SetActive(true);
        _gameEmpty.SetActive(false);
        _optionPanel.SetActive(false);
        _scorePanel.SetActive(false);
        _creditPanel.SetActive(false);
        _loosePanel.SetActive(false);

        EventBus.OnGameOver += GameLosse;
    }

    private void OnDestroy()
    {
        EventBus.OnGameOver -= GameLosse;
    }

    private void GameLosse()
    {
       _loosePanel.SetActive(true);
       Debug.Log("Loose");
       EventBus.OnGamePause?.Invoke();
    }

    public void GameStart()
    {
        _gameEmpty.SetActive(true);
        _menuEmpty.SetActive(false);
        EventBus.OnGameStart?.Invoke();
    }

    public void ReturnMenu()
    {
        _menuEmpty.SetActive(true);
        _loosePanel.SetActive(false);
        _gameEmpty.SetActive(false);
    }

    public void OpenGamePanel()
    {
        _gamePanel.SetActive(true);
        _optionPanel.SetActive(false);
        _scorePanel.SetActive(false);
        _creditPanel.SetActive(false);
    }
    
    public void OpenOptionPanel()
    {
        _gamePanel.SetActive(false);
        _optionPanel.SetActive(true);
        _scorePanel.SetActive(false);
        _creditPanel.SetActive(false);
    }
    
    public void OpenScorePanel()
    {
        _gamePanel.SetActive(false);
        _optionPanel.SetActive(false);
        _scorePanel.SetActive(true);
        _creditPanel.SetActive(false);
    }
    
    public void OpenCreditPanel()
    {
        _optionPanel.SetActive(false);
        _scorePanel.SetActive(false);
        _creditPanel.SetActive(true);
    }
    
    public void ChangeLangue(int langueID)
    {
        if (_isActive)
            return;
        StartCoroutine(SetLangue(langueID));
    }
    
    IEnumerator SetLangue(int langueID)
    {
        _isActive = true;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[langueID];
        _isActive = false;
    }
}
