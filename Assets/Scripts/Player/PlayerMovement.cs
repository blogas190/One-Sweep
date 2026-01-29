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
    // SPEED SETTINGS
    // ============================================================
    [Header("Speed Settings")]
    [Tooltip("Initial movement speed when starting to move")]
    public float startSpeed = 5f;

    [Tooltip("Target speed the player accelerates/decelerates towards naturally")]
    public float targetSpeed = 10f;

    [Tooltip("Maximum movement speed achievable")]
    public float maxSpeed = 15f;

    [Tooltip("Rate at which speed increases per second")]
    public float accelerationRate = 1f;

    [Tooltip("Rate at which speed decreases per second")]
    public float decelerationRate = 1f;

    [Tooltip("Maximum acceleration rate when boosting")]
    public float accelerationMax = 2f;

    [Tooltip("How fast you lose speed while being in air")]
    public float airSpeedLoss = 1f;
    // ============================================================
    // JUMP SETTINGS
    // ============================================================
    [Header("Jump Settings")]
    [Tooltip("Upward force applied when jumping")]
    public float jumpForce = 10f;

    [Tooltip("Vertical force applied during wall jump")]
    public float wallJumpForceVertical = 20f;

    [Tooltip("Horizontal force applied during wall jump")]
    public float wallJumpForceHorizontal = 20f;

    [Tooltip("Gravity multiplier at jump apex for hang time (lower = more float)")]
    [Range(0.1f, 1f)]
    public float jumpGravityModifier = 0.3f;

    [Tooltip("Time after leaving ground where player can still jump (seconds)")]
    [Range(0f, 0.5f)]
    public float coyoteTime = 0.15f;

    [Tooltip("Cooldown between wall jumps to prevent spam (seconds)")]
    [Range(0f, 1f)]
    public float wallJumpCooldown = 0.3f;

    // ============================================================
    // DASH SETTINGS
    // ============================================================
    [Header("Dash Settings")]
    [Tooltip("Force applied when dashing on ground")]
    public float groundDashForce = 200f;

    [Tooltip("Force applied when dashing in air")]
    public float airDashForce = 200f;

    [Tooltip("Duration of ground dash (seconds)")]
    public float dashTime = 1f;

    [Tooltip("Duration of air dash (seconds)")]
    public float airDashTime = 1f;

    [Tooltip("Speed boost added after dash")]
    public float speedBuff = 5f;

    // ============================================================
    // RAIL SETTINGS
    // ============================================================
    [Header("Rail Settings")]
    [Tooltip("Speed bonus added when on rail")]
    public float railSpeed = 20f;

    [Tooltip("Vertical offset for detecting rail entry")]
    public float railCheckOffset = 1.5f;

    [Tooltip("Vertical offset for player position on rail")]
    public float railOffset = 1.8f;

    // ============================================================
    // STICKY SURFACE SETTINGS
    // ============================================================
    [Header("Sticky Surface Settings")]
    [Tooltip("Movement speed on sticky surfaces")]
    public float stickySurfaceSpeed = 10f;

    [Tooltip("Gravity multiplier on sticky surfaces (lower = less gravity)")]
    [Range(0f, 1f)]
    public float stickyGravityMultiplier = 0.2f;

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
    private float coyoteTimeCounter = 0f;
    private float wallJumpCooldownTimer = 0f;

    // Physics & Effects
    private Vector3 dashVector;
    private bool hasReducedGravity = false;
    private float startGravity = -25f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        p_rb = GetComponent<Rigidbody>();
        energy = GetComponent<EnergyController>();
        speed = startSpeed;
        prevAccelerationRate = accelerationRate;
        startGravity = Physics.gravity.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (CheckDeathState()) return;
        UpdateRailMovement();
        UpdateMovementConstraints();
        UpdateStickySurface();
        UpdateRotation();
        UpdateAcceleration();
        UpdateVelocity();
        UpdateCoyoteTime();
        HandleJump();
        UpdateGravityModifiers();
        HandleDash();
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

    private void UpdateStickySurface()
    {
        if (onStickySurface)
        {
            StickySurfaceMovement();
        }
    }

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
        float desiredSpeed;

        if (speed < startSpeed)
        {
            speed = startSpeed;
        }
        else if (speed > maxSpeed)
        {
            speed = maxSpeed;
        }

        if(Grounded() && (moveLeft || moveRight) && !braking)
        {
            if(accelerationRate == accelerationMax)
            {
                desiredSpeed = maxSpeed;
            }
            else
            {
                desiredSpeed = targetSpeed;
            }
            
            speed = Mathf.MoveTowards(speed, desiredSpeed, accelerationRate * Time.fixedDeltaTime);
        }
        else if (Grounded() && (moveLeft || moveRight) && braking)
        {
            speed = Mathf.MoveTowards(speed, startSpeed, accelerationRate * Time.fixedDeltaTime);
        }
        else if (!Grounded() && (moveLeft || moveRight))
        {
            speed -= airSpeedLoss * Time.fixedDeltaTime;
        }
    }

    private void UpdateVelocity()
    {
        Vector3 velocity = p_rb.linearVelocity;
        if ((moveLeft || moveRight) && Grounded() && !dash || onRail)
        {
            velocity.x = direction * speed;
        }
        p_rb.linearVelocity = velocity;
    }

    private void UpdateCoyoteTime()
    {
        // Coyote time logic
        if (Grounded())
        {
            coyoteTimeCounter = coyoteTime; // Reset counter when grounded
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
                p_rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                JumpFeedback.PlayFeedbacks();
                gameStates.MultVerticalGravity(jumpGravityModifier);
                hasReducedGravity = true;
                animator.SetTrigger("Jump");
                coyoteTimeCounter = 0f;
                jump = false;
            }
            else if (onWall && wallJumpCooldownTimer <= 0f) // Changed to else if so it doesn't check wall jump after successful ground jump
            {
                if (hasReducedGravity)
                {
                    gameStates.MultVerticalGravity(1f / jumpGravityModifier);
                    hasReducedGravity = false;
                }

                if (moveLeft)
                {
                    Vector3 wallJumpVector = new Vector3(-wallJumpForceHorizontal, wallJumpForceVertical, 0f);
                    p_rb.AddForce(wallJumpVector, ForceMode.Impulse);
                }
                if (moveRight)
                {
                    Vector3 wallJumpVector = new Vector3(wallJumpForceHorizontal, wallJumpForceVertical, 0f);
                    p_rb.AddForce(wallJumpVector, ForceMode.Impulse);
                }
                wallJumpCooldownTimer = wallJumpCooldown;
                jump = false;
            }
            else if (onStickySurface)
            {
                StopStickySurface();
                gameStates.MultVerticalGravity(jumpGravityModifier);
                p_rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
        // Safety check: Reset gravity if grounded and it's still reduced (shouldn't happen)
        if (Grounded() && hasReducedGravity)
        {
            gameStates.MultVerticalGravity(1f / jumpGravityModifier);
            hasReducedGravity = false;
            Debug.LogWarning("Gravity was stuck reduced - force reset to normal");
        }

        if (!Grounded() && p_rb.linearVelocity.y > -0.5f && p_rb.linearVelocity.y < 0.5f)
        {
            // Near the peak - reduce gravity for hang time
            if (Physics.gravity.y == startGravity)
            {
                gameStates.MultVerticalGravity(jumpGravityModifier);
                hasReducedGravity = true;
            }
        }
        else if (hasReducedGravity && !Grounded())
        {
            // Reset when leaving the apex (but not grounded yet)
            gameStates.MultVerticalGravity(1f / jumpGravityModifier);
            hasReducedGravity = false;
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
                    speed = startSpeed; //for debugging, resets the speed on changing directions

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
                    speed = startSpeed;

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
                    if (Grounded())
                    {
                        Vector2 input = context.ReadValue<Vector2>();
                        float dir = input.x;

                        if (dir < 0)
                        {
                            if (moveLeft) { accelerationRate = accelerationMax;}
                            if (moveRight) { braking = true; }
                        }
                        else if (dir >0)
                        {
                            if (moveLeft) { braking = true;}
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
        if (context.performed && !dash && energy.currentEnergy >= energy.dashEnergy)
        {
            float dashDirection = (moveLeft || moveRight) ? direction : lastDirection;
            if (Grounded())
            {
                dashVector = new Vector3(dashDirection, 0f, 0f) * groundDashForce;
                currentDashTime = dashTime;
            }
            else
            {
                dashVector = new Vector3(dashDirection, 0f, 0f) * airDashForce;
                currentDashTime = airDashTime;
            }
            p_rb.AddForce(dashVector, ForceMode.Impulse);

            dash = true;
            if (speed < maxSpeed) { speed += speedBuff; }
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
                if (!hasReducedGravity)
                { 
                    gameStates.MultVerticalGravity(jumpGravityModifier);
                    hasReducedGravity = true;
                }
            }
            else 
            { 
                dashVector = Vector3.down * verticalDashForce;
                if (hasReducedGravity)
                {
                    gameStates.MultVerticalGravity(1f / jumpGravityModifier);
                    hasReducedGravity = false;
                }
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
        // Match rail's Z position
        if ((transform.position.y - railCheckOffset) < collisionPoint.y)
        {
            transform.position = new Vector3(transform.position.x, collisionPoint.y + railOffset, rail.transform.position.z);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, rail.transform.position.z);
        }

        p_rb.linearVelocity = Vector3.zero;
        p_rb.angularVelocity = Vector3.zero;
        p_rb.constraints = RigidbodyConstraints.FreezeRotation;

        accelerationRate += railSpeed;
        onRail = true;
        RailMovement(rail);
        RailFeedbackStart.PlayFeedbacks();
    }

    public void RailMovement(GameObject rail)
    {
        if (onRail)
        {
            p_rb.constraints = RigidbodyConstraints.None;
            p_rb.constraints = RigidbodyConstraints.FreezeRotation;
            Vector3 velocity = p_rb.linearVelocity;
            velocity.x = direction * speed;
        }
        else
        {
            return;
        }
    }

    public void RailStopMovement()
    {
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
        accelerationRate = prevAccelerationRate;
        RailFeedbackEnd.PlayFeedbacks();
        RailFeedbackStart.StopFeedbacks();
    }

    //--------------------Sticky surface-----------------------------

    public void StartStickySurface(Vector3 surfaceNormal)
    {
        Debug.Log("Started sticky surface movement");
        onStickySurface = true;
        stickySurfaceNormal = surfaceNormal;

        // Get current velocity
        Vector3 currentVelocity = p_rb.linearVelocity;

        // Convert horizontal speed to vertical movement direction
        float horizontalSpeed = Mathf.Abs(currentVelocity.x);
        // Transfer horizontal speed to vertical speed
        speed = Mathf.Max(speed, horizontalSpeed);

        // Reduce gravity effect
        p_rb.useGravity = false;

        // Apply custom gravity
        p_rb.AddForce(Vector3.down * Physics.gravity.magnitude * stickyGravityMultiplier, ForceMode.Acceleration);
    }

    public void StickySurfaceMovement()
    {
        if (!onStickySurface) return;

        Vector3 velocity = stickySurfaceNormal.normalized * speed;

        if (speed < maxSpeed)
        {
            speed += accelerationRate * Time.fixedDeltaTime;
        }

        p_rb.linearVelocity = velocity;
    }

    public void StopStickySurface()
    {
        Debug.Log("Stopped sticky surface movement");
        onStickySurface = false;
        Vector3 velocity = p_rb.linearVelocity;
        float verticalSpeed = Mathf.Abs(velocity.y);

        velocity.x = verticalSpeed;
        velocity.y = 0f;
        p_rb.linearVelocity = velocity;

        // Keep the speed for continued horizontal movement
        speed = Mathf.Max(startSpeed, verticalSpeed);

        // Re-enable normal gravity
        p_rb.useGravity = true;
    }

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
        if (newSpeed < maxSpeed) speed = newSpeed;
        else speed = maxSpeed;
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
        if (Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer) || onRail) { ground = true;
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
}