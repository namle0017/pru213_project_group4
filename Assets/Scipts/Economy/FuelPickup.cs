using UnityEngine;

public class FuelPickup : MonoBehaviour
{
    public float fuelAmount = 25f;
    private GameSession gameSession;

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
        Debug.Log("FuelPickup: Player nhat " + fuelAmount + " fuel.");
        Destroy(gameObject);
    }
}
