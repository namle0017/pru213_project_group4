using UnityEngine;
using UnityEngine.EventSystems;

public class PedalButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Pedal Configuration")]
    [Tooltip("Tích chọn nếu đây là chân ga (Gas), bỏ tích nếu là chân phanh (Brake/Reverse)")]
    [SerializeField] private bool _isGasPedal;

    [Header("Sprite Settings")]
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _pressedSprite;

    private UnityEngine.UI.Image _buttonImage;
    private bool _isPressed = false;

    public bool IsGasPedal => _isGasPedal;
    public bool IsPressed => _isPressed;

    private void Start()
    {
        _buttonImage = GetComponent<UnityEngine.UI.Image>();
        if (_buttonImage != null && _normalSprite != null)
        {
            _buttonImage.sprite = _normalSprite;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
    }

    private void OnDisable()
    {
        // Tránh bị kẹt trạng thái nhấn khi nút bị ẩn đi đột ngột
        _isPressed = false;
    }

    private void Update()
    {
        // Kiểm tra xem phím tương ứng trên bàn phím có được nhấn hay không
        bool isKeyboardPressed = false;
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (_isGasPedal)
            {
                isKeyboardPressed = UnityEngine.InputSystem.Keyboard.current.rightArrowKey.isPressed 
                                    || UnityEngine.InputSystem.Keyboard.current.dKey.isPressed;
            }
            else
            {
                isKeyboardPressed = UnityEngine.InputSystem.Keyboard.current.leftArrowKey.isPressed 
                                    || UnityEngine.InputSystem.Keyboard.current.aKey.isPressed;
            }
        }

        // Ưu tiên hiển thị nếu nhấn bằng chuột hoặc bàn phím
        bool shouldShowPressed = _isPressed || isKeyboardPressed;

        if (_buttonImage != null)
        {
            Sprite targetSprite = shouldShowPressed ? _pressedSprite : _normalSprite;
            if (targetSprite != null && _buttonImage.sprite != targetSprite)
            {
                _buttonImage.sprite = targetSprite;
            }
        }
    }
}
