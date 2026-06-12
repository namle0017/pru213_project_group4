using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DailyRewardController : MonoBehaviour
{
    private const string DailyCurrentDayKey = "DailyCurrentDay";
    private const string DailyLastClaimDateKey = "DailyLastClaimDate";
    private const string DailyClaimedMaskKey = "DailyClaimedMask";
    private const string DateFormat = "yyyy-MM-dd";
    private const int DaysInCycle = 7;
    private const int FullCycleMask = 0b1111111;
    private const int DuplicateMotorCoins = 1000;

    private static readonly int[] CoinRewards = { 50, 100, 150, 200, 250, 300 };

    [Header("Buttons")]
    [SerializeField] private Button homeButton;
    [SerializeField] private Button claimButton;
    [SerializeField] private Button claimedButton;
    [SerializeField] private Button popupOkButton;

    [Header("Popup")]
    [SerializeField] private GameObject rewardPopupRoot;
    [SerializeField] private Image popupRewardIcon;
    [SerializeField] private TextMeshProUGUI popupTitleText;
    [SerializeField] private TextMeshProUGUI popupLine1Text;
    [SerializeField] private TextMeshProUGUI popupLine2Text;

    [Header("Reward Sprites")]
    [SerializeField] private Sprite coinRewardSprite;
    [SerializeField] private Sprite motorRewardSprite;

    [Header("Scene")]
    [SerializeField] private string menuSceneName = "Menu";

    private readonly Transform[] daySlots = new Transform[DaysInCycle];
    private readonly Image[] claimBadges = new Image[DaysInCycle];
    private readonly GameObject[] dayLabels = new GameObject[DaysInCycle];
    private readonly GameObject[] rewardIcons = new GameObject[DaysInCycle];
    private readonly GameObject[] rewardAmountTexts = new GameObject[DaysInCycle];

    private int currentDay;
    private int claimedMask;
    private string lastClaimDate;

    public int CurrentDayForDebug => currentDay;
    public int ClaimedMaskForDebug => claimedMask;
    public string LastClaimDateForDebug => lastClaimDate;
    public bool HasClaimedTodayForDebug => HasClaimedToday();

    private void Awake()
    {
        AutoWireReferences();
        RegisterButtonEvents();
        LoadDailyState();
        RefreshUI();
    }

    public void GoHome()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    public void ClaimTodayReward()
    {
        LoadDailyState();

        if (HasClaimedToday())
        {
            RefreshUI();
            return;
        }

        int claimedDay = Mathf.Clamp(currentDay, 1, DaysInCycle);
        ApplyReward(claimedDay);

        claimedMask |= GetDayMask(claimedDay);
        lastClaimDate = GetTodayString();

        currentDay = claimedDay < DaysInCycle ? claimedDay + 1 : 1;

        SaveDailyState();
        RefreshUI();
    }

    public void HideRewardPopup()
    {
        if (rewardPopupRoot != null)
        {
            rewardPopupRoot.SetActive(false);
        }
    }

    [ContextMenu("Daily/Test Simulate New Day")]
    public void DevSimulateNewDay()
    {
        PlayerPrefs.SetString(DailyLastClaimDateKey, string.Empty);
        PlayerPrefs.Save();
        LoadDailyState();
        RefreshUI();
        Debug.Log("DailyRewardController: Simulated a new day.");
    }

    [ContextMenu("Daily/Test Set Day 7 Available")]
    public void DevSetDay7Available()
    {
        DevSetDayAvailable(DaysInCycle);
    }

    public void DevSetDayAvailable(int day)
    {
        int safeDay = Mathf.Clamp(day, 1, DaysInCycle);
        PlayerPrefs.SetInt(DailyCurrentDayKey, safeDay);
        PlayerPrefs.SetString(DailyLastClaimDateKey, string.Empty);
        PlayerPrefs.Save();
        LoadDailyState();
        RefreshUI();
        Debug.Log("DailyRewardController: Day " + safeDay + " is available for testing.");
    }

    [ContextMenu("Daily/Test Reset Daily Reward")]
    public void DevResetDailyReward()
    {
        PlayerPrefs.DeleteKey(DailyCurrentDayKey);
        PlayerPrefs.DeleteKey(DailyLastClaimDateKey);
        PlayerPrefs.DeleteKey(DailyClaimedMaskKey);
        PlayerPrefs.Save();
        LoadDailyState();
        RefreshUI();
        Debug.Log("DailyRewardController: Daily reward data reset.");
    }

    public void DevLockMotor()
    {
        SaveSystem.SaveVehicleUnlocked(VehicleIds.Motor, false);
        Debug.Log("DailyRewardController: Motor locked for daily reward testing.");
    }

    public void DevUnlockMotor()
    {
        SaveSystem.SaveVehicleUnlocked(VehicleIds.Motor, true);
        Debug.Log("DailyRewardController: Motor unlocked for daily reward testing.");
    }

    public void DevAddCoins(int amount)
    {
        SaveSystem.AddCoins(amount);
        Debug.Log("DailyRewardController: Added " + amount + " coins for testing.");
    }

    public void DevRefresh()
    {
        LoadDailyState();
        RefreshUI();
    }

    private void AutoWireReferences()
    {
        homeButton ??= FindComponentByName<Button>("HomeButton");
        claimButton ??= FindComponentByName<Button>("ClaimButton");
        claimedButton ??= FindComponentByName<Button>("ClaimedButton");
        popupOkButton ??= FindComponentByName<Button>("PopupOkButton");

        rewardPopupRoot ??= FindGameObjectByName("RewardPopupRoot");
        popupRewardIcon ??= FindComponentByName<Image>("PopupRewardIcon");
        popupTitleText ??= FindComponentByName<TextMeshProUGUI>("PopupTitleText");
        popupLine1Text ??= FindComponentByName<TextMeshProUGUI>("PopupLine1Text");
        popupLine2Text ??= FindComponentByName<TextMeshProUGUI>("PopupLine2Text");

        for (int i = 0; i < DaysInCycle; i++)
        {
            Transform slot = FindTransformByName("DaySlot" + (i + 1));
            daySlots[i] = slot;

            if (slot == null)
            {
                continue;
            }

            Transform badgeTransform = slot.Find("ClaimBadge");
            if (badgeTransform != null)
            {
                claimBadges[i] = badgeTransform.GetComponent<Image>();
            }

            Transform dayLabelTransform = slot.Find("DayLabel");
            if (dayLabelTransform != null)
            {
                dayLabels[i] = dayLabelTransform.gameObject;
            }

            Transform rewardIconTransform = slot.Find("RewardIcon");
            if (rewardIconTransform != null)
            {
                rewardIcons[i] = rewardIconTransform.gameObject;
            }

            Transform rewardAmountTextTransform = slot.Find("RewardAmountText");
            if (rewardAmountTextTransform != null)
            {
                rewardAmountTexts[i] = rewardAmountTextTransform.gameObject;
            }

            Image rewardIcon = rewardIconTransform != null
                ? rewardIconTransform.GetComponent<Image>()
                : null;

            if (rewardIcon != null)
            {
                if (i == DaysInCycle - 1 && motorRewardSprite == null)
                {
                    motorRewardSprite = rewardIcon.sprite;
                }
                else if (i == 0 && coinRewardSprite == null)
                {
                    coinRewardSprite = rewardIcon.sprite;
                }
            }
        }
    }

    private void RegisterButtonEvents()
    {
        if (rewardPopupRoot != null)
        {
            rewardPopupRoot.SetActive(false);
        }

        if (homeButton != null)
        {
            homeButton.onClick.RemoveListener(GoHome);
            homeButton.onClick.AddListener(GoHome);
        }

        if (claimButton != null)
        {
            claimButton.onClick.RemoveListener(ClaimTodayReward);
            claimButton.onClick.AddListener(ClaimTodayReward);
        }

        if (popupOkButton != null)
        {
            popupOkButton.onClick.RemoveListener(HideRewardPopup);
            popupOkButton.onClick.AddListener(HideRewardPopup);
        }

        if (claimedButton != null)
        {
            claimedButton.interactable = false;
        }
    }

    private void LoadDailyState()
    {
        currentDay = Mathf.Clamp(PlayerPrefs.GetInt(DailyCurrentDayKey, 1), 1, DaysInCycle);
        claimedMask = PlayerPrefs.GetInt(DailyClaimedMaskKey, 0);
        lastClaimDate = PlayerPrefs.GetString(DailyLastClaimDateKey, string.Empty);

        if (currentDay == 1 && claimedMask == FullCycleMask && !HasClaimedToday())
        {
            claimedMask = 0;
            PlayerPrefs.SetInt(DailyClaimedMaskKey, claimedMask);
            PlayerPrefs.Save();
        }
    }

    private void SaveDailyState()
    {
        PlayerPrefs.SetInt(DailyCurrentDayKey, currentDay);
        PlayerPrefs.SetString(DailyLastClaimDateKey, lastClaimDate);
        PlayerPrefs.SetInt(DailyClaimedMaskKey, claimedMask);
        PlayerPrefs.Save();
    }

    private void RefreshUI()
    {
        AutoWireReferences();
        bool claimedToday = HasClaimedToday();

        if (claimButton != null)
        {
            claimButton.gameObject.SetActive(!claimedToday);
        }

        if (claimedButton != null)
        {
            claimedButton.gameObject.SetActive(claimedToday);
            claimedButton.interactable = false;
        }

        for (int i = 0; i < claimBadges.Length; i++)
        {
            bool isClaimed = (claimedMask & GetDayMask(i + 1)) != 0;
            SetDaySlotClaimedVisual(i, isClaimed);
        }
    }

    private void SetDaySlotClaimedVisual(int index, bool isClaimed)
    {
        if (claimBadges[index] != null)
        {
            claimBadges[index].gameObject.SetActive(isClaimed);
        }

        if (dayLabels[index] != null)
        {
            dayLabels[index].SetActive(!isClaimed);
        }

        if (rewardIcons[index] != null)
        {
            rewardIcons[index].SetActive(!isClaimed);
        }

        if (rewardAmountTexts[index] != null)
        {
            rewardAmountTexts[index].SetActive(!isClaimed);
        }
    }

    private void ApplyReward(int day)
    {
        if (day >= 1 && day <= 6)
        {
            int coins = CoinRewards[day - 1];
            SaveSystem.AddCoins(coins);
            ShowRewardPopup("REWARD", "+" + coins + " COINS", "See you tomorrow!", coinRewardSprite);
            return;
        }

        if (!SaveSystem.IsVehicleUnlocked(VehicleIds.Motor))
        {
            SaveSystem.SaveVehicleUnlocked(VehicleIds.Motor, true);
            ShowRewardPopup("SPECIAL", "MOTOR UNLOCKED", "See you tomorrow!", motorRewardSprite);
            return;
        }

        SaveSystem.AddCoins(DuplicateMotorCoins);
        ShowRewardPopup("DUPLICATE", "MOTOR ALREADY UNLOCKED", "Converted to 1000 COINS", coinRewardSprite);
    }

    private void ShowRewardPopup(string title, string line1, string line2, Sprite icon)
    {
        if (popupTitleText != null)
        {
            popupTitleText.text = title;
        }

        if (popupLine1Text != null)
        {
            popupLine1Text.text = line1;
        }

        if (popupLine2Text != null)
        {
            popupLine2Text.text = line2;
        }

        if (popupRewardIcon != null)
        {
            popupRewardIcon.sprite = icon;
            popupRewardIcon.enabled = icon != null;
            popupRewardIcon.preserveAspect = true;
        }

        if (rewardPopupRoot != null)
        {
            rewardPopupRoot.SetActive(true);
        }
    }

    private bool HasClaimedToday()
    {
        return lastClaimDate == GetTodayString();
    }

    private static string GetTodayString()
    {
        return DateTime.Now.ToString(DateFormat);
    }

    private static int GetDayMask(int day)
    {
        return 1 << (Mathf.Clamp(day, 1, DaysInCycle) - 1);
    }

    private static T FindComponentByName<T>(string objectName) where T : Component
    {
        GameObject gameObject = FindGameObjectByName(objectName);
        return gameObject != null ? gameObject.GetComponent<T>() : null;
    }

    private static GameObject FindGameObjectByName(string objectName)
    {
        Transform transform = FindTransformByName(objectName);
        return transform != null ? transform.gameObject : null;
    }

    private static Transform FindTransformByName(string objectName)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform transform in transforms)
        {
            if (transform.name != objectName)
            {
                continue;
            }

            if (!transform.gameObject.scene.IsValid() || !transform.gameObject.scene.isLoaded)
            {
                continue;
            }

            return transform;
        }

        return null;
    }
}
