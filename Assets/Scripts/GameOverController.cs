using TMPro;
using UnityEngine;

public class GameOverController : MonoBehaviour
{
    public GameObject gameOverUI;

    private bool isTriggered;

    void Start()
    {
        isTriggered = false;
        gameOverUI.SetActive(false);
    }

    void Update()
    {
        if (!isTriggered && GameManager.Instance.CurrentState == GameState.GameOver)
        {
            isTriggered = true;
            gameOverUI.SetActive(true);
        }
    }

    public void RestartGame()
    {
        // Reload the current scene to restart the game
        GameManager.Instance.Resume();
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
}
