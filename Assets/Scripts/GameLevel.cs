using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level")]
public class GameLevel : ScriptableObject
{
    public string levelName;
    public string leaderboardId;
    public Vector2 playerStartingPosition;
    // public int levelStartingY;
    public GameObject levelPrefab;
}
