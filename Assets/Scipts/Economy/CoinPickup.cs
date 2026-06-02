using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;

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

        GameSession.Instance.AddCoin(coinValue);
        Destroy(gameObject);
    }
}
