using TMPro;
using UnityEngine;
using System.Threading.Tasks;

public class ClockManager : MonoBehaviour
{
    public TextMeshProUGUI clockText;
    public TextMeshProUGUI bestScoreText;
    public TextMeshProUGUI playerNameText;

    private float elapsedTime;

    public float ElapsedTime => elapsedTime;

    async void Start()
    {
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

    public async Task UpdateBestScoreDisplay(string leaderboardId)
    {
        bestScoreText.text = "Best: Loading...";
        int bestScoreMs = await PersistenceManager.Instance.GetBestScore(leaderboardId);
        bestScoreText.text = "Best: " + ScoreUtils.FormatMilliseconds(bestScoreMs);
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

    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateClockDisplay();
    }
}
