using UnityEngine;
public class DeathBox : MonoBehaviour
{
    public bool isDashable = false;
    private GameStates states;
    private GameObject playerObj;
    private PlayerMovement playerMovement;
    private Animator animator;

    void Start()
    {
        states = FindAnyObjectByType<GameStates>();
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
                if (states != null && !states.deathState)
                {
                    states.StartDeath();
                    if (animator != null)
                    {
                        animator.SetTrigger("Crashed");
                    }
                }
                else
                {
                    Debug.LogError("GameStates not found!");
                }
            }
        }
    }
}