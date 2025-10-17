using UnityEngine;

public class TurretBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public bool isDashable = false; // Like DeathBox

    private Vector3 velocity;
    private bool isArched = false;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float arcHeight;
    private float journeyTime;
    private float elapsedTime;
    private float lifetime;

    private GameStates gameStates;
    private PlayerMovement playerMovement;
    private Rigidbody rb;

    void Start()
    {
        gameStates = FindAnyObjectByType<GameStates>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerMovement = playerObj.GetComponent<PlayerMovement>();
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Configure rigidbody
        rb.useGravity = false; // We'll handle movement manually
        rb.isKinematic = true;
    }

    void Update()
    {
        if (isArched)
        {
            UpdateArchedMovement();
        }
        else
        {
            UpdateStraightMovement();
        }

        // Destroy bullet after lifetime
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void InitializeStraight(Vector3 direction, float speed, float bulletLifetime)
    {
        velocity = direction.normalized * speed;
        lifetime = bulletLifetime;
        isArched = false;
    }

    public void InitializeArched(Vector3 start, Vector3 target, float height, float bulletLifetime)
    {
        startPos = start;
        targetPos = target;
        arcHeight = height;
        lifetime = bulletLifetime;

        // Calculate journey time based on distance
        float distance = Vector3.Distance(start, target);
        journeyTime = distance / 10f; // Adjust this multiplier to control arc speed

        elapsedTime = 0f;
        isArched = true;
    }

    void UpdateStraightMovement()
    {
        transform.position += velocity * Time.deltaTime;
    }

    void UpdateArchedMovement()
    {
        elapsedTime += Time.deltaTime;
        float t = elapsedTime / journeyTime;

        if (t <= 1f)
        {
            Vector3 newPos = Vector3.Lerp(startPos, targetPos, t);
            newPos.y += arcHeight * Mathf.Sin(t * Mathf.PI);
            transform.position = newPos;
        }
        else
        {
            // Continue straight after reaching target
            Vector3 direction = (targetPos - startPos).normalized;
            transform.position += direction * 10f * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Same logic as DeathBox
            if (playerMovement == null)
            {
                Debug.LogError("PlayerMovement component not found!");
                return;
            }

            if (isDashable && playerMovement.GetDash())
            {
                // Player can dash through this bullet
                return;
            }
            else
            {
                // Kill the player
                if (gameStates != null)
                {
                    gameStates.StartDeath();
                }
                else
                {
                    Debug.LogError("GameStates not found!");
                }

                // Destroy the bullet
                Destroy(gameObject);
            }
        }

        // Destroy bullet on collision with non-player objects (except triggers)
        if (!other.isTrigger && !other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}