using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

[ExecuteAlways]
public class GaragePlaceholderController : MonoBehaviour
{
    private const string MenuSceneName = "Menu";

    [Header("Test Unlock")]
    [SerializeField] private bool unlockF1OnPlay;
    [SerializeField] private bool unlockMotorOnPlay;

    [Header("Preview Sprites")]
    [SerializeField] private Sprite basicCarPreviewSprite;
    [SerializeField] private Sprite f1CarPreviewSprite;
    [SerializeField] private Sprite motorPreviewSprite;
    [SerializeField] private Sprite lockSprite;
    [SerializeField] private Sprite lockedButtonSprite;
    [SerializeField] private Sprite selectButtonSprite;
    [SerializeField] private Sprite selectedButtonSprite;

    [Header("Labels")]
    [SerializeField] private string lockedText = "LOCKED";
    [SerializeField] private string lockedHintText = "SPIN TO UNLOCK";
    [SerializeField] private string selectText = "SELECT";
    [SerializeField] private string selectedText = "IN USE";

    [Header("Colors")]
    [SerializeField] private Color grayscaleColor = new Color(0.45f, 0.45f, 0.45f, 1f);
    [SerializeField] private Color lockPlateColor = new Color(0.12f, 0.12f, 0.12f, 0.92f);
    [SerializeField] private Color selectPlateColor = new Color(0.12f, 0.35f, 0.95f, 0.92f);
    [SerializeField] private Color selectedPlateColor = new Color(0.10f, 0.65f, 0.22f, 0.96f);
    [SerializeField] private Color chainColor = new Color(0.7f, 0.7f, 0.7f, 0.95f);

    private void OnEnable()
    {
        RefreshGarage();
    }

    private void Start()
    {
        ApplyTestUnlocks();
        RefreshGarage();
    }

    private void OnValidate()
    {
        RefreshGarage();
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        AudioService.PlayBackClose();
        SceneManager.LoadScene(MenuSceneName);
    }

    public void OnBasicCarClicked()
    {
        HandleVehicleClicked(VehicleIds.BasicCar, "Basic Car");
    }

    public void OnF1CarClicked()
    {
        HandleVehicleClicked(VehicleIds.F1Car, "F1 Car");
    }

    public void OnMotorClicked()
    {
        HandleVehicleClicked(VehicleIds.Motor, "Motor");
    }

    public void UnlockVehicleAndRefresh(string vehicleId)
    {
        if (string.IsNullOrWhiteSpace(vehicleId))
        {
            Debug.LogWarning("Garage: vehicleId rong, khong the unlock.");
            return;
        }

        SaveSystem.SaveVehicleUnlocked(vehicleId, true);
        Debug.Log("Garage: test unlock vehicle -> " + vehicleId);
        RefreshGarage();
    }

    [ContextMenu("Test/Unlock F1")]
    public void UnlockF1ForTest()
    {
        UnlockVehicleAndRefresh(VehicleIds.F1Car);
    }

    [ContextMenu("Test/Unlock Motor")]
    public void UnlockMotorForTest()
    {
        UnlockVehicleAndRefresh(VehicleIds.Motor);
    }

    [ContextMenu("Test/Lock F1")]
    public void LockF1ForTest()
    {
        LockVehicleAndRefresh(VehicleIds.F1Car);
    }

    [ContextMenu("Test/Lock Motor")]
    public void LockMotorForTest()
    {
        LockVehicleAndRefresh(VehicleIds.Motor);
    }

    [ContextMenu("Test/Refresh Garage")]
    public void RefreshGarageForTest()
    {
        RefreshGarage();
    }

    public void LockVehicleAndRefresh(string vehicleId)
    {
        if (string.IsNullOrWhiteSpace(vehicleId))
        {
            Debug.LogWarning("Garage: vehicleId rong, khong the lock.");
            return;
        }

        SaveSystem.SaveVehicleUnlocked(vehicleId, false);

        if (SaveSystem.LoadSelectedVehicle() == vehicleId)
        {
            SaveSystem.SaveSelectedVehicle(VehicleIds.BasicCar);
        }

        Debug.Log("Garage: test lock vehicle -> " + vehicleId);
        RefreshGarage();
    }

    private void HandleVehicleClicked(string vehicleId, string displayName)
    {
        if (!Application.isPlaying)
        {
            RefreshGarage();
            return;
        }

        if (!SaveSystem.IsVehicleUnlocked(vehicleId))
        {
            AudioService.PlayErrorNotEnoughCoin();
            Debug.Log("Garage: " + displayName + " is locked. Spin to unlock.");
            return;
        }

        SaveSystem.SaveSelectedVehicle(vehicleId);
        AudioService.PlayButtonClick();
        Debug.Log("Garage: selected " + displayName + " | vehicleId=" + vehicleId);
        RefreshGarage();
    }

