using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapSelect2Controller : MonoBehaviour
{
    [Header("Top HUD")]
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Cards")]
    [SerializeField] private MapCardUI groundCard;
    [SerializeField] private MapCardUI desertCard;
    [SerializeField] private MapCardUI marsCard;

    [Header("Page Navigation")]
    [SerializeField] private Button pageLeftButton;
    [SerializeField] private Button pageRightButton;
    [SerializeField] private string previousSceneName = "LevelSelect";
    [SerializeField] private string nextSceneName = "LevelSelect3";

    private void Start()
    {
        AutoFindReferences();
        InitializeCards();
        WirePageButtons();
        RefreshAll();
    }

    public void RefreshAll()
    {
        if (coinText != null)
        {
            coinText.text = SaveSystem.LoadTotalCoins().ToString();
        }

        RefreshCard(groundCard);
        RefreshCard(desertCard);
        RefreshCard(marsCard);
    }

    public void BackToMainMenu()
    {
        AudioService.PlayBackClose();
        SceneManager.LoadScene("Menu");
    }

    public void HandleMapCardAction(MapCardUI card)
    {
        if (card == null)
        {
            return;
        }

        if (SaveSystem.IsMapUnlocked(card.MapId))
        {
            AudioService.PlayButtonClick();
            SceneManager.LoadScene(card.SceneName);
            return;
        }

        if (!SaveSystem.SpendCoins(card.Cost))
        {
            AudioService.PlayErrorNotEnoughCoin();
            Debug.LogWarning("Not enough coins to unlock " + card.MapId);
            return;
        }

        SaveSystem.UnlockMap(card.MapId);
        AudioService.PlayBuySuccess();
        RefreshAll();
    }

    public void LoadPreviousPage()
    {
        AudioService.PlayBackClose();
        SceneManager.LoadScene(previousSceneName);
    }

    public void LoadNextPage()
    {
        AudioService.PlayButtonClick();
        SceneManager.LoadScene(nextSceneName);
    }

    [ContextMenu("Test/Unlock Forest")]
    private void UnlockForestForTest()
    {
        SaveSystem.UnlockMap("Forest");
        RefreshAll();
        Debug.Log("MapSelect2Controller: Unlocked Forest for test.");
    }

    [ContextMenu("Test/Unlock Arctic")]
    private void UnlockArcticForTest()
    {
        SaveSystem.UnlockMap("Arctic");
        RefreshAll();
        Debug.Log("MapSelect2Controller: Unlocked Arctic for test.");
    }

    [ContextMenu("Test/Unlock Highway")]
    private void UnlockHighwayForTest()
    {
        SaveSystem.UnlockMap("Highway");
        RefreshAll();
        Debug.Log("MapSelect2Controller: Unlocked Highway for test.");
    }

    [ContextMenu("Test/Lock Forest")]
    private void LockForestForTest()
    {
        PlayerPrefs.SetInt("MapUnlocked_Forest", 0);
        PlayerPrefs.Save();
        RefreshAll();
        Debug.Log("MapSelect2Controller: Locked Forest for test.");
    }

    [ContextMenu("Test/Lock Arctic")]
    private void LockArcticForTest()
    {
        PlayerPrefs.SetInt("MapUnlocked_Arctic", 0);
        PlayerPrefs.Save();
        RefreshAll();
        Debug.Log("MapSelect2Controller: Locked Arctic for test.");
    }

    [ContextMenu("Test/Lock Highway")]
    private void LockHighwayForTest()
    {
        PlayerPrefs.SetInt("MapUnlocked_Highway", 0);
        PlayerPrefs.Save();
        RefreshAll();
        Debug.Log("MapSelect2Controller: Locked Highway for test.");
    }

    private void InitializeCards()
    {
        if (groundCard != null)
        {
            groundCard.Setup(this, "Forest", "ForestMap", 3000, "FOREST");
        }

        if (desertCard != null)
        {
            desertCard.Setup(this, "Arctic", "ArcticMap", 4500, "ARCTIC");
        }

        if (marsCard != null)
        {
            marsCard.Setup(this, "Highway", "HighwayMap", 6000, "HIGHWAY");
        }
    }

    private void RefreshCard(MapCardUI card)
    {
        if (card != null)
        {
            card.RefreshUI();
        }
    }

    private void AutoFindReferences()
    {
        if (coinText == null)
        {
            GameObject coinTextObject = GameObject.Find("CoinText");
            if (coinTextObject != null)
            {
                coinText = coinTextObject.GetComponent<TextMeshProUGUI>();
            }
        }

        if (groundCard == null)
        {
            groundCard = FindCard("Forest");
        }

        if (desertCard == null)
        {
            desertCard = FindCard("Arctic");
        }

        if (marsCard == null)
        {
            marsCard = FindCard("Highway");
        }

        if (pageLeftButton == null)
        {
            pageLeftButton = FindButton("PageLeftButton");
        }

        if (pageRightButton == null)
        {
            pageRightButton = FindButton("PageRightButton");
        }
    }

    private void WirePageButtons()
    {
        if (pageLeftButton != null)
        {
            pageLeftButton.onClick.RemoveListener(LoadPreviousPage);
            pageLeftButton.onClick.AddListener(LoadPreviousPage);
        }

        if (pageRightButton != null)
        {
            pageRightButton.onClick.RemoveListener(LoadNextPage);
            pageRightButton.onClick.AddListener(LoadNextPage);
        }
    }

    private static MapCardUI FindCard(string objectName)
    {
        GameObject cardObject = GameObject.Find(objectName);
        return cardObject != null ? cardObject.GetComponent<MapCardUI>() : null;
    }

    private static Button FindButton(string objectName)
    {
        GameObject buttonObject = GameObject.Find(objectName);
        return buttonObject != null ? buttonObject.GetComponent<Button>() : null;
    }
}
