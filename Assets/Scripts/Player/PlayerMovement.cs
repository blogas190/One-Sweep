using UnityEngine;
using UnityEngine.InputSystem;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

public class PlayerMovement : MonoBehaviour
{
    // ============================================================
    // DEBUG & TESTING
    // ============================================================
    [Tooltip("Enable testing mode for easier direction changes")]
    public bool isTesting = false;

    // ============================================================
    // SETTINGS
    // ============================================================
    [Header("Settings")]
    [SerializeField] private PlayerMovementSO _settings;

    // ============================================================
    // REFERENCES
    // ============================================================
    [Header("References")]
    [Tooltip("Reference to the GameStates manager")]
    public GameStates gameStates;

    [Tooltip("Reference to the player GameObject")]
    public GameObject player;

    [Tooltip("Reference to the rail detection object")]
    public GameObject railCheck;

    [Tooltip("Reference to the GameManager")]
    public GameManager gameManager;

    [Tooltip("Player animator component")]
    public Animator animator;

    [Tooltip("Energy controller for ability costs")]
    public EnergyController energy;

    private Rigidbody p_rb;

    // ============================================================
    // FEEDBACKS
    // ============================================================
    [Header("Feedbacks")]
    [Tooltip("Feedback played when dash starts")]
    public MMFeedbacks DashFeedbackStart;

    [Tooltip("Feedback played when dash ends")]
    public MMFeedbacks DashFeedbackEnd;

    [Tooltip("Feedback played when vertical dash starts")]
    public MMFeedbacks VerticalDashFeedbackStart;

    [Tooltip("Feedback played when vertical dash ends")]
    public MMFeedbacks VerticalDashFeedbackEnd;

    [Tooltip("Feedback played when jumping")]
    public MMFeedbacks JumpFeedback;

    [Tooltip("Feedback played when jump is reset/ready")]
    public MMFeedbacks JumpResetFeedback;

    [Tooltip("Feedback played when entering a rail")]
    public MMFeedbacks RailFeedbackStart;

    [Tooltip("Feedback played when exiting a rail")]
    public MMFeedbacks RailFeedbackEnd;

    // ============================================================
    // STICKY SURFACE SETTINGS
    // ============================================================
    //[Header("Sticky Surface Settings")]
    //[Tooltip("Movement speed on sticky surfaces")]
    //public float stickySurfaceSpeed = 10f;

    //[Tooltip("Gravity multiplier on sticky surfaces (lower = less gravity)")]
    //[Range(0f, 1f)]
    //public float stickyGravityMultiplier = 0.2f;

    // ============================================================
    // GROUND CHECK SETTINGS
    // ============================================================
    [Header("Ground Check Settings")]
    [Tooltip("Layer mask for ground detection")]
    public LayerMask groundLayer;

    [Tooltip("Transform position for ground check sphere")]
    public Transform groundCheck;

    [Tooltip("Radius of ground check sphere")]
    public float groundCheckRadius = 0.2f;

    // ============================================================
    // PRIVATE STATE VARIABLES
    // ============================================================
    // Movement State
    private float direction = 0f;
    private float lastDirection = 0f;
    private bool moveLeft = false;
    private bool moveRight = false;
    private float speed;
    private bool movementEnabled = true;
    private float prevAccelerationRate;
    private bool braking;

    // Input State
    private bool jump = false;
    private bool dash = false;
    private bool verticalDash = false;

    // Surface State
    private bool onRail = false;
    private bool onWall = false;
    private bool onStickySurface = false;
    private Vector3 stickySurfaceNormal;

    // Timers & Counters
    private float currentDashTime = 0f;
    private bool wasGrounded = true; // tracks previous frame grounded state for landing detection
    private float coyoteTimeCounter = 0f;
    private float wallJumpCooldownTimer = 0f;

    // Physics & Effects
    private Vector3 dashVector;
    private float startGravity = -25f;

