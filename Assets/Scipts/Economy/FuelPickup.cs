using UnityEngine;

public class FuelPickup : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] [Range(0f, 1f)] private float pickupVolume = 1f;

    private GameSession gameSession;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (gameSession == null)
        {
            gameSession = FindAnyObjectByType<GameSession>(FindObjectsInactive.Exclude);
        }

        if (gameSession == null)
        {
            Debug.LogError("FuelPickup: Khong tim thay GameSession trong scene.");
            return;
        }

        // Nạp đầy bình như Hill Climb Racing gốc — random 15-35 gây death spiral ở zone xa
        gameSession.AddFuel(gameSession.MaxFuel);
        AudioService.PlayClip(AudioPaths.FuelPickup, pickupVolume);
        Debug.Log("FuelPickup: Player nhat fuel, nap day binh.");
        Destroy(gameObject);
    }
}
