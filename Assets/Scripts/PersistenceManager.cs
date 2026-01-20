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

    private const string BestScoreKey = "BestScoreNew";

    public async void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 1. Initialize the SDK
        await UnityServices.InitializeAsync();

        // If we have a cached session, try to sign in.
        if (AuthenticationService.Instance.SessionTokenExists && !AuthenticationService.Instance.IsSignedIn)
        {
            try 
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"[CACHE RESTORED] Signed in as PlayerID: {AuthenticationService.Instance.PlayerId}");
                Debug.Log($"[CACHE RESTORED] Player Name: {AuthenticationService.Instance.PlayerName}");
            }
            catch (Exception e)
            {
                Debug.Log($"No cached session to restore: {e.Message}");
            }
        }

        PlayerAccountService.Instance.SignedIn += OnUnityAccountSignedIn;

        // This will now fire accurately because we awaited the sign-in above
        if (AuthenticationService.Instance.IsSignedIn)
        {
            OnLoginComplete?.Invoke();
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
                try 
                {
                    // Attempt to link the current anonymous session to this Unity account
                    await AuthenticationService.Instance.LinkWithUnityAsync(accessToken);
                }
                catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
                {
                    // REDIRECT LOGIC: The Unity account belongs to someone else.
                    // We must sign out of the current anonymous ID and sign in as the Unity ID user.
                    Debug.Log("This Unity Account is already linked to another player. Switching identities...");
                    
                    AuthenticationService.Instance.SignOut(); 
                    await AuthenticationService.Instance.SignInWithUnityAsync(accessToken);
                }
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
        // Check if the current player identity has a Unity ID attached to it
        if (!AuthenticationService.Instance.IsSignedIn) return false;
        
        return !string.IsNullOrEmpty(AuthenticationService.Instance.PlayerInfo.GetUnityId());
    }

    public bool HasPlayerName()
    {
        return !string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName);
    }

    public async Task SetBestScore(int score, string leaderboardId)
    {
        // Save best score in leaderboard
        if (AuthenticationService.Instance.IsSignedIn)
        {
            try
            {
                await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
                Debug.Log("Score submitted to UGS cloud leaderboard for leaderboard: " + leaderboardId);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Leaderboard submission failed: {e.Message}");
            }
        }

    }

    public async Task<int> GetBestScore(string leaderboardId)
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

    public void SignOut()
    {
        Debug.Log("Signing out and clearing session cache");

        // 1. Sign out of the current active session
        AuthenticationService.Instance.SignOut();

        // 2. Sign out of the Unity Player Account service
        if (PlayerAccountService.Instance.IsSignedIn)
        {
            PlayerAccountService.Instance.SignOut();
        }

        // Delete the token from the device's storage
        // Without this, the next anonymous login will restore the old ID
        AuthenticationService.Instance.ClearSessionToken();
    }

    public async Task ResetData()
    {
        // TODO: for debug purposes only. Note leaderboard can only be reset from UGS dashboard
        SignOut();

        PlayerPrefs.DeleteAll();
        AuthenticationService.Instance.ClearSessionToken();

        SceneManager.LoadScene("MainMenuScene");
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardData(string leaderboardId)
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

    public int GetPlayerCurrentLevel()
    {
        // TODO: implement datastore
        return 0;
    }

    public bool IsDevMode 
    {
        get 
        {
            #if UNITY_EDITOR
                return isDevMode;
            #else
                return false; // Always false in the actual game build
            #endif
        }
    }
}