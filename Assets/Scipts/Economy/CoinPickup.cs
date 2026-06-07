using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [Header("Coin Value Range")]
    public int minCoinValue = 3;
    public int maxCoinValue = 5;

    private int coinValue;
    private GameSession gameSession;

    private void Awake()
    {
        // Random so coin khi object duoc tao ra (inclusive ca 2 dau)
        coinValue = Random.Range(minCoinValue, maxCoinValue + 1);
        Debug.Log("CoinPickup: Spawn voi " + coinValue + " coin.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("CoinPickup trigger by: " + other.gameObject.name + " | Tag: " + other.tag);

        if (!other.CompareTag("Player"))
        {
            return;
        }

        Debug.Log("Picked coin");

        if (gameSession == null)
        {
            gameSession = FindFirstObjectByType<GameSession>();
        }

        if (gameSession == null)
        {
            Debug.LogError("CoinPickup: Khong tim thay GameSession trong scene.");
            return;
        }

        gameSession.AddCoin(coinValue);
        Debug.Log("CoinPickup: Player nhat " + coinValue + " coin.");
        Destroy(gameObject);
    }
}
