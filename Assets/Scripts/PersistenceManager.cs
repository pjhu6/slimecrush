using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PersistenceManager : MonoBehaviour
{
    public static PersistenceManager Instance { get; private set; }

    [Header("Dev Settings")]
    [SerializeField] private bool isDevMode = false;
    [SerializeField] private Button resetButton;

    private const string devLeaderboardId = "SlimeCrushDev";
    private const string prodLeaderboardId = "SlimeCrush";
    private const string BestScoreKey = "BestScoreNew";
    private string leaderboardId => isDevMode ? devLeaderboardId : prodLeaderboardId;

    public async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        await UnityServices.InitializeAsync();
        await SignIn();  // Always sign in first, player name can change later
    }

    void Start()
    {
        // Dev mode
        if (isDevMode)
        {
            resetButton.gameObject.SetActive(true);
            resetButton.onClick.AddListener(() => _ = ResetData());
        }
        else
        {
            resetButton.gameObject.SetActive(false);
        }
    }

    // Local data getters/setters

    public async Task SignIn()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Player signed in anonymously with ID: " + AuthenticationService.Instance.PlayerId);
        }
    }

    public async Task SetPlayerName(string newName)
    {
        await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
        Debug.Log($"Player Name Saved: {newName}");
    }

    public string GetPlayerName()
    {
        return AuthenticationService.Instance.PlayerName;
    }

    public bool IsSignedIn()
    {
        return AuthenticationService.Instance.IsSignedIn;
    }

    public bool HasPlayerName()
    {
        return !string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName);
    }

    public async Task SetBestScore(int score)
    {
        // Save best score in leaderboard
        if (AuthenticationService.Instance.IsSignedIn)
        {
            try
            {
                await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
                Debug.Log("Score submitted to UGS cloud leaderboard.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Leaderboard submission failed: {e.Message}");
            }
        }
        
    }

    public async Task<int> GetBestScore()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return int.MaxValue;

        try
        {
            var scoreResponse = await LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId);
            Debug.Log("Fetched score from cloud.");
            return (int)scoreResponse.Score;
        }
        catch (System.Exception)
        {
            // Return max value int will represent no score
            return int.MaxValue; 
        }
    }

    public async Task<bool> HasBestScore()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return false;

        try
        {
            var scoreResponse = await LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId);
            return scoreResponse != null;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    public async Task ResetData()
    {
        // TODO: for debug purposes only. Note leaderboard can only be reset from UGS dashboard
        SceneManager.LoadScene("MainMenuScene");
        AuthenticationService.Instance.SignOut();
        PlayerPrefs.DeleteAll();
        AuthenticationService.Instance.ClearSessionToken();
        await SignIn();
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardData()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            return null;
        }

        try
        {
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(
                leaderboardId, 
                new GetScoresOptions { Limit = 10 }
            );

            Debug.Log("Fetched leaderboard data successfully.");
            return scoresResponse.Results;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to fetch leaderboard: {e.Message}");
            return null;
        }
    }
}