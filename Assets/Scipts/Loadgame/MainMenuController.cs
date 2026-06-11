using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;

    private const string GarageSceneName = "GarageScene";

    private void Start()
    {
        SaveSystem.SaveTotalCoins(5000);
        RefreshCoins();
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
        SceneManager.LoadScene("LevelSelect");
    }

    public void OnShop()
    {
        OpenGarage();
    }

    public void OpenGarage()
    {
        SceneManager.LoadScene(GarageSceneName);
    }

    public void OnDaily()
    {
        Debug.Log("Daily Reward coming soon");
    }

    public void OnLuckySpin()
    {
        Debug.Log("Lucky Spin coming soon");
    }

    public void OnQuit()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
