using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public AudioSource musicSource;

    public GameState CurrentState { get; private set; } = GameState.Playing;

    private ClockManager clockManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        clockManager = FindFirstObjectByType<ClockManager>();

        Instance = this;
    }

    public void Pause()
    {
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;
    }

    public void Win()
    {
        CurrentState = GameState.Victory;
        SaveTimeAsBestScore();
        musicSource.Stop();
    }

    private void SaveTimeAsBestScore()
    {
        if (clockManager == null)
            return;

        // Convert seconds → milliseconds (int-safe)
        int elapsedMilliseconds = Mathf.FloorToInt(clockManager.ElapsedTime * 1000f);

        if (!PersistenceManager.Instance.HasBestScore() || elapsedMilliseconds < PersistenceManager.Instance.GetBestScore())
        {
            Debug.Log("Setting new best score: " + elapsedMilliseconds + " ms, was: " + PersistenceManager.Instance.GetBestScore() + " ms");
            PersistenceManager.Instance.SetBestScore(elapsedMilliseconds);
        }
        else {
            Debug.Log($"Run complete: {elapsedMilliseconds} ms (best: {PersistenceManager.Instance.GetBestScore()} ms)");
        }
    }
}
