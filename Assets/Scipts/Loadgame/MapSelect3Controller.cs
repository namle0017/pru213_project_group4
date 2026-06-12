using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapSelect3Controller : MonoBehaviour
{
    [Header("Top HUD")]
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Cards")]
    [SerializeField] private MapCardUI moonCard;
    [SerializeField] private MapCardUI alienCard;

    [Header("Page Navigation")]
    [SerializeField] private Button pageLeftButton;
    [SerializeField] private Button pageRightButton;
    [SerializeField] private string previousSceneName = "LevelSelect2";

    private void Start()
    {
        AutoFindReferences();
        InitializeCards();
        WirePageButtons();
        RefreshAll();
        DisableNextPageButton();
    }

    public void RefreshAll()
    {
        if (coinText != null)
        {
            coinText.text = SaveSystem.LoadTotalCoins().ToString();
        }

        RefreshCard(moonCard);
        RefreshCard(alienCard);
    }

    public void BackToMainMenu()
    {
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
            SceneManager.LoadScene(card.SceneName);
            return;
        }

        if (!SaveSystem.SpendCoins(card.Cost))
        {
            Debug.LogWarning("Not enough coins to unlock " + card.MapId);
            return;
        }

        SaveSystem.UnlockMap(card.MapId);
        RefreshAll();
    }

    public void LoadPreviousPage()
    {
        SceneManager.LoadScene(previousSceneName);
    }

    [ContextMenu("Test/Unlock Moon")]
    private void UnlockMoonForTest()
    {
        SaveSystem.UnlockMap("Moon");
        RefreshAll();
        Debug.Log("MapSelect3Controller: Unlocked Moon for test.");
    }

    [ContextMenu("Test/Unlock Alien")]
    private void UnlockAlienForTest()
    {
        SaveSystem.UnlockMap("Alien");
        RefreshAll();
        Debug.Log("MapSelect3Controller: Unlocked Alien for test.");
    }

    [ContextMenu("Test/Lock Moon")]
    private void LockMoonForTest()
    {
        PlayerPrefs.SetInt("MapUnlocked_Moon", 0);
        PlayerPrefs.Save();
        RefreshAll();
        Debug.Log("MapSelect3Controller: Locked Moon for test.");
    }

    [ContextMenu("Test/Lock Alien")]
    private void LockAlienForTest()
    {
        PlayerPrefs.SetInt("MapUnlocked_Alien", 0);
        PlayerPrefs.Save();
        RefreshAll();
        Debug.Log("MapSelect3Controller: Locked Alien for test.");
    }

    private void InitializeCards()
    {
        if (moonCard != null)
        {
            moonCard.Setup(this, "Moon", "MoonMap", 8500, "MOON");
        }

        if (alienCard != null)
        {
            alienCard.Setup(this, "Alien", "AlienMap", 10000, "ALIEN");
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

        if (moonCard == null)
        {
            moonCard = FindCard("Moon");
        }

        if (alienCard == null)
        {
            alienCard = FindCard("Alien");
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
    }

    private void DisableNextPageButton()
    {
        if (pageRightButton != null)
        {
            pageRightButton.gameObject.SetActive(false);
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
