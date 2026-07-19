using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class SoundToggleButtonUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private RectTransform coinCounterRect;

    [Header("Sprites")]
    [SerializeField] private Sprite speakerOnSprite;
    [SerializeField] private Sprite speakerOffSprite;

    [Header("Layout")]
    [SerializeField] private bool autoAlignUnderCoinCounter;
    [SerializeField] private Vector2 buttonSize = new Vector2(80f, 80f);
    [SerializeField] private float spacingBelowCoinCounter = 16f;

    private Button button;
    private RectTransform rectTransform;

    private void Awake()
    {
        CacheReferences();
        BindButton();
        UpdateIcon();
        ApplyOptionalLayout();
    }

    private void OnEnable()
    {
        CacheReferences();
        BindButton();
        UpdateIcon();
        ApplyOptionalLayout();
    }

    private void OnValidate()
    {
        CacheReferences();
        UpdateIcon();
        ApplyOptionalLayout();
    }

    public void HandleToggleClicked()
    {
        AudioSettingsController.ToggleMute();
        UpdateIcon();
    }

    public void RefreshVisual()
    {
        UpdateIcon();
        ApplyOptionalLayout();
    }

    private void ApplyOptionalLayout()
    {
        if (!autoAlignUnderCoinCounter)
        {
            return;
        }

        AlignUnderCoinCounter();
    }

    private void CacheReferences()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }

        if (coinCounterRect == null)
        {
            GameObject coinCounterObject = GameObject.Find("CoinCounter");
            if (coinCounterObject != null)
            {
                coinCounterRect = coinCounterObject.GetComponent<RectTransform>();
            }
        }
    }

    private void BindButton()
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(HandleToggleClicked);
        button.onClick.AddListener(HandleToggleClicked);
    }

    private void UpdateIcon()
    {
        if (iconImage == null)
        {
            return;
        }

        bool muted = AudioSettingsController.IsMuted();
        Sprite targetSprite = muted ? speakerOffSprite : speakerOnSprite;

        if (targetSprite != null)
        {
            iconImage.sprite = targetSprite;
            iconImage.preserveAspect = true;
            iconImage.color = Color.white;
            return;
        }

        iconImage.color = muted
            ? new Color(0.95f, 0.35f, 0.35f, 1f)
            : Color.white;
    }

    private void AlignUnderCoinCounter()
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.sizeDelta = buttonSize;

        if (coinCounterRect == null)
        {
            rectTransform.anchoredPosition = new Vector2(340f, -176f);
            return;
        }

        float coinCounterRightEdge = coinCounterRect.anchoredPosition.x + coinCounterRect.sizeDelta.x;
        float buttonX = coinCounterRightEdge - buttonSize.x;
        float buttonY = coinCounterRect.anchoredPosition.y - coinCounterRect.sizeDelta.y - spacingBelowCoinCounter;
        rectTransform.anchoredPosition = new Vector2(buttonX, buttonY);
    }
}
