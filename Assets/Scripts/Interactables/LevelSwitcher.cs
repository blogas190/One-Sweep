using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSwitcher : MonoBehaviour
{
    public string nextLevel = "Level0";
    public CleaningProgressManager cleaningManager;

    private void Start()
    {
        // Ensure the CameraTaker collider has a trigger listener
        Transform cameraTaker = transform.Find("CameraTaker");
        if (cameraTaker != null)
        {
            CameraTakerHandler handler = cameraTaker.gameObject.AddComponent<CameraTakerHandler>();
            handler.parentSwitcher = this;
        }
        else
        {
            Debug.LogWarning("CameraTaker child not found under " + gameObject.name);
        }
    }

    // Called from the CameraTakerHandler when the player collides
    public void DisableCameraFollow()
    {
        GameObject mainCam = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCam != null)
        {
            CameraController controller = mainCam.GetComponent<CameraController>();
            if (controller != null)
            {
                controller.SetFollow(false);
            }
            else
            {
                Debug.LogWarning("MainCamera found but missing CameraController component.");
            }
        }
        else
        {
            Debug.LogWarning("No GameObject with tag MainCamera found.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && cleaningManager.GetFullyCleanedDirtSpots() == cleaningManager.GetTotalDirtSpots())
        {
            SceneManager.LoadScene(nextLevel);
        }
    }
}

// Separate class for handling the CameraTaker's collision
public class CameraTakerHandler : MonoBehaviour
{
    [HideInInspector] public LevelSwitcher parentSwitcher;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && parentSwitcher.cleaningManager.GetFullyCleanedDirtSpots() == parentSwitcher.cleaningManager.GetTotalDirtSpots())
        {
            parentSwitcher.DisableCameraFollow();
        }
    }
}