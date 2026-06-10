using UnityEngine;

public static class SaveSystem
{
    public const string TotalCoinsKey = "TotalCoins_Save";
    public const string HighScoreGroundMapKey = "HighScore_GroundMap";
    private const string GroundMapId = "Ground";

    public static int LoadTotalCoins()
    {
        return PlayerPrefs.GetInt(TotalCoinsKey, 0);
    }

    public static void SaveTotalCoins(int totalCoins)
    {
        PlayerPrefs.SetInt(TotalCoinsKey, Mathf.Max(0, totalCoins));
        PlayerPrefs.Save();
    }

    public static bool SpendCoins(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        int totalCoins = LoadTotalCoins();
        if (totalCoins < amount)
        {
            return false;
        }

        SaveTotalCoins(totalCoins - amount);
        return true;
    }

    public static bool IsMapUnlocked(string mapId)
    {
        if (string.IsNullOrWhiteSpace(mapId))
        {
            return false;
        }

        if (mapId == GroundMapId)
        {
            return true;
        }

        return PlayerPrefs.GetInt(GetMapUnlockKey(mapId), 0) == 1;
    }

    public static void UnlockMap(string mapId)
    {
        if (string.IsNullOrWhiteSpace(mapId) || mapId == GroundMapId)
        {
            return;
        }

        PlayerPrefs.SetInt(GetMapUnlockKey(mapId), 1);
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

    private static string GetMapUnlockKey(string mapId)
    {
        return "MapUnlocked_" + mapId;
    }
}
