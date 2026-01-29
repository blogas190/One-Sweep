using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float lengthDetectDown = 2f;
    public float lengthDetectUp = 2f;
    public float lengthDetectZ = 20f;
    public float playerHeight = 2f;

    void Update()
    {
        Vector3 downCast = new Vector3(transform.position.x, transform.position.y - playerHeight, transform.position.z);
        Ray rayDown = new Ray(downCast, Vector3.down);
        Ray rayUp = new Ray(transform.position, Vector3.up);
        Ray rayZ = new Ray(transform.position, Vector3.forward);

        RaycastHit hit;
        RaycastHit hitUp;
        RaycastHit hitZ;

        if (Physics.Raycast(rayDown, out hit, lengthDetectDown))
        {
            DirtSpot dirt = hit.collider.GetComponent<DirtSpot>();
            if (dirt != null) dirt.CleanAtWorldPos(hit.point);
        }
        if (Physics.Raycast(rayUp, out hitUp, lengthDetectUp))
        {
            DirtSpot dirt = hitUp.collider.GetComponent<DirtSpot>();
            if (dirt != null) dirt.CleanAtWorldPos(hitUp.point);
        }
        if (Physics.Raycast(rayZ, out hitZ, lengthDetectZ))
        {
            VerticalDirtSpot dirt = hitZ.collider.GetComponent<VerticalDirtSpot>();
            if (dirt != null) dirt.CleanAtWorldPos(hitZ.point);
        }
    }

    // ------------------------------
    // GIZMOS
    // ------------------------------
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Down Ray
        Vector3 downOrigin = new Vector3(transform.position.x, transform.position.y - playerHeight, transform.position.z);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(downOrigin, 0.05f);
        Gizmos.DrawLine(downOrigin, downOrigin + Vector3.down * lengthDetectDown);

        // Up Ray
        Vector3 upOrigin = transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(upOrigin, 0.05f);
        Gizmos.DrawLine(upOrigin, upOrigin + Vector3.up * lengthDetectUp);

        // Forward Ray (Z+)
        Vector3 forwardOrigin = transform.position;
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(forwardOrigin, 0.05f);
        Gizmos.DrawLine(forwardOrigin, forwardOrigin + Vector3.forward * lengthDetectZ);
    }
}
