using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;

public class PersistenceManager : MonoBehaviour
{
    public static PersistenceManager Instance { get; private set; }

    private const string leaderboardId = "SlimeCrush";
    private const string BestScoreKey = "BestScoreNew";

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
        PlayerPrefs.SetInt(BestScoreKey, score);
        PlayerPrefs.Save();
        Debug.Log($"New Best Score saved locally: {score}");

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

    public int GetBestScore()
    {
        return PlayerPrefs.GetInt(BestScoreKey);
    }

    public bool HasBestScore()
    {
        return PlayerPrefs.HasKey(BestScoreKey);
    }

    public async Task ResetData()
    {
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