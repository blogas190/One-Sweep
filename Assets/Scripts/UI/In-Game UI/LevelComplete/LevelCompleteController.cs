using UnityEngine.InputSystem;

public class LevelCompleteController : BaseMenu
{
    private string nextLevel;

    public void SetNextLevel(string next)
    {
        nextLevel = "Level " + next;
    }
    public void OnContinue()
    {
        SceneChanger.instance.LoadScene(nextLevel);
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
