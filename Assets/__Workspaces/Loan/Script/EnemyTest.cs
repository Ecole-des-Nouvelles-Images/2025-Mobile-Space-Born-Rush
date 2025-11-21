using UnityEngine;

public class EnemyTest : MonoBehaviour, IDamageable
{
    
    private float _health = 10;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float damage)
    {
        if (_health > 0)
        {
            _health -= damage;
            Debug.Log(_health);
        }
        else
        {
            Debug.Log("Die !");
        }
    }
}
