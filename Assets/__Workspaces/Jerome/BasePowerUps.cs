using UnityEngine;

public abstract class BasePowerUps : MonoBehaviour
{
    public float Duration = 5f;
    public float Value = 1f;

    protected abstract void ExecuteEffect(GameObject player);

    public void ApplyEffect(GameObject player)
    {
        ExecuteEffect(player);
        PlayCollectionEffects();
        Destroy(gameObject);
    }

    private void PlayCollectionEffects()
    {
        // Common collection effects
    }
}
