using UnityEngine;

[CreateAssetMenu(fileName = "Player Movement Settings", menuName = "Scriptable Objects/Player Movement Settings")]
public class PlayerMovementSO : ScriptableObject
{
    // ============================================================
    // ABILITIES SETTINGS
    // ============================================================
    [Header("Abilities Settings")]
    [SerializeField] private bool _canControlSpeed = true;
    public bool CanControlSpeed => _canControlSpeed;

    [SerializeField] private bool _canDash = true;
    public bool CanDash => _canDash;

    // ============================================================
    // SPEED SETTINGS
    // ============================================================
    [Header("Speed Settings")]
    [Tooltip("Initial movement speed when starting to move")]
    [SerializeField] private float _startSpeed = 5f;
    public float StartSpeed => _startSpeed;

    [Tooltip("Target speed the player accelerates/decelerates towards naturally")]
    [SerializeField] private float _targetSpeed = 20f;
    public float TargetSpeed => _targetSpeed;

    [Tooltip("Maximum movement speed achievable")]
    [SerializeField] private float _maxSpeed = 40f;
    public float MaxSpeed => _maxSpeed;

    [Tooltip("Rate at which speed increases per second")]
    [SerializeField] private float _accelerationRate = 4f;
    public float AccelerationRate => _accelerationRate;

    [Tooltip("Rate at which speed decreases per second")]
    [SerializeField] private float _decelerationRate = 1f;
    public float DecelerationRate => _decelerationRate;

    [Tooltip("Maximum acceleration rate when boosting")]
    [SerializeField] private float _accelerationMax = 8f;
    public float AccelerationMax => _accelerationMax;

    [Tooltip("How fast you lose speed while being in air")]
    [SerializeField] private float _airSpeedLoss = 0.5f;
    public float AirSpeedLoss => _airSpeedLoss;

    [Tooltip("On landing, snap speed to actual horizontal velocity (0 = keep air speed as-is, 1 = fully sync to real velocity)")]
    [Range(0f, 1f)]
    [SerializeField] private float _landingSpeedSync = 0.8f;
    public float LandingSpeedSync => _landingSpeedSync;

    // ============================================================
    // JUMP SETTINGS
    // ============================================================
    [Header("Jump Settings")]
    [Tooltip("Upward force applied when jumping")]
    [SerializeField] private float _jumpForce = 750f;
    public float JumpForce => _jumpForce;

    [Tooltip("Vertical force applied during wall jump")]
    [SerializeField] private float _wallJumpForceVertical = 1000f;
    public float WallJumpForceVertical => _wallJumpForceVertical;

    [Tooltip("Horizontal force applied during wall jump")]
    [SerializeField] private float _wallJumpForceHorizontal = 1000f;
    public float WallJumpForceHorizontal => _wallJumpForceHorizontal;

    [Tooltip("Gravity multiplier at jump apex for hang time (lower = more float)")]
    [Range(0.1f, 1f)]
    [SerializeField] private float _jumpGravityModifier = 0.5f;
    public float JumpGravityModifier => _jumpGravityModifier;

    [Tooltip("Time after leaving ground where player can still jump (seconds)")]
    [Range(0f, 0.5f)]
    [SerializeField] private float _coyoteTime = 0.15f;
    public float CoyoteTime => _coyoteTime;

    [Tooltip("Cooldown between wall jumps to prevent spam (seconds)")]
    [Range(0f, 1f)]
    [SerializeField] private float _wallJumpCooldown = 0.2f;
    public float WallJumpCooldown => _wallJumpCooldown;

    // ============================================================
    // DASH SETTINGS
    // ============================================================
    [Header("Dash Settings")]
    [Tooltip("Force applied when dashing on ground")]
    [SerializeField] private float _groundDashForce = 8000f;
    public float GroundDashForce => _groundDashForce;

    [Tooltip("Force applied when dashing in air")]
    [SerializeField] private float _airDashForce = 2000f;
    public float AirDashForce => _airDashForce;

    [Tooltip("Duration of ground dash (seconds)")]
    [SerializeField] private float _dashTime = 0.05f;
    public float DashTime => _dashTime;

    [Tooltip("Duration of air dash (seconds)")]
    [SerializeField] private float _airDashTime = 0.001f;
    public float AirDashTime => _airDashTime;

    [Tooltip("Speed boost added after dash")]
    [SerializeField] private float _speedBuff = 5f;
    public float SpeedBuff => _speedBuff;

    // ============================================================
    // RAIL SETTINGS
    // ============================================================
    [Header("Rail Settings")]
    [Tooltip("Speed bonus added when on rail")]
    [SerializeField] private float _railSpeed = 3f;
    public float RailSpeed => _railSpeed;

    [Tooltip("Vertical offset for detecting rail entry")]
    [SerializeField] private float _railCheckOffset = 1.5f;
    public float RailCheckOffset => _railCheckOffset;

    [Tooltip("Vertical offset for player position on rail")]
    [SerializeField] private float _railOffset = 1.8f;
    public float RailOffset => _railOffset;

    // ============================================================
    // PHYSICS SETTINGS
    // ============================================================
    [Header("Physics Settings")]
    [Tooltip("Vertical velocity window (±) in which apex hang-time gravity kicks in")]
    [Range(0.1f, 3f)]
    [SerializeField] private float _apexVelocityThreshold = 0.5f;
    public float ApexVelocityThreshold => _apexVelocityThreshold;
}