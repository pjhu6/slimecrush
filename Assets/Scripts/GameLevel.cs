using UnityEngine;

public class GameLevel
{
    public string LevelName { get; private set; }
    public Vector2 StartingPosition { get; private set; }

    public GameLevel(string levelName, int startingX, int startingY)
    {
        LevelName = levelName;
        StartingPosition = new Vector2(startingX, startingY);
    }
}