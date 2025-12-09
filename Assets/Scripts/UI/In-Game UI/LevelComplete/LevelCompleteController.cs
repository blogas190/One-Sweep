using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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
        GameManager.instance.ResumeGame();
    }

    public void OnComplete(InputAction.CallbackContext context)
    {
        if(GameManager.instance.currentState == GameState.levelComplete)
        {
            if(context.performed)
            {
                OnContinue();
            }
        }
    }
    //Add score amount
    //Add level completion grade
}
