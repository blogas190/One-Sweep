using UnityEngine;

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
    public CircularPlane plane = CircularPlane.XZ; // XZ is horizontal in 3D

    [Header("Point Following")]
    [Tooltip("Each point can have its own waiting time defined.")]
    public Waypoint[] waypoints;

    [Header("Debug")]
    public bool showPath = true;
    public Color pathColor = Color.yellow;

    public enum MovementType
    {
        Straight,
        Circular,
        FollowPoints
    }

    public enum CircularPlane
    {
        XY,  // 2D style (vertical circle)
        XZ,  // Horizontal circle (most common in 3D)
        YZ   // Side circle
    }

    private bool isMoving = false;
    private float progress = 0f;
    private int currentPointIndex = 0;
    private bool movingForward = true;
    private Vector3 initialPosition;

    // Wait time variables
    private bool isWaiting = false;
    private float waitTimer = 0f;

    void Reset()
    {
        Vector3 currentPos = transform.position;

        // Set default values based on current object position
        startPoint = currentPos;
        endPoint = currentPos + Vector3.right * 5f;
        center = currentPos;

        // Use the new Waypoint struct for defaults
        waypoints = new Waypoint[] {
            new Waypoint { position = currentPos, waitTime = 1f },
            new Waypoint { position = currentPos + Vector3.right * 5f, waitTime = 1f },
            new Waypoint { position = currentPos + Vector3.forward * 5f, waitTime = 1f },
        };
    }

    void Start()
    {
        InitializeDefaultValues();
        initialPosition = transform.position;

        if (autoStart)
        {
            StartMovement();
        }
    }

    void InitializeDefaultValues()
    {
        Vector3 currentPos = transform.position;

        // Set default straight movement points if they haven't been changed from zero
        if (startPoint == Vector3.zero && endPoint == Vector3.zero)
        {
            startPoint = currentPos;
            endPoint = currentPos + Vector3.right * 5f;
        }

        // Set default circular center if it hasn't been changed from zero
        if (center == Vector3.zero)
        {
            center = currentPos;
        }

        // Set default points array if it's empty or null
        if (waypoints == null || waypoints.Length == 0)
        {
            waypoints = new Waypoint[] {
                new Waypoint { position = currentPos, waitTime = 1f },
                new Waypoint { position = currentPos + Vector3.right * 5f, waitTime = 1f },
            };
        }
    }

    void Update()
    {
        if (!isMoving) return;

        if (isWaiting)
        {
            // Handle waiting state
            waitTimer += Time.deltaTime;
            if (waitTimer >= GetCurrentWaitTime())
            {
                isWaiting = false;
                waitTimer = 0f;
                // Once waiting is over, perform the step logic
                ContinueMovement();
            }
        }
        else
        {
            // Handle movement state
            switch (movementType)
            {
                case MovementType.Straight:
                    MoveStraight();
                    break;
                case MovementType.Circular:
                    MoveCircular();
                    break;
                case MovementType.FollowPoints:
                    MoveFollowPoints();
                    break;
            }
        }
    }

    // Determines the required wait time based on movement type and current index
    float GetCurrentWaitTime()
    {
        if (movementType == MovementType.FollowPoints)
        {
            if (waypoints.Length > 0 && currentPointIndex >= 0 && currentPointIndex < waypoints.Length)
            {
                return waypoints[currentPointIndex].waitTime;
            }
            return 0f; // Default wait time if points array is invalid
        }
        else // Straight or Circular
        {
            return useGlobalWaitTime ? globalWaitTime : 0f;
        }
    }

    // Called when the waiting phase is complete.
    void ContinueMovement()
    {
        switch (movementType)
        {
            case MovementType.Straight:
                // After waiting, swap points and reset progress to start the new journey
                Vector3 temp = startPoint;
                startPoint = endPoint;
                endPoint = temp;
                progress = 0f;
                break;
            case MovementType.FollowPoints:
                // After waiting, advance the point index
                if (movingForward)
                {
                    currentPointIndex++;
                }
                else
                {
                    currentPointIndex--;
                }
                break;
            case MovementType.Circular:
                // For circular loop, simply reset progress
                progress = 0f;
                break;
        }
    }

    // --- Movement Implementations ---

    void MoveStraight()
    {
        progress += speed * Time.deltaTime / Vector3.Distance(startPoint, endPoint);

        Vector3 currentPos = Vector3.Lerp(startPoint, endPoint, progress);
        transform.position = currentPos;

        if (progress >= 1f)
        {
            if (loop)
            {
                // Enter waiting state if wait time is used
                if (useGlobalWaitTime && globalWaitTime > 0f)
                {
                    isWaiting = true;
                }
                else
                {
                    // If no wait time, immediately swap points and continue
                    Vector3 temp = startPoint;
                    startPoint = endPoint;
                    endPoint = temp;
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

    void MoveCircular()
    {
        progress += speed * Time.deltaTime; // Progress here is distance traveled, not 0-1 interpolation

        float angle = progress / radius; // Calculate angle based on arc length/radius
        if (!clockwise) angle = -angle;

        Vector3 offset = GetCircularOffset(angle);
        transform.position = center + offset;

        // Check if a full circle is completed (progress > 2 * PI * radius)
        if (progress * 2f * Mathf.PI >= 360f * Mathf.Deg2Rad * radius)
        {
            if (loop)
            {
                // Enter waiting state if wait time is used
                if (useGlobalWaitTime && globalWaitTime > 0f)
                {
                    isWaiting = true;
                    // Keep progress at 0 for smooth loop start after wait
                    progress = 0f;
                }
                else
                {
                    progress = 0f; // Immediately reset progress
                }
            }
            else
            {
                isMoving = false;
                // Clamp position to final rotation if needed, but for circular it just stops.
            }
        }
    }

    // Helper to calculate circular offset based on plane
    Vector3 GetCircularOffset(float angle)
    {
        Vector3 offset = Vector3.zero;
        float cosA = Mathf.Cos(angle) * radius;
        float sinA = Mathf.Sin(angle) * radius;

        switch (plane)
        {
            case CircularPlane.XY:
                offset = new Vector3(cosA, sinA, 0);
                break;
            case CircularPlane.XZ:
                // Note: Cosine is usually X, Sine is Z for horizontal plane
                offset = new Vector3(cosA, 0, sinA);
                break;
            case CircularPlane.YZ:
                offset = new Vector3(0, cosA, sinA);
                break;
        }
        return offset;
    }

    void MoveFollowPoints()
    {
        if (waypoints.Length < 2) return;

        // Get the target position from the current waypoint struct
        Vector3 currentTargetPos = waypoints[currentPointIndex].position;
        Vector3 currentPos = transform.position;

        Vector3 direction = (currentTargetPos - currentPos).normalized;
        float distance = Vector3.Distance(currentPos, currentTargetPos);

        if (distance > 0.05f) // Use a small epsilon for checking arrival
        {
            Vector3 newPos = Vector3.MoveTowards(currentPos, currentTargetPos, speed * Time.deltaTime);
            transform.position = newPos;
        }
        else
        {
            // Snap to point and initiate wait
            transform.position = currentTargetPos;

            // Check if there is a wait time at this specific point
            if (GetCurrentWaitTime() > 0f)
            {
                isWaiting = true;
            }
            else
            {
                // If no wait time, immediately advance to the next point
                AdvancePointIndex();
            }
        }
    }

    // Handles the complex index advancement logic for FollowPoints (looping/ping-pong)
    void AdvancePointIndex()
    {
        if (loop)
        {
            if (movingForward)
            {
                currentPointIndex++;
                if (currentPointIndex >= waypoints.Length)
                {
                    // Reverse direction when hitting the end
                    currentPointIndex = waypoints.Length - 2; // Move back to the second to last point
                    movingForward = false;
                }
            }
            else // moving backward
            {
                currentPointIndex--;
                if (currentPointIndex < 0)
                {
                    // Reverse direction when hitting the start
                    currentPointIndex = 1; // Move to the second point
                    movingForward = true;
                }
            }
        }
        else // No loop
        {
            currentPointIndex++;
            if (currentPointIndex >= waypoints.Length)
            {
                isMoving = false;
                currentPointIndex = waypoints.Length - 1;
            }
        }
    }


    // --- Public Control Methods ---

    public void StartMovement()
    {
        isMoving = true;
        progress = 0f;
        currentPointIndex = 0;
        movingForward = true;
        isWaiting = false;
        waitTimer = 0f;

        // Set initial position based on movement type
        switch (movementType)
        {
            case MovementType.Straight:
                transform.position = startPoint;
                break;
            case MovementType.Circular:
                Vector3 startOffset = GetCircularOffset(0f);
                transform.position = center + startOffset;
                break;
            case MovementType.FollowPoints:
                if (waypoints.Length > 0)
                {
                    transform.position = waypoints[0].position;
                    // Start waiting immediately at the first point if it has a wait time
                    if (waypoints[0].waitTime > 0f)
                    {
                        isWaiting = true;
                    }
                }
                break;
        }
    }

    public void StopMovement()
    {
        isMoving = false;
        isWaiting = false;
        waitTimer = 0f;
    }

    public void ResetToStart()
    {
        StopMovement();
        StartMovement();
    }

    // Public getters for other scripts
    public bool IsMoving() => isMoving;
    public float GetProgress() => progress;

    // Updated GetCurrentTarget to handle waypoints
    public Vector3 GetCurrentTarget()
    {
        switch (movementType)
        {
            case MovementType.Straight:
                return endPoint;
            case MovementType.Circular:
                return center;
            case MovementType.FollowPoints:
                if (waypoints.Length > 0 && currentPointIndex >= 0 && currentPointIndex < waypoints.Length)
                    return waypoints[currentPointIndex].position;
                break;
        }
        return transform.position;
    }

    // --- Debug Visualization ---

    void OnDrawGizmosSelected()
    {
        if (!showPath) return;

        Gizmos.color = pathColor;

        switch (movementType)
        {
            case MovementType.Straight:
                DrawStraightPath();
                break;
            case MovementType.Circular:
                DrawCircularPath();
                break;
            case MovementType.FollowPoints:
                DrawPointsPath();
                break;
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
        // Draw circle
        int segments = 64;
        float angleStep = 2f * Mathf.PI / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep;
            float angle2 = (i + 1) * angleStep;

            Vector3 point1 = center + GetCircularOffset(angle1);
            Vector3 point2 = center + GetCircularOffset(angle2);

            Gizmos.DrawLine(point1, point2);
        }

        // Draw center
        Gizmos.DrawWireSphere(center, 0.1f);
    }

    void DrawPointsPath()
    {
        if (waypoints.Length < 2) return;

        for (int i = 0; i < waypoints.Length; i++)
        {
            // Draw point sphere
            Gizmos.color = (i == currentPointIndex && isMoving) ? Color.red : pathColor;
            Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);

            // Draw line to next point
            if (i < waypoints.Length - 1)
            {
                Gizmos.color = pathColor;
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }

        // Draw loop connection if looping
        if (loop && waypoints.Length > 2)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }
    }
}
