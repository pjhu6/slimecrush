using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    // public GameObject transitionImage;
    // public float animationDuration = 1.2f;

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
        // StartCoroutine(PlayTransitionAndLoad());
    }

    // IEnumerator PlayTransitionAndLoad()
    // {
    //     // transitionImage.SetActive(true);

    //     // yield return new WaitForSeconds(animationDuration);

    //     SceneManager.LoadScene("GameScene");
    // }
}
