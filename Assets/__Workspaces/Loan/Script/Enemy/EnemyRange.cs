using UnityEngine;

public class EnemyRange : EnemyHandler
{
    [SerializeField] private GameObject _weapon;
    private bool _isFirstDead = true;
    void Update()
    {
        if (IsPaused)
            return;
        if (IsWait)
        {
            CurrentState = new Wait();
        }

        if (IsAttacking)
        {
            CurrentState = new Attack();
        }

        if (IsDead && _isFirstDead)
        {
            CurrentState = new Dead();
            Death(this.gameObject);
            _isFirstDead = false;
            Destroy(_weapon);
        }
        
        CurrentState.Excute(this);
    }
}
