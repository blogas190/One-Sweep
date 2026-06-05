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

        Debug.Log($"GameManager Awake {GetInstanceID()}");

        if (instance != null && instance != this)
        {
            Debug.Log($"Destroying duplicate {GetInstanceID()}");
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);

        }
    void Update()
    {
        Debug.Log($"GM {GetInstanceID()} State: {currentState}");
        Debug.Log($"TimeScale = {Time.timeScale}");
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
        SetState(GameState.paused);
    }

    public void ResumeGame()
    {
        Debug.Log("ResumeGame called");
        Debug.Log(System.Environment.StackTrace);
        SetState(GameState.playing);
    }

    public void StartDialogue()
    {
        Debug.Log("StartDialogue called");
        Debug.Log(System.Environment.StackTrace);
        SetState(GameState.dialogue);
    }

    public void LevelComplete()
    {
        SetState(GameState.levelComplete);
        UIManager.instance.ShowLevelComplete();
    }

    public void LoadMainMenu()
    {
        PlayerPrefs.Save();
        saveManager.SaveGame();
        SceneChanger.instance.LoadScene("Main Menu");
    }

    public void SetState(GameState newState)
    {
        Debug.Log($"SET STATE: {currentState} -> {newState}");
        currentState = newState;

        switch (currentState)
        {
            case GameState.mainMenu:
                Time.timeScale = 1;
                break;
            case GameState.playing:
                Time.timeScale = 1;
                break;
            case GameState.paused:
                Time.timeScale = 0;
                break;
            case GameState.gameOver:
                Time.timeScale = 0;
                break;
            case GameState.dialogue:
                Time.timeScale = 1;
                break;
            case GameState.levelComplete:
                Time.timeScale = 0;
                break;
        }
    }

    //LoadNextLeveL
    //LoadSelectedLevel
    //RestartLevel
}
