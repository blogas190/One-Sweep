using UnityEngine;

// Add this to any platform that has a Mover component.
// It detects when the player lands on top and pushes the platform's
// per-frame delta onto the player's Rigidbody so they ride correctly.

[RequireComponent(typeof(Mover))]
public class MovingPlatform : MonoBehaviour
{
    [Tooltip("Layer(s) the player is on.")]
    public LayerMask playerLayer;

    private Mover mover;

    // All Rigidbodies currently standing on this platform
    private System.Collections.Generic.HashSet<Rigidbody> riders
        = new System.Collections.Generic.HashSet<Rigidbody>();

    void Awake()
    {
        mover = GetComponent<Mover>();
    }

    // Called AFTER Mover.FixedUpdate has already set deltaPosition
    void FixedUpdate()
    {
        if (mover.deltaPosition == Vector3.zero) return;

        foreach (Rigidbody rb in riders)
        {
            if (rb == null) continue;
            // Move the rider by the same amount the platform moved this step
            rb.MovePosition(rb.position + mover.deltaPosition);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsInLayer(collision.gameObject, playerLayer)) return;

        // Only carry the rider when they are ON TOP of the platform
        if (IsOnTop(collision))
        {
            Rigidbody rb = collision.rigidbody;
            if (rb != null) riders.Add(rb);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (!IsInLayer(collision.gameObject, playerLayer)) return;

        Rigidbody rb = collision.rigidbody;
        if (rb == null) return;

        if (IsOnTop(collision))
            riders.Add(rb);
        else
            riders.Remove(rb);   // Slid off the side — stop carrying
    }

    void OnCollisionExit(Collision collision)
    {
        if (!IsInLayer(collision.gameObject, playerLayer)) return;
        Rigidbody rb = collision.rigidbody;
        if (rb != null) riders.Remove(rb);
    }

    // Returns true if the average contact normal points upward enough
    // to mean the player is standing on top (not hitting the side).

    bool IsOnTop(Collision collision)
    {
        Vector3 avgNormal = Vector3.zero;
        foreach (ContactPoint cp in collision.contacts)
            avgNormal += cp.normal;
        avgNormal.Normalize();

        // Normal points from platform surface toward player.
        // If it's mostly up, the player is on top.
        return avgNormal.y > 0.5f;
    }

    static bool IsInLayer(GameObject go, LayerMask mask)
    {
        return (mask.value & (1 << go.layer)) != 0;
    }
}