using UnityEngine;

public class Wait : BaseState
{
    public override void Excute(EnemyHandler enemy)
    {
        if (Physics.Raycast(enemy.Raycast,out RaycastHit hit,enemy.CurrentRange,enemy.EnemyMask))
        {
            // if (hit.collider.TryGetComponent<PlayerController>(out var player))
            // {
            //     enemy.IsAttacking = true;
            //     enemy.IsWait = false;
            // }
            
            enemy.IsAttacking = true;
            enemy.IsWait = false;
        }
    }
}
