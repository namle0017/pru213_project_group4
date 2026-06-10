using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameSession gameSession;

    private bool isPaused;

    private void Awake()
    {
        Time.timeScale = 1f;
        HidePausePanelImmediate();
    }

    private void Start()
    {
        if (gameSession == null)
        {
            gameSession = FindFirstObjectByType<GameSession>();
        }

        RefreshPauseButtonState();
    }

    private void Update()
    {
        if (gameSession == null)
        {
            gameSession = FindFirstObjectByType<GameSession>();
        }

        if (gameSession != null && gameSession.IsGameOver)
        {
            if (isPaused)
            {
                ResumeGame();
            }

            SetPauseButtonVisible(false);
            return;
        }

        SetPauseButtonVisible(!isPaused);
    }

    public void PauseGame()
    {
        if (gameSession != null && gameSession.IsGameOver)
        {
            return;
        }

        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        SetPauseButtonVisible(false);
        Debug.Log("PauseManager: Game paused.");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        HidePausePanelImmediate();
        RefreshPauseButtonState();
        Debug.Log("PauseManager: Game resumed.");
    }

    public void RestartGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    private void HidePausePanelImmediate()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    private void RefreshPauseButtonState()
    {
        bool shouldShow = !isPaused && (gameSession == null || !gameSession.IsGameOver);
        SetPauseButtonVisible(shouldShow);
    }

    private void SetPauseButtonVisible(bool visible)
    {
        if (pauseButton != null && pauseButton.activeSelf != visible)
        {
            pauseButton.SetActive(visible);
        }
    }
}
