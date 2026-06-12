using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CoinPickup : MonoBehaviour
{
    private const string CoinPickupClipPath = "Assets/Audio/coin_pickup.mp3";

    [Header("Coin Value Range")]
    public int minCoinValue = 3;
    public int maxCoinValue = 5;

    [Header("Audio")]
    [SerializeField] private AudioClip coinPickupClip;
    [SerializeField] [Range(0f, 1f)] private float pickupVolume = 1f;

    private int coinValue;
    private GameSession gameSession;

    private void Awake()
    {
        TryAssignEditorAudio();
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
        PlayPickupSound();
        Debug.Log("CoinPickup: Player nhat " + coinValue + " coin.");
        Destroy(gameObject);
    }

    private void PlayPickupSound()
    {
        if (coinPickupClip == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(coinPickupClip, transform.position, pickupVolume);
    }

    private void TryAssignEditorAudio()
    {
#if UNITY_EDITOR
        if (coinPickupClip == null)
        {
            coinPickupClip = AssetDatabase.LoadAssetAtPath<AudioClip>(CoinPickupClipPath);
        }
#endif
    }
}
