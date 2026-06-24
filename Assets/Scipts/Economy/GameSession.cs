using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Coin")]
    [SerializeField] private int currentCoins = 0;
    [SerializeField] private int totalCoins = 0;   
    public int TotalCoins => totalCoins; 

    [Header("Fuel")]
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float currentFuel = 100f;
    [SerializeField] private float idleFuelDrainPerSecond = 0.5f;
    [SerializeField] private float gasFuelDrainPerSecond = 4f;

    [Header("Distance Score")]
    [SerializeField] private float currentDistance = 0f;
    [SerializeField] private float highScore = 0f;
    [SerializeField] private string highScoreKey = SaveSystem.HighScoreGroundMapKey;

    [Header("State")]
    [SerializeField] private bool isGameOver = false;

    private float startPlayerX;
    private bool hasStartPosition;
    private VehicelControl vehicleControl;
    private Rigidbody2D playerRigidbody;

    public Transform Player => player;
    public int CurrentCoins => currentCoins;
    public float CurrentFuel => currentFuel;
    public float MaxFuel => maxFuel;
    public float CurrentDistance => currentDistance;
    public float HighScore => highScore;
    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Auto-derive per-map high score key from scene name so each map tracks its own best run
        highScoreKey = "HighScore_" + SceneManager.GetActiveScene().name;
        highScore = SaveSystem.LoadHighScore(highScoreKey);
        totalCoins = SaveSystem.LoadTotalCoins();

        currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);
        CacheStartPosition();
        CachePlayerComponents();
    }

    private void Update()
    {
        if (isGameOver)
        {
            return;
        }

        if (player == null)
        {
            return;
        }

        if (!hasStartPosition)
        {
            CacheStartPosition();
        }

        if (vehicleControl == null || playerRigidbody == null)
        {
            CachePlayerComponents();
        }

        DrainFuel();
        UpdateDistanceScore();
        SaveHighScoreIfNeeded();

        if (currentFuel <= 0f)
        {
            GameOver();
        }
    }

    /// <summary>
    /// Gọi sau khi xe được đặt vào đúng vị trí bắt đầu (ví dụ sau khi InfiniteTerrain drop xe).
    /// Reset startPlayerX về vị trí hiện tại để tính distance từ đúng điểm xuất phát.
    /// </summary>
    public void ResetStartPosition()
    {
        if (player == null) return;
        startPlayerX  = player.position.x;
        currentDistance = 0f;
        hasStartPosition = true;
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
        hasStartPosition = false;
        CacheStartPosition();
        CachePlayerComponents();
    }

    public void AddCoin(int amount)    {
        if (amount <= 0)
        {
            return;
        }

        currentCoins += amount;
        totalCoins += amount;
        SaveSystem.SaveTotalCoins(totalCoins);
    }

    public void AddFuel(float amount)
    {
        if (amount <= 0f || isGameOver)
        {
            return;
        }

        currentFuel = Mathf.Clamp(currentFuel + amount, 0f, maxFuel);
    }

    public void GameOver()
    {
        if (isGameOver)
        {
            return;
        }

        currentFuel = 0f;
        isGameOver = true;
        StopPlayerMovement();
        SaveHighScoreIfNeeded();

        Debug.Log("Game Over: Het fuel. Tổng vàng hiện có: " + totalCoins);
        AudioService.PlayClip(AudioPaths.GameOver, 1f);
        NotifyGameOverPanel();

    }

    private void CacheStartPosition()
    {
        if (player == null)
        {
            return;
        }

        startPlayerX = player.position.x;
        hasStartPosition = true;
    }

    private void CachePlayerComponents()
    {
        if (player == null)
        {
            return;
        }

        vehicleControl = player.GetComponent<VehicelControl>();
        playerRigidbody = player.GetComponent<Rigidbody2D>();
    }

    private void DrainFuel()
    {
        float fuelDrainRate = idleFuelDrainPerSecond;

        if (IsGasPressed())
        {
            fuelDrainRate = gasFuelDrainPerSecond;
        }

        currentFuel -= fuelDrainRate * Time.deltaTime;
        currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);
    }

    private bool IsGasPressed()
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        return Keyboard.current.rightArrowKey.isPressed
            || Keyboard.current.dKey.isPressed
            || Keyboard.current.leftArrowKey.isPressed
            || Keyboard.current.aKey.isPressed;
    }

    private void StopPlayerMovement()
    {
        if (vehicleControl != null)
        {
            vehicleControl.enabled = false;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
        }
    }

    private void UpdateDistanceScore()
    {
        float distanceFromStart = player.position.x - startPlayerX;
        float newDistance = Mathf.Max(0f, distanceFromStart);

        if (newDistance > currentDistance)
        {
            currentDistance = newDistance;
        }
    }

    private void SaveHighScoreIfNeeded()
    {
        if (currentDistance <= highScore)
        {
            return;
        }

        highScore = currentDistance;
        SaveSystem.SaveHighScore(highScoreKey, highScore);
    }

    private void NotifyGameOverPanel()
    {
        GameOverPanel panel = FindFirstObjectByType<GameOverPanel>();

        if (panel == null)
        {
            GameOverPanel[] panels = Resources.FindObjectsOfTypeAll<GameOverPanel>();
            foreach (GameOverPanel candidate in panels)
            {
                if (candidate.gameObject.scene.IsValid() && candidate.gameObject.scene.isLoaded)
                {
                    panel = candidate;
                    break;
                }
            }
        }

        if (panel != null)
        {
            panel.ShowNow(this);
            return;
        }

        Debug.LogWarning("GameSession: Khong tim thay GameOverPanel trong scene.");
    }
}
