using UnityEngine;
using UnityEngine.SceneManagement;


public enum GameState
{
    mainMenu,
    playing,
    paused,
    gameOver
}
public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    public GameState currentState { get; private set; } = GameState.playing; //change later to a GameStae.mainMenu

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TogglePause()
    {
        if (currentState == GameState.paused)
        {
            ResumeGame();
        }
        else if (currentState == GameState.playing)
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        currentState = GameState.paused;
        PauseController.instance.Show();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        currentState = GameState.playing;
        PauseController.instance.Hide();
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Main Menu");
    }

    //LoadNextLeveL
    //LoadSelectedLevel
    //RestartLevel
}
