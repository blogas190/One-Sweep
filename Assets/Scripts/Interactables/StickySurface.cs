using UnityEngine;

public class StickySurface : MonoBehaviour
{
    [Header("Sticky Surface Settings")]
    public bool useCustomNormal = false;
    public Vector3 customSurfaceNormal = Vector3.up;

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();

        if (playerMovement != null)
        {
            Debug.Log("Player entered sticky surface");

            Vector3 surfaceNormal;

            if (useCustomNormal)
            {
                surfaceNormal = customSurfaceNormal.normalized;
            }
            else
            {
                // Calculate surface normal automatically
                surfaceNormal = CalculateSurfaceNormal(other.transform.position);
            }

            playerMovement.StartStickySurface(surfaceNormal);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();

        if (playerMovement != null)
        {
            Debug.Log("Player left sticky surface");
            playerMovement.StopStickySurface();
        }
    }

    private Vector3 CalculateSurfaceNormal(Vector3 playerPosition)
    {
        // Try to calculate the surface normal by raycasting from the player to this surface
        Vector3 directionToSurface = (transform.position - playerPosition).normalized;
        RaycastHit hit;

        if (Physics.Raycast(playerPosition, directionToSurface, out hit, 10f))
        {
            if (hit.collider == GetComponent<Collider>())
            {
                return hit.normal;
            }
        }

        // Fallback to transform's up direction
        return transform.up;
    }

    private void OnValidate()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (useCustomNormal)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, customSurfaceNormal * 2f);
        }
    }
}