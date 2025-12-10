using UnityEngine;

public class EnemyCAC : EnemyHandler
{
    void Update()
    {
        if (IsPaused)
            return;
        Raycast = new Ray(Transform.position, Transform.forward);
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
        
        Debug.DrawRay(Raycast.origin, Raycast.direction * CurrentRange, Color.blue);
    }
}
