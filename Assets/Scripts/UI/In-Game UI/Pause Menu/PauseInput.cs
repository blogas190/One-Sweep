using UnityEngine;
using UnityEngine.InputSystem;

public class PauseInput : MonoBehaviour
{
    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed && GameManager.instance.currentState == GameState.playing)
        {
            UIManager.instance.TogglePause();
            GameManager.instance.PauseGame();
        }

        else if (context.performed && GameManager.instance.currentState == GameState.paused)
        {
            UIManager.instance.TogglePause();
            GameManager.instance.ResumeGame();
        }
    }
}
