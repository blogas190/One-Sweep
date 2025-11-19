using UnityEngine;
using UnityEngine.SceneManagement;


public enum GameState
{
    mainMenu,
    playing,
    paused,
    gameOver,
    dialogue,
    levelComplete
}
public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    public GameState currentState { get; private set; } = GameState.playing; //change later to a GameStae.mainMenu
    public SaveManager saveManager;

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

    public void ToggleDialogue()
    {
        if (currentState == GameState.dialogue)
        {
            ResumeGame();
        }
        else if (currentState == GameState.playing)
        {
            StartDialogue();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        currentState = GameState.paused;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        currentState = GameState.playing;
    }

    public void StartDialogue()
    {
        currentState = GameState.dialogue;
    }

    public void LevelComplete()
    {
        currentState = GameState.levelComplete;
        Time.timeScale = 0;
        UIManager.instance.ShowLevelComplete();
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1;
        currentState = GameState.mainMenu;
        saveManager.SaveGame();
        SceneManager.LoadScene("Main Menu");
    }

    //LoadNextLeveL
    //LoadSelectedLevel
    //RestartLevel
}
