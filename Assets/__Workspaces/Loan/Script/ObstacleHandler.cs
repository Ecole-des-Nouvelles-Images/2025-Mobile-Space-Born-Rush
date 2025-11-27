using UnityEngine;

public class ObstacleHandler : MonoBehaviour
{
    public ObstacleData ObstacleData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (ObstacleData.IsStunning)
            {
                EventBus.OnPlayerTakeDamage?.Invoke(ObstacleData.Damage);
                EventBus.OnPlayerTakeStun?.Invoke(ObstacleData.StunTime);
            }
            else
            {
                EventBus.OnPlayerTakeDamage?.Invoke(ObstacleData.Damage);
                Destroy(gameObject);
            }
        }
    }
}
