using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Audio")]
    public AudioSource musicSource;

    [Header("End Scene")]
    [SerializeField] private Transform endSceneContainer;
    [SerializeField] private GameObject bestScorePrefab;
    

    [Header("Game State")]
    public GameState CurrentState { get; private set; } = GameState.Playing;

    [Header("Levels")]
    [SerializeField] private LevelData levelData;

    private ClockManager clockManager;

    private int latestTimeMs;  // TODO: make this a map for each level

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        clockManager = FindFirstObjectByType<ClockManager>();
        string leaderboardId = ScoreUtils.GetLeaderboardIdForLevel(levelData.currentLevel, PersistenceManager.Instance.IsDevMode);
        clockManager.UpdateBestScoreDisplay(leaderboardId);

        // Load level
        if (levelData.currentLevel == null)
        {
            Debug.LogError("No level data found");
            return;
        }

        LoadLevel(levelData.currentLevel);
        
        Instance = this;
    }

    public void Pause()
    {
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;
    }

    public void Win()
    {
        CurrentState = GameState.Victory;

        // Save the latest time to use later. We will save this time later.
        latestTimeMs = Mathf.FloorToInt(clockManager.ElapsedTime * 1000f);

        musicSource.Stop();
    }

    public IEnumerator EndLevelCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        string leaderboardId = ScoreUtils.GetLeaderboardIdForLevel(levelData.currentLevel, PersistenceManager.Instance.IsDevMode);

        // Fetch best score from cloud
        var getScoreTask = PersistenceManager.Instance.GetBestScore(leaderboardId);
        yield return new WaitUntil(() => getScoreTask.IsCompleted);
        int currentBest = getScoreTask.Result;

        // Check if latest score is faster.
        // If no score exists, currentBest is int.MaxValue, so this will be true
        if (latestTimeMs < currentBest)
        {
            Debug.Log($"New Best Score! {latestTimeMs} is better than {currentBest}");

            // Start task to save the new best score to cloud
            var setScoreTask = PersistenceManager.Instance.SetBestScore(latestTimeMs, leaderboardId);
            yield return new WaitUntil(() => setScoreTask.IsCompleted);

            // Show the new best score screen
            GameObject screenObj = Instantiate(bestScorePrefab, endSceneContainer);
            BestScoreScreen screenScript = screenObj.GetComponent<BestScoreScreen>();
            screenScript.Initialize(latestTimeMs);

            // Wait for the prefab to tell us it is finished
            yield return new WaitUntil(() => screenScript.IsFinished);

        }
        else 
        {
            Debug.Log("Not a new best score.");
        }

        Resume();


        // TODO cutscene in this scene, then next level
        UnityEngine.SceneManagement.SceneManager.LoadScene("WinScene");
    }

    private void LoadLevel(GameLevel level)
    {
        Debug.Log($"Loading Level: {level.name}");

        // 2. Spawn the level prefab at 0, 0
        Instantiate(level.levelPrefab, Vector3.zero, Quaternion.identity);

        // 3. Position the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.transform.position = level.playerStartingPosition;
    }
}
