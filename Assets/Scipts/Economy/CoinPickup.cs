using UnityEngine;
public class CoinPickup : MonoBehaviour
{
    [Header("Coin Value Range")]
    public int minCoinValue = 3;
    public int maxCoinValue = 5;

    [Header("Audio")]
    [SerializeField] [Range(0f, 1f)] private float pickupVolume = 1f;

    private int coinValue;
    private GameSession gameSession;

    private void Awake()
    {
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
            gameSession = FindAnyObjectByType<GameSession>(FindObjectsInactive.Exclude);
        }

        if (gameSession == null)
        {
            Debug.LogError("CoinPickup: Khong tim thay GameSession trong scene.");
            return;
        }

        gameSession.AddCoin(coinValue);
        PlayPickupSound();
        Debug.Log("CoinPickup: Player nhat " + coinValue + " coin.");
        Destroy(gameObject);
    }

    private void PlayPickupSound()
    {
        AudioService.PlayClip(AudioPaths.CoinPickup, pickupVolume);
    }
}
