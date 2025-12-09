using UnityEngine;

public class ReverseWall : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Cooldown time before this wall can reverse direction again (seconds)")]
    [Range(0.1f, 2f)]
    public float directionChangeCooldown = 0.5f;

    private float lastDirectionChangeTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Check if enough time has passed since last direction change
            if (Time.time - lastDirectionChangeTime < directionChangeCooldown)
            {
                Debug.Log("Direction change on cooldown, ignoring");
                return;
            }

            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.ChangeDirection();
                playerMovement.SetOnWall(true);
                lastDirectionChangeTime = Time.time; // Start cooldown
                Debug.Log("Player on wall - direction changed");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.SetOnWall(false);
                Debug.Log("No longer on wall");
            }
        }
    }
}