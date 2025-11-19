using UnityEngine;

public class PauseController : BaseMenu
{
    public SaveManager saveManager;
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
}
