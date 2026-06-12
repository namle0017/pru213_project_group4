using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSelectController : MonoBehaviour
{
    [Header("Top HUD")]
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Cards")]
    [SerializeField] private MapCardUI groundCard;
    [SerializeField] private MapCardUI desertCard;
    [SerializeField] private MapCardUI marsCard;

    private void Start()
    {
        InitializeCards();
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
        SceneManager.LoadScene("Menu");
    }

    public void LoadNextPage()
    {
        SceneManager.LoadScene("LevelSelect2");
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

    private void InitializeCards()
    {
        if (groundCard != null)
        {
            groundCard.Setup(this, "Ground", "GroundMap", 0, "GROUND");
        }

        if (desertCard != null)
        {
            desertCard.Setup(this, "Desert", "DessertMap", 500, "DESERT");
        }

        if (marsCard != null)
        {
            marsCard.Setup(this, "Mars", "MarsMap", 1500, "MARS");
        }
    }

    private void RefreshCard(MapCardUI card)
    {
        if (card != null)
        {
            card.RefreshUI();
        }
    }
}
