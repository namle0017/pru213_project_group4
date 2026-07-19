using System.Collections;
using TMPro;
using UnityEngine;

public class AirFlipDetector : MonoBehaviour
{
    private GameSession _gameSession;
    private Transform _playerTransform;
    private WheelDustController[] _wheels;

    [Header("Air Time Settings")]
    [Tooltip("Thời gian ở trên không tối thiểu để được nhận thưởng (giây)")]
    [SerializeField] private float _minAirTimeForBonus = 1.0f;
    [Tooltip("Tiền thưởng cho mỗi giây ở trên không")]
    [SerializeField] private int _coinsPerAirSecond = 600;

    [Header("Flip Settings")]
    [Tooltip("Tiền thưởng cho mỗi lần nhào lộn thành công (360 độ)")]
    [SerializeField] private int _flipBonusCoins = 500;

    private float _lastZRotation;
    private float _accumulatedAirRotation;
    private float _airTime;
    private bool _wasGrounded = true;
    private bool _hasTouchedGroundOnce = false;

    private void Start()
    {
        _gameSession = GameSession.Instance;
        if (_gameSession == null)
        {
            _gameSession = FindAnyObjectByType<GameSession>(FindObjectsInactive.Exclude);
        }
    }

    private void Update()
    {
        if (_gameSession == null)
        {
            _gameSession = GameSession.Instance;
            if (_gameSession == null) return;
        }

        if (_gameSession.IsGameOver) return;

        // Cập nhật tham chiếu tới Player hiện tại nếu đổi xe hoặc mới spawn
        if (_playerTransform == null || _playerTransform != _gameSession.Player)
        {
            _playerTransform = _gameSession.Player;
            if (_playerTransform != null)
            {
                _wheels = _playerTransform.GetComponentsInChildren<WheelDustController>(true);
                _lastZRotation = _playerTransform.eulerAngles.z;
                _accumulatedAirRotation = 0f;
                _airTime = 0f;
                _wasGrounded = true;
                _hasTouchedGroundOnce = false;
            }
        }

        if (_playerTransform == null) return;

        // Kiểm tra xem xe có đang chạm đất không (chỉ cần ít nhất 1 bánh chạm đất)
        bool isGrounded = false;
        if (_wheels != null && _wheels.Length > 0)
        {
            foreach (var wheel in _wheels)
            {
                if (wheel != null && wheel.IsTouchingGround)
                {
                    isGrounded = true;
                    break;
                }
            }
        }

        // Đánh dấu đã chạm đất lần đầu tiên kể từ khi bắt đầu/spawn
        if (isGrounded && !_hasTouchedGroundOnce)
        {
            _hasTouchedGroundOnce = true;
        }

        if (!isGrounded)
        {
            // Nếu chưa từng chạm đất lần nào (đang rơi tự do lúc vào game), bỏ qua tính điểm
            if (!_hasTouchedGroundOnce)
            {
                _lastZRotation = _playerTransform.eulerAngles.z;
                _wasGrounded = isGrounded;
                return;
            }

            // --- ĐANG Ở TRÊN KHÔNG ---
            _airTime += Time.deltaTime;

            // Tính toán góc xoay Z lũy kế
            float currentZ = _playerTransform.eulerAngles.z;
            float deltaAngle = Mathf.DeltaAngle(_lastZRotation, currentZ);
            _accumulatedAirRotation += deltaAngle;
            _lastZRotation = currentZ;

            // Kiểm tra xem có xoay đủ 360 độ hay không
            if (_accumulatedAirRotation >= 360f)
            {
                TriggerFlipBonus("BACKFLIP!");
                _accumulatedAirRotation -= 360f;
            }
            else if (_accumulatedAirRotation <= -360f)
            {
                TriggerFlipBonus("FRONTFLIP!");
                _accumulatedAirRotation += 360f;
            }
        }
        else
        {
            // --- ĐANG CHẠM ĐẤT ---
            // Nếu vừa mới tiếp đất từ trên không
            if (!_wasGrounded)
            {
                if (_airTime >= _minAirTimeForBonus)
                {
                    TriggerAirTimeBonus(_airTime);
                }
            }

            // Reset các giá trị khi chạm đất
            _airTime = 0f;
            _accumulatedAirRotation = 0f;
            _lastZRotation = _playerTransform.eulerAngles.z;
        }

        _wasGrounded = isGrounded;
    }

    private void TriggerFlipBonus(string flipType)
    {
        _gameSession.AddCoin(_flipBonusCoins);
        AudioService.PlayRewardPopup();
        ShowBonusPopup($"{flipType}\n+{_flipBonusCoins} Coins");
        Debug.Log($"AirFlipDetector: {flipType} bonus triggered! +{_flipBonusCoins} Coins");
    }

    private void TriggerAirTimeBonus(float airTime)
    {
        // Tính toán tiền dựa trên số giây trên không (làm tròn)
        int bonusAmount = Mathf.RoundToInt(airTime * _coinsPerAirSecond);
        _gameSession.AddCoin(bonusAmount);
        AudioService.PlayRewardPopup();
        ShowBonusPopup($"AIR TIME: {airTime:F1}s\n+{bonusAmount} Coins");
        Debug.Log($"AirFlipDetector: Air Time bonus triggered for {airTime:F1}s! +{bonusAmount} Coins");
    }

    private void ShowBonusPopup(string message)
    {
        Canvas canvas = FindAnyObjectByType<Canvas>(FindObjectsInactive.Exclude);
        if (canvas == null) return;

        // Tạo Text bay lên không trung mượt mà
        GameObject textObj = new GameObject("AirBonusText", typeof(RectTransform), typeof(CanvasGroup), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(canvas.transform, false);

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.65f); // Hiển thị ở nửa trên màn hình chơi
        rect.anchorMax = new Vector2(0.5f, 0.65f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(600f, 150f);

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = message;
        tmp.fontSize = 42f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.85f, 0f); // Màu vàng cam sang xịn mịn
        tmp.outlineColor = Color.black;
        tmp.outlineWidth = 0.25f;

        // Copy font hiện có của scene để đồng bộ mỹ thuật
        TextMeshProUGUI HUDTemplate = FindAnyObjectByType<TextMeshProUGUI>(FindObjectsInactive.Exclude);
        if (HUDTemplate != null)
        {
            tmp.font = HUDTemplate.font;
            tmp.fontSharedMaterial = HUDTemplate.fontSharedMaterial;
        }

        StartCoroutine(AnimateBonusPopup(textObj, rect, textObj.GetComponent<CanvasGroup>()));
    }

    private IEnumerator AnimateBonusPopup(GameObject obj, RectTransform rect, CanvasGroup group)
    {
        float duration = 1.8f;
        float elapsed = 0f;
        Vector2 startPos = rect.anchoredPosition;
        Vector2 targetPos = startPos + new Vector2(0f, 120f); // Bay lên 120px

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

            // Pop scale animation: phóng to lúc mới xuất hiện rồi thu nhỏ về bình thường
            float scale = 1f;
            if (t < 0.15f)
            {
                scale = Mathf.Lerp(0.3f, 1.4f, t / 0.15f);
            }
            else if (t < 0.3f)
            {
                scale = Mathf.Lerp(1.4f, 1.0f, (t - 0.15f) / 0.15f);
            }
            rect.localScale = new Vector3(scale, scale, 1f);

            // Mờ dần về cuối hiệu ứng
            if (t > 0.6f)
            {
                group.alpha = Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
            }

            yield return null;
        }

        Destroy(obj);
    }
}
