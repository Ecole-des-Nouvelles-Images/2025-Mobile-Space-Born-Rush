using UnityEngine;

public class Dead : BaseState
{
    private bool _first;
    public override void Excute(EnemyHandler enemy)
    {
        if (!_first)
        {
            EventBus.OnEnemyDead?.Invoke(enemy.gameObject);
            _first = true;
        }
    }

    
}
