using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class AirTricks : MonoBehaviour
{
    public float upTrickForce = 500f;
    public float upTrickTime = 0.1f;
    public float downTrickForce = 1000f;
    public float downTrickTime = 0.1f;
    public float cleanTime = 1f;
    private float currentCleanTime;
    public float cleanBuff = 500000f;
    public float cleanGravityMod = 0.3f;

    private PlayerMovement player;
    private GameStates states;
    private PlayerController controller;
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
        player.Renderer.material.color = color;
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

            if(directionY < -0.5f)
            {
                AirTrickDown();
            }

            else if(directionY > 0.5f)
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
        player.VerticalDash(true, upTrickForce, upTrickTime); // going up
        //Debug.Log("Air Trick Up!");
        ////Dash reset. Change to max 1 later
        //if(player.dashNumber < 3) player.dashNumber++;

    }

    private void AirTrickLeft() // regain dashes
    {

        animationDelay = 1f;
        SetAnimation(Color.red, animationDelay);
        Debug.Log("Air Trick Left!");
        if(player.dashNumber < 3) player.dashNumber++;
    }

    private void AirTrickRight() // cleaning nuke
    {
        //SetAnimation(Color.yellow, animationDelay);
        //Debug.Log("Air Trick Right!");
        //if(player.dashNumber < 3) player.dashNumber++;
        prevLengthUp = controller.lengthDetectUp;
        prevLengthDown = controller.lengthDetectDown;

        states.MultVerticalGravity(cleanGravityMod);
        if (player.dashNumber >= 1)
        {
            BigClean(cleanTime);
        }
    }

    private void AirTrickDown()
    {
        player.VerticalDash(false, downTrickForce, downTrickTime); // going down
    }

    private IEnumerator RevertAnimationAfterDelay(float delay)
    {
        trickInProgress = true;

        yield return new WaitForSeconds(delay);
        player.Renderer.material.color = Color.green;

        trickInProgress = false;
    }

    void BigClean(float cleanTime)
    {

        player.Renderer.material.color = Color.yellow;
        controller.lengthDetectUp = cleanBuff;
        controller.lengthDetectDown = cleanBuff;

        StartCoroutine(RevertClean(cleanTime));
    }

    private IEnumerator RevertClean(float delay)
    {
        trickInProgress = true;

        yield return new WaitForSeconds(delay);
        player.Renderer.material.color = Color.green;

        controller.lengthDetectUp = prevLengthUp;
        controller.lengthDetectDown = prevLengthDown;
        trickInProgress = false;
        float gravityBack;
        gravityBack = (1 / cleanGravityMod);
        states.MultVerticalGravity(gravityBack);
    }
}
