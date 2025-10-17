using UnityEngine;

public class ReverseWall : MonoBehaviour
{
    private PlayerMovement playerMovement;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.ChangeDirection();
                playerMovement.SetOnWall(true);
                Debug.Log("Player on wall");
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
