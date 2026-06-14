using UnityEngine;

public class FuelPickup : MonoBehaviour
{
    [Header("Fuel Amount Range")]
    public float minFuelAmount = 15f;
    public float maxFuelAmount = 35f;

    [Header("Audio")]
    [SerializeField] [Range(0f, 1f)] private float pickupVolume = 1f;

    private float fuelAmount;
    private GameSession gameSession;

    private void Awake()
    {
        // Random lượng fuel khi object được tạo ra
        fuelAmount = Random.Range(minFuelAmount, maxFuelAmount);
        Debug.Log("FuelPickup: Spawn voi " + fuelAmount.ToString("F1") + " fuel.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("FuelPickup trigger by: " + other.gameObject.name + " | Tag: " + other.tag);

        if (!other.CompareTag("Player"))
        {
            return;
        }

        Debug.Log("Picked fuel");

        if (gameSession == null)
        {
            gameSession = FindFirstObjectByType<GameSession>();
        }

        if (gameSession == null)
        {
            Debug.LogError("FuelPickup: Khong tim thay GameSession trong scene.");
            return;
        }

        gameSession.AddFuel(fuelAmount);
        AudioService.PlayClip(AudioPaths.FuelPickup, pickupVolume);
        Debug.Log("FuelPickup: Player nhat " + fuelAmount.ToString("F1") + " fuel.");
        Destroy(gameObject);
    }
}
