using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Services.Leaderboards.Models;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Transform scrollContent;
    [SerializeField] private LeaderboardTitle titlePrefab;
    [SerializeField] private LeaderboardRow rowPrefab;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button previousPageButton;
    [SerializeField] private TMP_Text playerRankText;
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text playerNameText;

    [Header("Levels/Pagination")]
    [SerializeField] private int currentLevelIndex;

    private void Start()
    {
        leaderboardPanel.SetActive(false);

        // Default to first level page
        currentLevelIndex = 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && PersistenceManager.Instance.IsSignedIn())
        {
            ToggleLeaderboard();
        }
    }

    // Attached to button
    public void NextPage()
    {
        if (currentLevelIndex < LevelsManager.Instance.Levels.Length - 1)
        {
            currentLevelIndex++;
            UpdateLeaderboardByIndex(currentLevelIndex);
        }
    }

    public void PreviousPage()
    {
        if (currentLevelIndex > 0)
        {
            currentLevelIndex--;
            UpdateLeaderboardByIndex(currentLevelIndex);
        }
    }

    public void CloseLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }

    private async void ToggleLeaderboard()
    {
        bool isOpening = !leaderboardPanel.activeSelf;
        leaderboardPanel.SetActive(isOpening);

        if (isOpening)
        {
            UpdateLeaderboardByIndex(currentLevelIndex);
        }
    }

    private async void LoadLeaderboardData(GameLevel gameLevel)
    {
        // 1. Clear existing entries
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }

        loadingText.gameObject.SetActive(true);
        
        // 2. Get all leaderboard data for level
        string leaderboardId = ScoreUtils.GetLeaderboardIdForLevel(gameLevel, PersistenceManager.Instance.IsDevMode);

        // 3. Set title
        LeaderboardTitle newTitle = Instantiate(titlePrefab, scrollContent);
        newTitle.Initialize(gameLevel.levelName);

        // 4. Set scores
        List<LeaderboardEntry> scores = await PersistenceManager.Instance.GetLeaderboardData(leaderboardId);
        if (scores != null)
        {
            // New prefab for each row
            foreach (var entry in scores)
            {
                int score = (int)entry.Score;
                
                LeaderboardRow newRow = Instantiate(rowPrefab, scrollContent);
                newRow.Initialize(entry.Rank, entry.PlayerName, score);
            }
        }

        // Get and set player rank/score
        var playerEntry = await PersistenceManager.Instance.GetPlayerLeaderboardEntry(leaderboardId);

        if (playerEntry != null)
        {
            // + 1 since rank is 0-indexed
            playerNameText.text = PersistenceManager.Instance.GetPlayerName();
            playerRankText.text = $"{playerEntry.Rank + 1}"; 
            playerScoreText.text = $"{ScoreUtils.FormatMilliseconds((int)playerEntry.Score)}";
        }
        else
        {
            playerNameText.text = PersistenceManager.Instance.GetPlayerName();
            playerRankText.text = "-";
            playerScoreText.text = ScoreUtils.FormatMilliseconds(int.MaxValue);
        }

        // Set loading to false
        loadingText.gameObject.SetActive(false);
    }

    private void UpdateNextPreviousButtons()
    {
        previousPageButton.interactable = currentLevelIndex > 0;
        nextPageButton.interactable = currentLevelIndex < LevelsManager.Instance.Levels.Length - 1;
    }

    private void UpdateLeaderboardByIndex(int levelIndex)
    {
        LoadLeaderboardData(LevelsManager.Instance.Levels[levelIndex]);
        UpdateNextPreviousButtons();
    }

    public void UpdateCurrentLeaderboard()
    {
        UpdateLeaderboardByIndex(currentLevelIndex);
    }
}