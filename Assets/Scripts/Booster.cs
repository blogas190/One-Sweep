using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Booster : MonoBehaviour
{
    [Header("Booster Settings")]
    [Tooltip("Direction of the boost. Use the transform's forward direction for visualization.")]
    public Vector3 boostDirection = Vector3.forward;

    [Tooltip("Force applied to the player.")]
    public float boostForce = 10f;

    [Tooltip("Whether to use this transform's forward vector as the boost direction.")]
    public bool useTransformForward = true;

    private void Reset()
    {
        // Make sure collider is set to trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                Vector3 dir = useTransformForward ? transform.forward : boostDirection.normalized;
                rb.AddForce(dir * boostForce, ForceMode.VelocityChange);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw arrow for visualization
        Gizmos.color = Color.cyan;
        Vector3 dir = useTransformForward ? transform.forward : boostDirection.normalized;
        Gizmos.DrawRay(transform.position, dir * 2f);
        Gizmos.DrawSphere(transform.position + dir * 2f, 0.1f);
    }
}
