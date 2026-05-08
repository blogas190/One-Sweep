using UnityEngine;
using System.Collections;

public enum TurretState { Shooting, Reloading, Activating, Disabled }

public class Turret : MonoBehaviour
{
    [Header("Turret Settings")]
    public bool startsActive = true;
    public float fireRate = 2f;
    public int shotsPerClip = 10;
    public Transform firePoint;
    public GameObject bulletPrefab;

    [Header("Animation Settings")]
    public Animator animator;
    public string shootingTrigger = "Fire";
    public string startReloadTrigger = "StartReload";
    public float startReloadDelay = 1.0f;
    public string reloadBool = "IsReloading";
    public string activateTrigger = "Activate";
    public float activationDelay = 1.5f;
    public float reloadDuration = 3f;
    public bool syncAnimSpeedToFireRate = true;

    [Header("Trajectory & Projectile")]
    public TrajectoryType trajectoryType = TrajectoryType.Straight;
    public Vector3 fireDirection = Vector3.right;
    public Transform target;
    public Vector3 targetPosition;
    public float arcHeight = 5f;
    public float bulletSpeed = 10f;
    public float bulletLifetime = 10f;

    [Header("Visual Debugging")]
    public bool showTrajectory = true;
    public Color trajectoryColor = Color.red;
    public int trajectoryResolution = 30;

    private float lastFireTime;
    private int currentShots;
    private TurretState currentState;
    private Coroutine activeSequence;

    public enum TrajectoryType { Straight, Arched }

    void Start()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = Vector3.zero;
            firePoint = firePointObj.transform;
        }

        fireDirection = fireDirection.normalized;

        if (startsActive)
        {
            EnableTurret();
        }
        else
        {
            DisableTurret();
        }
    }

    void Update()
    {
        if (currentState != TurretState.Shooting) return;

        if (Time.time >= lastFireTime + fireRate)
        {
            FireBullet();
            lastFireTime = Time.time;
            currentShots--;

            if (currentShots <= 0)
            {
                if (activeSequence != null) StopCoroutine(activeSequence);
                activeSequence = StartCoroutine(ReloadSequence());
            }
        }
    }

    // --- Public API ---

    /// <summary>
    /// Powers on the turret. It will play the Activation animation before shooting.
    /// </summary>
    public void EnableTurret()
    {
        if (activeSequence != null) StopCoroutine(activeSequence);
        activeSequence = StartCoroutine(InitialActivation());
    }

    /// <summary>
    /// Immediately stops the turret and puts it into the Inactive/Reloading visual state.
    /// </summary>
    public void DisableTurret()
    {
        if (activeSequence != null) StopCoroutine(activeSequence);
        currentState = TurretState.Disabled;

        if (animator != null)
        {
            animator.ResetTrigger(shootingTrigger);
            animator.ResetTrigger(activateTrigger);
            animator.SetTrigger(startReloadTrigger);  // trigger the closing animation
            animator.SetBool(reloadBool, true);
        }
    }

    // --- Sequences ---

    IEnumerator InitialActivation()
    {
        currentState = TurretState.Activating;

        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetBool(reloadBool, false);
            animator.SetTrigger(activateTrigger);
        }

        yield return new WaitForSeconds(activationDelay);

        currentShots = shotsPerClip;
        currentState = TurretState.Shooting;
        lastFireTime = Time.time;
    }

    IEnumerator ReloadSequence()
    {
        currentState = TurretState.Reloading;
        if (animator != null) animator.speed = 1f;

        // 1. Play "Folding"
        if (animator != null)
        {
            animator.SetTrigger(startReloadTrigger);
            yield return new WaitForSeconds(startReloadDelay);
        }

        // 2. Inactive Loop
        if (animator != null) animator.SetBool(reloadBool, true);
        yield return new WaitForSeconds(reloadDuration);

        // 3. Unfold/Activate
        yield return StartCoroutine(InitialActivation());
    }

    // --- Firing Logic ---

    void FireBullet()
    {
        if (bulletPrefab == null || firePoint == null) return;

        if (animator != null)
        {
            if (syncAnimSpeedToFireRate) animator.speed = 1f / fireRate;
            animator.SetTrigger(shootingTrigger);
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        TurretBullet bulletScript = bullet.GetComponent<TurretBullet>();

        if (bulletScript != null)
        {
            if (trajectoryType == TrajectoryType.Straight)
                bulletScript.InitializeStraight(fireDirection, bulletSpeed, bulletLifetime);
            else
                bulletScript.InitializeArched(firePoint.position, target != null ? target.position : targetPosition, arcHeight, bulletLifetime);
        }
    }

    // --- Gizmos ---
    void OnDrawGizmos()
    {
        if (!showTrajectory || firePoint == null) return;
        Gizmos.color = trajectoryColor;
        if (trajectoryType == TrajectoryType.Straight) DrawStraightTrajectory();
        else DrawArchedTrajectory();
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
        for (int i = 0; i < trajectoryResolution; i++)
        {
            float t = (float)i / (trajectoryResolution - 1);
            Vector3 point = CalculateArchedPoint(startPos, targetPos, arcHeight, t);
            if (i > 0)
            {
                Vector3 prevPoint = CalculateArchedPoint(startPos, targetPos, arcHeight, (float)(i - 1) / (trajectoryResolution - 1));
                Gizmos.DrawLine(prevPoint, point);
            }
            if (i == trajectoryResolution - 1) Gizmos.DrawWireSphere(point, 0.3f);
        }
    }

    Vector3 CalculateArchedPoint(Vector3 start, Vector3 end, float height, float t)
    {
        Vector3 point = Vector3.Lerp(start, end, t);
        point.y += height * Mathf.Sin(t * Mathf.PI);
        return point;
    }
}