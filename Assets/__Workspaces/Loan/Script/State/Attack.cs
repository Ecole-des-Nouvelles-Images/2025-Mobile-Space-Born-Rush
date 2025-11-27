using UnityEngine;

public class Attack : BaseState
{
    private float _fireRate = 1f;
    public override void Excute(EnemyHandler enemy)
    {
        if (enemy.IsRange)
        {
            enemy.Raycast = new Ray(enemy.Transform.position, enemy.Transform.forward);
            if (Physics.Raycast(enemy.Raycast,out RaycastHit hit,enemy.Range,enemy.EnemyMask))
            {
                if (hit.collider.TryGetComponent<PlayerController>(out var player))
                {
                    return;
                }
            }
            else
            {
                enemy.SpeedDirection = new Vector3(0, 0, -enemy.GameUpgrades.GameSpeed);
                enemy.IsAttacking = false;
                enemy.IsWait =  true;
            }
        }
        else
        {
            // enemy.Raycast = new Ray(enemy.Transform.position, enemy.Transform.forward);
            if (Physics.Raycast(enemy.Raycast,out RaycastHit hit,enemy.Range,enemy.EnemyMask))
            {
                if (hit.collider.TryGetComponent<PlayerController>(out var player))
                {
                    
                    if (enemy.CurrentFireRate <= 0)
                    {
                        player.TakeDamage(enemy.Damage);
                        enemy.CurrentFireRate = _fireRate;
                    }
                    else
                    {
                        enemy.CurrentFireRate -= Time.deltaTime;
                    }
                    
                }
            }
            else
            {
                enemy.SpeedDirection = new Vector3(0, 0, -enemy.GameUpgrades.GameSpeed);
                enemy.IsAttacking = false;
                enemy.IsWait =  true;
            }
        }
    }
}
