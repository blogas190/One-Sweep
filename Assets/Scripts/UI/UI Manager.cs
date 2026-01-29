using Michsky.UI.Reach;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI screens")]
    public HudController hud;
    public PauseController pauseMenu;
    public LevelCompleteController levelComplete;
    public SettingsController settings;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        pauseMenu.Hide();
        levelComplete.Hide();
        settings.Hide();
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
            if(settings.isSettingsOpen == true)
            {
                settings.Hide();
                settings.isSettingsOpen = false;
            }
            else
            {
                ResumeGame();
            }
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

    public void Settings()
    {
        pauseMenu.Hide();
        settings.Show();
        settings.isSettingsOpen = true;
    }

    public void SettingsBack()
    {
        settings.Hide();
        pauseMenu.Show();
    }
}
