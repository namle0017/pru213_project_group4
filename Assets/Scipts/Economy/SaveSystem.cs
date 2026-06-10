using UnityEngine;

public static class SaveSystem
{
    public const string TotalCoinsKey = "TotalCoins_Save";
    public const string HighScoreGroundMapKey = "HighScore_GroundMap";

    public static int LoadTotalCoins()
    {
        return PlayerPrefs.GetInt(TotalCoinsKey, 0);
    }

    public static void SaveTotalCoins(int totalCoins)
    {
        PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);
        PlayerPrefs.Save();
    }

    public static float LoadHighScore(string highScoreKey)
    {
        return PlayerPrefs.GetFloat(highScoreKey, 0f);
    }

    public static void SaveHighScore(string highScoreKey, float highScore)
    {
        PlayerPrefs.SetFloat(highScoreKey, highScore);
        PlayerPrefs.Save();
    }
}
