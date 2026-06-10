using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// Spawn Coin va Fuel phia truoc player theo difficulty zone.
/// Pickup pattern duoc random theo seed de giu tinh semi-random nhung van lap lai duoc.
/// </summary>
public class PickupSpawnerRuntime : MonoBehaviour
{
    [Serializable]
    private class DifficultyZone
    {
        [Min(0f)] public float minDistance = 0f;
        [Min(0f)] public float maxDistance = 100f;

        [Header("Fuel")]
        [Min(1f)] public float fuelIntervalMin = 60f;
        [Min(1f)] public float fuelIntervalMax = 80f;

        [Header("Coin Cluster")]
        [Min(1f)] public float coinIntervalMin = 18f;
        [Min(1f)] public float coinIntervalMax = 25f;
        [Min(1)] public int minCoinsPerCluster = 3;
        [Min(1)] public int maxCoinsPerCluster = 3;
        public float coinHeightOffset = 0.6f;

        public bool Contains(float distance)
        {
            return distance >= minDistance && distance < maxDistance;
        }
    }

    [Header("References")]
    [Tooltip("Transform cua player (xe). De trong neu muon auto find theo tag Player.")]
    public Transform player;

    [Header("Prefabs")]
    public GameObject coinPrefab;
    public GameObject fuelPrefab;

    [Header("Deterministic Spawn")]
    [Tooltip("Cung seed se cho ra cung pickup pattern.")]
    public int seed = 12345;

    [Header("Spawn Ahead")]
    [Tooltip("Spawn cach player bao xa ve phia truoc de tranh pop-in.")]
    public float spawnAheadDistance = 25f;

    [Header("Fuel Safety")]
    [Tooltip("Fuel dau tien khong spawn qua gan diem bat dau.")]
    public float minimumFuelSpawnDistance = 60f;

    [Tooltip("Fuel luon nam gan mat dat de de lay.")]
    public float fuelGroundYOffset = 0.45f;

    [Header("Coin Risk")]
    [Tooltip("Cum coin dau tien spawn som de tao dong luc cho nguoi choi.")]
    public float firstCoinSpawnDistance = 15f;

    [Tooltip("Cum coin thu hai o doan dau de nguoi choi tiep tuc co reward som.")]
    public float secondCoinSpawnDistance = 50f;

    [Tooltip("Khoang cach ngang giua cac coin trong cung 1 cum.")]
    public float coinClusterSpacing = 1.2f;

    [Tooltip("Do cao tang them cho tung coin trong cum de cum coin nhin tu nhien hon.")]
    public float coinArcHeightStep = 0.08f;

    [Header("Terrain Detection")]
    [Tooltip("Raycast bat dau tu do cao nay (m).")]
    public float raycastStartHeight = 20f;

    [Tooltip("Layer cua terrain. Nen gan rieng layer Ground de raycast on dinh hon.")]
    public LayerMask terrainLayer;

    [Header("Cleanup")]
    [Tooltip("Destroy pickup khi no nam sau player hon muc nay (m).")]
    public float cleanupBehindDistance = 100f;

    [Header("Difficulty Zones")]
    [SerializeField] private DifficultyZone tutorialZone = new DifficultyZone
    {
        minDistance = 0f,
        maxDistance = 100f,
        fuelIntervalMin = 60f,
        fuelIntervalMax = 80f,
        coinIntervalMin = 18f,
        coinIntervalMax = 24f,
        minCoinsPerCluster = 1,
        maxCoinsPerCluster = 2,
        coinHeightOffset = 0.6f
    };

    [SerializeField] private DifficultyZone easyZone = new DifficultyZone
    {
        minDistance = 100f,
        maxDistance = 300f,
        fuelIntervalMin = 80f,
        fuelIntervalMax = 110f,
        coinIntervalMin = 15f,
        coinIntervalMax = 25f,
        minCoinsPerCluster = 3,
        maxCoinsPerCluster = 5,
        coinHeightOffset = 0.9f
    };

    [SerializeField] private DifficultyZone mediumZone = new DifficultyZone
    {
        minDistance = 300f,
        maxDistance = 600f,
        fuelIntervalMin = 100f,
        fuelIntervalMax = 140f,
        coinIntervalMin = 18f,
        coinIntervalMax = 30f,
        minCoinsPerCluster = 4,
        maxCoinsPerCluster = 6,
        coinHeightOffset = 1.15f
    };

    [SerializeField] private DifficultyZone hardZone = new DifficultyZone
    {
        minDistance = 600f,
        maxDistance = 999999f,
        fuelIntervalMin = 120f,
        fuelIntervalMax = 170f,
        coinIntervalMin = 20f,
        coinIntervalMax = 35f,
        minCoinsPerCluster = 5,
        maxCoinsPerCluster = 7,
        coinHeightOffset = 1.45f
    };

    private readonly List<GameObject> _spawnedPickups = new List<GameObject>();

    private System.Random _random;
    private float _startPlayerX;
    private float _nextFuelX;
    private float _nextCoinX;
    private int _tutorialCoinClustersSpawned;
    private bool _isInitialized;

