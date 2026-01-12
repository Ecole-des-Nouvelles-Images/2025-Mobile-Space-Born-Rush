using System.Collections.Generic;
using UnityEngine;

public class UpgradeUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _upgradeCanvas;
    [SerializeField] private UpgradeChoiceUI _upgradeChoicePrefab; // The prefab reference
    [SerializeField] private Transform _choicesParent; // Parent to instantiate under

    private List<UpgradeData> _currentChoices;
    private List<UpgradeChoiceUI> _activeChoiceInstances; // Track spawned instances
    private UpgradeManager _upgradeManager;
    private UpgradeGenerator _upgradeGenerator;

    private void Start()
    {
        _upgradeManager = FindAnyObjectByType<UpgradeManager>();
        if (!_upgradeManager) Debug.LogError("UpgradeManager not found in scene!");
        
        _upgradeGenerator = FindAnyObjectByType<UpgradeGenerator>();
        if (!_upgradeGenerator) Debug.LogError("UpgradeGenerator not found in scene!");
        
        _upgradeCanvas.SetActive(false);
        _activeChoiceInstances = new List<UpgradeChoiceUI>();
    }

    private void OnEnable()
    {
        EventBus.OnPlayerLevelUp += ShowUpgradeChoices;
        EventBus.OnUpgradeApplied += OnUpgradeApplied;
    }

    private void OnUpgradeApplied(UpgradeData upgradedData)
    {
        if (_currentChoices != null && _currentChoices.Contains(upgradedData))
        {
            CloseUpgradePanel();
        }
    }
    private void ShowUpgradeChoices(List<UpgradeData> upgradeChoices)
    {
        if (upgradeChoices == null || upgradeChoices.Count == 0)
        {
            Debug.LogError("No upgrade choices provided!");
            return;
        }

        if (!_upgradeChoicePrefab)
        {
            Debug.LogError("UpgradeChoicePrefab is not assigned!");
            return;
        }

        _currentChoices = upgradeChoices;

        // Clear previous choices
        ClearExistingChoices();

        // Create new choice instances
        for (int i = 0; i < upgradeChoices.Count; i++)
        {
            UpgradeChoiceUI choiceInstance = CreateChoiceInstance(upgradeChoices[i], i);
            _activeChoiceInstances.Add(choiceInstance);
        }

        // Show the upgrade panel
        _upgradeCanvas.SetActive(true);
        
        PauseGame();
    }
    private void OnDisable()
    {
        EventBus.OnPlayerLevelUp -= ShowUpgradeChoices;
        EventBus.OnUpgradeApplied -= OnUpgradeApplied;
    }


    private UpgradeChoiceUI CreateChoiceInstance(UpgradeData upgrade, int index)
    {
        // Instantiate the prefab
        UpgradeChoiceUI choiceInstance = Instantiate(_upgradeChoicePrefab, _choicesParent);
        
        // Set up the choice
        choiceInstance.DisplayUpgrade(upgrade, index);
        choiceInstance.SetSelectionCallback(OnUpgradeSelected);
        
        return choiceInstance;
    }

    private void ClearExistingChoices()
    {
        // Destroy all active choice instances
        foreach (UpgradeChoiceUI choice in _activeChoiceInstances)
        {
            if (choice)
            {
                Destroy(choice.gameObject);
            }
        }
        _activeChoiceInstances.Clear();
    }

    // Called by UI buttons when player selects an upgrade
    private void OnUpgradeSelected(int choiceIndex)
    {
        if (!_upgradeManager)
        {
            Debug.LogError("UpgradeManager not found!");
            CloseUpgradePanel();
            return;
        }

        if (_currentChoices == null || choiceIndex < 0 || choiceIndex >= _currentChoices.Count)
        {
            Debug.LogError($"Invalid upgrade selection: {choiceIndex}");
            CloseUpgradePanel();
            return;
        }

        UpgradeData selectedUpgrade = _currentChoices[choiceIndex];

        // Apply the upgrade through the UpgradeManager
        _upgradeManager.ApplyUpgrade(selectedUpgrade);
        _upgradeGenerator.SelectedIncrement(selectedUpgrade);
        
    }


    private void CloseUpgradePanel()
    {
            DisablePanel();
    }

    private void DisablePanel()
    {
        _upgradeCanvas.SetActive(false);
        ClearExistingChoices(); // Clean up instances
        ResumeGame();
        _currentChoices = null;
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        EventBus.OnGamePause?.Invoke();
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        EventBus.OnGameResume?.Invoke();
    }
}