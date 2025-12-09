using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSwitcher : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("Name of the next level/scene to load")]
    public string nextLevel = "Main Menu";

    [Tooltip("Percentage of cleaning required to complete level (0-100)")]
    [Range(0f, 100f)]
    public float requiredCleanPercentage = 80f;

    [Header("References")]
    [Tooltip("Reference to the cleaning progress manager")]
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

    // Check if the required cleaning percentage has been reached
    public bool HasMetCleaningRequirement()
    {
        float currentPercentage = cleaningManager.GetTotalCleaningPercentage();
        return currentPercentage >= requiredCleanPercentage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && HasMetCleaningRequirement())
        {
            GameManager.instance.LevelComplete();
            UIManager.instance.levelComplete.SetNextLevel(nextLevel);
        }
    }
}

// Separate class for handling the CameraTaker's collision
public class CameraTakerHandler : MonoBehaviour
{
    [HideInInspector] public LevelSwitcher parentSwitcher;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && parentSwitcher.HasMetCleaningRequirement())
        {
            parentSwitcher.DisableCameraFollow();
        }
    }
}