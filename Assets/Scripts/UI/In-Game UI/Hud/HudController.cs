using Michsky.UI.Reach;
using MoreMountains.Feedbacks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HudController : BaseMenu
{
    public TextMeshProUGUI cleanPercent;
    private StackedBar energyBar;
    public TextMeshProUGUI timer;
    public float updateRate = 0.1f;

    private float lastUpdateTime;
    private int lastCleanPercent = 0;
    private float lastEnergyValue;
    private bool hasTriggeredFeedback = false;
    private GameObject mainCamera;
    private CameraController camera;
    private EnergyController energy;

    //[SerializeField] DripEffect dripEffect;

    private PlayerMovement playerMovement;
    [SerializeField] private Image speedBar;

    void Start()
    {
        energy = GameObject.FindGameObjectWithTag("Player").GetComponent<EnergyController>();
        lastEnergyValue = energy.currentEnergy;
        energyBar = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<StackedBar>();
        energyBar.SetValue(energy.currentEnergy);

        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
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
        if(energy != null)
        {

            if(lastEnergyValue != energy.currentEnergy)
            {
                energyBar.SetValue(energy.currentEnergy);
                lastEnergyValue = energy.currentEnergy;
            }
        }
        if (CleaningProgressManager.Instance != null) //cleaning percentage
        {
            float totalProgress = CleaningProgressManager.Instance.GetTotalCleaningPercentage();

            if (totalProgress != lastCleanPercent)
            {
                // Progress is changing - cleaning is happening
                cleanPercent.text = Mathf.FloorToInt(totalProgress).ToString() + "%";
                lastCleanPercent = Mathf.FloorToInt(totalProgress);
                hasTriggeredFeedback = false; // Reset flag when cleaning resumes

                //if (dripEffect != null)
                //{
                //    dripEffect.currentPercentage = lastCleanPercent;
                //}
            }
            //else
            //{
            //    // Progress hasn't changed - cleaning has stopped
            //    if (!hasTriggeredFeedback)
            //    {
            //        cleanPercentFeedbackEnd.PlayFeedbacks();
            //        hasTriggeredFeedback = true;
            //    }
            //}
        }

        if (timer != null)
        {
            float t = Time.timeSinceLevelLoad;
            int minutes = (int)(t / 60f);
            int seconds = (int)(t % 60f);
            timer.text = string.Format("{0}:{1:00}", minutes, seconds);
        }

        //if (camera != null && cameraMode != null) //camera mode display
        //{
        //    if (camera.IsFollow())
        //    {
        //        string _cameraText = "☐ CAM MODE: FOLLOW PLAYER";
        //        if (cameraMode.text != _cameraText)
        //        {
        //            cameraMode.text = _cameraText;
        //            cameraModeFeedback.PlayFeedbacks();
        //        }

        //    }
        //    else if (!camera.IsFollow())
        //    {
        //        string _cameraText = "☐ CAM MODE: FREE CAMERA";
        //        if (cameraMode.text != _cameraText)
        //        {
        //            cameraMode.text = _cameraText;
        //            cameraModeFeedback.PlayFeedbacks();
        //        }
        //    }
        //}

        if (playerMovement != null && speedBar != null)
        {
            float speed = playerMovement.GetCurrentSpeed();
            float speedPercent = Mathf.InverseLerp(playerMovement._settings.StartSpeed, playerMovement._settings.MaxSpeed, speed);
            speedBar.fillAmount = speedPercent;
        }
    }
}
