using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject optionsMenuPanel;
    [SerializeField] string levelToStart;

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        optionsMenuPanel.SetActive(false);
    }

    public void OnOptionsMenu()
    {
        mainMenuPanel.SetActive(false);
        optionsMenuPanel.SetActive(true);
    }

    public void OnQuit()
    {
        Application.Quit();
    }

    public void OnPlay()
    {
        SceneManager.LoadScene(levelToStart);
        //Later change to proper level select and load;
    }

    public void OnBack()
    {
        optionsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}
