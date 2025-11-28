using UnityEngine;

public class Dead : BaseState
{
    public override void Excute(EnemyHandler enemy)
    {
        enemy.Death(); 
    }

    
}
