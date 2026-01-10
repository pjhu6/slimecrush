using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public string videoUrl;
    public VideoPlayer videoPlayer;

    void Awake()
    {
        videoPlayer.playOnAwake = false;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoUrl;

        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetDirectAudioVolume(0, 0.2f);

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;

        videoPlayer.Prepare();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Cutscene skipped by player.");
            End();
        }
    }


    void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("Video prepared, starting playback.");
        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Video finished, loading main menu.");
        End();
    }

    private void End()
    {
        // TODO: move to next level or something
        SceneManager.LoadScene("MainMenuScene");
    }
}
