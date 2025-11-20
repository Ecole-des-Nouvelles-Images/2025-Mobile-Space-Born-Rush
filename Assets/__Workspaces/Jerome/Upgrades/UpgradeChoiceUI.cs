using System;
using UnityEngine;

public class UpgradeChoiceUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMPro.TextMeshProUGUI nameText;
    [SerializeField] private TMPro.TextMeshProUGUI descriptionText;
    [SerializeField] private UnityEngine.UI.Image iconImage;
    [SerializeField] private UnityEngine.UI.Button selectButton;

    private int choiceIndex;
    private Action<int> onSelectedCallback;

    public void DisplayUpgrade(UpgradeData upgrade, int index)
    {
        choiceIndex = index;
        
        // Update UI elements
        nameText.text = upgrade.Name;
        descriptionText.text = upgrade.Description;
        iconImage.sprite = upgrade.Icon;
        
        // Remove previous listeners and add new one
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnSelectButtonClicked);
    }

    private void OnSelectButtonClicked()
    {
        onSelectedCallback?.Invoke(choiceIndex);
    }

    // Called by parent UpgradeUIManager
    public void SetSelectionCallback(Action<int> callback)
    {
        onSelectedCallback = callback;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}