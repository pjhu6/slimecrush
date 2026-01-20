using UnityEngine;

public class WinPlatformManager : MonoBehaviour
{
    [SerializeField] private int targetLevel;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance.CurrentLevel != targetLevel)
            return;

        // Check if the object colliding is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Use DotTest to check if player is above
            if (collision.transform.DotTest(transform, Vector2.down))
            {
                // TODO move win detection to platform, not player. Each win platform will detect if their win
                // Has been detected already and which level to complete
                GameManager.Instance.Win();
            }
        }
    }
}
