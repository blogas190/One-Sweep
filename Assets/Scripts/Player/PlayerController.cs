using UnityEngine;
using MoreMountains.Feedbacks;

public class PlayerController : MonoBehaviour
{
    public MMFeedbacks cleaningDownFeedback;
    public MMFeedbacks cleaningZFeedback;
    private bool isCleaningDown = false;
    private bool isCleaningZ = false;
    private bool wasCleaningDown = false;
    private bool wasCleaningZ = false;
    public float lengthDetectDown = 2f;
    public float lengthDetectUp = 2f;
    public float lengthDetectZ = 20f;
    public float playerHeight = 2f;

    void Update()
    {
        isCleaningDown = false;
        isCleaningZ = false;
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
            if (dirt != null)
            {
                dirt.CleanAtWorldPos(hit.point);

                if(dirt.IsBeingCleaned())
                {
                    isCleaningDown = true;
                }
            }
            ;
        }
        if (Physics.Raycast(rayUp, out hitUp, lengthDetectUp))
        {
            DirtSpot dirt = hitUp.collider.GetComponent<DirtSpot>();
            if (dirt != null)
            {
                dirt.CleanAtWorldPos(hitUp.point);
            }
        }
        if (Physics.Raycast(rayZ, out hitZ, lengthDetectZ))
        {
            VerticalDirtSpot dirt = hitZ.collider.GetComponent<VerticalDirtSpot>();
            if (dirt != null)
            {
                dirt.CleanAtWorldPos(hitZ.point);

                if(dirt.IsBeingCleaned())
                {
                    isCleaningZ = true;
                }
            }
        }

        if(isCleaningDown && !wasCleaningDown)
        {
            cleaningDownFeedback.PlayFeedbacks();
        }
        if(wasCleaningDown && !isCleaningDown)
        {
            cleaningDownFeedback.StopFeedbacks();
        }
        if(isCleaningZ && !wasCleaningZ)
        {
            cleaningZFeedback.PlayFeedbacks();
        }
        if(wasCleaningZ && !isCleaningZ)
        {
            cleaningZFeedback.StopFeedbacks();
        }

        wasCleaningDown = isCleaningDown;
        wasCleaningZ = isCleaningZ;
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
