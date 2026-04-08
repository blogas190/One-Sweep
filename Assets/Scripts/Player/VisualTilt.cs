using UnityEngine;

/// <summary>
/// Attach this to the visual child GameObject (the one with the mesh/skinned mesh).
/// The Rigidbody on the root stays rotation-frozen.
/// Facing direction (left/right) is handled by PlayerMovement scaling localScale.x by -1 — 
/// this component never touches that. It ONLY adjusts the Z-axis tilt to match slopes/rails.
/// </summary>
public class VisualTilt : MonoBehaviour
{
    // --------------------------------------------------------
    // Inspector
    // --------------------------------------------------------

    [Header("References")]
    [Tooltip("The PlayerMovement on the root Rigidbody object.")]
    public PlayerMovement playerMovement;

    [Header("Raycast Settings")]
    [Tooltip("How far below the feet to cast. Should be slightly longer than half your collider height.")]
    public float rayLength = 1.2f;

    [Tooltip("Half-width between the two foot rays. Wider = more stable on corners.")]
    public float raySpread = 0.2f;

    [Tooltip("Layer(s) that count as ground / rail surface.")]
    public LayerMask groundLayer;

    [Header("Rotation Settings")]
    [Tooltip("How fast the visual rotates to match the slope (degrees per second).")]
    public float tiltSpeed = 720f;

    [Tooltip("How fast the visual returns to upright when airborne (degrees per second).")]
    public float uprightSpeed = 360f;

    [Tooltip("Maximum tilt angle allowed (clamps extreme geometry).")]
    [Range(0f, 60f)]
    public float maxTiltAngle = 40f;

    // --------------------------------------------------------
    // Private state
    // --------------------------------------------------------

    private float currentTiltZ = 0f;
    private float targetTiltZ = 0f;
    private Vector3 lastSurfaceNormal = Vector3.up;

    void Start()
    {
        if (playerMovement == null)
            playerMovement = GetComponentInParent<PlayerMovement>();
    }

    void LateUpdate()
    {
        bool grounded = playerMovement != null && playerMovement.Grounded();

        if (grounded)
        {
            Vector3 normal = SampleSurfaceNormal();
            lastSurfaceNormal = normal;

            // Convert surface normal to a Z tilt angle.
            // normal.x tells us how much the surface leans left/right in world space.
            float facingSign = transform.parent != null ? Mathf.Sign(transform.parent.localScale.x) : 1f;
            float rawAngle = Mathf.Atan2(normal.x * facingSign, normal.y) * Mathf.Rad2Deg;
            targetTiltZ = Mathf.Clamp(rawAngle, -maxTiltAngle, maxTiltAngle);

            currentTiltZ = Mathf.MoveTowards(currentTiltZ, targetTiltZ, tiltSpeed * Time.deltaTime);
        }
        else
        {
            // Airborne: ease back to upright.
            currentTiltZ = Mathf.MoveTowards(currentTiltZ, 0f, uprightSpeed * Time.deltaTime);
        }

        // Apply ONLY the Z tilt. X and Y stay zero so the root's localScale.x
        // flip for facing direction is never interfered with.
        transform.localEulerAngles = new Vector3(0f, 180f, currentTiltZ);
    }

    // --------------------------------------------------------
    // Helpers
    // --------------------------------------------------------

    private Vector3 SampleSurfaceNormal()
    {
        Vector3 origin = transform.position;
        Vector3 left = origin + Vector3.left * raySpread;
        Vector3 right = origin + Vector3.right * raySpread;

        bool hitL = Physics.Raycast(left, Vector3.down, out RaycastHit infoL, rayLength, groundLayer);
        bool hitR = Physics.Raycast(right, Vector3.down, out RaycastHit infoR, rayLength, groundLayer);

        if (hitL && hitR) return (infoL.normal + infoR.normal).normalized;
        if (hitL) return infoL.normal;
        if (hitR) return infoR.normal;

        return lastSurfaceNormal;
    }

    // --------------------------------------------------------
    // Gizmos
    // --------------------------------------------------------

    void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin + Vector3.left * raySpread, origin + Vector3.left * raySpread + Vector3.down * rayLength);
        Gizmos.DrawLine(origin + Vector3.right * raySpread, origin + Vector3.right * raySpread + Vector3.down * rayLength);
    }
}