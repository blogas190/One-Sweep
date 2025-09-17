using UnityEngine;
using UnityEngine.InputSystem;

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
    public int dashNumber = 3;
    public float speedBuff = 5f;

    [Header("Rail Settings")]
    public float railSpeed = 20f;

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

        // Check for delayed rail attachment
        railCheck.GetComponent<RailCheck>().CheckDelayedAttachment();

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
        if (Grounded() && (moveLeft || moveRight) && speed < maxSpeed)
        {
            if (speed < startSpeed)
            {
                speed = startSpeed;
            }
            else
            { speed += accelerationRate * Time.fixedDeltaTime; }
        }

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
                p_rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                jump = false;
            }
            if (onWall)
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

            if (currentDashTime <= 0f)
            {
                dash = false;
                Debug.Log("Dash ended");
            }
        }

        if (onRail && railCheck.GetComponent<RailCheck>().currentRail != null)
        {
            RailMovement(railCheck.GetComponent<RailCheck>().currentRail);
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
        if (context.performed)
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
            }
            else
            {
                dashVector = new Vector3(dashDirection, 0f, 0f) * airDashForce;
            }
            p_rb.AddForce(dashVector, ForceMode.Impulse);

            currentDashTime = dashTime;
            dash = true;
            dashNumber--;
            if (speed < maxSpeed) { speed += speedBuff; }
            Debug.Log("Dash started");
        }
    }

    //--------------------Rails-----------------------------

    public void RailStartMovement(GameObject rail)
    {
        float bottomY = p_renderer.bounds.min.y / 2;
        Vector3 railTop = new Vector3(transform.position.x, rail.transform.position.y + bottomY, rail.transform.position.z);

        // Only snap to rail position and stop Y velocity if player is at or above the target height
        if (transform.position.y >= railTop.y)
        {
            transform.position = railTop;

            // Cancel any upward velocity when at rail height
            Vector3 velocity = p_rb.linearVelocity;
            velocity.y = 0f;
            p_rb.linearVelocity = velocity;

            p_rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            // Player is still below rail, let them continue moving up
            // Just set the X and Z position for rail alignment
            transform.position = new Vector3(transform.position.x, transform.position.y, rail.transform.position.z);
            p_rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        accelerationRate += railSpeed;
        RailMovement(rail);
    }

    public void RailStartMovementAngled(GameObject rail)
    {
        // Use raycasting to find the exact point below the player
        transform.position = new Vector3(transform.position.x, transform.position.y, rail.transform.position.z);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, Mathf.Infinity))
        {
            Vector3 railContactPoint = hit.point + hit.normal * 0.1f;

            // Only snap to rail and stop Y velocity if player is at or above the target height
            if (transform.position.y >= railContactPoint.y)
            {
                transform.position = railContactPoint;

                // Align player with rail's angle
                Quaternion slopeRotation = Quaternion.FromToRotation(transform.up, hit.normal);
                transform.rotation = slopeRotation * transform.rotation;

                // Cancel any upward velocity when at rail height
                Vector3 velocity = p_rb.linearVelocity;
                velocity.y = 0f;
                p_rb.linearVelocity = velocity;

                p_rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
            else
            {
                // Player is still below rail, let them continue moving up
                p_rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }

        accelerationRate += railSpeed;
        onRail = true;
        RailMovementAngled(rail);
    }

    public void RailMovementAngled(GameObject rail)
    {
        // First, ensure we're still properly positioned on the rail
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, 2f))
        {
            // Maintain position slightly above rail surface
            Vector3 targetPosition = hit.point + hit.normal * 0.1f;
            transform.position = targetPosition;
        }

        // Calculate movement direction along the rail
        Vector3 railDirection = Vector3.Cross(hit.normal, transform.right).normalized;
        Vector3 movement = railDirection * direction * speed * Time.fixedDeltaTime;

        // Apply movement
        transform.position += movement;
    }

    public void RailMovement(GameObject rail)
    {
        Vector3 railVector = new Vector3(direction, 0f, 0f);
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
        if (Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer) || onRail) { ground = true; }
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