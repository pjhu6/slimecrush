using UnityEngine;

public class MovableManager : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float moveDistance = 5f;

    private Vector2 startPosition;
    private Vector2 lastPosition;

    public Vector2 Delta { get; private set; }

    void Start()
    {
        startPosition = transform.position;
        lastPosition = startPosition;
    }

    void Update()
    {
        float xOffset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        Vector2 targetPosition = startPosition + new Vector2(xOffset, 0);

        transform.position = targetPosition;
    }

    void LateUpdate()
    {
        Delta = (Vector2)transform.position - lastPosition;
        lastPosition = transform.position;
    }
}
