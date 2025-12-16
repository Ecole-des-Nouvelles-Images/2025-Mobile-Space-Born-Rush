using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeChoiceUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button selectButton;

    private int choiceIndex;
    private System.Action<int> onSelectedCallback;
    

    public void DisplayUpgrade(UpgradeData upgrade, int index)
    {
        if (!upgrade)
        {
            Debug.LogError("Cannot display null upgrade!");
            gameObject.SetActive(false);
            return;
        }

        choiceIndex = index;
        
        // Update UI elements
        if (nameText)
            nameText.text = upgrade.Name ?? "Missing Name";
        
        if (descriptionText)
            descriptionText.text = upgrade.Description ?? "Missing Description";
        
            iconImage.sprite = upgrade.Icon;
            iconImage.color = upgrade.Icon ? Color.white : new Color(0.8f, 0.8f, 0.8f);

        
        if (selectButton)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnSelectButtonClicked);
        }

        gameObject.SetActive(true);
    }

    private void OnSelectButtonClicked()
    {
        onSelectedCallback?.Invoke(choiceIndex);
    }

    public void SetSelectionCallback(System.Action<int> callback)
    {
        onSelectedCallback = callback;
    }

    // Called when this instance is destroyed
    private void OnDestroy()
    {
        // Clean up listeners to prevent memory leaks
        if (selectButton != null)
            selectButton.onClick.RemoveAllListeners();
    }
}