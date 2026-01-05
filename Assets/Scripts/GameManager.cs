using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public AudioSource musicSource;

    public GameState CurrentState { get; private set; } = GameState.Playing;

    private const string BestScoreKey = "BestScore";
    public int BestScore { get; private set; }

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

        // Load best score on startup
        BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
    }

    // Call this when a run ends or score changes
    public void TrySetBestScore(int score)
    {
        if (score > BestScore)
        {
            BestScore = score;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save(); // Important for WebGL
        }
    }

    public void ResetBestScore()
    {
        BestScore = 0;
        PlayerPrefs.DeleteKey(BestScoreKey);
        PlayerPrefs.Save();
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

        if (elapsedMilliseconds < BestScore)
        {
            Debug.Log($"New best score: {elapsedMilliseconds} ms (old: {BestScore} ms)");
            BestScore = elapsedMilliseconds;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save(); // WebGL-safe
        }
        else {
            Debug.Log($"Run complete: {elapsedMilliseconds} ms (best: {BestScore} ms)");
        }
    }
}
