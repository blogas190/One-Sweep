using UnityEngine;
using TMPro;

public class DebugController : MonoBehaviour
{
    public TextMeshProUGUI debugText;
    public GameObject player;

    public bool showDebug = true;
    public float updateRate;

    private float lastUpdateTime;
    private Rigidbody playerRb;
    private PlayerMovement playerMovement;
    private int dirtCount;

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

        dirtCount = GameObject.FindGameObjectsWithTag("Dirt").Length;
        // Update at specified rate to avoid performance issues
        if (Time.time - lastUpdateTime >= updateRate)
        {
            UpdateDebugDisplay();
            lastUpdateTime = Time.time;
        }
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
            debugInfo += $"Actual Velocity: {playerMovement.GetMagnitude()}\n";
            debugInfo += $"Dashes Remaining: {playerMovement.dashNumber}\n";
        }
        
        debugInfo += $"Number of dirt objects: {dirtCount}\n";
        debugInfo += $"FPS: {(1f / Time.deltaTime):F0}\n";

        debugText.text = debugInfo;
    }
}
