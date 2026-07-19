using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;

    private const string GarageSceneName = "GarageScene";
    private const string DailyRewardSceneName = "DailyRewardScene";

    private void Start()
    {
        RefreshCoins();
    }

    [ContextMenu("Dev/Add 5000 Coins")]
    private void DevAddCoins()
    {
        SaveSystem.AddCoins(5000);
        RefreshCoins();
        Debug.Log("MainMenuController: Added 5000 coins for dev testing.");
    }

    public void RefreshCoins()
    {
        if (coinText == null)
        {
            return;
        }

        coinText.text = SaveSystem.LoadTotalCoins().ToString();
    }

    public void OnPlay()
    {
        AudioService.PlayButtonClick();
        SceneManager.LoadScene("LevelSelect");
    }

    public void OnShop()
    {
        OpenGarage();
    }

    public void OpenGarage()
    {
        AudioService.PlayButtonClick();
        SceneManager.LoadScene(GarageSceneName);
    }

    public void OnDaily()
    {
        OnDailyReward();
    }

    public void OnDailyReward()
    {
        AudioService.PlayButtonClick();
        SceneManager.LoadScene(DailyRewardSceneName);
    }

    public void OnLuckySpin()
    {
        AudioService.PlayButtonClick();
        SceneManager.LoadScene("SpinScene");
    }

    public void OnQuit()
    {
        AudioService.PlayButtonClick();
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
