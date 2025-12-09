using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

public class GameStates : MonoBehaviour
{
    public static GameStates instance { get; private set; }
    private PlayerMovement player;

    public CleaningProgressManager cleaningProgressManager;
    private GameManager gameManager;
    public Animator playerAnimator;
    [HideInInspector] 
    public bool deathState = false;
    private float prevGravity;
    public MMFeedbacks DeathFeedback;

    private float deathRestartTimer = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindAnyObjectByType<PlayerMovement>();
        prevGravity = Physics.gravity.y;
        gameManager = GetComponent<GameManager>();
    }

    public void StartDeath()
    {
        Debug.Log("Player failed. Restart after 2 seconds");
        deathState = true;
        playerAnimator.SetTrigger("Death");
        DeathFeedback.PlayFeedbacks();
        //using a coroutine to have a delay for the fall animation
        StartCoroutine(Death());
    }

    public void RestartScene()
    {
        if (gameManager.currentState == GameState.playing)
        {
            if (CleaningProgressManager.Instance != null)
            {
                CleaningProgressManager.Instance.Reset();
            }
            Physics.gravity = new Vector3(0, prevGravity, 0);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
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
