using UnityEngine;

public class KillTriggerHandler : MonoBehaviour
{
    [SerializeField] private float _percent;
    private PlayerStatController _playerStat;
    [SerializeField]private LayerMask _layerMask;

    private void Start()
    {
        _playerStat = FindAnyObjectByType<PlayerStatController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //TODO Swap to pooling
        if (other.gameObject.CompareTag("Enemy"))
        {
            EventBus.OnEnemyGotPastPlayer?.Invoke();
            Destroy(other.gameObject);
        }
    }
}
