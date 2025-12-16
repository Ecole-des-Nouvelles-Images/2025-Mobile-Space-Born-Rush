using UnityEngine;

public class EnemyCAC : EnemyHandler
{
    private bool _firstDead = true;
    void Update()
    {
        if (IsPaused)
            return;
        Raycast = new Ray(OriginRaycast.position, Transform.forward);
        if (IsWait)
        {
            CurrentState = new Wait();
        }

        if (IsAttacking)
        {
            CurrentState = new Attack();
        }

        if (IsDead && _firstDead)
        {
            CurrentState = new Dead();
            Death(this.gameObject);
            _firstDead = false;
        }
        
        CurrentState.Excute(this);
        
        Debug.DrawRay(Raycast.origin, Raycast.direction * CurrentRange, Color.blue);
    }
}