    private IEnumerator Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogWarning("PickupSpawnerRuntime: Khong tim thay object co tag Player.");
                yield break;
            }
        }

        _random = new System.Random(seed);
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        InitializeSpawnState();
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        if (!_isInitialized)
        {
            return;
        }

        float frontX = player.position.x + spawnAheadDistance;

        while (fuelPrefab != null && frontX >= _nextFuelX)
        {
            SpawnFuel(_nextFuelX);
            _nextFuelX += GetNextFuelInterval(_nextFuelX);
        }

        while (coinPrefab != null && frontX >= _nextCoinX)
        {
            SpawnCoinCluster(_nextCoinX);
            _nextCoinX = GetNextCoinSpawnX(_nextCoinX);
        }

        CleanupOldPickups();
    }

    private void SpawnFuel(float spawnX)
    {
        float groundY = GetGroundY(spawnX);
        Vector3 spawnPosition = new Vector3(spawnX, groundY + fuelGroundYOffset, 0f);
        GameObject fuel = Instantiate(fuelPrefab, spawnPosition, Quaternion.identity);
        _spawnedPickups.Add(fuel);
    }

    private void SpawnCoinCluster(float startX)
    {
        DifficultyZone zone = GetZoneForDistance(GetDistanceFromStart(startX));
        int coinCount = NextInt(zone.minCoinsPerCluster, zone.maxCoinsPerCluster);

        for (int i = 0; i < coinCount; i++)
        {
            float spawnX = startX + i * coinClusterSpacing;
            float groundY = GetGroundY(spawnX);
            float arcOffset = i * coinArcHeightStep;
            Vector3 spawnPosition = new Vector3(spawnX, groundY + zone.coinHeightOffset + arcOffset, 0f);
            GameObject coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
            _spawnedPickups.Add(coin);
        }
    }

    private void CleanupOldPickups()
    {
        float cleanupX = player.position.x - cleanupBehindDistance;

        for (int i = _spawnedPickups.Count - 1; i >= 0; i--)
        {
            GameObject pickup = _spawnedPickups[i];

            if (pickup == null)
            {
                _spawnedPickups.RemoveAt(i);
                continue;
            }

            if (pickup.transform.position.x < cleanupX)
            {
                Destroy(pickup);
                _spawnedPickups.RemoveAt(i);
            }
        }
    }

    private float GetNextFuelInterval(float worldX)
    {
        DifficultyZone zone = GetZoneForDistance(GetDistanceFromStart(worldX));
        return NextFloat(zone.fuelIntervalMin, zone.fuelIntervalMax);
    }

    private float GetNextCoinInterval(float worldX)
    {
        DifficultyZone zone = GetZoneForDistance(GetDistanceFromStart(worldX));
        return NextFloat(zone.coinIntervalMin, zone.coinIntervalMax);
    }

    private float GetNextCoinSpawnX(float currentCoinX)
    {
        if (_tutorialCoinClustersSpawned == 1)
        {
            _tutorialCoinClustersSpawned++;
            return _startPlayerX + secondCoinSpawnDistance;
        }

        if (_tutorialCoinClustersSpawned == 0)
        {
            _tutorialCoinClustersSpawned++;
            return _startPlayerX + firstCoinSpawnDistance;
        }

        return currentCoinX + GetNextCoinInterval(currentCoinX);
    }

    private void InitializeSpawnState()
    {
        if (player == null)
        {
            return;
        }

        _startPlayerX = player.position.x;
        _nextFuelX = _startPlayerX + Mathf.Max(minimumFuelSpawnDistance, GetNextFuelInterval(_startPlayerX));
        _nextCoinX = _startPlayerX + firstCoinSpawnDistance;
        _tutorialCoinClustersSpawned = 1;
        _isInitialized = true;
    }

    private float GetDistanceFromStart(float worldX)
    {
        return Mathf.Max(0f, worldX - _startPlayerX);
    }

    private DifficultyZone GetZoneForDistance(float distance)
    {
        if (tutorialZone.Contains(distance))
        {
            return tutorialZone;
        }

        if (easyZone.Contains(distance))
        {
            return easyZone;
        }

        if (mediumZone.Contains(distance))
        {
            return mediumZone;
        }

        return hardZone;
    }

    private int NextInt(int minInclusive, int maxInclusive)
    {
        if (maxInclusive <= minInclusive)
        {
            return minInclusive;
        }

        return _random.Next(minInclusive, maxInclusive + 1);
    }

    private float NextFloat(float minInclusive, float maxInclusive)
    {
        if (maxInclusive <= minInclusive)
        {
            return minInclusive;
        }

        double normalized = _random.NextDouble();
        return minInclusive + (float)(normalized * (maxInclusive - minInclusive));
    }

    private float GetGroundY(float x)
    {
        Vector2 origin = new Vector2(x, raycastStartHeight);
        float distance = raycastStartHeight * 2f;

        RaycastHit2D hit = terrainLayer.value == 0
            ? Physics2D.Raycast(origin, Vector2.down, distance)
            : Physics2D.Raycast(origin, Vector2.down, distance, terrainLayer);

        if (hit.collider != null)
        {
            return hit.point.y;
        }

        Debug.LogWarning($"PickupSpawnerRuntime: Khong tim thay terrain tai X={x:F1}, dung Y player.");
        return player != null ? player.position.y : 0f;
    }
}
