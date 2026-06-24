using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameSession gameSession;
    private int _gameSessionSearchRetries;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI totalCoinText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Slider fuelSlider;
    [SerializeField] private Image fuelFillImage;

    [Header("Fuel Bar Colors")]
    [SerializeField] private Color highFuelColor = Color.green;
    [SerializeField] private Color mediumFuelColor = Color.yellow;
    [SerializeField] private Color lowFuelColor = Color.red;

    private void Start()
    {
        if (gameSession == null)
        {
            gameSession = FindAnyObjectByType<GameSession>(FindObjectsInactive.Exclude);
        }

        if (gameSession == null)
        {
            Debug.LogWarning("HUDController: GameSession not found in Start — will retry in Update.");
        }

        UpdateHUD();
    }

    private void Update()
    {
        if (gameSession == null)
        {
            // Fixed: retry for up to 120 frames (~2s), then stop to avoid per-frame overhead
            if (_gameSessionSearchRetries > 120) return;

            _gameSessionSearchRetries++;
            gameSession = FindAnyObjectByType<GameSession>(FindObjectsInactive.Exclude);

            if (gameSession == null) return;

            _gameSessionSearchRetries = 0;
        }

        UpdateHUD();
    }

    private void UpdateHUD()
    {
        if (coinText != null)
        {
            coinText.text = "x" + gameSession.CurrentCoins;
        }
        if (totalCoinText != null)
        {
            totalCoinText.text = "Total Coins: " + gameSession.TotalCoins;
        }

        if (distanceText != null)
        {
            int distanceValue = Mathf.FloorToInt(gameSession.CurrentDistance);
            distanceText.text = distanceValue + " m";
        }

        if (highScoreText != null)
        {
            int highScoreValue = Mathf.FloorToInt(gameSession.HighScore);
            highScoreText.text = highScoreValue + " m";
        }

        if (fuelSlider != null)
        {
            fuelSlider.minValue = 0f;
            fuelSlider.maxValue = gameSession.MaxFuel;
            fuelSlider.value = gameSession.CurrentFuel;
        }

        if (fuelFillImage != null)
        {
            float fuelPercent = 0f;

            if (gameSession.MaxFuel > 0f)
            {
                fuelPercent = gameSession.CurrentFuel / gameSession.MaxFuel;
            }

            if (fuelPercent > 0.6f)
            {
                fuelFillImage.color = highFuelColor;
            }
            else if (fuelPercent > 0.3f)
            {
                fuelFillImage.color = mediumFuelColor;
            }
            else
            {
                fuelFillImage.color = lowFuelColor;
            }
        }
    }
}
