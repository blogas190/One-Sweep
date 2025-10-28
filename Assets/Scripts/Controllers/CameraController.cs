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
    public float maxVerticalOffset = 5f;
    public float maxHorizontalOffset = 10f;

    [Header("State")]
    [SerializeField] private bool isFollowing = true;
    [SerializeField] private bool freeCam = false;

    private Vector2 mouseInput;
    private Vector3 freeCamPosition;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

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

        if (isFollowing)
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

        Vector3 targetPosition = player.transform.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, cameraSpeed * Time.deltaTime);
        transform.LookAt(player.transform.position);
    }

    void FreeCamMovement()
    {
        // Move camera position based on mouse delta
        Vector3 move = new Vector3(mouseInput.x, mouseInput.y, 0) * freeCamSensitivity;
        freeCamPosition += move;

        // Clamp within allowed area relative to player (optional)
        if (player != null)
        {
            freeCamPosition.x = Mathf.Clamp(freeCamPosition.x, player.transform.position.x - maxHorizontalOffset, player.transform.position.x + maxHorizontalOffset);
            freeCamPosition.y = Mathf.Clamp(freeCamPosition.y, player.transform.position.y - maxVerticalOffset, player.transform.position.y + maxVerticalOffset);
        }

        transform.position = Vector3.Lerp(transform.position, freeCamPosition, cameraSpeed * Time.deltaTime);
    }

    public void SetFollow(bool follow)
    {
        isFollowing = follow;
        freeCam = !follow;
    }
}