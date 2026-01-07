using UnityEngine;
using TMPro;

public class LeaderboardRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText;

    public void Initialize(int rank, string playerName, int score)
    {
        rankText.text = $"#{rank + 1}"; // Rank is 0-indexed originally
        nameText.text = playerName;
        scoreText.text = FormatMilliseconds(score);
    }

    private string FormatMilliseconds(int ms)
    {
        int minutes = ms / 60000;
        int seconds = (ms / 1000) % 60;
        int milliseconds = (ms / 10) % 100;

        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }
}