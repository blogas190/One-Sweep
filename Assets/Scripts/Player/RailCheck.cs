using UnityEngine;

public class RailCheck : MonoBehaviour
{
    private PlayerMovement playerMovement;
    [HideInInspector] public GameObject currentRail;
    [HideInInspector] public string currentRailType; // Track rail type for delayed attachment

    void Start()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("AngledRail"))
        {
            Debug.Log("RailCheck detected angled rail");
            currentRail = other.gameObject;
            currentRailType = "AngledRail";
            Vector3 closestPoint = other.ClosestPoint(transform.position);

            // Normal angled rail attachment
            playerMovement.SetOnRail(true, currentRail);
            playerMovement.RailStartMovementAngled(currentRail, closestPoint);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("AngledRail"))
        {
            Debug.Log("RailCheck left rail");
            currentRail = null;
            currentRailType = "";
            playerMovement.SetOnRail(false, null);
            playerMovement.RailStopMovement();
        }
    }
}