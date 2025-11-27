using UnityEngine;

public class EnemyRange : EnemyHandler
{
    void Update()
    {
        if (IsWait)
        {
            CurrentState = new Wait();
        }

        if (IsAttacking)
        {
            CurrentState = new Attack();
        }

        if (IsDead)
        {
            EventBus.OnEnemyDead?.Invoke(gameObject);
            CurrentState = new Dead();
        }
        
        CurrentState.Excute(this);
    }
}
