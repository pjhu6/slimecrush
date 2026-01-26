using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovableManager : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float moveDistance = 5f;

    private Vector2 startPosition;
    private Rigidbody2D rb;
    private Vector2 platformVelocity;
    
    // Automatic Detection
    private PlayerManager playerManager;

    // Expose platform velocity, player needs it to adjust grapple point
    public Vector2 PlatformVelocity => platformVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        startPosition = transform.position;
    }

    void FixedUpdate()
    {
        // Move platform
        float xOffset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        Vector2 targetPosition = startPosition + new Vector2(xOffset, 0);
        platformVelocity = (targetPosition - rb.position) / Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }

    void LateUpdate()
    {
        // Inject movement into Player
        // We use LateUpdate because player movement happens first in FixedUpdate
        // We then add/subtract to it using the platform velocity
        if (playerManager != null)
        {
            Vector2 playerVelocity = playerManager.Velocity;
            playerVelocity.x += platformVelocity.x;

            playerManager.Velocity = playerVelocity;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Use DotTest to check if player is above
            if (collision.transform.DotTest(transform, Vector2.down))
            {
                playerManager = collision.gameObject.GetComponent<PlayerManager>();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerManager = null;
        }
    }

    public void AttachPlayer(PlayerManager player)
    {
        Debug.Log("Player attached to mover");
        playerManager = player;
    }

    public void DetachPlayer(PlayerManager player)
    {
        if (playerManager == player)
        {
            playerManager = null;
        }
    }
}