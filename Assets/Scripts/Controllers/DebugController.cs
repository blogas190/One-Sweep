using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DebugController : MonoBehaviour
{
    public TextMeshProUGUI debugText;
    public GameObject player;

    public bool showDebug = true;
    public float updateRate = 0.1f; // Update more frequently to show real-time progress

    private float lastUpdateTime;
    private Rigidbody playerRb;
    private PlayerMovement playerMovement;
    private EnergyController energy;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player");
            }
        }

        // Get components from player
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            playerRb = player.GetComponent<Rigidbody>();
            energy = player.GetComponent<EnergyController>();
        }

        // Find debug text automatically if not assigned
        if (debugText == null)
        {
            debugText = GameObject.Find("Debug Text")?.GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        if (!showDebug || debugText == null || player == null)
            return;

        // Update at specified rate to avoid performance issues
        if (Time.time - lastUpdateTime >= updateRate)
        {
            UpdateDebugDisplay();
            lastUpdateTime = Time.time;
        }
    }

    public void ShowDebug(InputAction.CallbackContext context)
    {
        showDebug = !showDebug;
        if (!showDebug) debugText.text = null;
    }

    void UpdateDebugDisplay()
    {
        string debugInfo = "DEBUG INFO:\n";

        if (playerMovement != null)
        {
            debugInfo += $"Speed: {playerMovement.GetCurrentSpeed():F2}\n";
            debugInfo += $"Starting Speed: {playerMovement.startSpeed:F2}\n";
            debugInfo += $"Maximum Speed: {playerMovement.maxSpeed:F2}\n";
            debugInfo += $"Grounded: {playerMovement.Grounded()}\n";
            debugInfo += $"Current Energy: {energy.currentEnergy:F2}\n";
            debugInfo += $"Actual Velocity: {playerMovement.GetMagnitude()}\n";
            debugInfo += $"Acceleratiion rate: {playerMovement.accelerationRate}\n";
        }

        // Use the new cleaning progress system
        if (CleaningProgressManager.Instance != null)
        {
            float totalProgress = CleaningProgressManager.Instance.GetTotalCleaningPercentage();
            int totalSpots = CleaningProgressManager.Instance.GetTotalDirtSpots();
            int remainingSpots = CleaningProgressManager.Instance.GetRemainingDirtSpots();
            int cleanedSpots = CleaningProgressManager.Instance.GetFullyCleanedDirtSpots();

            debugInfo += $"Total Dirt Spots: {totalSpots}\n";
            debugInfo += $"Fully Cleaned: {cleanedSpots}\n";
            debugInfo += $"Remaining: {remainingSpots}\n";
            debugInfo += $"Cleaning Progress: {totalProgress:F1}%\n";

            // Uncomment this line for debugging the calculation:
            // debugInfo += $"Debug: {CleaningProgressManager.Instance.GetDetailedProgress()}\n";
        }
        else
        {
            // Fallback to old system if CleaningProgressManager is not available
            int dirtCount = GameObject.FindGameObjectsWithTag("Dirt").Length;
            debugInfo += $"Number of dirt objects: {dirtCount}\n";
        }

        debugInfo += $"FPS: {(1f / Time.deltaTime):F0}\n";

        debugText.text = debugInfo;
    }
}