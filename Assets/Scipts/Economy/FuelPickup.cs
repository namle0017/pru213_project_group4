using UnityEngine;

public class FuelPickup : MonoBehaviour
{
    [SerializeField] private float fuelAmount = 25f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (GameSession.Instance == null)
        {
            return;
        }

        GameSession.Instance.AddFuel(fuelAmount);
        Destroy(gameObject);
    }
}
