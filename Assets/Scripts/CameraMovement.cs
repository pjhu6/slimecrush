using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float cameraOffset = 2f;

    private Transform player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void LateUpdate()
    {
        Vector3 newPosition = transform.position;
        newPosition.x = player.position.x;
        newPosition.y = player.position.y + cameraOffset;
        transform.position = newPosition;
    }
}
