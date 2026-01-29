using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LevelMenuController : MonoBehaviour
{
    [Header("Data Bridge")]
    [SerializeField] private LevelData levelData;

    [Header("UI References")]
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Transform container;

    private readonly List<GameObject> spawnedButtons = new();

    private async void OnEnable()
    {
        await GenerateMenuAsync();
    }

    private async Task GenerateMenuAsync()
    {
        ClearMenu();

        foreach (GameLevel level in LevelsManager.Instance.Levels)
        {
            Button newButton = Instantiate(buttonPrefab, container);
            spawnedButtons.Add(newButton.gameObject);

            LevelButton levelButton = newButton.GetComponent<LevelButton>();

            if (levelButton != null)
            {
                levelButton.levelNameText.text = level.levelName;
                levelButton.scoreText.text = "Loading...";
            }

            GameLevel levelCopy = level;
            newButton.onClick.AddListener(() => SelectLevel(levelCopy));

            UpdateScoreAsync(levelButton, level);
        }
    }

    private async Task UpdateScoreAsync(LevelButton levelButton, GameLevel level)
    {
        if (levelButton == null) return;

        string leaderboardId = ScoreUtils.GetLeaderboardIdForLevel(
            level,
            PersistenceManager.Instance.IsDevMode
        );

        var entry = await PersistenceManager.Instance
            .GetPlayerLeaderboardEntry(leaderboardId);

        if (!levelButton) return;

        if (entry == null)
        {
            levelButton.SetNoScore();
            return;
        }

        levelButton.SetScore((int)entry.Score);
    }

    private void ClearMenu()
    {
        foreach (var obj in spawnedButtons)
        {
            Destroy(obj);
        }
        spawnedButtons.Clear();
    }

    private void SelectLevel(GameLevel selectedLevel)
    {
        levelData.currentLevel = selectedLevel;
        SceneManager.LoadScene("GameScene");
    }

    public void CloseLevelMenu()
    {
        gameObject.SetActive(false);
    }
}
