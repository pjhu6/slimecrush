using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Leaderboards.Models;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Transform scrollContent;
    [SerializeField] private LeaderboardRow rowPrefab;
    [SerializeField] private TMP_Text loadingText;

    private void Start()
    {
        leaderboardPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && PersistenceManager.Instance.IsSignedIn())
        {
            ToggleLeaderboard();
        }
    }

    private async void ToggleLeaderboard()
    {
        bool isOpening = !leaderboardPanel.activeSelf;
        leaderboardPanel.SetActive(isOpening);

        if (isOpening)
        {
            foreach (Transform child in scrollContent)
            {
                Destroy(child.gameObject);
            }

            loadingText.gameObject.SetActive(true);
            List<LeaderboardEntry> scores = await PersistenceManager.Instance.GetLeaderboardData();

            if (scores != null)
            {
                // New prefab for each row
                foreach (var entry in scores)
                {
                    int score = (int)entry.Score;
                    int stars = ScoreUtils.GetStarsFromScore(score);
                    
                    LeaderboardRow newRow = Instantiate(rowPrefab, scrollContent);
                    newRow.Initialize(entry.Rank, entry.PlayerName, score, stars);
                }
            }
            loadingText.gameObject.SetActive(false);
        }
    }
}