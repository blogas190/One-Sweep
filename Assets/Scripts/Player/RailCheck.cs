using UnityEngine;
using UnityEngine.InputSystem;

public class RailCheck : MonoBehaviour
{
    private PlayerMovement playerMovement;
    [HideInInspector] public GameObject currentRail;
    [HideInInspector] public string currentRailType;
    public GameObject blockedRail; // Rail that player can't reconnect to yet

    private float directionX = 0f;
    private float directionY = 0f;

    void Start()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AngledRail"))
        {
            Debug.Log("RailCheck detected angled rail");

            // Check if this is the blocked rail
            if (blockedRail != null && other.gameObject == blockedRail)
            {
                Debug.Log("Cannot reconnect to blocked rail yet");
                return;
            }

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

            // If leaving the blocked rail, clear the block
            if (blockedRail != null && other.gameObject == blockedRail)
            {
                Debug.Log("Cleared blocked rail");
                blockedRail = null;
            }

            currentRail = null;
            currentRailType = "";
            playerMovement.SetOnRail(false, null);
            playerMovement.RailStopMovement();
        }
    }

    // Method to block reconnection to a specific rail
    public void BlockRailReconnection(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        directionX = input.x;
        directionY = input.y;
        Debug.Log("Blocked rail called");

        if (directionY < -0.5f && playerMovement.IsOnRail())
        {
            blockedRail = currentRail;
            Debug.Log("Blocked reconnection to rail: " + blockedRail.name);
            currentRail = null;
            playerMovement.SetOnRail(false, null);
            playerMovement.RailStopMovement();
        }
    }
}