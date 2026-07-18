using UnityEngine;
using UnityEngine.InputSystem;

public class VehicelControl : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _frontTireRB;
    [SerializeField] private Rigidbody2D _backTireRB;
    [SerializeField] private Rigidbody2D _carRb;
    [SerializeField] private float _speed = 150f;
    [SerializeField] private float _rotationSpeed = 300f;

    [Header("Effects")]
    [Tooltip("Hệ thống Particle tạo khói xe")]
    [SerializeField] private ParticleSystem _exhaustSmoke;

    [Header("Engine Audio Settings")]
    [Tooltip("Component Audio Source phát tiếng động cơ")]
    [SerializeField] private AudioSource _engineAudioSource;
    [Tooltip("Độ cao âm thanh tối thiểu (khi xe đứng yên)")]
    [SerializeField] private float _minPitch = 0.8f;
    [Tooltip("Độ cao âm thanh tối đa (khi xe chạy cực đại)")]
    [SerializeField] private float _maxPitch = 2.2f;
    [Tooltip("Tốc độ xe tương ứng với mức Pitch tối đa")]
    [SerializeField] private float _maxSpeedForPitch = 20f;

    [Header("Speed Multipliers")]
    [Tooltip("Tỷ lệ tốc độ khi đi lùi (nhỏ hơn 1 sẽ giúp lùi lại từ từ)")]
    [SerializeField] private float _reverseSpeedMultiplier = 0.5f;

    [Header("Deceleration Settings")]
    [Tooltip("Tốc độ hãm khi THẢ tất cả các nút (trôi tự nhiên)")]
    [SerializeField] private float _coastDecelRate = 3f;

    [Tooltip("Tốc độ hãm khi NHẤN phanh/đổi chiều đột ngột (phanh gấp)")]
    [SerializeField] private float _activeBrakeDecelRate = 12f;

    private float _moveInput;
    private bool _isCoasting;

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current == null) return;

        // Nhấn nút mũi tên phải (hoặc phím D) để tiến lên
        if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
        {
            _moveInput = 1f;
            _isCoasting = false;
        }
        // Nhấn nút mũi tên trái (hoặc phím A) để đi ngược lại (lùi)
        else if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
        {

            _moveInput = -1f;
            _isCoasting = false;
        }
        // Không nhấn phím nào -> Trôi tự nhiên
        else
        {
            _moveInput = 0f;
            _isCoasting = true;
        }
        // --- XỬ LÝ HIỆU ỨNG KHÓI ---
        if (_exhaustSmoke != null)
        {
            // Bật khói khi xe đang đạp ga tiến hoặc lùi (không phải đang trôi tự do)
            if (!_isCoasting)
            {
                if (!_exhaustSmoke.isPlaying)
                {
                    _exhaustSmoke.Play();
                }
            }
            // Tắt khói khi thả ga
            else
            {
                if (_exhaustSmoke.isPlaying)
                {
                    _exhaustSmoke.Stop();
                }
            }
        }

        // --- XỬ LÝ ÂM THANH ĐỘNG CƠ ---
        if (_engineAudioSource != null && _carRb != null)
        {
            // Lấy tốc độ thực tế của xe (độ dài vector vận tốc 2D)
            float currentSpeed = _carRb.linearVelocity.magnitude;

            // Quy đổi tốc độ từ khoảng 0 -> _maxSpeedForPitch sang tỉ lệ 0 -> 1
            float speedRatio = Mathf.Clamp01(currentSpeed / _maxSpeedForPitch);

            // Tính toán Pitch tương ứng từ _minPitch đến _maxPitch dựa trên tỉ lệ tốc độ
            float targetPitch = Mathf.Lerp(_minPitch, _maxPitch, speedRatio);

            // Cập nhật độ cao âm thanh cho Audio Source
            _engineAudioSource.pitch = targetPitch;
        }



    }


    private void FixedUpdate()
    {
        if (_isCoasting)
        {
            // Trôi tự do một đoạn ngắn rồi dừng lại: Giảm tốc độ mượt mà từ từ (Sử dụng _coastDecelRate)
            _frontTireRB.angularVelocity = Mathf.MoveTowards(_frontTireRB.angularVelocity, 0f, _speed * Time.fixedDeltaTime * _coastDecelRate);
            _backTireRB.angularVelocity = Mathf.MoveTowards(_backTireRB.angularVelocity, 0f, _speed * Time.fixedDeltaTime * _coastDecelRate);
            _carRb.angularVelocity = Mathf.MoveTowards(_carRb.angularVelocity, 0f, _rotationSpeed * Time.fixedDeltaTime * (_coastDecelRate * 0.5f));
            _carRb.linearVelocity = Vector2.MoveTowards(_carRb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * _coastDecelRate * 1.5f);
        }
        else
        {
            // Kiểm tra xem người chơi có đang "Phanh gấp để đổi chiều" hay không
            // Tiến lên mà nhấn Lùi (Left), hoặc Lùi lại mà nhấn Tiến (Right)
            bool isActiveBraking = (_moveInput < 0f && _carRb.linearVelocity.x > 0.1f) || (_moveInput > 0f && _carRb.linearVelocity.x < -0.1f);

            if (isActiveBraking)
            {
                // Phanh chủ động đổi chiều: Hãm tốc độ cực kỳ nhanh chóng (Sử dụng _activeBrakeDecelRate)
                _frontTireRB.angularVelocity = Mathf.MoveTowards(_frontTireRB.angularVelocity, 0f, _speed * Time.fixedDeltaTime * _activeBrakeDecelRate);
                _backTireRB.angularVelocity = Mathf.MoveTowards(_backTireRB.angularVelocity, 0f, _speed * Time.fixedDeltaTime * _activeBrakeDecelRate);
                _carRb.angularVelocity = Mathf.MoveTowards(_carRb.angularVelocity, 0f, _rotationSpeed * Time.fixedDeltaTime * (_activeBrakeDecelRate * 0.5f));
                _carRb.linearVelocity = Vector2.MoveTowards(_carRb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * _activeBrakeDecelRate * 1.5f);
            }
            else
            {
                // Di chuyển bình thường khi đã dừng hoặc đi đúng hướng
                float currentSpeed = _speed;

                // Nếu đi lùi (nhấn mũi tên trái) thì di chuyển lùi từ từ theo _reverseSpeedMultiplier
                if (_moveInput < 0f)
                {
                    currentSpeed *= _reverseSpeedMultiplier;
                }

                _frontTireRB.AddTorque(-_moveInput * currentSpeed * Time.fixedDeltaTime);
                _backTireRB.AddTorque(-_moveInput * currentSpeed * Time.fixedDeltaTime);
                _carRb.AddTorque(-_moveInput * _rotationSpeed * Time.fixedDeltaTime);
            }
        }
    }
}
