using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    // Called when player selects an upgrade in the UI
    public void ApplyUpgrade(UpgradeData upgrade)
    {
        // First, broadcast the generic "upgrade applied" event
        EventBus.OnUpgradeApplied?.Invoke(upgrade);
        
        // Then broadcast the specific category event
        switch (upgrade.category)
        {
            case UpgradeCategory.PlayerStat:
                EventBus.OnStatUpgrade?.Invoke(upgrade.Type, upgrade.Value);
                break;
                
            case UpgradeCategory.WeaponStat:
                EventBus.OnWeaponUpgrade?.Invoke(upgrade.Type, upgrade.Value);
                break;
                
            case UpgradeCategory.GameSystem:
                EventBus.OnGameSystemUpgrade?.Invoke(upgrade.Type, upgrade.Value);
                break;
        }
        
        Debug.Log($"Applied upgrade: {upgrade.Name}");
    }
}