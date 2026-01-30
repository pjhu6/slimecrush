using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public TMP_Text levelNameText;
    public TMP_Text scoreText;

    [Header("Star Settings")]
    [SerializeField] private Image[] starIcons;
    [SerializeField] private Sprite filledStar;
    [SerializeField] private Sprite emptyStar;

    private void Awake()
    {
        // Initialize by showing 0 stars (all empty)
        SetStars(0);
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

    private void SetStarsFromScore(int score)
    {
        int starsToShow = ScoreUtils.GetStarsFromScore(score);
        SetStars(starsToShow);
    }

    private void SetStars(int count)
    {
        for (int i = 0; i < starIcons.Length; i++)
        {
            // 1. Swap the sprite based on the count
            starIcons[i].sprite = (i < count) ? filledStar : emptyStar;

            // // 2. Ensure the image is fully visible (in case it was previously hidden via alpha)
            // Color c = starIcons[i].color;
            // c.a = 1f; 
            // starIcons[i].color = c;
        }
    }
}