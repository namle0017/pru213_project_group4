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
