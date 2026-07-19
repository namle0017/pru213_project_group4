using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class InfiniteTerrain : MonoBehaviour
{
    private sealed class ChunkInfo
    {
        public GameObject chunkObject;
        public float startX;
        public float endX;
    }

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
    private Queue<ChunkInfo>   _chunks = new Queue<ChunkInfo>();
    private bool              _isInitialized;
    private bool              _hasStarted;
    private Coroutine         _dropCoroutine;

    // ─────────────────────────────────────────────────────────────────

    private void Start()
    {
        _hasStarted = true;
        TryInitializeTerrain();
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;

        if (_hasStarted)
        {
            TryInitializeTerrain();
        }
    }

    // ── Coroutine drop xe ─────────────────────────────────────────────

    private IEnumerator DropAfterColliderReady()
    {
        if (player == null)
        {
            yield break;
        }

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
        if (chunkPrefab == null)
        {
            return;
        }

        EnsurePlayerReference();

        if (!_isInitialized)
        {
            TryInitializeTerrain();
            return;
        }

        if (player == null)
        {
            return;
        }

        float frontEdge = player.position.x + chunksAhead * _chunkWidth;

        // ponytail: cap chunk/frame. Cursor luon tien (xem SpawnChunk) nen khong the
        // freeze, nhung cap chan spawn dot-bien khi player teleport/di rat nhanh.
        // Neu can nhieu hon, tang chunksAhead.
        int spawnBudget = chunksBehind + 1 + chunksAhead + 4;
        while (_nextChunkStartX < frontEdge && spawnBudget-- > 0)
            SpawnChunk();

        float destroyBefore = player.position.x - chunksBehind * _chunkWidth;
        while (_chunks.Count > 0)
        {
            var oldest = _chunks.Peek();
            if (oldest == null || oldest.chunkObject == null)
            {
                _chunks.Dequeue();
                continue;
            }

            if (oldest.endX < destroyBefore)
            {
                _chunks.Dequeue();
                Destroy(oldest.chunkObject);
            }
            else break;
        }
    }

    // ── Spawn chunk ───────────────────────────────────────────────────

    private void SpawnChunk()
    {
        float startX          = _nextChunkStartX;
        float chunkNoiseOffset = _noiseOffset;

        // ── NGUYEN NHAN COC LOI CU ──────────────────────────────────────
        // Truoc day cursor (_nextChunkStartX) chi tang khi build thanh cong.
        // 1 chunk build loi (SSC null / SpriteShape exception) -> cursor dung ->
        // while-loop trong Update khong tien -> terrain ngung moc -> map huu han.
        // Fix: TANG CURSOR TRUOC. Chunk loi chi bi skip (toi da 1 gap), khong bao
        // gio lam chet ca he. Log ben duoi chi dich danh map/chunk neu con loi.
        _noiseOffset     += (pointsPerChunk - 1) * noiseStep;
        _nextChunkStartX += _chunkWidth;

        // Spawn tại gốc tọa độ (0,0), spline dùng world position trực tiếp
        var go = Instantiate(chunkPrefab,
                             Vector3.zero,
                             Quaternion.identity,
                             transform);
        go.name = $"Chunk_{_chunks.Count + 1}";

        var ssc = go.GetComponent<SpriteShapeController>();
        if (ssc == null)
        {
            Debug.LogError($"InfiniteTerrain[{gameObject.scene.name}]: chunkPrefab '{chunkPrefab.name}' thieu SpriteShapeController tren root. Skip chunk tai X={startX:F1}.");
            Destroy(go);
            return;
        }

        try
        {
            BuildSpline(ssc, startX, chunkNoiseOffset);

            // ── NGUYEN NHAN COC LOI: FRUSTUM CULLING ────────────────────────
            // Chunk instantiate tai goc (0,0) nhung BuildSpline dat spline o world-X xa
            // (vd 300..338). SpriteShapeController.InitBounds() tinh culling AABB =
            // localBounds * m_BoundsScale (default 2). Bounds nho khong phu vi tri that
            // -> Unity frustum-cull chunk -> OnWillRenderObject KHONG chay -> BakeMesh
            // KHONG chay -> mesh khong doi ra vi tri that -> collider bake tu mesh cu ->
            // KHONG co dat cho xe -> xe roi + raycast "khong tim thay terrain".
            // GROUND chay duoc chi vi prefab set BoundsScale=256; MARS vi design spline
            // dai 860m. Ep boundsScale lon tren MOI chunk de khong bao gio bi cull.
            ssc.boundsScale = 10000f;

            // Bake ngay lap tuc (khong doi frame render, vi neu bi cull se khong bao gio
            // toi): mesh cap nhat bounds that + collider khop ngay -> khong roi 1 frame.
            ssc.BakeMesh().Complete();
            ssc.BakeCollider();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"InfiniteTerrain[{gameObject.scene.name}]: dung chunk X={startX:F1} noiseOff={chunkNoiseOffset:F2} (prefab '{chunkPrefab.name}'): {ex}. Skip chunk.");
            Destroy(go);
            return;
        }

        _chunks.Enqueue(new ChunkInfo
        {
            chunkObject = go,
            startX = startX,
            endX = startX + _chunkWidth
        });
    }

    private void TryInitializeTerrain()
    {
        if (_isInitialized)
        {
            return;
        }

        EnsurePlayerReference();

        if (player == null)
        {
            Debug.LogWarning("InfiniteTerrain: Chua tim thay player hop le de khoi tao terrain.");
            return;
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
        {
            SpawnChunk();
        }

        _isInitialized = true;

        if (_dropCoroutine != null)
        {
            StopCoroutine(_dropCoroutine);
        }

        _dropCoroutine = StartCoroutine(DropAfterColliderReady());

        Debug.Log("InfiniteTerrain: Initialized with player " + player.name + " at X=" + player.position.x.ToString("F1"));
    }

    private void EnsurePlayerReference()
    {
        if (player != null && player.gameObject != null && player.gameObject.activeInHierarchy)
        {
            return;
        }

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            player = taggedPlayer.transform;
        }
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
