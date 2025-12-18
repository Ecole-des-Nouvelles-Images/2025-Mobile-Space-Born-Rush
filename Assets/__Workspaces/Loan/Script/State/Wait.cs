using UnityEngine;

public class Wait : BaseState
{
    public override void Excute(EnemyHandler enemy)
    {
        if (enemy.IsRange)
        {
            if (Physics.Raycast(enemy.Raycast, out RaycastHit hit, enemy.CurrentRange, enemy.EnemyMask))
            {
                enemy.IsAttacking = true;
                enemy.IsWait = false;
            }
        }
        else
        {
            Ray downRay = new Ray(enemy.DownRaycast.position, enemy.Transform.forward);
            Ray UpRay = new Ray(enemy.UpRaycast.position, enemy.Transform.forward);
            if (Physics.Raycast(enemy.Raycast,out RaycastHit hit,enemy.CurrentRange,enemy.EnemyMask) || 
                Physics.Raycast(downRay,out RaycastHit hite,enemy.CurrentRange,enemy.EnemyMask)||
                Physics.Raycast(UpRay,out RaycastHit hitte,enemy.CurrentRange,enemy.EnemyMask))
            {
                enemy.IsAttacking = true;
                enemy.IsWait = false;
            }
        }
    }
}
