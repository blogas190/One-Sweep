using UnityEngine;

/// <summary>
/// Moves a platform along various paths.
/// Requires a Rigidbody set to Kinematic � uses MovePosition so physics
/// contacts are resolved properly and standing Rigidbodies inherit movement.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Mover : MonoBehaviour
{
    // Custom struct for point following, allowing individual wait times.
    [System.Serializable]
    public struct Waypoint
    {
        public Vector3 position;
        [Tooltip("Time in seconds to wait at this point before moving to the next.")]
        public float waitTime;
    }

    [Header("Movement Settings")]
    public MovementType movementType = MovementType.Straight;
    public float speed = 5f;
    public bool autoStart = true;
    public bool loop = true;
    [Tooltip("If enabled, the object rotates to face its direction of travel for all movement types.")]
    public bool faceMovementDirection = false;
    [Tooltip("How fast the object rotates to face the travel direction (degrees/sec). 0 = instant snap.")]
    public float rotationSpeed = 0f;

    [Header("Wait Time Settings")]
    [Tooltip("If checked, the object will wait before starting movement or when reaching end points (Straight/Circular).")]
    public bool useGlobalWaitTime = false;
    [Tooltip("Time in seconds to wait at the start/end points for Straight/Circular movement.")]
    public float globalWaitTime = 2f;

    [Header("Straight Movement")]
    public Vector3 startPoint;
    public Vector3 endPoint;

    [Header("Circular Movement")]
    public Vector3 center;
    public float radius = 2f;
    public bool clockwise = true;
    public CircularPlane plane = CircularPlane.XZ;
    [Tooltip("Starting angle in degrees (0 = right/+X on XZ plane). Clockwise direction applies from this angle.")]
    public float startAngle = 0f;

    [Header("Point Following")]
    [Tooltip("Each point can have its own waiting time defined.")]
    public Waypoint[] waypoints;

    [Header("Debug")]
    public bool showPath = true;
    public Color pathColor = Color.yellow;

    public enum MovementType { Straight, Circular, FollowPoints }
    public enum CircularPlane { XY, XZ, YZ }

    //internals
    private Rigidbody rb;

    private bool isMoving = false;
    private float progress = 0f;
    private int currentPointIndex = 0;
    private bool movingForward = true;

    private bool isWaiting = false;
    private float waitTimer = 0f;

    // Exposed so MovingPlatform can read the per-frame delta
    [HideInInspector] public Vector3 deltaPosition;
    private Vector3 previousPosition;

    void OnValidate()
    {
        // Snap the object to its starting position in the editor whenever
        // relevant fields are changed, so the user gets live preview feedback.
#if UNITY_EDITOR
        if (Application.isPlaying) return;

        // Ensure Rigidbody exists (OnValidate can fire before Awake)
        if (rb == null) rb = GetComponent<Rigidbody>();

        switch (movementType)
        {
            case MovementType.Straight:
                transform.position = startPoint;
                break;

            case MovementType.Circular:
                float angleRad = startAngle * Mathf.Deg2Rad;
                transform.position = center + GetCircularOffset(angleRad);
                break;

            case MovementType.FollowPoints:
                if (waypoints != null && waypoints.Length > 0)
                    transform.position = waypoints[0].position;
                break;
        }
#endif
    }

    void Reset()
    {
        Vector3 p = transform.position;
        startPoint = p;
        endPoint = p + Vector3.right * 5f;
        center = p;
        waypoints = new Waypoint[]
        {
            new Waypoint { position = p,                        waitTime = 1f },
            new Waypoint { position = p + Vector3.right * 5f,  waitTime = 1f },
            new Waypoint { position = p + Vector3.forward * 5f,waitTime = 1f },
        };
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;          // Must be kinematic we drive it manually
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooth visual movement
    }

    void Start()
    {
        InitializeDefaultValues();
        previousPosition = transform.position;

        if (autoStart) StartMovement();
    }

    // Wait timer runs in Update (normal time); actual movement is in FixedUpdate
    void Update()
    {
        if (!isMoving || !isWaiting) return;

        waitTimer += Time.deltaTime;
        if (waitTimer >= GetCurrentWaitTime())
        {
            isWaiting = false;
            waitTimer = 0f;
            ContinueMovement();
        }
    }

    void FixedUpdate()
    {
        deltaPosition = Vector3.zero;

        if (!isMoving || isWaiting) return;

        Vector3 before = rb.position;

        switch (movementType)
        {
            case MovementType.Straight: MoveStraight(); break;
            case MovementType.Circular: MoveCircular(); break;
            case MovementType.FollowPoints: MoveFollowPoints(); break;
        }

        // Record how far the platform moved this physics step
        deltaPosition = rb.position - before;
    }

    void MoveStraight()
    {
        float dist = Vector3.Distance(startPoint, endPoint);
        if (dist < 0.001f) return;

        progress += speed * Time.fixedDeltaTime / dist;
        Vector3 newPos = Vector3.Lerp(startPoint, endPoint, progress);
        rb.MovePosition(newPos);

        if (faceMovementDirection)
        {
            Vector3 dir = (endPoint - startPoint).normalized;
            ApplyFacingDirection(dir);
        }

        if (progress >= 1f)
        {
            if (loop)
            {
                if (useGlobalWaitTime && globalWaitTime > 0f)
                {
                    isWaiting = true;
                }
                else
                {
                    Vector3 tmp = startPoint; startPoint = endPoint; endPoint = tmp;
                    progress = 0f;
                }
            }
            else
            {
                isMoving = false;
                progress = 1f;
            }
        }
    }

    // Rotates the rigidbody to face the given world-space direction.
    // Uses slerp if rotationSpeed > 0, otherwise snaps instantly.
    void ApplyFacingDirection(Vector3 dir)
    {
        if (dir == Vector3.zero) return;
        Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
        if (rotationSpeed > 0f)
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, target, rotationSpeed * Time.fixedDeltaTime));
        else
            rb.MoveRotation(target);
    }

    void MoveCircular()
    {
        // progress stores the current angle in radians directly.
        // Angular speed = linear speed / radius  (arc-length formula: s = r * theta)
        float angularSpeed = speed / radius;
        progress += angularSpeed * Time.fixedDeltaTime;

        float angle = (clockwise ? progress : -progress) + startAngle * Mathf.Deg2Rad;

        rb.MovePosition(center + GetCircularOffset(angle));

        // Apply tangent rotation if requested
        if (faceMovementDirection)
        {
            Vector3 tangent = GetCircularTangent(angle);
            ApplyFacingDirection(tangent);
        }

        // One full revolution = 2π radians
        if (progress >= 2f * Mathf.PI)
        {
            progress -= 2f * Mathf.PI; // keep sub-revolution remainder
            if (!loop)
            {
                isMoving = false;
            }
            else if (useGlobalWaitTime && globalWaitTime > 0f)
            {
                isWaiting = true;
            }
        }
    }

    // Returns the tangent (forward) direction at the given angle on the circle.
    Vector3 GetCircularTangent(float angle)
    {
        // Derivative of GetCircularOffset: d/dangle (cos*r, 0, sin*r) = (-sin*r, 0, cos*r)
        float ms = clockwise ? 1f : -1f; // match direction
        float nx = -Mathf.Sin(angle) * ms;
        float ny = Mathf.Cos(angle) * ms;
        switch (plane)
        {
            case CircularPlane.XY: return new Vector3(nx, ny, 0f).normalized;
            case CircularPlane.XZ: return new Vector3(nx, 0f, ny).normalized;
            case CircularPlane.YZ: return new Vector3(0f, nx, ny).normalized;
        }
        return Vector3.forward;
    }

    // Returns the "up" axis perpendicular to the circular plane, used for LookRotation.
    Vector3 GetCircularUp()
    {
        switch (plane)
        {
            case CircularPlane.XY: return Vector3.back;
            case CircularPlane.XZ: return Vector3.up;
            case CircularPlane.YZ: return Vector3.right;
        }
        return Vector3.up;
    }

    Vector3 GetCircularOffset(float angle)
    {
        float c = Mathf.Cos(angle) * radius;
        float s = Mathf.Sin(angle) * radius;
        switch (plane)
        {
            case CircularPlane.XY: return new Vector3(c, s, 0);
            case CircularPlane.XZ: return new Vector3(c, 0, s);
            case CircularPlane.YZ: return new Vector3(0, c, s);
        }
        return Vector3.zero;
    }

    void MoveFollowPoints()
    {
        if (waypoints.Length < 2) return;

        Vector3 target = waypoints[currentPointIndex].position;
        Vector3 current = rb.position;
        float dist = Vector3.Distance(current, target);

        if (dist > 0.05f)
        {
            rb.MovePosition(Vector3.MoveTowards(current, target, speed * Time.fixedDeltaTime));
            if (faceMovementDirection)
                ApplyFacingDirection((target - current).normalized);
        }
        else
        {
            rb.MovePosition(target);
            if (GetCurrentWaitTime() > 0f) isWaiting = true;
            else AdvancePointIndex();
        }
    }

    float GetCurrentWaitTime()
    {
        if (movementType == MovementType.FollowPoints)
        {
            if (waypoints.Length > 0 && currentPointIndex >= 0 && currentPointIndex < waypoints.Length)
                return waypoints[currentPointIndex].waitTime;
            return 0f;
        }
        return useGlobalWaitTime ? globalWaitTime : 0f;
    }

    void ContinueMovement()
    {
        switch (movementType)
        {
            case MovementType.Straight:
                Vector3 tmp = startPoint; startPoint = endPoint; endPoint = tmp;
                progress = 0f;
                break;
            case MovementType.FollowPoints:
                AdvancePointIndex();
                break;
            case MovementType.Circular:
                progress = 0f;
                break;
        }
    }

    void AdvancePointIndex()
    {
        if (loop)
        {
            currentPointIndex++;
            if (currentPointIndex >= waypoints.Length)
                currentPointIndex = 0; // wrap to start
        }
        else
        {
            currentPointIndex++;
            if (currentPointIndex >= waypoints.Length)
            {
                isMoving = false;
                currentPointIndex = waypoints.Length - 1;
            }
        }
    }

    void InitializeDefaultValues()
    {
        Vector3 p = transform.position;
        if (startPoint == Vector3.zero && endPoint == Vector3.zero)
        { startPoint = p; endPoint = p + Vector3.right * 5f; }
        if (center == Vector3.zero) center = p;
        if (waypoints == null || waypoints.Length == 0)
        {
            waypoints = new Waypoint[]
            {
                new Waypoint { position = p,                       waitTime = 1f },
                new Waypoint { position = p + Vector3.right * 5f, waitTime = 1f },
            };
        }
    }

    public void StartMovement()
    {
        isMoving = true; progress = 0f; currentPointIndex = 0;
        movingForward = true; isWaiting = false; waitTimer = 0f;

        switch (movementType)
        {
            case MovementType.Straight:
                rb.MovePosition(startPoint);
                break;
            case MovementType.Circular:
                rb.MovePosition(center + GetCircularOffset(startAngle * Mathf.Deg2Rad));
                break;
            case MovementType.FollowPoints:
                if (waypoints.Length > 0)
                {
                    rb.MovePosition(waypoints[0].position);
                    if (waypoints[0].waitTime > 0f) isWaiting = true;
                }
                break;
        }
    }

    public void StopMovement() { isMoving = false; isWaiting = false; waitTimer = 0f; }
    public void ResetToStart() { StopMovement(); StartMovement(); }

    public bool IsMoving() => isMoving;
    public float GetProgress() => progress;
    public Vector3 GetCurrentTarget()
    {
        switch (movementType)
        {
            case MovementType.Straight: return endPoint;
            case MovementType.Circular: return center;
            case MovementType.FollowPoints:
                if (waypoints.Length > 0 && currentPointIndex >= 0 && currentPointIndex < waypoints.Length)
                    return waypoints[currentPointIndex].position;
                break;
        }
        return transform.position;
    }

    void OnDrawGizmosSelected()
    {
        if (!showPath) return;
        Gizmos.color = pathColor;
        switch (movementType)
        {
            case MovementType.Straight: DrawStraightPath(); break;
            case MovementType.Circular: DrawCircularPath(); break;
            case MovementType.FollowPoints: DrawPointsPath(); break;
        }
    }

    void DrawStraightPath()
    {
        Gizmos.DrawLine(startPoint, endPoint);
        Gizmos.DrawWireSphere(startPoint, 0.2f);
        Gizmos.DrawWireSphere(endPoint, 0.2f);
    }

    void DrawCircularPath()
    {
        int segments = 64;
        float step = 2f * Mathf.PI / segments;
        for (int i = 0; i < segments; i++)
            Gizmos.DrawLine(center + GetCircularOffset(i * step), center + GetCircularOffset((i + 1) * step));
        Gizmos.DrawWireSphere(center, 0.1f);
    }

    void DrawPointsPath()
    {
        if (waypoints.Length < 2) return;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Gizmos.color = (i == currentPointIndex && isMoving) ? Color.red : pathColor;
            Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);
            if (i < waypoints.Length - 1)
            { Gizmos.color = pathColor; Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position); }
        }
        if (loop && waypoints.Length > 2)
        { Gizmos.color = Color.cyan; Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position); }
    }
}