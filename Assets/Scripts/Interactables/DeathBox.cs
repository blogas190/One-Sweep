using UnityEngine;
public class DeathBox : MonoBehaviour
{
    public bool isDashable = false;
    private GameStates states;
    private GameObject playerObj;
    private PlayerMovement playerMovement;

    void Start()
    {
        states = FindAnyObjectByType<GameStates>();
        playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerMovement = playerObj.GetComponent<PlayerMovement>();
        }
        else
        {
            Debug.LogError("Player GameObject with 'Player' tag not found!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerMovement == null)
            {
                Debug.LogError("PlayerMovement component not found!");
                return;
            }

            if (isDashable && playerMovement.GetDash())
            {
                return;
            }
            else
            {
                if (states != null)
                {
                    states.StartDeath();
                }
                else
                {
                    Debug.LogError("GameStates not found!");
                }
            }
        }
    }
}