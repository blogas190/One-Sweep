using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject levelSelectPanel;
    [SerializeField] GameObject optionsMenuPanel;

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        optionsMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
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
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    public void OnBack()
    {
        if(levelSelectPanel.activeSelf)
        {
            levelSelectPanel.SetActive(false);
        }
        else if(optionsMenuPanel.activeSelf)
        {
            optionsMenuPanel.SetActive(false);
        }
        mainMenuPanel.SetActive(true);
    }
}
