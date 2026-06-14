using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameSession gameSession;

    private Button pauseOpenButtonComponent;
    private Button resumeButtonComponent;
    private Button restartButtonComponent;
    private Button mainMenuButtonComponent;
    private bool isPaused;

    private void Awake()
    {
        EnsureEventSystemExists();
        EnsureReferences();
        Time.timeScale = 1f;
        HidePausePanelImmediate();
    }

    private void Start()
    {
        EnsureReferences();
        EnsureButtonBindings();
        RefreshPauseButtonState();
    }

    private void Update()
    {
        EnsureReferences();
        EnsureButtonBindings();

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

        AudioService.PlayButtonClick();
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
        AudioService.PlayBackClose();
        isPaused = false;
        Time.timeScale = 1f;
        HidePausePanelImmediate();
        RefreshPauseButtonState();
        Debug.Log("PauseManager: Game resumed.");
    }

    public void RestartGame()
    {
        AudioService.PlayButtonClick();
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        AudioService.PlayBackClose();
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    private void EnsureReferences()
    {
        if (pausePanel == null)
        {
            pausePanel = FindSceneObjectByName("PausePanel");
        }

        if (pauseButton == null)
        {
            pauseButton = FindSceneObjectByName("PauseButton");
        }

        if (gameSession == null)
        {
            gameSession = FindFirstObjectByType<GameSession>();
        }

        if (pauseOpenButtonComponent == null && pauseButton != null)
        {
            pauseOpenButtonComponent = pauseButton.GetComponent<Button>();
        }

        if (resumeButtonComponent == null)
        {
            resumeButtonComponent = FindSceneComponentByName<Button>("ResumeButton");
        }

        if (restartButtonComponent == null)
        {
            restartButtonComponent = FindSceneComponentByName<Button>("PauseRestartButton");
        }

        if (mainMenuButtonComponent == null)
        {
            mainMenuButtonComponent = FindSceneComponentByName<Button>("PauseMainMenuButton");
        }
    }

    private void EnsureButtonBindings()
    {
        BindButton(pauseOpenButtonComponent, PauseGame);
        BindButton(resumeButtonComponent, ResumeGame);
        BindButton(restartButtonComponent, RestartGame);
        BindButton(mainMenuButtonComponent, LoadMainMenu);
    }

    private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private static GameObject FindSceneObjectByName(string objectName)
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform candidate in allTransforms)
        {
            if (candidate == null || candidate.name != objectName)
            {
                continue;
            }

            GameObject candidateObject = candidate.gameObject;
            if (!candidateObject.scene.IsValid() || !candidateObject.scene.isLoaded)
            {
                continue;
            }

            return candidateObject;
        }

        return null;
    }

    private static T FindSceneComponentByName<T>(string objectName) where T : Component
    {
        GameObject sceneObject = FindSceneObjectByName(objectName);
        if (sceneObject == null)
        {
            return null;
        }

        return sceneObject.GetComponent<T>();
    }

    private static void EnsureEventSystemExists()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
        Debug.Log("PauseManager: Auto-created missing EventSystem.");
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
