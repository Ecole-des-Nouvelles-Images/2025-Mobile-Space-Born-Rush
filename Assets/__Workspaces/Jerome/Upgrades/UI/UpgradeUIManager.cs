using System.Collections.Generic;
using UnityEngine;

public class UpgradeUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private UpgradeChoiceUI upgradeChoicePrefab; // The prefab reference
    [SerializeField] private Transform choicesParent; // Parent to instantiate under
    [SerializeField] private Animator panelAnimator;

    private List<UpgradeData> currentChoices;
    private List<UpgradeChoiceUI> activeChoiceInstances; // Track spawned instances
    private UpgradeManager upgradeManager;

    private void Start()
    {
        upgradeManager = FindObjectOfType<UpgradeManager>();
        if (upgradeManager == null)
        {
            Debug.LogError("UpgradeManager not found in scene!");
        }
        
        upgradePanel.SetActive(false);
        activeChoiceInstances = new List<UpgradeChoiceUI>();
    }

    private void OnEnable()
    {
        EventBus.OnPlayerLevelUp += ShowUpgradeChoices;
        EventBus.OnUpgradeApplied += OnUpgradeApplied;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerLevelUp -= ShowUpgradeChoices;
        EventBus.OnUpgradeApplied -= OnUpgradeApplied;
    }

    private void ShowUpgradeChoices(List<UpgradeData> upgradeChoices)
    {
        if (upgradeChoices == null || upgradeChoices.Count == 0)
        {
            Debug.LogError("No upgrade choices provided!");
            return;
        }

        if (!upgradeChoicePrefab)
        {
            Debug.LogError("UpgradeChoicePrefab is not assigned!");
            return;
        }

        currentChoices = upgradeChoices;

        // Clear previous choices
        ClearExistingChoices();

        // Create new choice instances
        for (int i = 0; i < upgradeChoices.Count; i++)
        {
            UpgradeChoiceUI choiceInstance = CreateChoiceInstance(upgradeChoices[i], i);
            activeChoiceInstances.Add(choiceInstance);
        }

        // Show the upgrade panel
        upgradePanel.SetActive(true);
        
        if (panelAnimator)
            panelAnimator.SetTrigger("Show");
        
        PauseGame();
    }

    private UpgradeChoiceUI CreateChoiceInstance(UpgradeData upgrade, int index)
    {
        // Instantiate the prefab
        UpgradeChoiceUI choiceInstance = Instantiate(upgradeChoicePrefab, choicesParent);
        
        // Set up the choice
        choiceInstance.DisplayUpgrade(upgrade, index);
        choiceInstance.SetSelectionCallback(OnUpgradeSelected);
        
        return choiceInstance;
    }

    private void ClearExistingChoices()
    {
        // Destroy all active choice instances
        foreach (UpgradeChoiceUI choice in activeChoiceInstances)
        {
            if (choice)
            {
                Destroy(choice.gameObject);
            }
        }
        activeChoiceInstances.Clear();
    }

    // Called by UI buttons when player selects an upgrade
    public void OnUpgradeSelected(int choiceIndex)
    {
        if (!upgradeManager)
        {
            Debug.LogError("UpgradeManager not found!");
            CloseUpgradePanel();
            return;
        }

        if (currentChoices == null || choiceIndex < 0 || choiceIndex >= currentChoices.Count)
        {
            Debug.LogError($"Invalid upgrade selection: {choiceIndex}");
            CloseUpgradePanel();
            return;
        }

        UpgradeData selectedUpgrade = currentChoices[choiceIndex];
        Debug.Log($"Player selected: {selectedUpgrade.Name}");

        // Apply the upgrade through the UpgradeManager
        upgradeManager.ApplyUpgrade(selectedUpgrade);
    }

    private void OnUpgradeApplied(UpgradeData upgradedData)
    {
        if (currentChoices != null && currentChoices.Contains(upgradedData))
        {
            CloseUpgradePanel();
        }
    }

    private void CloseUpgradePanel()
    {
        if (panelAnimator)
        {
            panelAnimator.SetTrigger("Hide");
            Invoke(nameof(DisablePanel), 0.3f);
        }
        else
        {
            DisablePanel();
        }
    }

    private void DisablePanel()
    {
        upgradePanel.SetActive(false);
        ClearExistingChoices(); // Clean up instances
        ResumeGame();
        currentChoices = null;
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