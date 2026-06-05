using UnityEngine;

public class PauseController : BaseMenu
{
    private SaveManager saveManager;

    private void OnEnable()
    {
        saveManager = FindAnyObjectByType<SaveManager>();
    }

    public void OnResume()
    {
        UIManager.instance.ResumeGame();
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

    public void OnSettings()
    {
        UIManager.instance.Settings();
    }
}
