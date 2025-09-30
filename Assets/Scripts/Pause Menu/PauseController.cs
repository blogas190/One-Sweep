using UnityEngine;

public class PauseController : BaseMenu
{
    public static PauseController instance { get; private set; }

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
        GameManager.instance.LoadMainMenu();
    }

    public void OnQuit()
    {
        Application.Quit();
    }
}
