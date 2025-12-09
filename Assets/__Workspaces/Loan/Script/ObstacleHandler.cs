using UnityEngine;

public class ObstacleHandler : MonoBehaviour
{
    public ObstacleData ObstacleData;
    [SerializeField] private ParticleSystem _particleSystem;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var playerController))
        {
            EventBus.OnPlayerTakeDamage?.Invoke(ObstacleData.Damage);
            EventBus.OnObstacleHit?.Invoke();
            if (ObstacleData.IsStunning)
            {
                EventBus.OnPlayerTakeStun?.Invoke(ObstacleData.StunTime);
            }
            else
            {
                if (_particleSystem != null)
                {
                    _particleSystem.Play();
                }
                Destroy(gameObject);
            }
        }
    }
}
