using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Coin")]
    [SerializeField] private int currentCoins = 0;

    [Header("Fuel")]
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float currentFuel = 100f;
    [SerializeField] private float fuelDrainPerSecond = 5f;

    [Header("Distance Score")]
    [SerializeField] private float currentDistance = 0f;
    [SerializeField] private float highScore = 0f;
    [SerializeField] private string highScoreKey = "HighScore_GroundMap";

    [Header("State")]
    [SerializeField] private bool isGameOver = false;

    private float startPlayerX;
    private bool hasStartPosition;

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
        highScore = PlayerPrefs.GetFloat(highScoreKey, 0f);
        currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);
        CacheStartPosition();
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

        DrainFuel();
        UpdateDistanceScore();
        SaveHighScoreIfNeeded();

        if (currentFuel <= 0f)
        {
            GameOver();
        }
    }

    public void AddCoin(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentCoins += amount;
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
        SaveHighScoreIfNeeded();
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

    private void DrainFuel()
    {
        currentFuel -= fuelDrainPerSecond * Time.deltaTime;
        currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);
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
        PlayerPrefs.SetFloat(highScoreKey, highScore);
        PlayerPrefs.Save();
    }
}
