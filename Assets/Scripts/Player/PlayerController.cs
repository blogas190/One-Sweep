using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float lengthDetectDown = 2f;
    public float lengthDetectUp = 2f;
    public float lengthDetectZ = 20f;
    public float playerHeight = 2f;
    // Update is called once per frame
    void Update()
    {
        Vector3 downCast = new Vector3(transform.position.x, transform.position.y-playerHeight, transform.position.z); // not at all an elegant solution to rays not passing thru the railcheck
        Ray rayDown = new Ray(downCast, Vector3.down);
        Ray rayUp = new Ray(transform.position, Vector3.up);
        Ray rayZ = new Ray(transform.position, Vector3.forward);

        RaycastHit hit;
        RaycastHit hitUp;
        RaycastHit hitZ;

        // Check down, up, forward rays
        if (Physics.Raycast(rayDown, out hit, lengthDetectDown))
        {
            DirtSpot dirt = hit.collider.GetComponent<DirtSpot>();
            if (dirt != null)
            {
                dirt.CleanAtWorldPos(hit.point);
            }
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
            }
        }
    }


}
