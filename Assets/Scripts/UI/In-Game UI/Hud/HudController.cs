using TMPro;
using UnityEngine;
using Michsky.UI.Reach;
using MoreMountains.Feedbacks;

public class HudController : BaseMenu
{

    public ProgressBar cleanPercent;
    public TextMeshProUGUI cameraMode;
    public MMFeedbacks cameraModeFeedback;
    public MMFeedbacks cleanPercentFeedbackEnd;
    public float updateRate = 0.1f;

    private float lastUpdateTime;
    private float lastCleanPercent = 0f;
    private bool hasTriggeredFeedback = false;
    private GameObject mainCamera;
    private CameraController camera;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            camera = mainCamera.GetComponent<CameraController>();
        }
    }

    void Update()
    {
        if (Time.time - lastUpdateTime >= updateRate)
        {
            UpdateUI();
            lastUpdateTime = Time.time;
        }
    }

    void UpdateUI()
    {
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
