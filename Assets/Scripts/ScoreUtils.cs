using UnityEngine;

public static class ScoreUtils
{
    public const int THREE_STAR_MAX_SCORE = 30000;  // 30 sec
    public const int TWO_STAR_MAX_SCORE = 60000;  // 1 min

    // Get number of stars based on score thresholds
    public static int GetStarsFromScore(int score)
    {
        if (score <= THREE_STAR_MAX_SCORE) return 3;
        if (score <= TWO_STAR_MAX_SCORE)   return 2;
        return 1;
    }

    // Formats milliseconds into MM:SS.xx
    public static string FormatMilliseconds(int ms)
    {
        // max int value means score doesn't exist, so return -- format instead of time
        if (ms == int.MaxValue)
        {
            return "--:--.--";
        }
        int minutes = ms / 60000;
        int seconds = (ms / 1000) % 60;
        int milliseconds = (ms / 10) % 100;

        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }

    public static string FormatMonospacedMilliseconds(int ms)
    {
        return GetMonospacedText(FormatMilliseconds(ms));
    }

    private static string GetMonospacedText(string text, string width = "0.55em")
    {
        return $"<mspace={width}>{text}</mspace>";
    }

    public static string GetLeaderboardIdForLevel(GameLevel level, bool isDevMode)
    {
        if (level == null)
        {
            Debug.LogError("GetLeaderboardIdForLevel called with null level!");
            return "";
        }

        // Hack, assume all dev leaderboards have "Dev" suffic
        return isDevMode ? $"{level.leaderboardId}Dev" : level.leaderboardId;
    }
}