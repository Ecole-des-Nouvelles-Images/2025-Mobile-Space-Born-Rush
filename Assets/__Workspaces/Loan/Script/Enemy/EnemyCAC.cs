using UnityEngine;

public class EnemyCAC : EnemyHandler
{
    private bool _firstDead = true;
    private bool _blending = false;
    private float _currentBlend = 0f;
    void Update()
    {
        
        if (IsPaused)
            return;
        float random = Random.Range(0f, 1f);

        if (_blending)
        {
            _currentBlend = Mathf.Lerp(_currentBlend,1f,Time.deltaTime * 5 );
        }
        
        if (!_blending && random < 0.15f * Time.deltaTime )
        {
            TriggerBlend();
        }
        
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

    private void TriggerBlend()
    {
        Animator.SetFloat("Blend", _currentBlend);
        _blending = true;
    }

    public void ResetTriggerBlend()
    {
        Animator.SetFloat("Blend", 0f);
        _blending = false;
    }
}
