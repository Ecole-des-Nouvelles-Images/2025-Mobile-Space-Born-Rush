using System.Collections.Generic;
using UnityEngine;

public class UpgradeUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _upgradeCanvas;
    [SerializeField] private UpgradeChoiceUI _upgradeChoicePrefab; // The prefab reference
    [SerializeField] private Transform _choicesParent; // Parent to instantiate under

    private List<UpgradeData> _currentChoices;
    private readonly List<UpgradeChoiceUI> _activeChoiceInstances = new(); // Track spawned instances
    // Level-ups received while a choice is already on screen (two XP drops in the same frame, ...).
    private readonly Queue<List<UpgradeData>> _pendingChoices = new();
    private const float MinDisplayTime = 0.3f; // unscaled seconds before a card can be picked
    private float _shownAt = float.NegativeInfinity;
    private int _shownId; // incremented each time a set of cards is displayed
    private UpgradeManager _upgradeManager;
    private UpgradeGenerator _upgradeGenerator;

    private void Start()
    {
        _upgradeManager = FindAnyObjectByType<UpgradeManager>();
        if (!_upgradeManager) Debug.LogError("UpgradeManager not found in scene!");

        _upgradeGenerator = FindAnyObjectByType<UpgradeGenerator>();
        if (!_upgradeGenerator) Debug.LogError("UpgradeGenerator not found in scene!");

        // Start runs on the first activation of "Game": don't hide a choice that is already displayed.
        if (_currentChoices == null)
            _upgradeCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        EventBus.OnPlayerLevelUp += ShowUpgradeChoices;
        EventBus.OnUpgradeApplied += OnUpgradeApplied;
        EventBus.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerLevelUp -= ShowUpgradeChoices;
        EventBus.OnUpgradeApplied -= OnUpgradeApplied;
        EventBus.OnGameOver -= OnGameOver;
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

        // A choice is already open: keep this level-up for after (it used to replace the current one).
        if (_currentChoices != null)
        {
            _pendingChoices.Enqueue(upgradeChoices);
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
        _shownAt = Time.unscaledTime;
        _shownId++;

        PauseGame();
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
                // Hide now: Destroy only happens at the end of the frame and the old cards would
                // still be selectable / clickable (and pushed around by the layout) until then.
                choice.gameObject.SetActive(false);
                Destroy(choice.gameObject);
            }
        }
        _activeChoiceInstances.Clear();
    }

    // Called by UI buttons when player selects an upgrade
    private void OnUpgradeSelected(int choiceIndex)
    {
        // Second click / A press in the same frame, after the panel was already closed.
        if (_currentChoices == null) return;

        // Cards just appeared (e.g. the next queued level-up): ignore the click so a double click /
        // A spam can't pick an upgrade the player hasn't even seen.
        if (Time.unscaledTime - _shownAt < MinDisplayTime) return;

        if (!_upgradeManager)
        {
            Debug.LogError("UpgradeManager not found!");
            CloseUpgradePanel();
            return;
        }

        if (choiceIndex < 0 || choiceIndex >= _currentChoices.Count)
        {
            Debug.LogError($"Invalid upgrade selection: {choiceIndex}");
            CloseUpgradePanel();
            return;
        }

        UpgradeData selectedUpgrade = _currentChoices[choiceIndex];
        int shownId = _shownId;

        // Apply the upgrade through the UpgradeManager
        _upgradeManager.ApplyUpgrade(selectedUpgrade);
        if (_upgradeGenerator) _upgradeGenerator.SelectedIncrement(selectedUpgrade);

        // Never stay frozen (timeScale 0) if OnUpgradeApplied did not close the panel.
        // Only close *this* choice: OnUpgradeApplied may already have opened the next queued one.
        if (_currentChoices != null && _shownId == shownId) CloseUpgradePanel();
    }

    private void CloseUpgradePanel()
    {
        DisablePanel();

        // Next queued level-up, if any.
        if (_pendingChoices.Count > 0)
            ShowUpgradeChoices(_pendingChoices.Dequeue());
    }

    private void DisablePanel()
    {
        _upgradeCanvas.SetActive(false);
        ClearExistingChoices(); // Clean up instances
        ResumeGame();
        _currentChoices = null;
    }

    private void OnGameOver()
    {
        // Back to the menu: nothing must stay open or queued for the next run.
        _pendingChoices.Clear();
        if (_currentChoices != null)
        {
            _upgradeCanvas.SetActive(false);
            ClearExistingChoices();
            _currentChoices = null;
            Time.timeScale = 1f;
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
