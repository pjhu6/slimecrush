using UnityEngine;
using TMPro;
using System.Collections;

public class BestScoreScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject[] starIcons;
    [SerializeField] private GameObject continueText;
    
    [Header("Animation Settings")]
    [SerializeField] private float totalAnimationDuration = 2.0f; // Controls both stars and count-up
    [SerializeField] private float delayBetweenStars = 0.2f;
    [SerializeField] private float bobAmount = 10f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float scorePopDuration = 0.3f;

    private float popDuration = 0.2f; // Calculated dynamically based on totalAnimationDuration and delayBetweenStars

    public bool IsFinished { get; private set; }
    private bool isSkipped = false;

    public void Initialize(int score)
    {
        isSkipped = false;
        IsFinished = false;
        
        // Hide all stars initially
        foreach (var star in starIcons)
        {
            star.SetActive(false);
            star.transform.localScale = Vector3.zero;
        }

        // Hide continue text initially
        continueText.SetActive(false);

        // Calculate popDuration based on the FULL array length (3) so timing is consistent
        if (starIcons.Length > 0)
        {
            popDuration = (totalAnimationDuration / starIcons.Length) - delayBetweenStars;
            popDuration = Mathf.Max(0.1f, popDuration);
        }

        StartCoroutine(RunSequence(score));
    }

    private IEnumerator RunSequence(int score)
    {
        // Start count up animation
        StartCoroutine(CountScoreAnimation(score));

        // Start star animation, yield so we wait for skip or finish normally
        int starCount = ScoreUtils.GetStarsFromScore(score);
        yield return StartCoroutine(AnimateStarsWithSkip(starCount));

        // If we skipped, force the stars to appear
        for (int i = 0; i < starCount; i++)
        {
            starIcons[i].SetActive(true);
            starIcons[i].transform.localScale = Vector3.one;
        }

        // Brief pause before continue is allowed
        yield return new WaitForSeconds(2f);

        // Show the continue text
        if (continueText != null) {
            continueText.SetActive(true);
        } 

        // Wait for the final Space press to close the screen
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        IsFinished = true;
    }

    private IEnumerator CountScoreAnimation(int targetScoreMs)
    {
        float elapsed = 0f;

        // Ensure visible immediately
        scoreText.transform.localScale = Vector3.one;

        // Count up
        while (elapsed < totalAnimationDuration && !isSkipped)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / totalAnimationDuration;

            int currentDisplayScore = (int)Mathf.Lerp(
                0,
                targetScoreMs,
                Mathf.SmoothStep(0, 1, t)
            );

            scoreText.text = ScoreUtils.FormatMonospacedMilliseconds(currentDisplayScore);
            yield return null;
        }

        // Force final value
        scoreText.text = ScoreUtils.FormatMonospacedMilliseconds(targetScoreMs);

        // Pop effect after done counting
        yield return StartCoroutine(CountPopEffect(scoreText.transform));

        // Let layout settle before bobbing
        yield return new WaitForSeconds(0.5f);

        // Bob for rest of lifetime
        Vector3 originalPos = scoreText.transform.localPosition;
        float bobTime = 0f;

        while (!IsFinished)
        {
            bobTime += Time.deltaTime;
            float newY = originalPos.y + Mathf.Sin(bobTime * bobSpeed) * bobAmount;
            scoreText.transform.localPosition = new Vector3(originalPos.x, newY, originalPos.z);
            yield return null;
        }
    }


    private IEnumerator AnimateStarsWithSkip(int starCount)
    {
        // We iterate based on earned stars, but the timing of each step matches the full array duration
        for (int i = 0; i < starCount; i++)
        {
            // Start the pop animation for this star
            starIcons[i].SetActive(true);
            StartCoroutine(StarPopEffect(starIcons[i]));

            // Since we don't wait for pop coroutine to finish, need to add for correct delay duration
            float waitTime = delayBetweenStars + popDuration;
            // Wait for the delay, but skip if space is pressed
            float timer = 0;
            while (timer < waitTime)
            {
                timer += Time.deltaTime;
                
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    // Stop and show all stars
                    isSkipped = true;
                    yield break; 
                }
                yield return null;
            }
        }
    }

    private IEnumerator StarPopEffect(GameObject target)
    {
        float elapsed = 0f;
        float growDuration = popDuration * 0.7f;   // 70% of time growing
        float settleDuration = popDuration * 0.3f; // 30% of time settling

        Vector3 startScale = Vector3.zero;
        Vector3 overshootScale = new Vector3(1.2f, 1.2f, 1.2f); 
        Vector3 finalScale = Vector3.one;

        // 1. Grow to Overshoot
        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            target.transform.localScale = Vector3.Lerp(startScale, overshootScale, elapsed / growDuration);
            yield return null;
        }

        // 2. Shrink back to Normal
        elapsed = 0f;
        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            target.transform.localScale = Vector3.Lerp(overshootScale, finalScale, elapsed / settleDuration);
            yield return null;
        }

        target.transform.localScale = finalScale;
    }

    private IEnumerator CountPopEffect(Transform target)
    {
        float elapsed = 0f;
        float growDuration = scorePopDuration * 0.5f;
        float settleDuration = scorePopDuration * 0.5f;

        Vector3 startScale = target.localScale;
        Vector3 overshootScale = startScale * 1.25f;

        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / growDuration);
            target.localScale = Vector3.Lerp(startScale, overshootScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / settleDuration);
            target.localScale = Vector3.Lerp(overshootScale, startScale, t);
            yield return null;
        }

        target.localScale = startScale;
    }
}