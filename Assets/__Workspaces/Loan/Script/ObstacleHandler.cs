using UnityEngine;

public class ObstacleHandler : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public ObstacleData ObstacleData;
    [SerializeField] private ParticleSystem _particleSystem;
    [Header("Sound Settings")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _audioClip;

    private void OnEnable()
    {
        if (_audioSource == null)
        {
            _audioSource = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController playerController))
        {
            EventBus.OnPlayerTakeDamage?.Invoke(ObstacleData.Damage);
            EventBus.OnObstacleHit?.Invoke();
            _audioSource.PlayOneShot(_audioClip);
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
