using UnityEngine;
using TMPro;
using Michsky.UI.Reach;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

public class UIController : MonoBehaviour
{
    public ProgressBar cleanPercent;
    public TextMeshProUGUI cameraMode;
    public MMFeedbacks cameraModeFeedback;
    public GameObject inGameUI;
    public GameObject player;
    public GameManager gameManager;
    public MMFeedbacks cleanPercentFeedbackEnd;
    public bool showUI = true;
    public float updateRate = 0.1f;

    private float lastUpdateTime;
    private Rigidbody playerRb;
    private PlayerMovement playerMovement;
    private float lastCleanPercent = 0f;
    private bool hasTriggeredFeedback = false;
    private GameObject mainCamera;
    private CameraController camera;


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

        if (mainCamera == null)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            camera = mainCamera.GetComponent<CameraController>();
        }

        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            playerRb = player.GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        if (!showUI || player == null || gameManager.currentState != GameState.playing)
        {
            inGameUI.SetActive(false);
            return;
        }

        if (Time.time - lastUpdateTime >= updateRate)
        {
            UpdateUI();
            lastUpdateTime = Time.time;
        }
    }

    void UpdateUI()
    {
        inGameUI.SetActive(true);

        if (CleaningProgressManager.Instance != null) //cleaning percentage
        {
            float totalProgress = CleaningProgressManager.Instance.GetTotalCleaningPercentage();

            if (totalProgress != lastCleanPercent)
            {
                // Progress is changing - cleaning is happening
                cleanPercent.SetValue(totalProgress);
                lastCleanPercent = totalProgress;
                hasTriggeredFeedback = false; // Reset flag when cleaning resumes
            }
            else
            {
                // Progress hasn't changed - cleaning has stopped
                if (!hasTriggeredFeedback)
                {
                    cleanPercentFeedbackEnd.PlayFeedbacks();
                    hasTriggeredFeedback = true;
                }
            }
        }

        if (camera != null && cameraMode != null) //camera mode display
        {
            if (camera.IsFollow())
            {
                string _cameraText = "☐ CAM MODE: FOLLOW PLAYER";
                if (cameraMode.text != _cameraText)
                {
                    cameraMode.text = _cameraText;
                    cameraModeFeedback.PlayFeedbacks();
                }

            }
            else if (!camera.IsFollow())
            {
                string _cameraText = "☐ CAM MODE: FREE CAMERA";
                if (cameraMode.text != _cameraText)
                {
                    cameraMode.text = _cameraText;
                    cameraModeFeedback.PlayFeedbacks();
                }
            }
        }
    }
}