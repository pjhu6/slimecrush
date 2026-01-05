using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public string videoUrl;
    public VideoPlayer videoPlayer;

    void Awake()
    {
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        if (videoPlayer)
        {
            videoPlayer.url = videoUrl;
            videoPlayer.playOnAwake = false;
            videoPlayer.Prepare();

            
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("Video prepared, starting playback.");
        videoPlayer.Play();
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Video finished, loading main menu.");
        SceneManager.LoadScene("MainMenuScene");
    }
}