    private void ApplyTestUnlocks()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (unlockF1OnPlay)
        {
            SaveSystem.SaveVehicleUnlocked(VehicleIds.F1Car, true);
            Debug.Log("Garage Test: unlocked F1 on play.");
        }

        if (unlockMotorOnPlay)
        {
            SaveSystem.SaveVehicleUnlocked(VehicleIds.Motor, true);
            Debug.Log("Garage Test: unlocked Motor on play.");
        }
    }

    private void RefreshGarage()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        HideLogo();
        EnsureGarageTitle();
        RefreshCoinText();
        ConfigureCard("GroundCard", VehicleIds.BasicCar, "BASIC CAR", basicCarPreviewSprite);
        ConfigureCard("F1_Car", VehicleIds.F1Car, "F1 CAR", f1CarPreviewSprite);
        ConfigureCard("Motor", VehicleIds.Motor, "MOTOR", motorPreviewSprite);
    }

    private void HideLogo()
    {
        GameObject logo = GameObject.Find("LogoImage");
        if (logo != null)
        {
            logo.SetActive(false);
        }
    }

    private void EnsureGarageTitle()
    {
        GameObject existing = GameObject.Find("GarageTitle");
        if (existing != null)
        {
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        GameObject titleObject = new GameObject("GarageTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObject.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = titleObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0f, -48f);
        rectTransform.sizeDelta = new Vector2(700f, 140f);

        TextMeshProUGUI titleText = titleObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI template = FindAnyObjectByType<TextMeshProUGUI>();
        if (template != null)
        {
            titleText.font = template.font;
            titleText.fontSharedMaterial = template.fontSharedMaterial;
        }

        titleText.text = "GARAGE";
        titleText.fontSize = 72f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        titleText.outlineColor = Color.black;
        titleText.outlineWidth = 0.25f;
    }

    private void RefreshCoinText()
    {
        GameObject coinObject = GameObject.Find("CoinText");
        if (coinObject == null)
        {
            return;
        }

        TextMeshProUGUI coinText = coinObject.GetComponent<TextMeshProUGUI>();
        if (coinText != null)
        {
            coinText.text = SaveSystem.LoadTotalCoins().ToString();
        }
    }

    private void ConfigureCard(string cardName, string vehicleId, string displayName, Sprite previewSprite)
    {
        GameObject card = GameObject.Find(cardName);
        if (card == null)
        {
            return;
        }

        TextMeshProUGUI titleText = GetTmpByPrefix(card.transform, "TitleText");
        TextMeshProUGUI statusText = GetTmpByPrefix(card.transform, "PriceText");
        Transform previewTransform = FindChildByPrefix(card.transform, "PreviewPlaceholder");
        Transform buttonTransform = FindChildByPrefix(card.transform, "ActionButton");
        Image previewImage = previewTransform != null ? previewTransform.GetComponent<Image>() : null;
        Button actionButton = buttonTransform != null ? buttonTransform.GetComponent<Button>() : null;
        Image actionButtonImage = buttonTransform != null ? buttonTransform.GetComponent<Image>() : null;

        bool isUnlocked = SaveSystem.IsVehicleUnlocked(vehicleId);
        bool isSelected = SaveSystem.LoadSelectedVehicle() == vehicleId;

        if (titleText != null)
        {
            titleText.text = displayName;
        }

        if (statusText != null)
        {
            statusText.gameObject.SetActive(!isUnlocked);
            if (!isUnlocked)
            {
                statusText.fontSize = 24f;
                statusText.enableAutoSizing = false;
                statusText.text = lockedText;
            }
        }

        if (previewImage != null)
        {
            if (previewSprite != null)
            {
                previewImage.sprite = previewSprite;
            }

            previewImage.preserveAspect = true;
            previewImage.color = isUnlocked ? Color.white : grayscaleColor;
        }

        if (actionButton != null)
        {
            actionButton.interactable = isUnlocked && !isSelected;

            if (Application.isPlaying)
            {
                actionButton.onClick.RemoveAllListeners();
                actionButton.onClick.AddListener(() => HandleVehicleClicked(vehicleId, displayName));
            }
        }

        if (actionButtonImage != null)
        {
            actionButtonImage.enabled = true;
            actionButtonImage.color = Color.white;
            if (!isUnlocked && lockedButtonSprite != null)
            {
                actionButtonImage.sprite = lockedButtonSprite;
                actionButtonImage.preserveAspect = true;
            }
            else if (isSelected && selectedButtonSprite != null)
            {
                actionButtonImage.sprite = selectedButtonSprite;
                actionButtonImage.preserveAspect = true;
            }
            else if (isUnlocked && selectButtonSprite != null)
            {
                actionButtonImage.sprite = selectButtonSprite;
                actionButtonImage.preserveAspect = true;
            }
        }

        HideOptionalVisual(card.transform, "SelectedHighlight");
        HideOptionalVisual(card.transform, "SelectedBadge");
        HideOptionalVisual(buttonTransform, "ActionStateText");
        HideOptionalVisual(buttonTransform, "ActionStateBackground");
        HideLegacyButtonText(buttonTransform);
        HideLegacyPriceIcon(card.transform);

        foreach (Image lockIconImage in GetImagesByPrefix(card.transform, "LockIconPlaceholder"))
        {
            lockIconImage.gameObject.SetActive(!isUnlocked);

            if (!isUnlocked && lockSprite != null)
            {
                lockIconImage.sprite = lockSprite;
                lockIconImage.color = Color.white;
            }
        }

        foreach (TextMeshProUGUI lockIconText in GetTmpChildrenByPrefix(card.transform, "LockIconText"))
        {
            lockIconText.gameObject.SetActive(!isUnlocked && lockSprite == null);
            lockIconText.text = "X";
        }

        foreach (Image leftChain in GetImagesByPrefix(card.transform, "LeftChainPlaceholder"))
        {
            leftChain.gameObject.SetActive(!isUnlocked);
            leftChain.color = chainColor;
        }

        foreach (Image rightChain in GetImagesByPrefix(card.transform, "RightChainPlaceholder"))
        {
            rightChain.gameObject.SetActive(!isUnlocked);
            rightChain.color = chainColor;
        }

        Debug.Log(
            $"Garage: {displayName} | unlocked={isUnlocked} | selected={isSelected} | buttonFound={(actionButton != null)} | previewFound={(previewImage != null)}");
    }

    private static TextMeshProUGUI GetTmpByPrefix(Transform parent, string childPrefix)
    {
        Transform child = FindChildByPrefix(parent, childPrefix);
        return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
    }

    private static Transform FindChildByPrefix(Transform parent, string childPrefix)
    {
        if (parent == null)
        {
            return null;
        }

        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child == parent)
            {
                continue;
            }

            if (child.name == childPrefix || child.name.StartsWith(childPrefix))
            {
                return child;
            }
        }

        return null;
    }

    private static List<Image> GetImagesByPrefix(Transform parent, string childPrefix)
    {
        List<Image> images = new List<Image>();
        if (parent == null)
        {
            return images;
        }

        foreach (Transform child in parent)
        {
            if (child.name == childPrefix || child.name.StartsWith(childPrefix))
            {
                Image image = child.GetComponent<Image>();
                if (image != null)
                {
                    images.Add(image);
                }
            }
        }

        return images;
    }

    private static List<TextMeshProUGUI> GetTmpChildrenByPrefix(Transform parent, string childPrefix)
    {
        List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
        if (parent == null)
        {
            return texts;
        }

        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child == parent)
            {
                continue;
            }

            if (child.name == childPrefix || child.name.StartsWith(childPrefix))
            {
                TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    texts.Add(text);
                }
            }
        }

        return texts;
    }

    private static void HideLegacyPriceIcon(Transform cardTransform)
    {
        Transform iconTransform = cardTransform.Find("icon");
        if (iconTransform != null)
        {
            iconTransform.gameObject.SetActive(false);
        }

        Transform iconAltTransform = cardTransform.Find("icon (1)");
        if (iconAltTransform != null)
        {
            iconAltTransform.gameObject.SetActive(false);
        }
    }

    private static void HideOptionalVisual(Transform parent, string childName)
    {
        if (parent == null)
        {
            return;
        }

        Transform child = parent.Find(childName);
        if (child != null)
        {
            child.gameObject.SetActive(false);
        }
    }

    private static void HideLegacyButtonText(Transform buttonTransform)
    {
        if (buttonTransform == null)
        {
            return;
        }

        Transform legacyButtonText = buttonTransform.Find("ButtonText");
        if (legacyButtonText != null)
        {
            legacyButtonText.gameObject.SetActive(false);
        }
    }

}
