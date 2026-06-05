using UnityEngine;
using System.Collections;

public class Turnarounder : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private float savedSpeed;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                // Save current speed before changing direction
                savedSpeed = playerMovement.GetCurrentSpeed();

                // Change direction and temporarily disable movement
                playerMovement.SetMovementEnabled(false);

                // Play animation logic
                StartCoroutine(Animation());
            }
        }
    }

    private IEnumerator Animation()
    {
        playerMovement.animator.SetTrigger("TurnAroundPole");
        playerMovement.TurnAroundPoleFeedback.PlayFeedbacks();

        yield return new WaitForSeconds(0.6f);

        playerMovement.ChangeDirection();

        // Restore movement with saved speed
        playerMovement.SetMovementEnabled(true);
        playerMovement.SetSpeed(savedSpeed);
    }
}