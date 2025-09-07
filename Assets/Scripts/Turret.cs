using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Turret Settings")]
    public float fireRate = 2f; // Seconds between shots
    public Transform firePoint; // Where bullets spawn from
    public GameObject bulletPrefab; // Bullet prefab to instantiate

    [Header("Trajectory Settings")]
    public TrajectoryType trajectoryType = TrajectoryType.Straight;
    public Vector3 fireDirection = Vector3.right; // Direction for straight shots

    [Header("Arched Trajectory Settings")]
    [Tooltip("Only used for arched trajectory")]
    public Transform target; // Target for arched shots (optional)
    public Vector3 targetPosition; // Manual target position if no transform
    public float arcHeight = 5f; // Height of the arc

    [Header("Bullet Properties")]
    public float bulletSpeed = 10f;
    public float bulletLifetime = 10f;

    [Header("Visual Debugging")]
    public bool showTrajectory = true;
    public Color trajectoryColor = Color.red;
    public int trajectoryResolution = 30; // Points to draw trajectory

    private float lastFireTime;
    private GameStates gameStates;

    public enum TrajectoryType
    {
        Straight,
        Arched
    }

    void Start()
    {
        gameStates = FindAnyObjectByType<GameStates>();

        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = Vector3.zero;
            firePoint = firePointObj.transform;
        }

        fireDirection = fireDirection.normalized;
    }

    void Update()
    {
        // Check if it's time to fire
        if (Time.time >= lastFireTime + fireRate)
        {
            FireBullet();
            lastFireTime = Time.time;
        }
    }

    void FireBullet()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        TurretBullet bulletScript = bullet.GetComponent<TurretBullet>();

        if (bulletScript != null)
        {
            // Configure bullet based on trajectory type
            switch (trajectoryType)
            {
                case TrajectoryType.Straight:
                    bulletScript.InitializeStraight(fireDirection, bulletSpeed, bulletLifetime);
                    break;

                case TrajectoryType.Arched:
                    Vector3 targetPos = target != null ? target.position : targetPosition;
                    bulletScript.InitializeArched(firePoint.position, targetPos, arcHeight, bulletLifetime);
                    break;
            }
        }

        Debug.Log($"Turret fired {trajectoryType} bullet");
    }

    // Visual debugging in scene view
    void OnDrawGizmos()
    {
        if (!showTrajectory || firePoint == null) return;

        Gizmos.color = trajectoryColor;

        switch (trajectoryType)
        {
            case TrajectoryType.Straight:
                DrawStraightTrajectory();
                break;

            case TrajectoryType.Arched:
                DrawArchedTrajectory();
                break;
        }

        // Draw fire point
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(firePoint.position, 0.2f);
    }

    void DrawStraightTrajectory()
    {
        Vector3 start = firePoint.position;
        Vector3 end = start + fireDirection * bulletSpeed * bulletLifetime;

        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, 0.3f);
    }

    void DrawArchedTrajectory()
    {
        Vector3 startPos = firePoint.position;
        Vector3 targetPos = target != null ? target.position : targetPosition;

        // Calculate trajectory points
        for (int i = 0; i < trajectoryResolution; i++)
        {
            float t = (float)i / (trajectoryResolution - 1);
            Vector3 point = CalculateArchedPoint(startPos, targetPos, arcHeight, t);

            if (i > 0)
            {
                float prevT = (float)(i - 1) / (trajectoryResolution - 1);
                Vector3 prevPoint = CalculateArchedPoint(startPos, targetPos, arcHeight, prevT);
                Gizmos.DrawLine(prevPoint, point);
            }

            if (i == trajectoryResolution - 1)
            {
                Gizmos.DrawWireSphere(point, 0.3f);
            }
        }
    }

    Vector3 CalculateArchedPoint(Vector3 start, Vector3 end, float height, float t)
    {
        Vector3 point = Vector3.Lerp(start, end, t);
        point.y += height * Mathf.Sin(t * Mathf.PI);
        return point;
    }
}