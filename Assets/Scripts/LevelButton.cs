using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public TMP_Text levelNameText;
    public TMP_Text scoreText;

    [SerializeField] private Image[] starIcons;

    private void Awake()
    {
        HideAllStars();
    }

    private void HideAllStars()
    {
        for (int i = 0; i < starIcons.Length; i++)
        {
            Color c = starIcons[i].color;
            c.a = 0f;
            starIcons[i].color = c;
        }
    }

    public void SetLoading()
    {
        scoreText.text = "Loading...";
        SetStars(0);
    }

    public void SetScore(int score)
    {
        scoreText.text = ScoreUtils.FormatMilliseconds(score);
        SetStarsFromScore(score);
    }

    public void SetNoScore()
    {
        scoreText.text = "--:--.--";
        SetStars(0);
    }
    
    // TODO: if have emptyStar sprite
    // private void SetStars(int score)
    // {
    //     int starsToShow = ScoreUtils.GetStarsFromScore(score);

    //     for (int i = 0; i < starImages.Length; i++)
    //     {
    //         starImages[i].sprite = i < starsToShow
    //             ? filledStar
    //             : emptyStar;
    //     }
    // }
    private void SetStarsFromScore(int score)
    {
        int starsToShow = ScoreUtils.GetStarsFromScore(score);

        SetStars(starsToShow);
    }

    private void SetStars(int count)
    {
        for (int i = 0; i < starIcons.Length; i++)
        {
            Color c = starIcons[i].color;
            c.a = i < count ? 1f : 0f;
            starIcons[i].color = c;
        }
    }
}
