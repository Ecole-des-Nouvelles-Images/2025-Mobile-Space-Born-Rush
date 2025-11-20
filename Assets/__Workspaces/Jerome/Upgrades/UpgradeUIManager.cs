using System.Collections.Generic;
using UnityEngine;

public class UpgradeUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private UpgradeChoiceUI[] choiceSlots; // Your UI button slots
    [SerializeField] private Animator panelAnimator; // Optional for animations

    private List<UpgradeData> currentChoices;
    private UpgradeManager upgradeManager;

    private void Start()
    {
        upgradeManager = FindObjectOfType<UpgradeManager>();
        if (upgradeManager == null)
        {
            Debug.LogError("UpgradeManager not found in scene!");
        }
        
        upgradePanel.SetActive(false);
        
        // Hide all slots initially
        foreach (var slot in choiceSlots)
        {
            slot.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // Listen for when upgrade choices are ready (from your EventBus)
        EventBus.OnPlayerLevelUp += ShowUpgradeChoices;
        
        // Listen for when upgrades are applied to potentially close UI
        EventBus.OnUpgradeApplied += OnUpgradeApplied;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerLevelUp -= ShowUpgradeChoices;
        EventBus.OnUpgradeApplied -= OnUpgradeApplied;
    }

    // This is called when EventBus.OnPlayerLevelUp fires with upgrade choices
    private void ShowUpgradeChoices(List<UpgradeData> upgradeChoices)
    {
        if (upgradeChoices == null || upgradeChoices.Count == 0)
        {
            Debug.LogError("No upgrade choices provided!");
            CloseUpgradePanel(); // Ensure UI doesn't get stuck open
            return;
        }

        if (upgradeChoices.Count > choiceSlots.Length)
        {
            Debug.LogWarning($"More choices ({upgradeChoices.Count}) than available slots ({choiceSlots.Length})");
        }

        currentChoices = upgradeChoices;
        Debug.Log($"Displaying {upgradeChoices.Count} upgrade choices");

        // Display each choice in available UI slots
        for (int i = 0; i < choiceSlots.Length; i++)
        {
            if (i < upgradeChoices.Count)
            {
                choiceSlots[i].gameObject.SetActive(true);
                choiceSlots[i].DisplayUpgrade(upgradeChoices[i], i);
                choiceSlots[i].SetSelectionCallback(OnUpgradeSelected); // FIXED: Connect callback
            }
            else
            {
                choiceSlots[i].gameObject.SetActive(false);
            }
        }

        // Show the upgrade panel
        upgradePanel.SetActive(true);
        
        // Optional: Trigger animation
        if (panelAnimator != null)
            panelAnimator.SetTrigger("Show");
        
        // Pause the game while player chooses
        PauseGame();
    }

    // Called by UI buttons when player selects an upgrade
    public void OnUpgradeSelected(int choiceIndex)
    {
        if (upgradeManager == null)
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
        
        // Note: The UI will close when OnUpgradeApplied is fired (see method below)
    }

    // Called when any upgrade is applied (could be from this UI or other sources)
    private void OnUpgradeApplied(UpgradeData upgradedData)
    {
        // FIXED: Only close if this was one of our current choices
        if (currentChoices != null && currentChoices.Contains(upgradedData))
        {
            CloseUpgradePanel();
        }
    }

    private void CloseUpgradePanel()
    {
        // Optional: Trigger hide animation
        if (panelAnimator != null)
        {
            panelAnimator.SetTrigger("Hide");
            // You might want to use an animation event to actually disable the panel
            Invoke(nameof(DisablePanel), 0.3f); // Match animation duration
        }
        else
        {
            DisablePanel();
        }
    }

    private void DisablePanel()
    {
        upgradePanel.SetActive(false);
        ResumeGame();
        
        // Clear current choices
        currentChoices = null;
        
        // Hide all slots
        foreach (var slot in choiceSlots)
        {
            slot.gameObject.SetActive(false);
        }
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