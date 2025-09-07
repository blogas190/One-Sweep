using UnityEngine;

public class RailCheck : MonoBehaviour
{
    public float deattachSpeed = 0.5f;
    public float attachSpeed = 0.5f;
    private PlayerMovement playerMovement;
    [HideInInspector] public GameObject currentRail;
    [HideInInspector] public string currentRailType; // Track rail type for delayed attachment

    void Start()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rail"))
        {
            Debug.Log("RailCheck detected rail");
            currentRail = other.gameObject;
            currentRailType = "Rail";

            // Check if player is jumping upward - if so, just set rail reference but don't attach yet
            if (playerMovement.GetComponent<Rigidbody>().linearVelocity.y > deattachSpeed)
            {
                Debug.Log("Player jumping upward - delaying rail attachment");
                playerMovement.SetOnRail(true, currentRail); // Just set the flag
                return;
            }

            // Normal rail attachment
            playerMovement.SetOnRail(true, currentRail);
            playerMovement.RailStartMovement(currentRail);
        }

        if (other.CompareTag("AngledRail"))
        {
            Debug.Log("RailCheck detected angled rail");
            currentRail = other.gameObject;
            currentRailType = "AngledRail";

            // Check if player is jumping upward - if so, just set rail reference but don't attach yet
            if (playerMovement.GetComponent<Rigidbody>().linearVelocity.y > deattachSpeed)
            {
                Debug.Log("Player jumping upward - delaying angled rail attachment");
                playerMovement.SetOnRail(true, currentRail); // Just set the flag
                return;
            }

            // Normal angled rail attachment
            playerMovement.SetOnRail(true, currentRail);
            playerMovement.RailStartMovementAngled(currentRail);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Rail") || other.CompareTag("AngledRail"))
        {
            Debug.Log("RailCheck left rail");
            currentRail = null;
            currentRailType = "";
            playerMovement.SetOnRail(false, null);
            playerMovement.RailStopMovement();
        }
    }

    // Method to check if delayed rail attachment should now happen
    public void CheckDelayedAttachment()
    {
        if (currentRail != null && !playerMovement.IsOnRail())
        {
            Rigidbody rb = playerMovement.GetComponent<Rigidbody>();

            // Attach to rail when player is no longer moving upward significantly
            if (rb.linearVelocity.y <= attachSpeed)
            {
                Debug.Log("Executing delayed rail attachment");

                if (currentRailType == "Rail")
                {
                    playerMovement.RailStartMovement(currentRail);
                }
                else if (currentRailType == "AngledRail")
                {
                    playerMovement.RailStartMovementAngled(currentRail);
                }
            }
        }
    }
}