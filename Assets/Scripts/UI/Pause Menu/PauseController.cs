using UnityEngine;

public class PauseController : BaseMenu
{
    public static PauseController instance { get; private set; }
    public SaveManager saveManager;

    private void Awake()
    {
        instance = this;
        Hide();
    }
    public void OnResume()
    {
        GameManager.instance.ResumeGame();
    }

    public void OnMainMenu()
    {
        saveManager.SaveGame();
        GameManager.instance.LoadMainMenu();
    }

    public void OnQuit()
    {
        saveManager.SaveGame();
        Application.Quit();
    }
}
