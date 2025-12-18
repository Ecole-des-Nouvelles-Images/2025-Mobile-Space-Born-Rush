using UnityEngine;

public class EnemyRange : EnemyHandler
{
    [SerializeField] private GameObject _weapon;
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

        if (IsDead)
        {
            CurrentState = new Dead();
            Destroy(_weapon);
        }
        
        CurrentState.Excute(this);
    }
}
