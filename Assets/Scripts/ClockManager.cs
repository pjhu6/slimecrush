using TMPro;
using UnityEngine;

public class ClockManager : MonoBehaviour
{
    public TextMeshProUGUI clockText;
    public TextMeshProUGUI bestScoreText;
    public TextMeshProUGUI playerNameText;

    private float elapsedTime;

    public float ElapsedTime => elapsedTime;

    void Start()
    {
        UpdateBestScoreDisplay();
        UpdatePlayerNameDisplay();
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing)
            return;

        elapsedTime += Time.deltaTime;
        UpdateClockDisplay();
    }

    void UpdateClockDisplay()
    {
        clockText.text = "Current: " + FormatTime(elapsedTime);
    }

    void UpdateBestScoreDisplay()
    {
        int bestScoreMs = PersistenceManager.Instance.GetBestScore();

        if (bestScoreMs <= 0)
        {
            bestScoreText.text = "Best: --:--.--";
            return;
        }

        bestScoreText.text = "Best: " + FormatMilliseconds(bestScoreMs);
    }

    void UpdatePlayerNameDisplay()
    {
        Debug.Log("Updating player name display.");
        string playerName = PersistenceManager.Instance.GetPlayerName();
        Debug.Log("Player name retrieved: " + playerName);
        playerNameText.text = playerName;
    }

    // Formats float seconds → MM:SS.xx
    string FormatTime(float timeSeconds)
    {
        int minutes = Mathf.FloorToInt(timeSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeSeconds * 100f) % 100f);

        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }

    // Formats int milliseconds → MM:SS.xx
    string FormatMilliseconds(int ms)
    {
        int minutes = ms / 60000;
        int seconds = (ms / 1000) % 60;
        int milliseconds = (ms / 10) % 100;

        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateClockDisplay();
    }
}
