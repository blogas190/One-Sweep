using UnityEngine;
using UnityEngine.InputSystem;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

public class PlayerMovement : MonoBehaviour
{
    public bool isTesting = false;
    [Header("Player Reference")]
    public GameStates gameStates;
    public GameObject player;
    private MeshRenderer p_renderer;
    public MeshRenderer Renderer => p_renderer;
    public GameObject railCheck;
    private Rigidbody p_rb;
    public GameManager gameManager;
    public Animator animator;

    [Header("Feedbacks")]
    public MMFeedbacks DashFeedbackStart;
    public MMFeedbacks DashFeedbackEnd;

    public MMFeedbacks VerticalDashFeedbackStart;
    public MMFeedbacks VerticalDashFeedbackEnd;

    public MMFeedbacks JumpFeedback;
    public MMFeedbacks JumpResetFeedback;

    public MMFeedbacks RailFeedbackStart;
    public MMFeedbacks RailFeedbackEnd;

    [Header("Speed Settings")]
    public float startSpeed = 5f;
    public float maxSpeed = 15f;
    public float accelerationRate = 1f;
    public float accelerationMax = 2f;

    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public float wallJumpForceVertical = 20f;
    public float wallJumpForceHorizontal = 20f;

    [Header("Dash Settings")]
    public float groundDashForce = 200f;
    public float airDashForce = 200f;
    public float dashTime = 1f;
    public float airDashTime = 1f;
    public int dashNumber = 3;
    public float speedBuff = 5f;

    [Header("Rail Settings")]
    public float railSpeed = 20f;
    public float railCheckOffset = 1.5f;
    public float railOffset = 1.8f;

    [Header("Sticky Surface Settings")]
    public float stickySurfaceSpeed = 10f;
    public float stickyGravityMultiplier = 0.2f; // Reduced gravity on sticky surfaces

    [Header("Ground Check Settings")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private bool onStickySurface = false;
    private Vector3 stickySurfaceNormal;

    private float direction = 0f;
    private float lastDirection = 0f;
    private bool moveLeft = false;
    private bool moveRight = false; // by default
    private bool jump = false;
    private bool dash = false;
    private bool verticalDash = false;
    private float speed;
    private bool onRail = false;
    private bool onWall = false;
    private float currentDashTime = 0f;
    private Vector3 dashVector;
    private bool movementEnabled = true;
    private float prevAccelerationRate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        p_renderer = GetComponent<MeshRenderer>();
        p_renderer.material.color = Color.green;

        p_rb = GetComponent<Rigidbody>();
        speed = startSpeed;
        prevAccelerationRate = accelerationRate;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //checking if the player failed
        if(gameStates != null && gameStates.deathState)
        {
            moveLeft = false;
            moveRight = false;
            jump = false;
            dash = false;

            //lastSpeed = speed;
            //speed = 0f;
            //p_rb.linearVelocity = Vector3.zero;
            p_rb.constraints = RigidbodyConstraints.FreezeAll;
            return;
        }


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


        if (!movementEnabled)
        {
            p_rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        if (onStickySurface)
        {
            StickySurfaceMovement();
        }

        if (moveLeft) { transform.rotation = Quaternion.Euler(0f, 180f, 0f); } // temporary decision, fix later so it only flips on input in a seperate function
        if (moveRight) { transform.rotation = Quaternion.Euler(0f, 0f, 0f); }
        //acceleration
        if (Grounded() && (moveLeft || moveRight))
        {
            if (speed < startSpeed)
            {
                speed = startSpeed;
            }
            else if (speed > maxSpeed)
            {
                speed = maxSpeed;
            }
            else
            { speed += accelerationRate * Time.fixedDeltaTime; }
        }
        animator.SetFloat("Speed", speed);

        // Updated movement that uses Unity physics and fixes wall collisions
        Vector3 velocity = p_rb.linearVelocity;
        if ((moveLeft || moveRight) && Grounded() && !dash || onRail)
        {
            velocity.x = direction * speed;
        }
        p_rb.linearVelocity = velocity;

        //Jump logic
        if (jump)
        {
            if (Grounded())
            {
                JumpResetFeedback.PlayFeedbacks();
                p_rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                JumpFeedback.PlayFeedbacks();
                animator.SetTrigger("Jump");
                jump = false;
            }
            if (onWall) // can add feedbacks here should work like jump, just smoke in a different place
            {
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
                jump = false;
            }
            if (onStickySurface)
            {
                StopStickySurface();

                // Apply normal jump force
                p_rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                jump = false;
                return; // Exit early since we handled the sticky surface jump
            }
            else
            {
                jump = false;
            }
        }

        //Dash logic
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
        if (context.performed)
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
                            if (moveLeft) { accelerationRate = accelerationMax; }
                            if (moveRight) { accelerationRate = -accelerationRate; }
                        }
                        else if (dir >0)
                        {
                            if (moveLeft) { accelerationRate = -accelerationRate; }
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
        if (context.performed && !dash && dashNumber >= 1)
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
            dashNumber--;
            if (speed < maxSpeed) { speed += speedBuff; }
            Debug.Log("Dash started");
            DashFeedbackStart.PlayFeedbacks();
            animator.SetTrigger("Dash");
        }
    }

    public void VerticalDash(bool isUp, float verticalDashForce, float verticalDashTime, bool usesEnergy = true, bool callFeedback = true)
    {
        if (dashNumber >= 1)
        {
            if (isUp)   { dashVector = Vector3.up * verticalDashForce; }
            else { dashVector = Vector3.down * verticalDashForce; }
            currentDashTime = verticalDashTime;
            p_rb.AddForce(dashVector, ForceMode.Impulse);

            verticalDash = true;
            if (usesEnergy)
            {
                dashNumber--;
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