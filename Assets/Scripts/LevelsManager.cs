using UnityEngine;

public class LevelsManager : MonoBehaviour
{
    public static LevelsManager Instance { get; private set; }

    [Header("Levels")]
    [SerializeField] private GameLevel[] levels;

    public GameLevel[] Levels => levels;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
