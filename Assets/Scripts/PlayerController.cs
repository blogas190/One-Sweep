using UnityEngine;

public class PlayerController : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 2f))
        {
            DirtSpot dirt = hit.collider.GetComponent<DirtSpot>();
            if (dirt != null)
            {
                dirt.CleanAtWorldPos(hit.point);
            }
        }
    }
}
