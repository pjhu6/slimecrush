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
    [SerializeField] public int CurrentLevel { get; private set; }  // TODO: set on level load
    [SerializeField] private GameLevel[] levels;

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
        CurrentLevel = PersistenceManager.Instance.GetPlayerCurrentLevel();

        // TODO incremental load if needed
        LoadAllLevels();
        PlacePlayerStartingPosition(CurrentLevel);
        
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

        // Fetch best score from cloud
        var getScoreTask = PersistenceManager.Instance.GetBestScore();
        yield return new WaitUntil(() => getScoreTask.IsCompleted);
        int currentBest = getScoreTask.Result;

        // Check if latest score is faster.
        // If no score exists, currentBest is int.MaxValue, so this will be true
        if (latestTimeMs < currentBest)
        {
            Debug.Log($"New Best Score! {latestTimeMs} is better than {currentBest}");

            // Start task to save the new best score to cloud
            var setScoreTask = PersistenceManager.Instance.SetBestScore(latestTimeMs);
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
        // UnityEngine.SceneManagement.SceneManager.LoadScene("WinScene");

        GoToNextLevel();
    }

    private void GoToNextLevel()
    {
        if (CurrentLevel >= levels.Length - 1)
        {   
            // TODO: final level, do something
            Debug.Log($"{CurrentLevel} is final level.");
            return;
        }

        Debug.Log($"Moving from level {CurrentLevel} to {CurrentLevel + 1}");
        CurrentLevel += 1;

        // TODO: give music source to gamelevel object
        musicSource.Play();
        // LoadLevel(currentLevel);
        // PlacePlayerStartingPosition(currentLevel);
    }

    private void LoadAllLevels()
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogWarning("No levels to load.");
            return;
        }

        for (int i = 0; i < levels.Length; i++)
        {
            GameLevel level = levels[i];

            if (level == null || level.levelPrefab == null)
            {
                Debug.LogWarning($"Level {i} is missing data or prefab.");
                continue;
            }

            Instantiate(
                level.levelPrefab,
                new Vector3(0f, level.levelStartingY, 0f),
                Quaternion.identity
            );

            Debug.Log($"Loaded level {i}");
        }
    }


    private void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length)
            return;

        GameLevel level = levels[levelIndex];

        Instantiate(
            level.levelPrefab,
            new Vector3(0f, level.levelStartingY, 0f),
            Quaternion.identity
        );
    }

    private void PlacePlayerStartingPosition(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length)
            return;

        GameLevel level = levels[levelIndex];

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.transform.position = level.playerStartingPosition;
    }
}
