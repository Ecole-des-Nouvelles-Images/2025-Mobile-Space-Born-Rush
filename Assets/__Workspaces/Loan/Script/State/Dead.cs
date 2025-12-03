using UnityEngine;

public class Dead : BaseState
{
    public override void Excute(EnemyHandler enemy)
    {
        EventBus.OnEnemyDead?.Invoke(enemy.gameObject);
    }

    
}
