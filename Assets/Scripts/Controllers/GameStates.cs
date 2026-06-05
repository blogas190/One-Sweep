using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using MoreMountains.Feedbacks;

public class GameStates : MonoBehaviour
{
    public static GameStates instance { get; private set; }
    public CleaningProgressManager cleaningProgressManager;
    private GameManager gameManager;
    [HideInInspector]
    public Animator playerAnimator;
    public bool deathState = false;
    private float prevGravity;
    public MMFeedbacks DeathFeedback;
    private float deathRestartTimer = 2f;
    private AudioManager audioManager;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        prevGravity = Physics.gravity.y;
        gameManager = GetComponent<GameManager>();
        CachePlayerAnimator();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CachePlayerAnimator();
    }

    private void CachePlayerAnimator()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerAnimator = player.GetComponentInChildren<Animator>();
        else
            Debug.LogWarning("GameStates: Could not find Player in scene.");
    }

    public void StartDeath()
    {
        Debug.Log("Player failed. Restart after 2 seconds");
        deathState = true;
        playerAnimator.SetTrigger("Death");
        DeathFeedback.PlayFeedbacks();
    }

    public void RestartScene()
    {
        deathState = false;
        AudioManager.Instance?.StopAllSFX();
        if (gameManager.currentState == GameState.playing)
        {
            if (CleaningProgressManager.Instance != null)
            {
                CleaningProgressManager.Instance.Reset();
            }
            Physics.gravity = new Vector3(0, prevGravity, 0);
            SceneChanger.instance.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void MultVerticalGravity(float gravityMod)
    {
        Physics.gravity = new Vector3(0, Physics.gravity.y * gravityMod, 0);
    }
}