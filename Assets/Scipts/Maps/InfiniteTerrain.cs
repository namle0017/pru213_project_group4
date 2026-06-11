using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class InfiniteTerrain : MonoBehaviour
{
    [Header("References")]
    public GameObject chunkPrefab;
    public Transform player;

    [Header("Chunk Shape")]
    [Range(4, 60)]
    public int pointsPerChunk    = 20;
    public float xStep           = 2f;
    public float yMultiplier     = 2f;
    public float noiseStep       = 0.4f;
    public float bottomDepth     = 10f;
    [Range(0f, 1f)]
    public float curveSmoothness = 0.5f;

    [Header("Chunk Management")]
    public int chunksAhead       = 3;
    public int chunksBehind      = 2;
    public float playerStartOffset = 10f;

    [Header("Player Drop")]
    public float playerDropHeight = 5f;
    [Tooltip("Dịch điểm spawn của xe sang phải (m)")]
    public float playerSpawnShiftX = 80f;

    // ── internal ──────────────────────────────────────────────────────
    private float             _chunkWidth;
    private float             _noiseOffset;
    private float             _nextChunkStartX;
    private float             _terrainStartX;
    private Queue<GameObject> _chunks = new Queue<GameObject>();

    // ─────────────────────────────────────────────────────────────────

    private void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else { Debug.LogError("InfiniteTerrain: Khong tim thay tag 'Player'."); return; }
        }

        if (chunkPrefab == null)
        {
            Debug.LogError("InfiniteTerrain: Chua gan chunkPrefab.");
            return;
        }

        _chunkWidth      = (pointsPerChunk - 1) * xStep;
        _noiseOffset     = 0f;
        _nextChunkStartX = player.position.x - playerStartOffset - chunksBehind * _chunkWidth;
        _terrainStartX   = _nextChunkStartX;

        int needed = chunksBehind + 1 + chunksAhead;
        for (int i = 0; i < needed; i++)
            SpawnChunk();

        StartCoroutine(DropAfterColliderReady());
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }

    // ── Coroutine drop xe ─────────────────────────────────────────────

    private IEnumerator DropAfterColliderReady()
    {
        var rbs = player.GetComponentsInChildren<Rigidbody2D>();
        foreach (var rb in rbs)
        {
            rb.simulated       = false;
            rb.linearVelocity  = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Chờ đủ frame để SpriteShape build collider xong
        yield return null;
        yield return null;
        yield return null;

        float targetX = _terrainStartX + playerStartOffset + playerSpawnShiftX;
        float groundY = GetGroundYAtX(targetX);
        float spawnY  = groundY + playerDropHeight;

        player.position = new Vector3(targetX, spawnY, player.position.z);

        // Bật lại physics → xe rơi xuống tự nhiên
        foreach (var rb in rbs)
        {
            rb.linearVelocity  = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated       = true;
        }

        // Reset điểm xuất phát để GameSession tính distance từ đây
        if (GameSession.Instance != null)
            GameSession.Instance.ResetStartPosition();
    }

    // ── Update ────────────────────────────────────────────────────────

    private void Update()
    {
        if (player == null || chunkPrefab == null) return;

        float frontEdge = player.position.x + chunksAhead * _chunkWidth;
        while (_nextChunkStartX < frontEdge)
            SpawnChunk();

        float destroyBefore = player.position.x - chunksBehind * _chunkWidth;
        while (_chunks.Count > 0)
        {
            var oldest = _chunks.Peek();
            if (oldest == null) { _chunks.Dequeue(); continue; }

            float chunkEndX = oldest.transform.position.x + _chunkWidth;
            if (chunkEndX < destroyBefore)
            {
                _chunks.Dequeue();
                Destroy(oldest);
            }
            else break;
        }
    }

    // ── Spawn chunk ───────────────────────────────────────────────────

    private void SpawnChunk()
    {
        float startX = _nextChunkStartX;

        // Spawn tại gốc tọa độ (0,0), spline dùng world position trực tiếp
        var go = Instantiate(chunkPrefab,
                             Vector3.zero,
                             Quaternion.identity,
                             transform);
        go.name = $"Chunk_{_chunks.Count + 1}";

        var ssc = go.GetComponent<SpriteShapeController>();
        if (ssc == null)
        {
            Debug.LogError("InfiniteTerrain: chunkPrefab thieu SpriteShapeController.");
            Destroy(go);
            return;
        }

        BuildSpline(ssc, startX, _noiseOffset);
        _chunks.Enqueue(go);

        _noiseOffset     += (pointsPerChunk - 1) * noiseStep;
        _nextChunkStartX += _chunkWidth;

        Debug.Log($"InfiniteTerrain: Spawned {go.name} at X={startX:F1}, noiseOff={_noiseOffset:F2}");
    }

    // ── Build spline ──────────────────────────────────────────────────

    private void BuildSpline(SpriteShapeController ssc, float startX, float noiseOff)
    {
        var spline = ssc.spline;
        spline.Clear();

        // Terrain points — đảm bảo không có 2 điểm trùng nhau
        for (int i = 0; i < pointsPerChunk; i++)
        {
            float nx  = noiseOff + i * noiseStep;
            float y   = Mathf.PerlinNoise(0f, nx) * yMultiplier;

            // Làm tròn để tránh floating point quá gần nhau gây tessellation error
            float x   = Mathf.Round((startX + i * xStep) * 1000f) / 1000f;
            y         = Mathf.Round(y * 1000f) / 1000f;

            spline.InsertPointAt(i, new Vector3(x, y, 0f));

            if (i != 0 && i != pointsPerChunk - 1)
                spline.SetTangentMode(i, ShapeTangentMode.Continuous);

            spline.SetLeftTangent (i, Vector3.left  * xStep * curveSmoothness);
            spline.SetRightTangent(i, Vector3.right * xStep * curveSmoothness);
        }

        // 2 điểm đáy khép kín — đặt thấp hơn để không overlap với terrain points
        float lastX  = startX + (pointsPerChunk - 1) * xStep;
        float floorY = -Mathf.Abs(bottomDepth); // luôn âm

        spline.InsertPointAt(pointsPerChunk,
            new Vector3(lastX,  floorY, 0f));
        spline.InsertPointAt(pointsPerChunk + 1,
            new Vector3(startX, floorY, 0f));
    }

    // ── Raycast ───────────────────────────────────────────────────────

    private float GetGroundYAtX(float x)
    {
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(x, 50f), Vector2.down, 100f);
        if (hit.collider != null)
            return hit.point.y;

        return Mathf.PerlinNoise(0f, 0f) * yMultiplier;
    }
}
