using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpinSceneController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private GameObject resultPopupRoot;
    [SerializeField] private TextMeshProUGUI resultTitleText;
    [SerializeField] private Image rewardImage;
    [SerializeField] private TextMeshProUGUI resultMessageText;
    [SerializeField] private Button okButton;
    [SerializeField] private Button spinAgainButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button spinButton;

    [Header("Placeholder Content")]
    [SerializeField] private Sprite placeholderRewardSprite;
    [SerializeField] private string menuSceneName = "Menu";

    private void Awake()
    {
        if (resultPopupRoot != null)
        {
            resultPopupRoot.SetActive(false);
        }

        if (homeButton != null)
        {
            homeButton.onClick.RemoveListener(GoHome);
            homeButton.onClick.AddListener(GoHome);
        }

        if (spinButton != null)
        {
            spinButton.onClick.RemoveListener(ShowPlaceholderResult);
            spinButton.onClick.AddListener(ShowPlaceholderResult);
        }

        if (okButton != null)
        {
            okButton.onClick.RemoveListener(HideResultPopup);
            okButton.onClick.AddListener(HideResultPopup);
        }

        if (spinAgainButton != null)
        {
            spinAgainButton.onClick.RemoveListener(HideResultPopup);
            spinAgainButton.onClick.AddListener(HideResultPopup);
        }
    }

    private void Start()
    {
        RefreshCoins();
    }

    public void RefreshCoins()
    {
        if (coinText == null)
        {
            return;
        }

        coinText.text = SaveSystem.LoadTotalCoins().ToString();
    }

    public void GoHome()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    public void ShowPlaceholderResult()
    {
        if (resultTitleText != null)
        {
            resultTitleText.text = "SPIN RESULT";
        }

        if (resultMessageText != null)
        {
            resultMessageText.text = "Lucky Spin UI is ready.\nReward logic will be added next.";
        }

        if (rewardImage != null)
        {
            rewardImage.sprite = placeholderRewardSprite;
            rewardImage.enabled = placeholderRewardSprite != null;
            rewardImage.preserveAspect = true;
        }

        if (resultPopupRoot != null)
        {
            resultPopupRoot.SetActive(true);
        }
    }

    public void HideResultPopup()
    {
        if (resultPopupRoot != null)
        {
            resultPopupRoot.SetActive(false);
        }
    }
}
