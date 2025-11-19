using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class GameStates : MonoBehaviour
{
    private PlayerMovement player;

    public CleaningProgressManager cleaningProgressManager;
    public Animator playerAnimator;
    [HideInInspector] 
    public bool deathState = false;
    private float prevGravity;

    private float deathRestartTimer = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindAnyObjectByType<PlayerMovement>();
        prevGravity = Physics.gravity.y;
    }

    public void StartDeath()
    {
        Debug.Log("Player failed. Restart after 2 seconds");
        deathState = true;
        playerAnimator.SetTrigger("Death");
        //using a coroutine to have a delay for the fall animation
        StartCoroutine(Death());
    }

    public void RestartScene()
    {
        if (CleaningProgressManager.Instance != null)
        {
            CleaningProgressManager.Instance.Reset();
        }
        Physics.gravity = new Vector3(0, prevGravity, 0);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MultVerticalGravity(float gravityMod)
    {
        Physics.gravity = new Vector3(0, Physics.gravity.y * gravityMod, 0);
    }

    private IEnumerator Death()
    {
        //Later we can set proper timers for restart

        yield return new WaitForSeconds(deathRestartTimer);
        //after animation restart the level
        RestartScene();
        deathState = false;
    }
}
