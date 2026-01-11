using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
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

    // Trigger this event when login (either type) is finished
    public event Action OnLoginComplete;

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

        PlayerAccountService.Instance.SignedIn += OnUnityAccountSignedIn;


        // TODO: create callback to display loading while fetching existing sesion
        if (AuthenticationService.Instance.SessionTokenExists)
        {
            try 
            {
                // This attempts to sign in using the stored token automatically
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                
                // If the user was previously a Unity User, we should also ensure 
                // the name is refreshed from the cloud.
                if (HasUnityID())
                {
                    await AuthenticationService.Instance.GetPlayerNameAsync();
                }

                Debug.Log("Session resumed for Player: " + AuthenticationService.Instance.PlayerId);
                OnLoginComplete?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Could not resume session: " + ex.Message);
                // If token is expired or invalid, we do nothing and let the UI show the login screen
            }
        }
    }

    void Start()
    {
        // Dev mode
        // TODO: add a position to load in player for each level
        // Call player object with setposition on scene load
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

    public async void StartUnityLogin()
    {
        try
        {
            await PlayerAccountService.Instance.StartSignInAsync();
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    private async void OnUnityAccountSignedIn()
    {
        Debug.Log("Unity Account Signed In callback triggered.");
        await SignInWithUnityAccount();
    }

    private async Task SignInWithUnityAccount()
    {
        try
        {
            string accessToken = PlayerAccountService.Instance.AccessToken;

            // 1. Player is not yet authenticated, signing up with Unity
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInWithUnityAsync(accessToken);
            }
            // 2. Player is authenticated, but does not yet have a Unity ID linked, so let's link
            else if (!HasUnityID())
            {
                // If already signed in anonymously, link progress
                await AuthenticationService.Instance.LinkWithUnityAsync(accessToken);
            }
            // 3. Player has authentication and a Unity ID
            else
            {
                Debug.Log("Player is already signed in to their Unity Player Account");
            }

            Debug.Log("Signed in with Unity. Player ID: " + AuthenticationService.Instance.PlayerId);

            // Explicitly refresh the name from the cloud profile
            // This prevents race conditions for subscribers to OnLoginComplete
            await AuthenticationService.Instance.GetPlayerNameAsync();
            OnLoginComplete?.Invoke();
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    private bool HasUnityID()
    {
        return AuthenticationService.Instance.PlayerInfo.GetUnityId() != null;
    }

    // Renamed to be public so the UI can trigger it explicitly for Guest mode
    public async Task SignInAnonymouslyFallback()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Player signed in anonymously with ID: " + AuthenticationService.Instance.PlayerId);

            OnLoginComplete?.Invoke(); // Notify other scripts
        }
    }

    public async Task SetPlayerName(string newName)
    {
        // Only update if the name is actually different to avoid errors
        if (AuthenticationService.Instance.PlayerName == newName) return;

        await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
        Debug.Log($"Player Name Saved: {newName}");
    }

    public string GetPlayerName()
    {
        // This will return the name from the Authentication Service
        return AuthenticationService.Instance.PlayerName;
    }

    public bool IsSignedIn()
    {
        return AuthenticationService.Instance.IsSignedIn;
    }

    public bool IsSignedInToUnity()
    {
        return PlayerAccountService.Instance.IsSignedIn;
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
        AuthenticationService.Instance.SignOut();

        if (PlayerAccountService.Instance.IsSignedIn)
        {
            PlayerAccountService.Instance.SignOut();
        }

        PlayerPrefs.DeleteAll();
        AuthenticationService.Instance.ClearSessionToken();

        SceneManager.LoadScene("MainMenuScene");
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
                new GetScoresOptions { Limit = 50 }  // TODO pageinate leaderboard 
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