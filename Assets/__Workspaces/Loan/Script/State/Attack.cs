using UnityEngine;

public class Attack : BaseState
{
    private float _fireRate = 1f;
    public override void Excute(EnemyHandler enemy)
    {
        if (enemy.IsRange)
        {
            enemy.Raycast = new Ray(enemy.Transform.position, enemy.Transform.forward);
            if (Physics.Raycast(enemy.Raycast,out RaycastHit hit,enemy.CurrentRange,enemy.EnemyMask))
            {
                if (hit.collider.TryGetComponent<PlayerController>(out var player))
                {
                    if (enemy.Animator != null)
                    {
                        enemy.Animator.SetBool("isAim", true);
                    }
                    return;
                }
            }
            else
            {
                enemy.IsAttacking = false;
                if (enemy.Animator != null)
                {
                    enemy.Animator.SetBool("isAim", false);
                }
                enemy.IsWait =  true;
            }
        }
        else
        {
            enemy.Raycast = new Ray(enemy.OriginRaycast.position, enemy.Transform.forward);
            Ray downRay = new Ray(enemy.DownRaycast.position, enemy.Transform.forward);
            Ray UpRay = new Ray(enemy.UpRaycast.position, enemy.Transform.forward);
            
            if (Physics.Raycast(enemy.Raycast,out RaycastHit hit,enemy.CurrentRange,enemy.EnemyMask))
            {
                if (hit.collider.TryGetComponent<PlayerController>(out var player))
                {
                    if (enemy.CurrentFireRate <= 0)
                    {
                        if (enemy.Animator != null)
                        {
                            enemy.Animator.SetTrigger("isAttack");
                        }
                        player.TakeDamage(enemy.CurrentDamage);
                        enemy.CurrentFireRate = _fireRate;
                    }
                    else
                    {
                        enemy.CurrentFireRate -= Time.deltaTime;
                    }
                    
                }
            }
            else if (Physics.Raycast(downRay,out RaycastHit hite,enemy.CurrentRange,enemy.EnemyMask))
            {
                if (hite.collider.TryGetComponent<PlayerController>(out var player))
                {
                    if (enemy.CurrentFireRate <= 0)
                    {
                        if (enemy.Animator != null)
                        {
                            enemy.Animator.SetTrigger("isAttack");
                        }
                        player.TakeDamage(enemy.CurrentDamage);
                        enemy.CurrentFireRate = _fireRate;
                    }
                    else
                    {
                        enemy.CurrentFireRate -= Time.deltaTime;
                    }
                    
                }
            }
            else if (Physics.Raycast(UpRay,out RaycastHit hits,enemy.CurrentRange,enemy.EnemyMask))
            {
                if (hits.collider.TryGetComponent<PlayerController>(out var player))
                {
                    if (enemy.CurrentFireRate <= 0)
                    {
                        if (enemy.Animator != null)
                        {
                            enemy.Animator.SetTrigger("isAttack");
                        }
                        player.TakeDamage(enemy.CurrentDamage);
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
                enemy.IsAttacking = false;
                enemy.IsWait =  true;
            }
        }
    }
}
