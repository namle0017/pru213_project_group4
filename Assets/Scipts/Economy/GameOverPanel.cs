using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameSession gameSession;
    [SerializeField] private StartGame startGame;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private GameObject gameplayHudRoot;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI coinsEarnedText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    private bool hasShownGameOver;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        EnsureInitialized();
        HidePanel();
        Debug.Log("GameOverPanel Awake.");
    }

    private void Start()
    {
        if (gameSession == null)
        {
            gameSession = FindFirstObjectByType<GameSession>();
        }

        if (startGame == null)
        {
            startGame = FindFirstObjectByType<StartGame>();
        }
    }

    private void Update()
    {
        if (hasShownGameOver)
        {
            return;
        }

        if (gameSession == null)
        {
            gameSession = FindFirstObjectByType<GameSession>();
            if (gameSession == null)
            {
                return;
            }
        }

        if (!gameSession.IsGameOver)
        {
            return;
        }

        ShowGameOver();
    }

    private void ShowGameOver()
    {
        hasShownGameOver = true;
        UpdateTexts();
        ShowPanel();
        Debug.Log("Game Over Panel shown.");
    }

    public void ShowNow(GameSession session)
    {
        if (session != null)
        {
            gameSession = session;
        }

        EnsureInitialized();
        ShowGameOver();
    }

    private void HidePanel()
    {
        EnsureInitialized();

        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void ShowPanel()
    {
        EnsureInitialized();

        if (canvasGroup == null)
        {
            return;
        }

        if (!panelRoot.activeSelf)
        {
            panelRoot.SetActive(true);
        }

        if (gameplayHudRoot != null)
        {
            gameplayHudRoot.SetActive(false);
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void EnsureInitialized()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        if (canvasGroup == null)
        {
            canvasGroup = panelRoot.GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = panelRoot.AddComponent<CanvasGroup>();
        }
    }

    private void UpdateTexts()
    {
        if (gameSession == null)
        {
            return;
        }

        if (titleText != null)
        {
            titleText.text = "GAME OVER";
        }

        if (distanceText != null)
        {
            distanceText.text = "Distance: " + Mathf.FloorToInt(gameSession.CurrentDistance) + " m";
        }

        if (coinsEarnedText != null)
        {
            coinsEarnedText.text = "Coins Earned: " + gameSession.CurrentCoins;
        }

        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + Mathf.FloorToInt(gameSession.HighScore) + " m";
        }
    }

    public void RestartGame()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log("GameOverPanel: Restart scene " + currentSceneName);

        AudioService.PlayButtonClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene(currentSceneName);
    }

    public void GoToMainMenu()
    {
        if (startGame == null)
        {
            startGame = FindFirstObjectByType<StartGame>();
        }

        if (startGame != null)
        {
            AudioService.PlayBackClose();
            startGame.LoadMainMenu();
            return;
        }

        Debug.LogError("GameOverPanel: Khong tim thay StartGame de ve Menu.");
    }
}
