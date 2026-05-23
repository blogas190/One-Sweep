using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public GameObject player;

    [Header("Camera Settings")]
    public float cameraSpeed = 5f;
    public float freeCamSensitivity = 0.1f;
    public Vector3 offset = new Vector3(0, 2, -10);
    [SerializeField] Vector3 maxSpeedOffset = new Vector3(0,2,-20);

    [Header("Freecam Bounds")]
    public Vector3 boundsMin;
    public Vector3 boundsMax;

    [Header("State")]
    [SerializeField] private bool isFollowing = true;
    [SerializeField] private bool freeCam = false;

    private Vector2 mouseInput;
    private Vector3 freeCamPosition;

    private PlayerMovement playerMovement;
    private float maxPlayerSpeed;
    private float startPlayerSpeed;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        playerMovement = player.GetComponent<PlayerMovement>();
        maxPlayerSpeed = playerMovement._settings.MaxSpeed;
        startPlayerSpeed = playerMovement._settings.StartSpeed;
        freeCamPosition = transform.position;
    }

    void Update()
    {
        if (isFollowing)
        {
            FollowPlayer();
        }
        else if (freeCam)
        {
            FreeCamMovement();
        }
    }

    // Input callback from Input System (Mouse action)
    public void OnMouse(InputAction.CallbackContext context)
    {
        mouseInput = context.ReadValue<Vector2>();
    }

    // Switch between Follow and FreeCam modes
    public void SwitchCameraMode(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (isFollowing && playerMovement.GetMagnitude() < 1)
        {
            isFollowing = false;
            freeCam = true;
            freeCamPosition = transform.position; // lock current position
        }
        else
        {
            freeCam = false;
            isFollowing = true;
        }
    }

    void FollowPlayer()
    {
        if (player == null) return;

        float speed = playerMovement.GetCurrentSpeed();

        float t = Mathf.InverseLerp(startPlayerSpeed, maxPlayerSpeed, speed);
        Vector3 activeOffset = Vector3.Lerp(offset, maxSpeedOffset, t);

        Vector3 targetPosition = player.transform.position + activeOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, cameraSpeed * Time.deltaTime);
        transform.LookAt(player.transform.position);
    }

    void FreeCamMovement()
    {
        if (playerMovement.GetMagnitude() > 1)
        {
            freeCam = false;
            isFollowing = true;
            return;
        }
        else
        {
            Vector3 move = new Vector3(mouseInput.x, mouseInput.y, 0) * freeCamSensitivity;
            freeCamPosition += move;

            // Clamp within level borders
            freeCamPosition = new Vector3(
                Mathf.Clamp(freeCamPosition.x, boundsMin.x, boundsMax.x),
                Mathf.Clamp(freeCamPosition.y, boundsMin.y, boundsMax.y),
                Mathf.Clamp(freeCamPosition.z, boundsMin.z, boundsMax.z)
            );

            transform.position = Vector3.Lerp(transform.position, freeCamPosition, cameraSpeed * Time.deltaTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = (boundsMin + boundsMax) / 2f;
        Vector3 size = boundsMax - boundsMin;
        Gizmos.DrawWireCube(center, size);
    }

    public void SetFollow(bool follow)
    {
        isFollowing = follow;
        freeCam = !follow;
    }

    public bool IsFollow()
    {
        return isFollowing;
    }
}