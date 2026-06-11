using UnityEngine;

public static class SaveSystem
{
    public const string TotalCoinsKey = "TotalCoins_Save";
    public const string HighScoreGroundMapKey = "HighScore_GroundMap";
    private const string GroundMapId = "Ground";
    private const string SelectedVehicleKey = "SelectedVehicle";

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

    public static bool IsVehicleUnlocked(string vehicleId)
    {
        if (string.IsNullOrWhiteSpace(vehicleId))
        {
            return false;
        }

        if (vehicleId == VehicleIds.BasicCar)
        {
            return true;
        }

        return PlayerPrefs.GetInt(GetVehicleUnlockKey(vehicleId), 0) == 1;
    }

    public static void SaveVehicleUnlocked(string vehicleId, bool isUnlocked)
    {
        if (string.IsNullOrWhiteSpace(vehicleId) || vehicleId == VehicleIds.BasicCar)
        {
            return;
        }

        PlayerPrefs.SetInt(GetVehicleUnlockKey(vehicleId), isUnlocked ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void SaveSelectedVehicle(string vehicleId)
    {
        string safeVehicleId = string.IsNullOrWhiteSpace(vehicleId) ? VehicleIds.BasicCar : vehicleId;

        if (!IsVehicleUnlocked(safeVehicleId))
        {
            Debug.LogWarning("SaveSystem: Vehicle " + safeVehicleId + " chua unlock, fallback ve basic_car.");
            safeVehicleId = VehicleIds.BasicCar;
        }

        PlayerPrefs.SetString(SelectedVehicleKey, safeVehicleId);
        PlayerPrefs.Save();
        Debug.Log("SaveSystem: Saved selected vehicle = " + safeVehicleId);
    }

    public static string LoadSelectedVehicle()
    {
        string selectedVehicleId = PlayerPrefs.GetString(SelectedVehicleKey, VehicleIds.BasicCar);

        if (!IsVehicleUnlocked(selectedVehicleId))
        {
            Debug.LogWarning("SaveSystem: Loaded vehicle " + selectedVehicleId + " khong hop le/chu a unlock, fallback ve basic_car.");
            return VehicleIds.BasicCar;
        }

        Debug.Log("SaveSystem: Loaded selected vehicle = " + selectedVehicleId);
        return selectedVehicleId;
    }

    private static string GetMapUnlockKey(string mapId)
    {
        return "MapUnlocked_" + mapId;
    }

    private static string GetVehicleUnlockKey(string vehicleId)
    {
        return "VehicleUnlocked_" + vehicleId;
    }
}
