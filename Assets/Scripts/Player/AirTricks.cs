using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

public class AirTricks : MonoBehaviour
{
    // ============================================================
    // SETTINGS
    // ============================================================
    [Header("Settings")]
    [SerializeField] private AirTricksSO _settings;

    [Header("Feedbacks")]
    public MMFeedbacks RightTrickFeedbackStart;
    public MMFeedbacks LeftTrickFeedback;

    [Header("References")]
    public RailCheck railCheck;
    public Animator animator;

    private PlayerMovement player;
    private GameStates states;
    private PlayerController controller;
    private EnergyController energy;
    private float directionX = 0f;
    private float directionY = 0f;
    private float prevLengthUp;
    private float prevLengthDown;

    private bool trickInProgress = false;

    void Start()
    {
        player = GetComponent<PlayerMovement>();
        states = FindAnyObjectByType<GameStates>();
        controller = GetComponent<PlayerController>();
        energy = GetComponent<EnergyController>();
    }

    void Update()
    {
        //checking if the player hits the ground during a trick
        if (trickInProgress && player.Grounded())
        {
            //sends a flag to the gameStates script which handles death
            states.StartDeath();
            trickInProgress = false;
        }
    }

    private bool InAir()
    {
        bool nearGround = false;

        Vector3 downCast = new Vector3(transform.position.x, transform.position.y - 0.8f, transform.position.z);
        Ray rayDown = new Ray(downCast, Vector3.down);
        RaycastHit hit;

        int groundLayer = LayerMask.GetMask("Ground");

        if (Physics.Raycast(rayDown, out hit, _settings.MinYDistance, groundLayer))
        {
            nearGround = true;
        }
        else
        {
            Debug.Log("NO NEAR GROUND DETECTED!");
        }

        if (player.Grounded() || nearGround)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    //Taking the input on which trick to perform
    //Need to add a way to handle diagonal inputs
    public void Trick(InputAction.CallbackContext context)
    {
        if (context.performed && !player.Grounded() && trickInProgress == false && InAir())
        {
            Vector2 input = context.ReadValue<Vector2>();
            directionX = input.x;
            directionY = input.y;

            if (directionX < -0.5f && _settings.CanLeft)
            {
                AirTrickLeft();
            }
            else if (directionX > 0.5f && _settings.CanRight)
            {
                AirTrickRight();
            }

            if (directionY < -0.5f && railCheck.blockedRail == null && _settings.CanDown)
            {
                AirTrickDown();
            }
            else if (directionY > 0.5f && _settings.CanUp)
            {
                AirTrickUp();
            }
        }
    }

    //Trick logic
    private void AirTrickUp()
    {
        if (energy.currentEnergy >= energy.upTrickEnergy)
        {
            player.VerticalDash(true, _settings.UpTrickForce, _settings.UpTrickTime);
            animator.SetTrigger("TrickUp");
        }
    }

    private void AirTrickLeft() // regain dashes
    {
        Debug.Log("Air Trick Left!");
        StartCoroutine(RevertAnimationAfterDelay(_settings.LeftTrickTime));
        animator.SetTrigger("LeftTrick");
        LeftTrickFeedback.PlayFeedbacks();
        energy.AddEnergy(energy.leftTrickEnergy);
    }

    private void AirTrickRight() // cleaning nuke
    {
        if (energy.currentEnergy >= energy.rightTrickEnergy)
        {
            prevLengthUp = controller.lengthDetectUp;
            prevLengthDown = controller.lengthDetectDown;

            states.MultVerticalGravity(_settings.CleanGravityMod);

            BigClean(_settings.CleanTime);
            animator.SetTrigger("TrickClean");
            energy.RemoveEnergy(energy.rightTrickEnergy);
        }
    }

    private void AirTrickDown()
    {
        if (energy.currentEnergy >= energy.downTrickEnergy)
        {
            player.VerticalDash(false, _settings.DownTrickForce, _settings.DownTrickTime);
            animator.SetTrigger("TrickDown");
        }
    }

    private IEnumerator RevertAnimationAfterDelay(float delay)
    {
        trickInProgress = true;

        yield return new WaitForSeconds(delay);

        trickInProgress = false;
    }

    void BigClean(float cleanTime)
    {
        controller.lengthDetectUp = _settings.CleanBuff;
        controller.lengthDetectDown = _settings.CleanBuff;

        StartCoroutine(RevertClean(cleanTime));
    }

    private IEnumerator RevertClean(float delay)
    {
        trickInProgress = true;

        RightTrickFeedbackStart.PlayFeedbacks();

        yield return new WaitForSeconds(delay);

        controller.lengthDetectUp = prevLengthUp;
        controller.lengthDetectDown = prevLengthDown;
        trickInProgress = false;
        float gravityBack = (1 / _settings.CleanGravityMod);
        states.MultVerticalGravity(gravityBack);

        RightTrickFeedbackStart.StopFeedbacks();
    }
}