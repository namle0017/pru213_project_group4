using UnityEngine;
using UnityEngine.U2D;

public class PickupSpawner : MonoBehaviour
{
    [Header("References")]
    public SpriteShapeController terrain;

    [Header("Prefabs")]
    public GameObject coinPrefab;
    public GameObject fuelPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Cứ mỗi bao nhiêu điểm terrain thì xét spawn 1 lần")]
    public int spawnInterval = 4;

    [Tooltip("Xác suất spawn coin tại mỗi điểm xét (0 - 1)")]
    [Range(0f, 1f)]
    public float coinSpawnChance = 0.5f;

    [Tooltip("Xác suất spawn fuel tại mỗi điểm xét (0 - 1)")]
    [Range(0f, 1f)]
    public float fuelSpawnChance = 0.2f;

    [Tooltip("Độ cao offset so với mặt terrain")]
    public float yOffset = 1.5f;

    void Start()
    {
        SpawnPickups();
    }

    void SpawnPickups()
    {
        if (terrain == null)
        {
            Debug.LogError("PickupSpawner: Chua gan SpriteShapeController (terrain).");
            return;
        }

        if (coinPrefab == null && fuelPrefab == null)
        {
            Debug.LogError("PickupSpawner: Chua gan bat ky prefab nao (coinPrefab / fuelPrefab).");
            return;
        }

        int pointCount = terrain.spline.GetPointCount();

        // Bỏ 2 điểm cuối (điểm đáy) và điểm đầu để tránh spawn ở rìa
        for (int i = 1; i < pointCount - 2; i++)
        {
            if (i % spawnInterval != 0)
                continue;

            Vector3 point = terrain.spline.GetPosition(i);
            Vector3 spawnPos = new Vector3(point.x, point.y + yOffset, 0f);

            // Ưu tiên fuel trước, nếu không thì xét coin (tránh chồng nhau)
            float roll = Random.value;

            if (fuelPrefab != null && roll < fuelSpawnChance)
            {
                GameObject fuel = Instantiate(fuelPrefab, spawnPos, Quaternion.identity);
                fuel.transform.SetParent(transform);
            }
            else if (coinPrefab != null && roll < fuelSpawnChance + coinSpawnChance)
            {
                GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);
                coin.transform.SetParent(transform);
            }
        }
    }
}
