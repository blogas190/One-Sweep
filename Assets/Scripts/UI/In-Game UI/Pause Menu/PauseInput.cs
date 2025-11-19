using UnityEngine;
using UnityEngine.InputSystem;

public class PauseInput : MonoBehaviour
{
    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UIManager.instance.TogglePause();
        }
    }
}
