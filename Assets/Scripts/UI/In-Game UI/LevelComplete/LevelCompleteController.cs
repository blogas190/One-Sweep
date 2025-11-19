using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteController : BaseMenu
{
    private string nextLevel;

    public void SetNextLevel(string next)
    {
        nextLevel = next;
    }
    public void OnContinue()
    {
        SceneManager.LoadScene(nextLevel);
    }
    //Add score amount
    //Add level completion grade
}
