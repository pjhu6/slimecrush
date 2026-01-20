using UnityEngine;

public class WinPlatformManager : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the object colliding is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Use DotTest to check if player is above
            if (collision.transform.DotTest(transform, Vector2.down))
            {
                GameManager.Instance.Win();
            }
        }
    }
}
