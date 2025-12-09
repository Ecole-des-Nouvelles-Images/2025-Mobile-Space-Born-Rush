using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyHandler : MonoBehaviour, IDamageable
{
    [Header("=====General Settings=====")] 
    public EnemyData EnemyData;
    public float CurrentHealth;
    public Vector3 SpeedDirection;
    public Transform Transform;
    public Rigidbody Rb;
    public GameUpgradeController GameUpgrades;
    public GameDirector GameDirector;

    [Header("<color=red>=====Attack Settings=====</color>")]
    // public WeaponData WeaponData;
    public float CurrentDamage;
    public bool IsRange;
    public float CurrentRange;
    public Ray Raycast;
    public float CurrentFireRate; 
    
    [Header("<color=cyan>======Animator======</color>")]
    public Animator Animator;
    
    [FormerlySerializedAs("HitParticles")]
    [Header("<color=blue>======Particule System======</color>")]
    [SerializeField] ParticleSystem _hitParticles;
    
    [Header("<color=magenta>=====Other Settings=====</color>")]
    public Vector3 KnockBackVelocity;
    public bool IsKnocked;
    public LayerMask EnemyMask;
    public GameObject XpPrefab;
    
    [Header("<color=yellow>=====Bool State Settings=====</color>")]
    public bool IsWait;
    public bool IsAttacking;
    public bool IsDead;
    
    
    public BaseState CurrentState = new Wait();
    
    private float _knockbackTimer = 3f;
    private float _knockbackForceMax = 8f;
    private float[] _lanes = new[] { -1.5f, 0f, 1.5f };

    private void OnEnable()
    {
        EventBus.OnEnemyDead += Death;
        EventBus.OnGameOver += GameOver;
    }

    private void OnDestroy()
    {
        EventBus.OnEnemyDead -= Death;
        EventBus.OnGameOver -= GameOver;
    }
    private void Start()
    {
        Rb = GetComponent<Rigidbody>();
        Transform = GetComponent<Transform>();
        GameUpgrades = FindAnyObjectByType<GameUpgradeController>();
        GameDirector = FindObjectOfType<GameDirector>();
        CurrentHealth = EnemyData.MaxHealth + GameDirector.DifficultyHealth;
        CurrentFireRate = EnemyData.FireRate;
        CurrentDamage = EnemyData.Damage + GameDirector.DifficultyDamage;
        IsWait =  true;
        if (Animator)
        {
            Animator = GetComponent<Animator>();
        }

        if (IsRange)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }

    private void FixedUpdate()
    {
        KnockBackVelocity = Vector3.Lerp(KnockBackVelocity, Vector3.zero, _knockbackTimer * Time.fixedDeltaTime);
        SpeedDirection = new Vector3(0f, 0f, -GameUpgrades.GameSpeed);
        
        Rb.linearVelocity = SpeedDirection +  KnockBackVelocity;
        
        if (!IsKnocked && KnockBackVelocity.magnitude < 0.05f)
        {
            SnapToLanes();
            IsKnocked = true;
        }
        
        if (KnockBackVelocity.magnitude > 0.1f)
        {
            IsKnocked = false;
        }

        if (CurrentHealth <= 0)
        {
            IsDead = true;
            IsWait = false;
            IsAttacking = false;
        }
    }
    public void TakeDamage(float damage)
    {
        if (Animator)
        {
            Animator.SetTrigger("isHit");
        }
        _hitParticles.Play();
        CurrentHealth -= damage;
    }

    private void Death(GameObject gameObject)
    {
        // if (Animator != null)
        // {
        //     Animator.SetTrigger("isDie");
        // }
        // if (XpPrefab != null)
        // {
        //     Instantiate(XpPrefab, transform.position, Quaternion.identity);
        // }
        Destroy(gameObject);
    }
    
    public void ApplyKnockback(Vector3 force)
    {
        KnockBackVelocity += force;
        
        KnockBackVelocity = Vector3.ClampMagnitude(KnockBackVelocity, _knockbackForceMax);
    }

    private void SnapToLanes()
    {
        float currentX = transform.position.x;
        float closeLane = _lanes[0];
        float betterDistance = Mathf.Abs(currentX - _lanes[0]);

        for (int i = 1; i < _lanes.Length; i++)
        {
            float diff =  Mathf.Abs(currentX - _lanes[i]);

            if (diff < betterDistance)
            {
                betterDistance = diff;
                closeLane = _lanes[i];
            }
        }
        transform.position = new Vector3(closeLane, transform.position.y, transform.position.z);
    }

    private void GameOver()
    {
        Destroy(this.gameObject);
    }
}
