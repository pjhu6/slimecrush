using UnityEngine;
using TMPro;

public class LeaderboardRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject[] starIcons;

    public void Initialize(int rank, string playerName, int score, int stars)
    {
        rankText.text = $"{rank + 1}"; // Rank is 0-indexed originally
        nameText.text = playerName;
        scoreText.text = ScoreUtils.FormatMilliseconds(score);
        SetStars(stars);
    }

    private void SetStars(int count)
    {
        // Loop through the icons and enable them up to the 'count'
        for (int i = 0; i < starIcons.Length; i++)
        {
            starIcons[i].SetActive(i < count);
        }
    }
}