using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapCardUI : MonoBehaviour
{
    [Header("Map Data")]
    [SerializeField] private string mapId;
    [SerializeField] private string sceneName;
    [SerializeField] private int cost;

    [Header("UI References")]
    [SerializeField] private GameObject cardRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private GameObject priceIconObject;
    [SerializeField] private Button actionButton;
    [SerializeField] private Image actionButtonImage;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private Image previewPlaceholder;

    [Header("Button Sprites")]
    [SerializeField] private Sprite playSprite;
    [SerializeField] private Sprite buySprite;

    private MonoBehaviour controller;

    public string MapId => mapId;
    public string SceneName => sceneName;
    public int Cost => cost;
    public GameObject CardRoot => cardRoot != null ? cardRoot : gameObject;

    public void Setup(MonoBehaviour owner, string newMapId, string newSceneName, int newCost, string displayName)
    {
        controller = owner;
        mapId = newMapId;
        sceneName = newSceneName;
        cost = newCost;

        if (titleText != null)
        {
            titleText.text = displayName;
        }
    }

    public void RefreshUI()
    {
        bool isUnlocked = SaveSystem.IsMapUnlocked(mapId);
        bool isFreeMap = cost <= 0;
        bool shouldShowPrice = !isUnlocked || isFreeMap || isUnlocked;

        if (priceText != null)
        {
            priceText.gameObject.SetActive(shouldShowPrice);

            if (isUnlocked)
            {
                if (isFreeMap)
                {
                    priceText.text = "FREE";
                }
                else
                {
                    priceText.text = "AVAILABLE";
                }
            }
            else
            {
                priceText.text = cost.ToString();
            }
        }

        if (priceIconObject != null)
        {
            priceIconObject.SetActive(!isUnlocked && !isFreeMap);
        }

        if (actionButtonImage != null)
        {
            actionButtonImage.sprite = isUnlocked ? playSprite : buySprite;
            actionButtonImage.preserveAspect = true;
        }

        if (actionButtonText != null)
        {
            actionButtonText.text = isUnlocked ? "PLAY" : "BUY";
        }
    }

    public void OnActionButtonPressed()
    {
        if (controller == null)
        {
            Debug.LogWarning("MapCardUI: Chua gan MapSelectController.");
            return;
        }

        if (controller is MapSelectController mapSelectController)
        {
            mapSelectController.HandleMapCardAction(this);
            return;
        }

        if (controller is MapSelect2Controller mapSelect2Controller)
        {
            mapSelect2Controller.HandleMapCardAction(this);
            return;
        }

        if (controller is MapSelect3Controller mapSelect3Controller)
        {
            mapSelect3Controller.HandleMapCardAction(this);
            return;
        }

        Debug.LogWarning("MapCardUI: Controller khong ho tro HandleMapCardAction.");
    }
}
