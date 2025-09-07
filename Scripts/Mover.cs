using UnityEngine;

public class Mover : MonoBehaviour
{
    [Header("Movement Settings")]
    public MovementType movementType = MovementType.Straight;
    public float speed = 5f;
    public bool autoStart = true;
    public bool loop = true;

    [Header("Straight Movement")]
    public Vector2 startPoint;
    public Vector2 endPoint;

    [Header("Circular Movement")]
    public Vector2 center;
    public float radius = 2f;
    public bool clockwise = true;

    [Header("Point Following")]
    public Vector2[] points;

    [Header("Debug")]
    public bool showPath = true;
    public Color pathColor = Color.yellow;

    public enum MovementType
    {
        Straight,
        Circular,
        FollowPoints
    }

    private bool isMoving = false;
    private float progress = 0f;
    private int currentPointIndex = 0;
    private bool movingForward = true;
    private Vector3 initialPosition;

    void Reset()
    {
        Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);

        // Set default values based on current object position
        startPoint = currentPos;
        endPoint = currentPos + Vector2.right;
        center = currentPos;
        points = new Vector2[] {
            currentPos,
            currentPos + Vector2.right,
            currentPos + Vector2.up,
            currentPos + Vector2.left
        };
    }

    void Start()
    {
        initialPosition = transform.position;

        if (autoStart)
        {
            StartMovement();
        }
    }

    void InitializeDefaultValues()
    {
        Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);

        // Set default straight movement points if they haven't been changed from zero
        if (startPoint == Vector2.zero && endPoint == Vector2.zero)
        {
            startPoint = currentPos;
            endPoint = currentPos + Vector2.right;
        }

        // Set default circular center if it hasn't been changed from zero
        if (center == Vector2.zero)
        {
            center = currentPos;
        }

        // Set default points array if it's empty or null
        if (points == null || points.Length == 0)
        {
            points = new Vector2[] {
                currentPos,
                currentPos + Vector2.right,
                currentPos + Vector2.up,
                currentPos + Vector2.left
            };
        }
    }

    void Update()
    {
        if (isMoving)
        {
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

    void MoveStraight()
    {
        progress += speed * Time.deltaTime;

        Vector2 currentPos = Vector2.Lerp(startPoint, endPoint, progress);
        transform.position = new Vector3(currentPos.x, currentPos.y, transform.position.z);

        if (progress >= 1f)
        {
            if (loop)
            {
                if (movingForward)
                {
                    // Swap points for ping-pong effect
                    Vector2 temp = startPoint;
                    startPoint = endPoint;
                    endPoint = temp;
                }
                progress = 0f;
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
        progress += speed * Time.deltaTime;

        float angle = progress * 2f * Mathf.PI;
        if (!clockwise) angle = -angle;

        Vector2 offset = new Vector2(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius
        );

        Vector2 currentPos = center + offset;
        transform.position = new Vector3(currentPos.x, currentPos.y, transform.position.z);

        if (progress >= 1f)
        {
            if (loop)
            {
                progress = 0f;
            }
            else
            {
                isMoving = false;
                progress = 1f;
            }
        }
    }

    void MoveFollowPoints()
    {
        if (points.Length < 2) return;

        Vector2 currentTarget = points[currentPointIndex];
        Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);

        Vector2 direction = (currentTarget - currentPos).normalized;
        float distance = Vector2.Distance(currentPos, currentTarget);

        if (distance > 0.1f)
        {
            Vector2 newPos = currentPos + direction * speed * Time.deltaTime;
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }
        else
        {
            // Reached current target point
            if (loop)
            {
                if (movingForward)
                {
                    currentPointIndex++;
                    if (currentPointIndex >= points.Length)
                    {
                        currentPointIndex = points.Length - 2;
                        movingForward = false;
                    }
                }
                else
                {
                    currentPointIndex--;
                    if (currentPointIndex < 0)
                    {
                        currentPointIndex = 1;
                        movingForward = true;
                    }
                }
            }
            else
            {
                currentPointIndex++;
                if (currentPointIndex >= points.Length)
                {
                    isMoving = false;
                    currentPointIndex = points.Length - 1;
                }
            }
        }
    }

    public void StartMovement()
    {
        isMoving = true;
        progress = 0f;
        currentPointIndex = 0;
        movingForward = true;

        // Set initial position based on movement type
        switch (movementType)
        {
            case MovementType.Straight:
                transform.position = new Vector3(startPoint.x, startPoint.y, transform.position.z);
                break;
            case MovementType.Circular:
                Vector2 startPos = center + new Vector2(radius, 0);
                transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);
                break;
            case MovementType.FollowPoints:
                if (points.Length > 0)
                    transform.position = new Vector3(points[0].x, points[0].y, transform.position.z);
                break;
        }
    }

    public void StopMovement()
    {
        isMoving = false;
    }

    public void ResetToStart()
    {
        StopMovement();
        progress = 0f;
        currentPointIndex = 0;
        movingForward = true;
        StartMovement();
    }

    // Public getters for other scripts
    public bool IsMoving() => isMoving;
    public float GetProgress() => progress;
    public Vector2 GetCurrentTarget()
    {
        switch (movementType)
        {
            case MovementType.Straight:
                return endPoint;
            case MovementType.Circular:
                return center;
            case MovementType.FollowPoints:
                if (points.Length > 0 && currentPointIndex < points.Length)
                    return points[currentPointIndex];
                break;
        }
        return Vector2.zero;
    }

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
        Vector3 start = new Vector3(startPoint.x, startPoint.y, transform.position.z);
        Vector3 end = new Vector3(endPoint.x, endPoint.y, transform.position.z);

        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(start, 0.2f);
        Gizmos.DrawWireSphere(end, 0.2f);
    }

    void DrawCircularPath()
    {
        Vector3 centerPos = new Vector3(center.x, center.y, transform.position.z);

        // Draw circle
        int segments = 64;
        float angleStep = 2f * Mathf.PI / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep;
            float angle2 = (i + 1) * angleStep;

            Vector3 point1 = centerPos + new Vector3(Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius, 0);
            Vector3 point2 = centerPos + new Vector3(Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius, 0);

            Gizmos.DrawLine(point1, point2);
        }

        // Draw center
        Gizmos.DrawWireSphere(centerPos, 0.1f);
    }

    void DrawPointsPath()
    {
        if (points.Length < 2) return;

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 point1 = new Vector3(points[i].x, points[i].y, transform.position.z);
            Vector3 point2 = new Vector3(points[i + 1].x, points[i + 1].y, transform.position.z);

            Gizmos.DrawLine(point1, point2);
            Gizmos.DrawWireSphere(point1, 0.2f);
        }

        // Draw last point
        if (points.Length > 0)
        {
            Vector3 lastPoint = new Vector3(points[points.Length - 1].x, points[points.Length - 1].y, transform.position.z);
            Gizmos.DrawWireSphere(lastPoint, 0.2f);
        }

        // Draw loop connection if looping
        if (loop && points.Length > 2)
        {
            Vector3 firstPoint = new Vector3(points[0].x, points[0].y, transform.position.z);
            Vector3 lastPoint = new Vector3(points[points.Length - 1].x, points[points.Length - 1].y, transform.position.z);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(lastPoint, firstPoint);
        }
    }
}