using UnityEngine;

public class MovableManager : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float moveDistance = 5f;
    public float startOffset = 0f;

    private Vector2 startPosition;
    private Vector2 lastPosition;

    public Vector2 Delta { get; private set; }

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Start()
    {
        startPosition = rb.position;
        lastPosition = startPosition;
    }

    void FixedUpdate()
    {
        // Calculate where we want to be based on time
        float xOffset = Mathf.Sin((Time.fixedTime + startOffset) * moveSpeed) * moveDistance;
        Vector2 targetPosition = startPosition + new Vector2(xOffset, 0f);

        // Move Rigidbody
        rb.MovePosition(targetPosition);

        // Calculate position delta and update
        Delta = targetPosition - lastPosition;

        lastPosition = targetPosition;
    }
}