    // Gravity state machine replaces the fragile hasReducedGravity bool.
    // Normal    : full gravity, no modifier active.
    // Apex      : reduced gravity applied at jump apex for hang time.
    // VerticalUp: reduced gravity while an upward vertical dash is active.
    private enum GravityState { Normal, Apex, VerticalUp }
    private GravityState gravityState = GravityState.Normal;

    [Tooltip("Vertical velocity window in which apex hang-time gravity kicks in")]
    [Range(0.1f, 3f)]
    public float apexVelocityThreshold = 0.5f;

    // Moving platform
    private Rigidbody currentPlatformRb = null;

    // Runtime copies of SO values that get mutated during play (rail/boost)
    private float accelerationRate;
    private float accelerationMax;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        p_rb = GetComponent<Rigidbody>();
        energy = GetComponent<EnergyController>();
        speed = _settings.StartSpeed;
        accelerationRate = _settings.AccelerationRate;
        accelerationMax = _settings.AccelerationMax;
        prevAccelerationRate = accelerationRate;
        startGravity = Physics.gravity.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (CheckDeathState()) return;
        UpdateRailMovement();
        UpdateMovementConstraints();
        //UpdateStickySurface();
        UpdateRotation();
        UpdateAcceleration();
        UpdateVelocity();
        UpdateCoyoteTime();
        HandleJump();
        UpdateGravityModifiers();
        HandleDash();
    }

    // Sets gravity to an absolute value, never stacked multiplications.
    // This is the single source of truth for all gravity changes.
    private void SetGravityState(GravityState newState)
    {
        if (gravityState == newState) return;
        gravityState = newState;

        float target = newState == GravityState.Normal
            ? startGravity
            : startGravity * _settings.JumpGravityModifier;

        Physics.gravity = new Vector3(Physics.gravity.x, target, Physics.gravity.z);
        // Keep gameStates in sync if it caches gravity internally
        // (call its setter only with the absolute final value, not a multiplier)
    }

    //--------------------FixedUpdate Functions----------------------------

    private bool CheckDeathState()
    {
        if (gameStates != null && gameStates.deathState)
        {
            // Reset all input flags
            moveLeft = false;
            moveRight = false;
            jump = false;
            dash = false;

            // Freeze X and Z movement, but allow Y (falling)
            p_rb.constraints = RigidbodyConstraints.FreezePositionX |
                              RigidbodyConstraints.FreezePositionZ |
                              RigidbodyConstraints.FreezeRotation;

            return true; // Exit FixedUpdate early
        }

        return false; // Continue with normal update
    }

    private void UpdateRailMovement()
    {
        if (onRail && railCheck.GetComponent<RailCheck>().currentRail != null)
        {
            RailMovement(railCheck.GetComponent<RailCheck>().currentRail);
        }
        else
        {
            Vector3 pos = transform.position;
            pos.z = 0f;
            transform.position = pos;
        }
    }

    private void UpdateMovementConstraints()
    {
        if (!movementEnabled)
        {
            p_rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    //private void UpdateStickySurface()
    //{
    //    if (onStickySurface)
    //    {
    //        StickySurfaceMovement();
    //    }
    //}

    private void UpdateRotation()
    {
        if (moveLeft)
        {
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }

        if (moveRight)
        {
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }
    }

    private void UpdateAcceleration()
    {
        animator.SetFloat("Speed", speed);

        if (speed < _settings.StartSpeed) speed = _settings.StartSpeed;
        else if (speed > _settings.MaxSpeed) speed = _settings.MaxSpeed;

        // Landing snap: on the first grounded frame after being airborne,
        // blend speed toward the actual horizontal rigidbody velocity so the
        // player never floats in from a high-speed jump and then slowly crawls.
        bool isGrounded = Grounded();
        if (isGrounded && !wasGrounded)
        {
            float actualHorizontalSpeed = Mathf.Abs(p_rb.linearVelocity.x);
            float snappedSpeed = Mathf.Lerp(speed, actualHorizontalSpeed, _settings.LandingSpeedSync);
            speed = Mathf.Clamp(snappedSpeed, _settings.StartSpeed, _settings.MaxSpeed);
        }
        wasGrounded = isGrounded;

        if (onRail)
        {
            speed = Mathf.MoveTowards(speed, _settings.MaxSpeed, accelerationRate * Time.fixedDeltaTime);
            return;
        }

        if (Grounded() && (moveLeft || moveRight) && !braking)
        {
            float desiredSpeed = (accelerationRate == accelerationMax) ? _settings.MaxSpeed : _settings.TargetSpeed;
            speed = Mathf.MoveTowards(speed, desiredSpeed, accelerationRate * Time.fixedDeltaTime);
        }
        else if (Grounded() && (moveLeft || moveRight) && braking)
        {
            speed = Mathf.MoveTowards(speed, _settings.StartSpeed, accelerationRate * Time.fixedDeltaTime);
        }
        else if (!Grounded() && (moveLeft || moveRight))
        {
            speed -= _settings.AirSpeedLoss * Time.fixedDeltaTime;
        }
    }

    private void UpdateVelocity()
    {
        Vector3 velocity = p_rb.linearVelocity;

        if (onRail)
        {
            velocity.x = direction * speed;
            p_rb.linearVelocity = velocity;
            return;
        }

        if ((moveLeft || moveRight) && Grounded() && !dash)
        {
            float platformVelocityX = currentPlatformRb != null ? currentPlatformRb.linearVelocity.x : 0f;
            velocity.x = direction * speed + platformVelocityX;
        }

        p_rb.linearVelocity = velocity;
    }

    private void UpdateCoyoteTime()
    {
        // Coyote time logic
        if (Grounded())
        {
            coyoteTimeCounter = _settings.CoyoteTime; // Reset counter when grounded
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime; // Count down when in air
        }

        if (wallJumpCooldownTimer > 0f)
        {
            wallJumpCooldownTimer -= Time.fixedDeltaTime;
        }
    }

    private void HandleJump()
    {
        if (jump)
        {
            if (coyoteTimeCounter > 0f)
            {
                JumpResetFeedback.PlayFeedbacks();
                p_rb.AddForce(Vector3.up * _settings.JumpForce, ForceMode.Impulse);
                JumpFeedback.PlayFeedbacks();
                // Snap to normal before launch so rapid mid-air re-jumps never inherit reduced gravity.
                SetGravityState(GravityState.Normal);
                animator.SetTrigger("Jump");
                coyoteTimeCounter = 0f;
                jump = false;
            }
            else if (onWall && wallJumpCooldownTimer <= 0f)
            {
                // Wall jump: restore normal gravity before applying force.
                SetGravityState(GravityState.Normal);

                if (moveLeft)
                {
                    Vector3 wallJumpVector = new Vector3(-_settings.WallJumpForceHorizontal, _settings.WallJumpForceVertical, 0f);
                    p_rb.AddForce(wallJumpVector, ForceMode.Impulse);
                }
                if (moveRight)
                {
                    Vector3 wallJumpVector = new Vector3(_settings.WallJumpForceHorizontal, _settings.WallJumpForceVertical, 0f);
                    p_rb.AddForce(wallJumpVector, ForceMode.Impulse);
                }
                wallJumpCooldownTimer = _settings.WallJumpCooldown;
                jump = false;
            }
            else if (onStickySurface)
            {
                //StopStickySurface();
                SetGravityState(GravityState.Normal);
                p_rb.AddForce(Vector3.up * _settings.JumpForce, ForceMode.Impulse);
                jump = false;
                return;
            }
            else
            {
                jump = false;
            }
        }
    }

    private void UpdateGravityModifiers()
    {
        // Grounded: always restore full gravity regardless of what state we were in.
        if (Grounded())
        {
            SetGravityState(GravityState.Normal);
            return;
        }

        // In-air logic. VerticalUp state is managed entirely by VerticalDash()
        // and HandleDash(), so we only touch Normal <-> Apex here.
        if (gravityState == GravityState.VerticalUp) return;

        float vy = p_rb.linearVelocity.y;

        // Apex window: velocity is near zero (neither strongly rising nor falling).
        bool atApex = Mathf.Abs(vy) <= _settings.ApexVelocityThreshold;

        if (atApex)
        {
            SetGravityState(GravityState.Apex);
        }
        else
        {
            // Outside apex window ? restore normal gravity so the fall feels snappy.
            SetGravityState(GravityState.Normal);
        }
    }

    private void HandleDash()
    {
        if (dash)
        {
            currentDashTime -= Time.fixedDeltaTime;
            p_rb.constraints = RigidbodyConstraints.FreezePositionY;
            p_rb.constraints = RigidbodyConstraints.FreezeRotation;

            if (currentDashTime <= 0f)
            {
                dash = false;
                p_rb.constraints = RigidbodyConstraints.None;
                p_rb.constraints = RigidbodyConstraints.FreezeRotation;
                energy.RemoveEnergy(energy.dashEnergy);
                DashFeedbackEnd.PlayFeedbacks();
                Debug.Log("Dash ended");
            }
        }
        if (verticalDash)
        {
            currentDashTime -= Time.fixedDeltaTime;

            if (currentDashTime <= 0f)
            {
                verticalDash = false;
                SetGravityState(GravityState.Normal);
                Debug.Log("Vertical Dash ended");
                VerticalDashFeedbackEnd.PlayFeedbacks();
            }
        }

    }
    //--------------------Player Movement----------------------------------

    public void Move(InputAction.CallbackContext context)
    {
        //Checking for player input using unity's input system
        if (context.performed && gameManager.currentState == GameState.playing)
        {
            if (isTesting)
            {
                if (Grounded())
                {
                    Vector2 input = context.ReadValue<Vector2>();
                    direction = input.x;
                    speed = _settings.StartSpeed; //for debugging, resets the speed on changing directions

                    //direction flags so we can limit the player's options later
                    if (direction < 0 && lastDirection != direction)
                    {
                        moveLeft = true;
                        moveRight = false;
                    }

                    else if (direction > 0 && lastDirection != direction)
                    {
                        moveLeft = false;
                        moveRight = true;
                    }

                    if (direction != 0)
                    {
                        lastDirection = direction;
                    }
                }
            }
            else
            {
                if (!moveRight && !moveLeft)
                {
                    Vector2 input = context.ReadValue<Vector2>();
                    direction = input.x;
                    speed = _settings.StartSpeed;

                    if (direction < 0 && lastDirection != direction)
                    {
                        moveLeft = true;
                        moveRight = false;
                    }

                    else if (direction > 0 && lastDirection != direction)
                    {
                        moveLeft = false;
                        moveRight = true;
                    }

                    if (direction != 0)
                    {
                        lastDirection = direction;
                    }
                }
                else // making the player go faster or slower
                {
                    if (Grounded() && _settings.CanControlSpeed)
                    {
                        Vector2 input = context.ReadValue<Vector2>();
                        float dir = input.x;

                        if (dir < 0)
                        {
                            if (moveLeft) { accelerationRate = accelerationMax; }
                            if (moveRight) { braking = true; }
                        }
                        else if (dir > 0)
                        {
                            if (moveLeft) { braking = true; }
                            if (moveRight) { accelerationRate = accelerationMax; }
                        }
                    }
                }
            }
        }
        else if (context.canceled)
        {
            if (!isTesting && (moveLeft || moveRight))
            {
                accelerationRate = prevAccelerationRate;
                braking = false;
            }
        }
    }

    public void ChangeDirection()
    {
        if (moveLeft)
        {
            moveLeft = false;
            moveRight = true;
            direction = 1;
            Debug.Log("Changed Directions from LEFT to RIGHT!");
        }
        else if (moveRight)
        {
            moveRight = false;
            moveLeft = true;
            direction = -1;
            Debug.Log("Changed Directions from RIGHT to LEFT!");
        }
        else if (!moveRight && !moveLeft)
        {
            Debug.Log("No move no change!");
            return;
        }

        lastDirection = direction;
    }

    public void Jump(InputAction.CallbackContext context)
    {

        //Checking for player input for jump
        if (context.performed && gameManager.currentState == GameState.playing)
        {
            jump = true;
            Debug.Log("Jump Attempt");
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && !dash && energy.currentEnergy >= energy.dashEnergy && _settings.CanDash)
        {
            float dashDirection = (moveLeft || moveRight) ? direction : lastDirection;
            if (Grounded())
            {
                dashVector = new Vector3(dashDirection, 0f, 0f) * _settings.GroundDashForce;
                currentDashTime = _settings.DashTime;
            }
            else
            {
                dashVector = new Vector3(dashDirection, 0f, 0f) * _settings.AirDashForce;
                currentDashTime = _settings.AirDashTime;
            }
            p_rb.AddForce(dashVector, ForceMode.Impulse);

            dash = true;
            if (speed < _settings.MaxSpeed) { speed += _settings.SpeedBuff; }
            Debug.Log("Dash started");
            DashFeedbackStart.PlayFeedbacks();
            animator.SetTrigger("Dash");
        }
    }

    public void VerticalDash(bool isUp, float verticalDashForce, float verticalDashTime, bool usesEnergy = true, bool callFeedback = true)
    {
        if (energy.currentEnergy >= energy.upTrickEnergy)
        {
            if (isUp)
            {
                dashVector = Vector3.up * verticalDashForce;
                SetGravityState(GravityState.VerticalUp);
            }
            else // downward dash
            {
                dashVector = Vector3.down * verticalDashForce;
                SetGravityState(GravityState.Normal);
            }
            currentDashTime = verticalDashTime;
            p_rb.AddForce(dashVector, ForceMode.Impulse);

            verticalDash = true;
            if (usesEnergy)
            {
                energy.RemoveEnergy(energy.upTrickEnergy);
            }
            Debug.Log("Vertical Dash started");
            if (callFeedback)
            {
                VerticalDashFeedbackStart.PlayFeedbacks();
            }
        }
    }

    //--------------------Rails-----------------------------

    public void RailStartMovementAngled(GameObject rail, Vector3 collisionPoint)
    {
        if ((transform.position.y - _settings.RailCheckOffset) < collisionPoint.y)
        {
            transform.position = new Vector3(transform.position.x, collisionPoint.y + _settings.RailOffset, rail.transform.position.z);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, rail.transform.position.z);
        }

        p_rb.linearVelocity = Vector3.zero;
        p_rb.angularVelocity = Vector3.zero;
        p_rb.constraints = RigidbodyConstraints.FreezeRotation;

        //speed += railSpeed;          // directly bump speed up
        accelerationRate += _settings.RailSpeed;
        accelerationMax += _settings.RailSpeed;
        if (speed > _settings.MaxSpeed) speed = _settings.MaxSpeed;

        onRail = true;
        RailMovement(rail);
        RailFeedbackStart.PlayFeedbacks();
    }

    public void RailMovement(GameObject rail)
    {
        if (!onRail) return;

        p_rb.constraints = RigidbodyConstraints.FreezeRotation;
        Vector3 velocity = p_rb.linearVelocity;
        velocity.x = direction * speed;
        p_rb.linearVelocity = velocity;
    }

    public void RailStopMovement()
    {
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
        accelerationRate = prevAccelerationRate;
        accelerationMax -= _settings.RailSpeed;
        RailFeedbackEnd.PlayFeedbacks();
        RailFeedbackStart.StopFeedbacks();
    }

    //--------------------Sticky surface-----------------------------

    //public void StartStickySurface(Vector3 surfaceNormal)
    //{
    //    Debug.Log("Started sticky surface movement");
    //    onStickySurface = true;
    //    stickySurfaceNormal = surfaceNormal;

    //    // Get current velocity
    //    Vector3 currentVelocity = p_rb.linearVelocity;

    //    // Convert horizontal speed to vertical movement direction
    //    float horizontalSpeed = Mathf.Abs(currentVelocity.x);
    //    // Transfer horizontal speed to vertical speed
    //    speed = Mathf.Max(speed, horizontalSpeed);

    //    // Reduce gravity effect
    //    p_rb.useGravity = false;

    //    // Apply custom gravity
    //    p_rb.AddForce(Vector3.down * Physics.gravity.magnitude * stickyGravityMultiplier, ForceMode.Acceleration);
    //}

    //public void StickySurfaceMovement()
    //{
    //    if (!onStickySurface) return;

    //    Vector3 velocity = stickySurfaceNormal.normalized * speed;

    //    if (speed < maxSpeed)
    //    {
    //        speed += accelerationRate * Time.fixedDeltaTime;
    //    }

    //    p_rb.linearVelocity = velocity;
    //}

    //public void StopStickySurface()
    //{
    //    Debug.Log("Stopped sticky surface movement");
    //    onStickySurface = false;
    //    Vector3 velocity = p_rb.linearVelocity;
    //    float verticalSpeed = Mathf.Abs(velocity.y);

    //    velocity.x = verticalSpeed;
    //    velocity.y = 0f;
    //    p_rb.linearVelocity = velocity;

    //    // Keep the speed for continued horizontal movement
    //    speed = Mathf.Max(startSpeed, verticalSpeed);

    //    // Re-enable normal gravity
    //    p_rb.useGravity = true;
    //}

    //----------------------------------------------------
    //Setters
    //----------------------------------------------------

    public void SetOnRail(bool railStatus, GameObject rail)
    {
        onRail = railStatus;
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;

        if (enabled)
        {
            p_rb.constraints = RigidbodyConstraints.None;
            p_rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    public void SetSpeed(float newSpeed)
    {
        if (newSpeed < _settings.MaxSpeed) speed = newSpeed;
        else speed = _settings.MaxSpeed;
        Vector3 velocity = p_rb.linearVelocity;
        velocity.x = direction * speed;
        p_rb.linearVelocity = velocity;
    }

    public void SetOnWall(bool wallStatus)
    {
        onWall = wallStatus;
    }

    //----------------------------------------------------
    //Getters
    //----------------------------------------------------

    public float GetCurrentSpeed() //returns the basic speed from moving and acceleration
    {
        return speed;
    }

    public float GetMagnitude() //returns how fast the player is actually moving
    {
        return p_rb.linearVelocity.magnitude;
    }

    public bool GetDash()
    {
        return dash;
    }

    public Vector3 GetCurrentDirection()
    {
        Vector3 dir = new Vector3(direction, 0, 0);
        return dir;
    }

    //----------------------------------------------------
    //State checks
    //----------------------------------------------------

    public bool Grounded()
    {
        bool ground = false;
        if (Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer) || onRail)
        {
            ground = true;
        }
        //Debug.Log("Grounded: " + ground);
        return ground;
    }

    public bool IsOnStickySurface()
    {
        return onStickySurface;
    }

    public bool IsOnRail()
    {
        return onRail;
    }

    //----------------------------------------------------
    // Moving platform tracking
    //----------------------------------------------------

    private void OnCollisionStay(Collision collision)
    {
        // Check if we're standing on top of a moving platform
        if (collision.rigidbody == null || !collision.rigidbody.isKinematic) return;

        foreach (ContactPoint cp in collision.contacts)
        {
            // Contact normal points from surface toward us if it's upward, we're on top
            if (cp.normal.y > 0.5f)
            {
                currentPlatformRb = collision.rigidbody;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.rigidbody == currentPlatformRb)
            currentPlatformRb = null;
    }
}