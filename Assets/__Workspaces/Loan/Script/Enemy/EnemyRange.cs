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
            CurrentState = new Dead();
        }
        
        CurrentState.Excute(this);
    }
}
