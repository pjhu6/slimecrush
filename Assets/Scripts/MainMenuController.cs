using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Profile setup")]
    [SerializeField] private GameObject profileCreate;
    [SerializeField] private TMP_InputField profileNameField;
    [SerializeField] private TMP_Text profileText;
    

    void Start()
    {
        if (PersistenceManager.Instance.HasPlayerName())
        {
            PostLogin();
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void EditName()
    {
        profileCreate.SetActive(true);
    }

    public async void CreateProfile()
    {
        
        string profileName = profileNameField.text.Trim();

        // if profile text without suffix is the same as the new name, don't do anything
        // this is a hack to avoid creating same name with diff #
        Debug.Log("Current profile text: " + profileText.text);
        Debug.Log("New profile name: " + profileName);
        if (profileName == profileText.text.Split('#')[0])
        {
            Debug.Log("Profile name unchanged, do nothing.");
            profileCreate.SetActive(false);
            return;
        }

        if (string.IsNullOrEmpty(profileName))
        {
            Debug.LogWarning("Profile name cannot be empty.");
            return;
        }
        await PersistenceManager.Instance.SetPlayerName(profileName);
        PostLogin();
    }

    private void PostLogin()
    {
        profileText.text = PersistenceManager.Instance.GetPlayerName();
        profileCreate.SetActive(false);
    }
}
