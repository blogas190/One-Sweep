using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    public float maxAngle = 5f; // Largest vertical angle allowed
    public float cameraSpeed;
    public Vector3 offset = new Vector3(0, 2, -10);
    [SerializeField] private bool isFollowing = true;


    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player"); // Just in case
        }
    }

    void Update()
    {
        if (isFollowing)
        {
            FollowPlayer();
        }    
    }

    void FollowPlayer()
    {
        if (player == null) return;
        
        Vector3 targetPosition = player.transform.position + offset;
        
        // Calculate the vertical angle by looking at the player
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        float verticalAngle = Mathf.Asin(directionToPlayer.y) * Mathf.Rad2Deg;
        
        // Always follow horizontally
        Vector3 cameraPosition = transform.position;
        cameraPosition.x = player.transform.position.x + offset.x;
        cameraPosition.z = player.transform.position.z + offset.z;
        
        // Check if vertical angle is within threshold
        if (Mathf.Abs(verticalAngle) <= maxAngle)
        {
            // Within range: maintain current Y position and look at player
            transform.position = cameraPosition;
            transform.LookAt(player.transform.position);
        }
        else
        {
            // Outside range: catch up vertically
            cameraPosition.y = Mathf.Lerp(transform.position.y, targetPosition.y, cameraSpeed * Time.deltaTime);
            transform.position = cameraPosition;
            transform.LookAt(player.transform.position);
        }
    }

    public void SetFollow(bool follow)
    {
        isFollowing = follow;
    }
}
