// A base class for all upgrade events
public abstract class UpgradeEvent
{
    public UpgradeType Type;
    public float Value;
}

// Specific events for different categories
public class StatUpgradeEvent : UpgradeEvent { }
public class WeaponUpgradeEvent : UpgradeEvent { }
public class GameSystemUpgradeEvent : UpgradeEvent { }