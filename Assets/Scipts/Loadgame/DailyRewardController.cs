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
    private const string DailyLastClaimedDayKey = "DailyLastClaimedDay";
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
    private int lastClaimedDay;

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
        AudioService.PlayBackClose();
        SceneManager.LoadScene(menuSceneName);
    }

    public void ClaimTodayReward()
    {
        LoadDailyState();

        if (HasClaimedToday())
        {
            AudioService.PlayErrorNotEnoughCoin();
            RefreshUI();
            return;
        }

        AudioService.PlayButtonClick();
        int claimedDay = Mathf.Clamp(currentDay, 1, DaysInCycle);
        ApplyReward(claimedDay);

        claimedMask |= GetDayMask(claimedDay);
        lastClaimDate = GetTodayString();
        lastClaimedDay = claimedDay;

        currentDay = claimedDay < DaysInCycle ? claimedDay + 1 : 1;

        SaveDailyState();
        RefreshUI();
    }

    public void HideRewardPopup()
    {
        AudioService.PlayBackClose();
        if (rewardPopupRoot != null)
        {
            rewardPopupRoot.SetActive(false);
        }
    }

    [ContextMenu("Daily/Test Simulate New Day")]
    public void DevSimulateNewDay()
    {
        PlayerPrefs.SetString(DailyLastClaimDateKey, string.Empty);
        PlayerPrefs.SetInt(DailyLastClaimedDayKey, 0);
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
        PlayerPrefs.SetInt(DailyLastClaimedDayKey, 0);
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
        PlayerPrefs.DeleteKey(DailyLastClaimedDayKey);
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
        homeButton ??= FindSceneComponentByName<Button>("HomeButton");
        claimButton ??= FindSceneComponentByName<Button>("ClaimButton");
        claimedButton ??= FindSceneComponentByName<Button>("ClaimedButton");
        popupOkButton ??= FindSceneComponentByName<Button>("PopupOkButton");

        rewardPopupRoot ??= FindSceneGameObjectByName("RewardPopupRoot");
        popupRewardIcon ??= FindSceneComponentByName<Image>("PopupRewardIcon");
        popupTitleText ??= FindSceneComponentByName<TextMeshProUGUI>("PopupTitleText");
        popupLine1Text ??= FindSceneComponentByName<TextMeshProUGUI>("PopupLine1Text");
        popupLine2Text ??= FindSceneComponentByName<TextMeshProUGUI>("PopupLine2Text");

        for (int i = 0; i < DaysInCycle; i++)
        {
            Transform slot = FindSceneTransformByName("DaySlot" + (i + 1));
            daySlots[i] = slot;

            if (slot == null)
            {
                Debug.LogWarning("DailyRewardController: Khong tim thay DaySlot" + (i + 1) + " trong scene " + gameObject.scene.name);
                continue;
            }

            Transform badgeTransform = FindChildRecursive(slot, "ClaimBadge");
            if (badgeTransform != null)
            {
                claimBadges[i] = badgeTransform.GetComponent<Image>();
            }

            Transform dayLabelTransform = FindChildRecursive(slot, "DayLabel");
            if (dayLabelTransform != null)
            {
                dayLabels[i] = dayLabelTransform.gameObject;
            }

            Transform rewardIconTransform = FindChildRecursive(slot, "RewardIcon");
            if (rewardIconTransform != null)
            {
                rewardIcons[i] = rewardIconTransform.gameObject;
            }

            Transform rewardAmountTextTransform = FindChildRecursive(slot, "RewardAmountText");
            if (rewardAmountTextTransform != null)
            {
                rewardAmountTexts[i] = rewardAmountTextTransform.gameObject;
            }
            else
            {
                Debug.LogWarning("DailyRewardController: DaySlot" + (i + 1) + " khong co RewardAmountText.");
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
        lastClaimedDay = Mathf.Clamp(PlayerPrefs.GetInt(DailyLastClaimedDayKey, 0), 0, DaysInCycle);

        if (HasClaimedToday() && lastClaimedDay == 0)
        {
            lastClaimedDay = currentDay == 1 ? DaysInCycle : currentDay - 1;
        }

        if (currentDay == 1 && claimedMask == FullCycleMask && !HasClaimedToday())
        {
            claimedMask = 0;
            PlayerPrefs.SetInt(DailyClaimedMaskKey, claimedMask);
            PlayerPrefs.Save();
        }

        NormalizeClaimedMaskFromProgress();
        RepairClaimedMaskIfNeeded();

        Debug.Log(
            "DailyRewardController LoadDailyState: currentDay=" + currentDay +
            ", claimedMask=" + claimedMask +
            ", lastClaimDate=" + lastClaimDate +
            ", lastClaimedDay=" + lastClaimedDay +
            ", claimedToday=" + HasClaimedToday());
    }

    private void SaveDailyState()
    {
        PlayerPrefs.SetInt(DailyCurrentDayKey, currentDay);
        PlayerPrefs.SetString(DailyLastClaimDateKey, lastClaimDate);
        PlayerPrefs.SetInt(DailyClaimedMaskKey, claimedMask);
        PlayerPrefs.SetInt(DailyLastClaimedDayKey, lastClaimedDay);
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
            bool isClaimed = IsDayClaimedForDisplay(i + 1);
            SetDaySlotClaimedVisual(i, isClaimed);
        }
    }

    private void SetDaySlotClaimedVisual(int index, bool isClaimed)
    {
        if (claimBadges[index] != null)
        {
            claimBadges[index].gameObject.SetActive(isClaimed);
            if (isClaimed)
            {
                claimBadges[index].transform.SetAsLastSibling();
            }
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
            AudioService.PlayUnlockSuccess();
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

        AudioService.PlayRewardPopup();
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

    private T FindSceneComponentByName<T>(string objectName) where T : Component
    {
        GameObject sceneObject = FindSceneGameObjectByName(objectName);
        return sceneObject != null ? sceneObject.GetComponent<T>() : null;
    }

    private GameObject FindSceneGameObjectByName(string objectName)
    {
        Transform sceneTransform = FindSceneTransformByName(objectName);
        return sceneTransform != null ? sceneTransform.gameObject : null;
    }

    private Transform FindSceneTransformByName(string objectName)
    {
        foreach (GameObject rootObject in gameObject.scene.GetRootGameObjects())
        {
            Transform foundTransform = FindChildRecursive(rootObject.transform, objectName);
            if (foundTransform != null)
            {
                return foundTransform;
            }
        }

        return null;
    }

    private void RepairClaimedMaskIfNeeded()
    {
        if (!HasClaimedToday())
        {
            return;
        }

        int inferredClaimedDay = lastClaimedDay > 0
            ? lastClaimedDay
            : (currentDay == 1 ? DaysInCycle : currentDay - 1);
        int inferredMask = GetDayMask(inferredClaimedDay);

        if ((claimedMask & inferredMask) != 0)
        {
            return;
        }

        claimedMask |= inferredMask;
        PlayerPrefs.SetInt(DailyClaimedMaskKey, claimedMask);
        PlayerPrefs.Save();
    }

    private void NormalizeClaimedMaskFromProgress()
    {
        int normalizedMask = 0;

        if (currentDay > 1)
        {
            for (int day = 1; day < currentDay; day++)
            {
                normalizedMask |= GetDayMask(day);
            }
        }
        else if (!HasClaimedToday())
        {
            if (claimedMask != 0)
            {
                claimedMask = 0;
                PlayerPrefs.SetInt(DailyClaimedMaskKey, claimedMask);
                PlayerPrefs.Save();
            }

            return;
        }

        if ((claimedMask & normalizedMask) == normalizedMask)
        {
            return;
        }

        claimedMask |= normalizedMask;
        PlayerPrefs.SetInt(DailyClaimedMaskKey, claimedMask);
        PlayerPrefs.Save();
    }

    private bool IsDayClaimedForDisplay(int day)
    {
        int dayMask = GetDayMask(day);
        if ((claimedMask & dayMask) != 0)
        {
            return true;
        }

        if (HasClaimedToday() && lastClaimedDay == day)
        {
            return true;
        }

        if (currentDay > 1 && day < currentDay)
        {
            return true;
        }

        return false;
    }

    private static Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }

            Transform nestedChild = FindChildRecursive(child, childName);
            if (nestedChild != null)
            {
                return nestedChild;
            }
        }

        return null;
    }
}
