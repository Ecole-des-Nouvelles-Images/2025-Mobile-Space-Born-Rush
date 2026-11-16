using UnityEngine;

/// <summary>
/// Movement feedback of the player:
/// - dust trail while sliding on the ground (stops in the air / when the slide ends);
/// - ground impulse (shockwave + dust burst) when jumping, always centred under the player.
/// The world scrolls toward the camera (the player stays in place), so the slide dust is simulated in
/// world space and pushed back at a fraction of the game speed: it stays "on the floor" behind the player.
/// </summary>
public class PlayerMovementVFX : MonoBehaviour
{
    [SerializeField] private PlayerController _player;

    [Tooltip("Looping system emitted while sliding on the ground.")]
    [SerializeField] private ParticleSystem _slideDust;

    [Tooltip("One-shot system played at the feet when jumping.")]
    [SerializeField] private ParticleSystem _jumpImpulse;

    [Tooltip("Part of the scrolling speed given to the slide dust (1 = glued to the floor). " +
             "Lower keeps the trail on screen longer behind the player.")]
    [SerializeField, Range(0f, 1f)] private float _slideFloorDrag = 0.45f;

    private GameUpgradeController _gameSpeed;
    private bool _isSlideDustOn;
    private bool _isPaused;

    // Jump impulse: simulated in local space and pinned under the player (x follows the lane change,
    // y stays on the floor while the player rises), so the ring is always centred on the player.
    private bool _isJumpPinned;
    private float _jumpGroundY;
    private Vector3 _jumpRestLocalPos;

    private void Awake()
    {
        if (_player == null) _player = GetComponentInParent<PlayerController>();
        _gameSpeed = FindAnyObjectByType<GameUpgradeController>();
        if (_jumpImpulse) _jumpRestLocalPos = _jumpImpulse.transform.localPosition;
    }

    private void OnEnable()
    {
        EventBus.OnGamePause += OnGamePause;
        EventBus.OnGameResume += OnGameResume;
    }

    private void OnDisable()
    {
        EventBus.OnGamePause -= OnGamePause;
        EventBus.OnGameResume -= OnGameResume;
    }

    private void Update()
    {
        if (_isPaused || _player == null || _slideDust == null) return;

        bool wantDust = _player.IsSliding && _player.IsGrounded;
        if (wantDust == _isSlideDustOn) return;

        _isSlideDustOn = wantDust;
        if (wantDust)
        {
            ApplyFloorScroll(_slideDust, _slideFloorDrag);
            _slideDust.Play(true);
        }
        else
        {
            // Let the last puffs fade out instead of popping.
            _slideDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    /// <summary>Called by PlayerController when the jump impulse is applied.</summary>
    public void PlayJump()
    {
        if (_jumpImpulse == null || _isPaused) return;
        var t = _jumpImpulse.transform;
        t.localPosition = _jumpRestLocalPos;
        _jumpGroundY = t.position.y; // feet height at take-off
        _isJumpPinned = true;
        _jumpImpulse.Clear(true);
        _jumpImpulse.Play(true);
    }

    private void LateUpdate()
    {
        if (!_isJumpPinned || _jumpImpulse == null) return;

        var t = _jumpImpulse.transform;
        if (!_jumpImpulse.IsAlive(true))
        {
            t.localPosition = _jumpRestLocalPos;
            _isJumpPinned = false;
            return;
        }

        Vector3 under = t.parent ? t.parent.TransformPoint(_jumpRestLocalPos) : t.position;
        t.position = new Vector3(under.x, _jumpGroundY, under.z);
    }

    private void ApplyFloorScroll(ParticleSystem root, float drag)
    {
        float speed = (_gameSpeed ? _gameSpeed.GameSpeed : 10f) * drag;
        foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
        {
            var vol = ps.velocityOverLifetime;
            if (!vol.enabled) continue; // only the systems set up to follow the floor
            vol.space = ParticleSystemSimulationSpace.World;
            vol.x = new ParticleSystem.MinMaxCurve(0f);
            vol.y = new ParticleSystem.MinMaxCurve(0f);
            vol.z = new ParticleSystem.MinMaxCurve(-speed);
        }
    }

    private void OnGamePause()
    {
        _isPaused = true;
        if (_slideDust) _slideDust.Pause(true);
        if (_jumpImpulse) _jumpImpulse.Pause(true);
    }

    private void OnGameResume()
    {
        _isPaused = false;
        // Resume what was frozen; Update() decides again whether the slide dust must emit.
        if (_slideDust && _slideDust.isPaused) _slideDust.Play(true);
        if (_jumpImpulse && _jumpImpulse.isPaused) _jumpImpulse.Play(true);
        if (_slideDust && !(_player && _player.IsSliding && _player.IsGrounded))
        {
            _slideDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            _isSlideDustOn = false;
        }
    }
}
