using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelMenuController : MonoBehaviour
{
    [Header("Data Bridge")]
    [SerializeField] private LevelData levelData;


    [Header("Levels")]
    // [SerializeField] public int CurrentLevel { get; private set; }
    [SerializeField] private GameLevel[] levels;

    [Header("UI References")]
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Transform container;

    void Start()
    {
        GenerateMenu();
    }

    private void GenerateMenu()
    {
        foreach (GameLevel level in levels)
        {
            // 1. Spawn the button
            Button newButton = Instantiate(buttonPrefab, container);

            // 2. Change the text
            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null) 
            {
                buttonText.text = level.levelName;
            }

            // 3. Assign the click logic
            // We use a temporary variable 'levelCopy' to avoid the "Last Item" bug in loops
            GameLevel levelCopy = level; 
            newButton.onClick.AddListener(() => {
                SelectLevel(levelCopy);
            });
        }
    }

    private void SelectLevel(GameLevel selectedLevel)
    {
        // Update the bridge and load the scene
        levelData.currentLevel = selectedLevel;
        SceneManager.LoadScene("GameScene");
    }
}
