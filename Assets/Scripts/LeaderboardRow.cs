using UnityEngine;
using TMPro;

public class LeaderboardRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText;

    public void Initialize(int rank, string playerName, int score)
    {
        rankText.text = $"{rank + 1}"; // Rank is 0-indexed originally
        nameText.text = playerName;
        scoreText.text = ScoreUtils.FormatMilliseconds(score);
    }
}