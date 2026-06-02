using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int coinValue = 1;
    private GameSession gameSession;

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
