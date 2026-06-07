using UnityEngine;

/// <summary>
/// Spawn Coin và Fuel phía trước player, bám sát mặt terrain.
///
/// FUEL: khoảng cách tăng dần + dao động theo "làn sóng" để tạo cảm giác
///       lúc thở phào (fuel gần hơn), lúc căng thẳng (fuel xa hơn).
///       Ví dụ: 50m đầu ~3 bình, 100m tiếp theo ~2 bình.
///
/// COIN: spawn cụm 3-5 coin, khoảng cách giữa các cụm tăng dần.
/// </summary>
public class PickupSpawnerRuntime : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform của player (xe). Để trống nếu bật Auto Find.")]
    public Transform player;

    [Header("Prefabs")]
    public GameObject coinPrefab;
    public GameObject fuelPrefab;

    [Header("Spawn Ahead")]
    [Tooltip("Spawn cách player bao xa về phía trước")]
    public float spawnAheadDistance = 20f;

    // ── FUEL ──────────────────────────────────────────────────────────
    [Header("Fuel - Khoang cach tang dan")]
    [Tooltip("Khoảng cách bình xăng lúc bắt đầu (m)")]
    public float fuelIntervalStart = 15f;

    [Tooltip("Khoảng cách bình xăng tối đa (m)")]
    public float fuelIntervalMax = 60f;

    [Tooltip("Tăng thêm bao nhiêu mét sau mỗi lần spawn fuel")]
    public float fuelIntervalGrowth = 3f;

    [Header("Fuel - Dao dong song (tension / relief)")]
    [Tooltip("Biên độ dao động khoảng cách (m). VD: 8 = lúc +8m lúc -8m so với base")]
    public float fuelWaveAmplitude = 8f;

    [Tooltip("Cứ sau bao nhiêu bình xăng thì hoàn thành 1 chu kỳ dao động")]
    public float fuelWavePeriod = 6f;

    // ── COIN ──────────────────────────────────────────────────────────
    [Header("Coin - Khoang cach tang dan")]
    [Tooltip("Khoảng cách giữa 2 cụm coin lúc đầu (m)")]
    public float coinIntervalStart = 5f;

    [Tooltip("Khoảng cách giữa 2 cụm coin tối đa (m)")]
    public float coinIntervalMax = 25f;

    [Tooltip("Tăng thêm bao nhiêu mét sau mỗi cụm coin")]
    public float coinIntervalGrowth = 0.8f;

    [Header("Coin - Cum coin")]
    [Tooltip("Số coin tối thiểu mỗi cụm")]
    public int minCoinsPerCluster = 3;

    [Tooltip("Số coin tối đa mỗi cụm")]
    public int maxCoinsPerCluster = 5;

    [Tooltip("Khoảng cách giữa các coin trong cùng 1 cụm (m)")]
    public float coinClusterSpacing = 1f;

    // ── TERRAIN DETECTION ─────────────────────────────────────────────
    [Header("Terrain Detection")]
    [Tooltip("Raycast bắt đầu từ độ cao này (m)")]
    public float raycastStartHeight = 20f;

    [Tooltip("Layer của terrain. Để mặc định (Nothing) = raycast tất cả layer")]
    public LayerMask terrainLayer;

    [Tooltip("Pickup nổi lên trên mặt đất bao nhiêu (m)")]
    public float groundYOffset = 0.5f;

    // ── INTERNAL ──────────────────────────────────────────────────────
    private float _nextFuelX;
    private float _nextCoinX;
    private float _currentFuelInterval;
    private float _currentCoinInterval;
    private int   _fuelSpawnCount;   // đếm số bình đã spawn (dùng cho sóng)

    // ─────────────────────────────────────────────────────────────────

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
            else
                Debug.LogWarning("PickupSpawnerRuntime: Khong tim thay tag 'Player'.");
        }

        if (player == null) return;

        _currentFuelInterval = fuelIntervalStart;
        _currentCoinInterval = coinIntervalStart;
        _fuelSpawnCount      = 0;

        _nextFuelX = player.position.x + spawnAheadDistance;
        _nextCoinX = player.position.x + spawnAheadDistance * 0.5f; // coin xuất hiện sớm hơn
    }

    private void Update()
    {
        if (player == null) return;

        float frontX = player.position.x + spawnAheadDistance;

        // ── Fuel ──
        if (fuelPrefab != null && frontX >= _nextFuelX)
        {
            SpawnFuel(_nextFuelX);

            // Tăng base interval
            _currentFuelInterval = Mathf.Min(
                _currentFuelInterval + fuelIntervalGrowth,
                fuelIntervalMax
            );

            // Cộng thêm dao động hình sin → lúc gần lúc xa tự nhiên
            float wave = Mathf.Sin(_fuelSpawnCount * Mathf.PI * 2f / fuelWavePeriod)
                         * fuelWaveAmplitude;

            float nextInterval = Mathf.Max(fuelIntervalStart, _currentFuelInterval + wave);
            _nextFuelX += nextInterval;
            _fuelSpawnCount++;
        }

        // ── Coin ──
        if (coinPrefab != null && frontX >= _nextCoinX)
        {
            SpawnCoinCluster(_nextCoinX);

            _currentCoinInterval = Mathf.Min(
                _currentCoinInterval + coinIntervalGrowth,
                coinIntervalMax
            );
            _nextCoinX += _currentCoinInterval;
        }
    }

    // ── Spawn helpers ─────────────────────────────────────────────────

    private void SpawnFuel(float spawnX)
    {
        float groundY = GetGroundY(spawnX);
        Instantiate(fuelPrefab, new Vector3(spawnX, groundY + groundYOffset, 0f), Quaternion.identity);
    }

    private void SpawnCoinCluster(float startX)
    {
        int count = Random.Range(minCoinsPerCluster, maxCoinsPerCluster + 1);
        for (int i = 0; i < count; i++)
        {
            float cx = startX + i * coinClusterSpacing;
            float groundY = GetGroundY(cx);
            Instantiate(coinPrefab, new Vector3(cx, groundY + groundYOffset, 0f), Quaternion.identity);
        }
    }

    /// <summary>
    /// Raycast từ trên xuống tại X để lấy Y mặt terrain.
    /// Fallback về Y của player nếu không tìm thấy.
    /// </summary>
    private float GetGroundY(float x)
    {
        Vector2 origin = new Vector2(x, raycastStartHeight);
        float   dist   = raycastStartHeight * 2f;

        RaycastHit2D hit = (terrainLayer.value == 0)
            ? Physics2D.Raycast(origin, Vector2.down, dist)
            : Physics2D.Raycast(origin, Vector2.down, dist, terrainLayer);

        if (hit.collider != null)
            return hit.point.y;

        Debug.LogWarning($"PickupSpawnerRuntime: Khong tim thay terrain tai X={x:F1}, dung Y player.");
        return player != null ? player.position.y : 0f;
    }
}
