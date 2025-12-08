using UnityEngine;

public class ObstacleHandler : MonoBehaviour
{
    public ObstacleData ObstacleData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var playerController))
        {
            //ScreenShake
            FeedbackManager.SmallShake();
            
            EventBus.OnPlayerTakeDamage?.Invoke(ObstacleData.Damage);
            EventBus.OnObstacleHit?.Invoke();
            if (ObstacleData.IsStunning)
            {
                EventBus.OnPlayerTakeStun?.Invoke(ObstacleData.StunTime);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
