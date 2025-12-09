using Michsky.UI.Reach;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI screens")]
    public HudController hud;
    public PauseController pauseMenu;
    public LevelCompleteController levelComplete;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        pauseMenu.Hide();
        levelComplete.Hide();
        hud.Show();
    }

    void Update()
    {
        if (GameManager.instance.currentState != GameState.playing)
        {
            hud.Hide();
        }
        else if (GameManager.instance.currentState == GameState.playing)
        {
            hud.Show();
        }
    }

    public void TogglePause()
    {
        if (GameManager.instance.currentState == GameState.paused)
        {
            ResumeGame();
        }
        else if (GameManager.instance.currentState == GameState.playing)
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenu.Show();
        hud.Hide();
        GameManager.instance.PauseGame();
    }

    public void ResumeGame()
    {
        pauseMenu.Hide();
        hud.Show();
        GameManager.instance.ResumeGame();
    }

    public void ShowLevelComplete()
    {
        hud.Hide();
        levelComplete.Show();
    }
}
