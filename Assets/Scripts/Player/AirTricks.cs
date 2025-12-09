using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

public class AirTricks : MonoBehaviour
{
    [Header("Trick Settings")]
    public float upTrickForce = 500f;
    public float upTrickTime = 0.1f;
    public float upTrickGravityMod = 0.4f;
    public float downTrickForce = 1000f;
    public float downTrickTime = 0.1f;
    public float cleanTime = 1f;
    private float currentCleanTime;
    public float cleanBuff = 500000f;
    public float cleanGravityMod = 0.3f;
    public float leftTrickTime = .75f;

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
    private float animationDelay;
    private float directionX = 0f;
    private float directionY = 0f;
    private float prevLengthUp;
    private float prevLengthDown;

    private bool trickInProgress = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        if(trickInProgress && player.Grounded())
        {
            //sends a flag to the gameStates script which handles death
            states.StartDeath();
            trickInProgress = false;
        }
    }
    
    //Coroutine starter. Later this can used for animation timing as well
    private void SetAnimation(Color color, float delay)
    {
        StartCoroutine(RevertAnimationAfterDelay(delay));
    }

    //Taking the input on which trick to perform
    //Need to add a way to handle diagonal inputs
    public void Trick(InputAction.CallbackContext context)
    {
        if(context.performed && !player.Grounded() && trickInProgress == false)
        {
            Vector2 input = context.ReadValue<Vector2>();
            directionX = input.x;
            directionY = input.y;

            if(directionX < -0.5f)
            {
                AirTrickLeft();
            }

            else if(directionX > 0.5f)
            {
                AirTrickRight();
            }

            if(directionY < -0.5f && railCheck.blockedRail == null)
            {
                AirTrickDown();
            }

            else if (directionY > 0.5f)
            {
                AirTrickUp();
            }
        }
    }

    //Trick logic
    private void AirTrickUp()
    {

        //animationDelay = 1f;
        //SetAnimation(Color.purple, animationDelay);
        if(energy.currentEnergy >= energy.upTrickEnergy)
        {
            player.VerticalDash(true, upTrickForce, upTrickTime); // going up
        }
        //Debug.Log("Air Trick Up!");
        ////Dash reset. Change to max 1 later
        //if(player.dashNumber < 3) player.dashNumber++;
    }

    private void AirTrickLeft() // regain dashes
    {
        Debug.Log("Air Trick Left!");
        StartCoroutine(RevertAnimationAfterDelay(leftTrickTime));
        animator.SetTrigger("LeftTrick");
        LeftTrickFeedback.PlayFeedbacks();
        energy.AddEnergy(energy.leftTrickEnergy);
    }

    private void AirTrickRight() // cleaning nuke
    {
        //SetAnimation(Color.yellow, animationDelay);
        //Debug.Log("Air Trick Right!");
        //if(player.dashNumber < 3) player.dashNumber++;
        if (energy.currentEnergy >= energy.rightTrickEnergy)
        {
            prevLengthUp = controller.lengthDetectUp;
            prevLengthDown = controller.lengthDetectDown;

            states.MultVerticalGravity(cleanGravityMod);
        
            BigClean(cleanTime);
            animator.SetTrigger("TrickClean");
            energy.RemoveEnergy(energy.rightTrickEnergy);
        }

    }

    private void AirTrickDown()
    {
        if(energy.currentEnergy >= energy.downTrickEnergy)
        {
            player.VerticalDash(false, downTrickForce, downTrickTime); // going down
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
        controller.lengthDetectUp = cleanBuff;
        controller.lengthDetectDown = cleanBuff;

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
        float gravityBack;
        gravityBack = (1 / cleanGravityMod);
        states.MultVerticalGravity(gravityBack);

        RightTrickFeedbackStart.StopFeedbacks();
    }
}
