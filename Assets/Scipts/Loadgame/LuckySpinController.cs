using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LuckySpinController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform wheelTransform;
    [SerializeField] private Button spinButton;
    [SerializeField] private GameObject resultPopupRoot;
    [SerializeField] private Image resultRewardImage;
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text resultMessageText;
    [SerializeField] private Button okButton;
    [SerializeField] private Button spinAgainButton;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private Button homeButton;

    [Header("Spin Settings")]
    [SerializeField] private int spinCost = 100;
    [SerializeField] private float spinDuration = 3f;
    [SerializeField] private int fullRotations = 5;
    [SerializeField] private int duplicateF1CoinReward = 500;
    [SerializeField] private float angleOffset = 0f;
    [SerializeField] private bool demoMode = true;
    [SerializeField] private Sprite coinRewardSprite;
    [SerializeField] private float boosted20CoinWeightMultiplier = 1.8f;
    [SerializeField] private float boosted50CoinWeightMultiplier = 1.6f;

    private const string MenuSceneName = "Menu";
    private const string F1VehicleId = "f1_car";
    private const int SegmentCount = 8;
    private const float SegmentAngle = 360f / SegmentCount;

    private bool isSpinning;
    private Sprite f1RewardSprite;
    private AudioSource wheelTickSource;

    private readonly RewardSegment[] demoRewards =
    {
        // Visual order on the wheel sprite, clockwise from top:
        // 0 = F1, 1 = 20, 2 = 50, 3 = 100, 4 = 150, 5 = 20, 6 = 200, 7 = 500
        new RewardSegment(1, RewardType.Coins, 20, 10f),
        new RewardSegment(2, RewardType.Coins, 50, 20f),
        new RewardSegment(3, RewardType.Coins, 100, 20f),
        new RewardSegment(4, RewardType.Coins, 150, 15f),
        new RewardSegment(5, RewardType.Coins, 20, 10f),
        new RewardSegment(6, RewardType.Coins, 200, 10f),
        new RewardSegment(7, RewardType.Coins, 500, 5f),
        new RewardSegment(0, RewardType.F1Car, 0, 10f),
    };

    private void Awake()
    {
        AutoAssignReferences();
        PreserveSpinButtonVisualWhenDisabled();

        if (resultRewardImage != null)
        {
            f1RewardSprite = resultRewardImage.sprite;
        }

        if (coinRewardSprite == null)
        {
            Debug.LogWarning("LuckySpinController: coinRewardSprite chua duoc gan. Reward coin se khong hien icon.");
        }

        if (resultPopupRoot != null)
        {
            resultPopupRoot.SetActive(false);
        }

        BindButtons();
        RefreshCoinText();
        EnsureWheelTickSource();
    }

    private void BindButtons()
    {
        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(GoHome);
        }

        if (spinButton != null)
        {
            spinButton.onClick.RemoveAllListeners();
            spinButton.onClick.AddListener(OnSpinPressed);
        }

        if (okButton != null)
        {
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(HideResultPopup);
        }

        if (spinAgainButton != null)
        {
            spinAgainButton.onClick.RemoveAllListeners();
            spinAgainButton.onClick.AddListener(OnSpinAgainPressed);
        }
    }

    private void AutoAssignReferences()
    {
        if (wheelTransform == null)
        {
            wheelTransform = FindRectTransform("SpinWheel_Rotating");
        }

        if (spinButton == null)
        {
            spinButton = FindButton("SpinButton");
        }

        if (resultPopupRoot == null)
        {
            GameObject popup = GameObject.Find("ResultPopupRoot");
            if (popup != null)
            {
                resultPopupRoot = popup;
            }
        }

        if (resultRewardImage == null)
        {
            resultRewardImage = FindImage("RewardImage");
        }

        if (resultTitleText == null)
        {
            resultTitleText = FindTmp("ResultTitleText");
        }

        if (resultMessageText == null)
        {
            resultMessageText = FindTmp("ResultMessageText");
        }

        if (okButton == null)
        {
            okButton = FindButton("OkButton");
        }

        if (spinAgainButton == null)
        {
            spinAgainButton = FindButton("SpinAgainButton");
        }

        if (coinText == null)
        {
            coinText = FindTmp("CoinText");
        }

        if (homeButton == null)
        {
            homeButton = FindButton("HomeButton");
        }
    }

    public void OnSpinPressed()
    {
        if (isSpinning)
        {
            return;
        }

        AudioService.PlayButtonClick();
        StartSpinFlow(false);
    }

    public void OnSpinAgainPressed()
    {
        HideResultPopup();

        if (isSpinning)
        {
            return;
        }

        AudioService.PlayButtonClick();
        StartSpinFlow(true);
    }

    private void StartSpinFlow(bool isSpinAgain)
    {
        if (SaveSystem.LoadTotalCoins() < spinCost)
        {
            ShowNotEnoughCoinsPopup();
            return;
        }

        if (!SaveSystem.SpendCoins(spinCost))
        {
            ShowNotEnoughCoinsPopup();
            return;
        }

        RefreshCoinText();
        RewardSegment reward = ChooseReward(isSpinAgain);
        AudioService.PlayClip(AudioPaths.SpinStart, 1f);
        StartCoroutine(SpinToRewardCoroutine(reward));
    }

    private IEnumerator SpinToRewardCoroutine(RewardSegment reward)
    {
        isSpinning = true;
        SetSpinButtonInteractable(false);
        SetPopupButtonsInteractable(false);
        PlayWheelTickLoop();

        float startAngle = GetCurrentWheelAngle();
        float targetAngle = CalculateTargetAngle(reward.segmentIndex);
        float deltaAngle = Mathf.Repeat(targetAngle - startAngle, 360f);
        float totalRotation = (fullRotations * 360f) + deltaAngle;
        float elapsed = 0f;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spinDuration);
            float eased = EaseOutCubic(t);
            float currentAngle = startAngle + (totalRotation * eased);
            SetWheelAngle(currentAngle);
            yield return null;
        }

        SetWheelAngle(startAngle + totalRotation);
        StopWheelTickLoop();
        ApplyReward(reward);

        isSpinning = false;
        SetPopupButtonsInteractable(true);
    }

    private RewardSegment ChooseReward(bool isSpinAgain)
    {
        if (!demoMode)
        {
            int index = Random.Range(0, demoRewards.Length);
            return demoRewards[index];
        }

        float totalWeight = 0f;
        for (int i = 0; i < demoRewards.Length; i++)
        {
            totalWeight += GetEffectiveWeight(demoRewards[i]);
        }

        float randomPoint = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < demoRewards.Length; i++)
        {
            cumulative += GetEffectiveWeight(demoRewards[i]);
            if (randomPoint <= cumulative)
            {
                return demoRewards[i];
            }
        }

        return demoRewards[demoRewards.Length - 1];
    }

    private float GetEffectiveWeight(RewardSegment reward)
    {
        float effectiveWeight = reward.weight;

        if (reward.rewardType != RewardType.Coins)
        {
            return effectiveWeight;
        }

        if (reward.coinAmount == 20)
        {
            effectiveWeight *= Mathf.Max(1f, boosted20CoinWeightMultiplier);
        }
        else if (reward.coinAmount == 50)
        {
            effectiveWeight *= Mathf.Max(1f, boosted50CoinWeightMultiplier);
        }

        return effectiveWeight;
    }

    private void ApplyReward(RewardSegment reward)
    {
        switch (reward.rewardType)
        {
            case RewardType.Coins:
                SaveSystem.AddCoins(reward.coinAmount);
                RefreshCoinText();
        ShowResultPopup("YOU WON!", "+" + reward.coinAmount + " COINS", PopupRewardVisual.Coin, true);
        break;

            case RewardType.F1Car:
                if (!SaveSystem.IsVehicleUnlocked(F1VehicleId))
                {
                    SaveSystem.SaveVehicleUnlocked(F1VehicleId, true);
                    AudioService.PlayUnlockSuccess();
                    PlayJackpotSound();
                    ShowResultPopup("JACKPOT!", "F1 CAR UNLOCKED!", PopupRewardVisual.F1, true);
                }
                else
                {
                    SaveSystem.AddCoins(duplicateF1CoinReward);
                    RefreshCoinText();
                    ShowResultPopup("DUPLICATE!", "F1 already unlocked! Converted to " + duplicateF1CoinReward + " coins", PopupRewardVisual.Coin, true);
                }
                break;
        }
    }

    private void ShowNotEnoughCoinsPopup()
    {
        AudioService.PlayErrorNotEnoughCoin();
        ShowResultPopup("NOT ENOUGH COINS", "You need " + spinCost + " coins to spin.", PopupRewardVisual.None, false);
    }

    private void ShowResultPopup(string title, string message, PopupRewardVisual rewardVisual, bool playPopupSound)
    {
        if (resultTitleText != null)
        {
            resultTitleText.text = title;
        }

        if (resultMessageText != null)
        {
            resultMessageText.text = message;
        }

        if (resultRewardImage != null)
        {
            Sprite rewardSprite = null;
            switch (rewardVisual)
            {
                case PopupRewardVisual.Coin:
                    rewardSprite = coinRewardSprite;
                    break;

                case PopupRewardVisual.F1:
                    rewardSprite = f1RewardSprite;
                    break;
            }

            resultRewardImage.enabled = rewardSprite != null;
            if (rewardSprite != null)
            {
                resultRewardImage.sprite = rewardSprite;
                resultRewardImage.preserveAspect = true;
            }
        }

        if (resultPopupRoot != null)
        {
            resultPopupRoot.SetActive(true);
        }

        if (playPopupSound)
        {
            AudioService.PlayRewardPopup();
        }
        SetSpinButtonInteractable(false);
        SetPopupButtonsInteractable(true);
    }

    public void HideResultPopup()
    {
        if (resultPopupRoot != null)
        {
            resultPopupRoot.SetActive(false);
        }

        if (!isSpinning)
        {
            SetSpinButtonInteractable(true);
        }
    }

    public void RefreshCoinText()
    {
        if (coinText != null)
        {
            coinText.text = SaveSystem.LoadTotalCoins().ToString();
        }
    }

    public void GoHome()
    {
        AudioService.PlayBackClose();
        SceneManager.LoadScene(MenuSceneName);
    }

    private float CalculateTargetAngle(int segmentIndex)
    {
        return angleOffset + (segmentIndex * SegmentAngle);
    }

    private float GetCurrentWheelAngle()
    {
        if (wheelTransform == null)
        {
            return 0f;
        }

        return wheelTransform.localEulerAngles.z;
    }

    private void SetWheelAngle(float zAngle)
    {
        if (wheelTransform == null)
        {
            return;
        }

        wheelTransform.localRotation = Quaternion.Euler(0f, 0f, zAngle);
    }

    private void SetSpinButtonInteractable(bool value)
    {
        if (spinButton != null)
        {
            spinButton.interactable = value;
        }
    }

    private void PreserveSpinButtonVisualWhenDisabled()
    {
        if (spinButton == null)
        {
            return;
        }

        ColorBlock colors = spinButton.colors;
        colors.disabledColor = colors.normalColor;
        spinButton.colors = colors;
    }

    private void PlayJackpotSound()
    {
        AudioClip jackpotClip = AudioService.LoadClip(AudioPaths.JackpotUnlock);
        if (jackpotClip == null)
        {
            return;
        }

        AudioService.PlayClip(jackpotClip, 1f);
    }

    private void EnsureWheelTickSource()
    {
        if (wheelTickSource != null)
        {
            return;
        }

        AudioClip tickClip = AudioService.LoadClip(AudioPaths.WheelTickLoop);
        if (tickClip == null)
        {
            return;
        }

        wheelTickSource = gameObject.GetComponent<AudioSource>();
        if (wheelTickSource == null)
        {
            wheelTickSource = gameObject.AddComponent<AudioSource>();
        }

        wheelTickSource.playOnAwake = false;
        wheelTickSource.loop = true;
        wheelTickSource.spatialBlend = 0f;
        wheelTickSource.volume = 0.8f;
        wheelTickSource.clip = tickClip;
    }

    private void PlayWheelTickLoop()
    {
        EnsureWheelTickSource();

        if (wheelTickSource != null && wheelTickSource.clip != null && !wheelTickSource.isPlaying)
        {
            wheelTickSource.Play();
        }
    }

    private void StopWheelTickLoop()
    {
        if (wheelTickSource != null && wheelTickSource.isPlaying)
        {
            wheelTickSource.Stop();
        }
    }

    private void SetPopupButtonsInteractable(bool value)
    {
        if (okButton != null)
        {
            okButton.interactable = value;
        }

        if (spinAgainButton != null)
        {
            spinAgainButton.interactable = value;
        }
    }

    private static float EaseOutCubic(float t)
    {
        float oneMinusT = 1f - t;
        return 1f - (oneMinusT * oneMinusT * oneMinusT);
    }

    private static RectTransform FindRectTransform(string objectName)
    {
        GameObject target = GameObject.Find(objectName);
        return target != null ? target.GetComponent<RectTransform>() : null;
    }

    private static Button FindButton(string objectName)
    {
        GameObject target = GameObject.Find(objectName);
        return target != null ? target.GetComponent<Button>() : null;
    }

    private static Image FindImage(string objectName)
    {
        GameObject target = GameObject.Find(objectName);
        return target != null ? target.GetComponent<Image>() : null;
    }

    private static TMP_Text FindTmp(string objectName)
    {
        GameObject target = GameObject.Find(objectName);
        return target != null ? target.GetComponent<TMP_Text>() : null;
    }

    private readonly struct RewardSegment
    {
        public readonly int segmentIndex;
        public readonly RewardType rewardType;
        public readonly int coinAmount;
        public readonly float weight;

        public RewardSegment(int segmentIndex, RewardType rewardType, int coinAmount, float weight)
        {
            this.segmentIndex = segmentIndex;
            this.rewardType = rewardType;
            this.coinAmount = coinAmount;
            this.weight = weight;
        }
    }

    private enum RewardType
    {
        Coins,
        F1Car
    }

    private enum PopupRewardVisual
    {
        None,
        Coin,
        F1
    }
}
