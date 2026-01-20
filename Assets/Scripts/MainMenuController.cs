using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Profile setup")]
    [SerializeField] private GameObject profileCreate;
    [SerializeField] private TMP_InputField profileNameField;
    [SerializeField] private TMP_Text profileText;
    [SerializeField] private Button signInButton;
    [SerializeField] private Button signOutButton;
    [SerializeField] private TMP_Text signedInText;

    [Header("Levels")]
    [SerializeField] private GameObject levelView;

    void Start()
    {
        // Disable level select view
        levelView.SetActive(false);
        
        PersistenceManager.Instance.OnLoginComplete += HandleUnitySignIn;

        // Check if we already have a full session (Name + Auth) from previous run
        if (PersistenceManager.Instance.IsSignedIn() && PersistenceManager.Instance.HasPlayerName())
        {
            FinalizeProfile();
        }
        else
        {
            // Show the profile screen if we don't have a name yet
            profileCreate.SetActive(true);
            HandleUnitySignInStatus();
        }
    }

    public void StartGame()
    {
        levelView.SetActive(true);
    }

    // TODO create sign out button
    public void EditName()
    {
        profileCreate.SetActive(true);
        // Prefill the field with existing name
        profileNameField.text = GetDisplayName(PersistenceManager.Instance.GetPlayerName());
        HandleUnitySignInStatus();
    }

    // This is triggered by the "Enter" button in the profile UI
    public async void CreateProfile()
    {
        string profileName = profileNameField.text.Trim();

        if (string.IsNullOrEmpty(profileName))
        {
            Debug.LogWarning("Profile name cannot be empty.");
            return;
        }

        // If the user did NOT sign in with Unity, they are a guest.
        // We must authenticate them anonymously now before setting the name.
        if (!PersistenceManager.Instance.IsSignedIn())
        {
            // Ensure the user is signed in (Unity OR anonymous)
            await PersistenceManager.Instance.SignInAnonymouslyFallback();
        }

        // Ignore no-op rename
        if (PersistenceManager.Instance.HasPlayerName() &&
            profileName == GetDisplayName(PersistenceManager.Instance.GetPlayerName()))
        {
            FinalizeProfile();
            return;
        }

        // Regardless of login method, set player name
        await PersistenceManager.Instance.SetPlayerName(profileName);

        FinalizeProfile();
    }

    public void SignInWithUnity()
    {
        Debug.Log("Starting sign in with unity callback.");
        // We just start the process; the PersistenceManager events will handle the UI update
        PersistenceManager.Instance.StartUnityLogin();
    }

    public void SignOut()
    {
        PersistenceManager.Instance.SignOut();
        profileCreate.SetActive(true);
        profileText.text = "";
        HandleUnitySignInStatus();
    }

    private void FinalizeProfile()
    {
        profileText.text = PersistenceManager.Instance.GetPlayerName();
        profileCreate.SetActive(false);
    }

    private void HandleUnitySignInStatus()
    {
        // if (PersistenceManager.Instance.IsSignedInToUnity())
        // {
        //     Debug.Log("Handle sign in status, IsSignedInToUnity = true");
        //     signInButton.gameObject.SetActive(false);
        //     signOutButton.gameObject.SetActive(true);
        //     signedInText.gameObject.SetActive(true);
        // }
        // else
        // {
        //     Debug.Log("Handle sign in status, IsSignedInToUnity = false");
        //     signInButton.gameObject.SetActive(true);
        //     signOutButton.gameObject.SetActive(false);
        //     signedInText.gameObject.SetActive(false);
        // }

        // TODO: do nothing for now
    }

    // This method is specifically for when login callback is invoked
    private void HandleUnitySignIn()
    {
        // If existing Unity account: skip profile create screen
        // If new Unity account, leave the profile create screen up to choose a name
        Debug.Log("HandleUnitySignIn()");
        if (PersistenceManager.Instance.HasPlayerName())
        {
            Debug.Log("Handle sign in status, has player name");
            FinalizeProfile();
        }
        else
        {
            // If they just logged in with Unity but have no name, we keep the screen open
            // but hide the Login button so they forced to enter a name.
            HandleUnitySignInStatus();
        }
    }

    // Util for stripping suffix from profile name
    private string GetDisplayName(string playerName)
    {
        return playerName.Split('#')[0];
    }
}