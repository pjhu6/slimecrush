using TMPro;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseUI;
    public AudioSource musicSource;

    void OnEnable()
    {
        musicSource.Play();
    }

    void Start()
    {
        pauseUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        if (GameManager.Instance.CurrentState == GameState.Paused)
        {
            GameManager.Instance.Resume();
            pauseUI.SetActive(false);
        }
        else if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            GameManager.Instance.Pause();
            pauseUI.SetActive(true);
        }
    }

    public void RestartGame()
    {
        // Reload the current scene to restart the game
        GameManager.Instance.Resume();
        pauseUI.SetActive(false);

        ClockManager clockManager = FindFirstObjectByType<ClockManager>();
        if (clockManager != null)
        {
            clockManager.ResetTimer();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    public void QuitToMainMenu()
    {
        musicSource.Stop();
        pauseUI.SetActive(false);
        GameManager.Instance.Resume();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
    }
}
