using UnityEngine;
public class DeathBox : MonoBehaviour
{
    public bool isDashable = false;
    private GameObject playerObj;
    private PlayerMovement playerMovement;
    private Animator animator;

    void Start()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();

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
                if (GameStates.instance != null && !GameStates.instance.deathState)
                {
                    GameStates.instance.StartDeath();
                }
                else if(GameStates.instance.deathState)
                {
                    Debug.Log("Player dead already, ignoring further death triggers.");
                    return;
                }
                else
                {
                    Debug.LogError("GameStates not found!");
                }
            }
        }
    }
}