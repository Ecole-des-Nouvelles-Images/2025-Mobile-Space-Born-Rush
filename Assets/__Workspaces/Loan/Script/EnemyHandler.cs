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
    public Transform OriginRaycast;
    public Transform DownRaycast;
    public Transform UpRaycast;
    
    [Header("<color=cyan>======Animator======</color>")]
    public Animator Animator;
    
    [FormerlySerializedAs("HitParticles")]
    [Header("<color=blue>======Particule System======</color>")]
    [SerializeField] private ParticleSystem _hitParticles;
    
    [Header("<color=white>=====Ragdoll Settings=====</color>")]
    [SerializeField] private BoxCollider _collider;
    [SerializeField] private Rigidbody _rbHips;
    [Tooltip("Backward speed (m/s) given to the whole body by the killing shot.")]
    [SerializeField] private float _deathBackSpeed = 9f;
    [Tooltip("Upward speed (m/s) so the body leaves the floor instead of sliding on it.")]
    [SerializeField] private float _deathUpSpeed = 2.2f;
    [Tooltip("Extra multiplier on the upper body (spine, head, arms): the torso goes first, the body folds backward.")]
    [SerializeField] private float _deathUpperBodyBoost = 1.35f;
    [Tooltip("Backward spin (rad/s) on the hips: flips the body over instead of keeping it upright.")]
    [SerializeField] private float _deathSpin = 6f;
    [Tooltip("Layers the ragdoll lies on (corridor floor).")]
    [SerializeField] private LayerMask _floorMask = 1 << 7; // Modul
    private Rigidbody[] _ragdollBodies;
    private Collider[] _ragdollColliders;

    [Header("<color=red>=====Death FX=====</color>")]
    [Tooltip("Blood pool spawned on the floor where the enemy dies.")]
    [SerializeField] private BloodDecal _bloodDecalPrefab;
    
    [Header("<color=orange>======Audio Settings=====</color>")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _audioClip;
    [SerializeField] private AudioClip _audioClipHit;
    [SerializeField]  private AudioClip _audioClipDeath;
    
    [Header("<color=magenta>=====Other Settings=====</color>")]
    public Vector3 KnockBackVelocity;
    public bool IsKnocked;
    public LayerMask EnemyMask;
    public GameObject XpPrefab;
    
    [Header("<color=yellow>=====Bool State Settings=====</color>")]
    public bool IsWait;
    public bool IsAttacking;
    public bool IsDead;


    protected BaseState CurrentState = new Wait();

    private const float _knockbackTimer = 3f;
    private const float _knockbackForceMax = 8f;
    private readonly float[] _lanes = { -1.5f, 0f, 1.5f };
    public bool IsPaused;

    private void OnEnable()
    {
        // EventBus.OnEnemyDead += Death;
        EventBus.OnPlayerDead += GameOver;
        EventBus.OnGamePause += GamePause;
        EventBus.OnGameResume += GameResume;
        EventBus.OnEnemyTakeDamage += OnEnemyTakeDamage; 
    }

    private void OnEnemyTakeDamage(float dmg)
    {
        TakeDamage(dmg);
    }

    private void OnDisable()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // EventBus.OnEnemyDead -= Death;
        EventBus.OnPlayerDead -= GameOver;
        EventBus.OnGamePause -= GamePause;
        EventBus.OnGameResume -= GameResume;
        EventBus.OnEnemyTakeDamage -= OnEnemyTakeDamage;
    }
    private void Start()
    {
        Rb = GetComponent<Rigidbody>();
        Transform = GetComponent<Transform>();
        OriginRaycast = OriginRaycast.GetComponent<Transform>();
        _rbHips = _rbHips.GetComponent<Rigidbody>();
        GameUpgrades = FindAnyObjectByType<GameUpgradeController>();
        GameDirector = FindAnyObjectByType<GameDirector>();
        CurrentHealth = EnemyData.MaxHealth + GameDirector.DifficultyHealth;
        CurrentFireRate = 0;
        CurrentDamage = EnemyData.Damage + GameDirector.DifficultyDamage;
        IsWait =  true;
        if (Animator)
        {
            Animator = GetComponent<Animator>();
        }


        transform.eulerAngles = new Vector3(0, 180, 0);

        CacheRagdoll();
        // Alive: the animation drives the bones. Dynamic bones (the old setup) fought the Animator and
        // stored garbage velocity that was released at death, throwing limbs through the floor.
        SetRagdollActive(false);
    }

    private void CacheRagdoll()
    {
        var bodies = new System.Collections.Generic.List<Rigidbody>();
        foreach (var body in GetComponentsInChildren<Rigidbody>(true))
            if (body.gameObject != gameObject) bodies.Add(body);
        _ragdollBodies = bodies.ToArray();

        var colliders = new System.Collections.Generic.List<Collider>();
        foreach (var body in _ragdollBodies)
            colliders.AddRange(body.GetComponents<Collider>());
        _ragdollColliders = colliders.ToArray();

        // The bones sit inside the root hit box: never let them push it (or be pushed by it).
        if (_collider)
            foreach (var col in _ragdollColliders)
                Physics.IgnoreCollision(_collider, col, true);
    }

    private void SetRagdollActive(bool active)
    {
        if (_ragdollBodies == null) return;
        foreach (var body in _ragdollBodies)
        {
            if (active)
            {
                body.isKinematic = false;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                // NO interpolation: the ragdoll lives under a module moved every frame by ModulMove.Update.
                // Interpolation rewrites the bone transforms each frame from the physics pose and throws
                // away the module offset of all frames but the last one before the physics step. In the
                // editor (~1 frame per step) it was invisible; in the uncapped PC build (many frames per
                // step) the body lagged behind its floor, got pushed into it and stayed stuck upright.
                body.interpolation = RigidbodyInterpolation.None;
                // Full CCD against the thin floor; fast but bounded push-out if a limb still overlaps.
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                body.maxDepenetrationVelocity = 6f;
                body.solverIterations = 12;
            }
            else
            {
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; // required before kinematic
                body.isKinematic = true;
            }
        }

        // Feet colliders were disabled: the feet sank into the floor. Every bone collides once dead.
        if (active)
            foreach (var col in _ragdollColliders)
                col.enabled = true;
    }

    private void FixedUpdate()
    {
        if (IsDead)
            return;
        
        KnockBackVelocity = Vector3.Lerp(KnockBackVelocity, Vector3.zero, _knockbackTimer * Time.fixedDeltaTime);
        SpeedDirection = new Vector3(0f, 0f, 0f);
        
        Rb.linearVelocity = SpeedDirection +  KnockBackVelocity;
        
        if (!IsKnocked && KnockBackVelocity.magnitude < 0.1f)
        {
            SnapToLanes();
            IsKnocked = true;
        }
        
        if (KnockBackVelocity.magnitude > 0.1f)
        {
            IsKnocked = false;
        }

        if (!(CurrentHealth <= 0)) return;
        
        IsDead = true;
        IsWait = false;
        IsAttacking = false;
    }
    public void TakeDamage(float damage)
    {
        // if (Animator)
        // {
        //     Animator.SetTrigger("isHit");
        // }
        if (_hitParticles && !_hitParticles.isPlaying)
        {
            _hitParticles.Play();
        }
        
        _audioSource.PlayOneShot(_audioClipHit);
        CurrentHealth -= damage;
    }

    protected void Death(GameObject gameObject)
    {
        if (Animator) Animator.enabled = false;
        if (_collider) _collider.enabled = false;

        // The root body kept its last (knockback) velocity and, without collider, dragged the whole
        // ragdoll with it. Freeze it: the ragdoll bones do the physics from now on.
        if (Rb)
        {
            Rb.linearVelocity = Vector3.zero;
            Rb.angularVelocity = Vector3.zero;
            Rb.interpolation = RigidbodyInterpolation.None; // same reason as the bones (see SetRagdollActive)
            Rb.isKinematic = true;
        }

        // Always have a floor: if the ray misses (module without the "Modul" layer, gap...), use the
        // enemy's own feet height, so the safety floor and the blood pool are never skipped.
        bool hasFloor = TryFindFloor(out RaycastHit floorHit);
        Vector3 floorPoint = hasFloor ? floorHit.point : transform.position;
        Vector3 floorNormal = hasFloor ? floorHit.normal : Vector3.up;
        AddRagdollSafetyFloor(floorPoint.y);

        SetRagdollActive(true);

        _audioSource.PlayOneShot(_audioClipDeath);
        ApplyDeathBlast();

        SpawnBloodDecal(floorPoint, floorNormal);
    }

    /// <summary>
    /// The old push was a single impulse on the hips (~7.8 kg) that the ~70 kg of jointed limbs damped
    /// into a slow topple. Now every bone gets the same velocity change (mass-independent), the torso a
    /// bit more, and the hips a backward spin: the body is thrown back as if hit by the killing shot.
    /// +z is "backward" for the enemy (it faces the player, toward -z).
    /// </summary>
    private void ApplyDeathBlast()
    {
        if (_ragdollBodies == null || _ragdollBodies.Length == 0) return;

        float side = Random.Range(-1f, 1f);
        Vector3 baseVelocity = new Vector3(side, _deathUpSpeed, _deathBackSpeed);

        foreach (var body in _ragdollBodies)
        {
            float boost = IsUpperBody(body.name) ? _deathUpperBodyBoost : 1f;
            body.AddForce(baseVelocity * boost, ForceMode.VelocityChange);
        }

        // Spin around world +x tips the head toward +z: the body falls on its back.
        if (_rbHips)
        {
            _rbHips.maxAngularVelocity = Mathf.Max(_rbHips.maxAngularVelocity, _deathSpin * 2f);
            _rbHips.AddTorque(new Vector3(_deathSpin, side * 1.5f, 0f), ForceMode.VelocityChange);
        }
    }

    private static bool IsUpperBody(string boneName)
    {
        return boneName.Contains("Spine") || boneName.Contains("Head") || boneName.Contains("Neck")
               || boneName.Contains("Arm") || boneName.Contains("Hand") || boneName.Contains("Shoulder");
    }

    private bool TryFindFloor(out RaycastHit hit)
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        if (Physics.Raycast(origin, Vector3.down, out hit, 4f, _floorMask, QueryTriggerInteraction.Ignore))
            return true;

        // Fallback: anything solid that is not a character / pickup.
        int notCharacters = ~LayerMask.GetMask("Enemy", "Player", "Drop", "Ignore Raycast", "UI");
        foreach (var h in Physics.RaycastAll(origin, Vector3.down, 4f, notCharacters, QueryTriggerInteraction.Ignore))
        {
            if (h.collider.transform.IsChildOf(transform)) continue;
            hit = h;
            return true;
        }
        return false;
    }

    /// <summary>
    /// The corridor lane colliders are only 5 cm thick and are moved every frame: falling limbs could
    /// tunnel through. A thick box just under the floor surface (2 cm lower, so the player never touches
    /// it) catches them. It is a child of the enemy, so it moves with the module and dies with the enemy.
    /// </summary>
    private void AddRagdollSafetyFloor(float floorY)
    {
        var floor = new GameObject("RagdollSafetyFloor");
        floor.layer = gameObject.layer;
        floor.transform.SetParent(transform, false);

        const float thickness = 1f;
        const float under = 0.02f;
        // Shifted backward (+z, away from the player): the death impulse throws the body that way.
        Vector3 worldCenter = new Vector3(transform.position.x, floorY - under - thickness * 0.5f, transform.position.z + 4f);
        floor.transform.position = worldCenter;
        floor.transform.rotation = Quaternion.identity;

        var box = floor.AddComponent<BoxCollider>();
        Vector3 scale = floor.transform.lossyScale;
        box.size = new Vector3(5f / scale.x, thickness / scale.y, 11f / scale.z); // -1.5 m .. +9.5 m: covers the stronger throw
    }

    private void SpawnBloodDecal(Vector3 point, Vector3 normal)
    {
        if (_bloodDecalPrefab == null) return;
        // Parented to the module (not the enemy) so the pool stays on the floor and scrolls with it.
        var decal = Instantiate(_bloodDecalPrefab, transform.parent);
        decal.Place(point, normal);
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

    private void GamePause()
    {
        IsPaused = true;
    }

    private void GameResume()
    {
        IsPaused = false;
    }
    private void GameOver()
    {
        Destroy(this.gameObject);
    }
}
