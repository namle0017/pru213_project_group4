using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VehicleUpgradeUI : MonoBehaviour
{
    [System.Serializable]
    public class UpgradeRow
    {
        [Tooltip("Engine, Suspension, Tires, hoặc Fuel")]
        public string upgradeType;
        public string displayName;
        
        [Header("UI References")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI levelText;
        public Button upgradeButton;
        public TextMeshProUGUI costText;
        
        [Tooltip("5 Image tương ứng với 5 cấp độ nổ")]
        public Image[] levelIndicators;
        
        [Header("Color Config")]
        public Color activeColor = Color.green;
        public Color inactiveColor = Color.gray;
    }

    [Header("UI Rows")]
    [SerializeField] private UpgradeRow[] upgradeRows;

    [Header("References")]
    [SerializeField] private GaragePlaceholderController garageController;

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        string selectedVehicleId = SaveSystem.LoadSelectedVehicle();
        
        if (upgradeRows == null || upgradeRows.Length == 0)
        {
            return;
        }

        foreach (UpgradeRow row in upgradeRows)
        {
            if (row == null) continue;

            int currentLevel = SaveSystem.GetUpgradeLevel(selectedVehicleId, row.upgradeType);
            int cost = SaveSystem.GetUpgradeCost(currentLevel);

            // Cập nhật tiêu đề hiển thị
            if (row.titleText != null)
            {
                row.titleText.text = row.displayName;
            }

            // Cập nhật text hiển thị cấp độ
            if (row.levelText != null)
            {
                row.levelText.text = "Lvl " + currentLevel;
            }

            // Cập nhật các ô vuông chỉ báo cấp độ
            if (row.levelIndicators != null)
            {
                for (int i = 0; i < row.levelIndicators.Length; i++)
                {
                    if (row.levelIndicators[i] != null)
                    {
                        row.levelIndicators[i].color = (i < currentLevel) ? row.activeColor : row.inactiveColor;
                    }
                }
            }

            // Cập nhật Nút Nâng Cấp
            if (row.upgradeButton != null)
            {
                row.upgradeButton.onClick.RemoveAllListeners();

                if (currentLevel >= 5)
                {
                    // Đạt cấp tối đa
                    row.upgradeButton.interactable = false;
                    if (row.costText != null)
                    {
                        row.costText.text = "MAX";
                    }
                }
                else
                {
                    int totalCoins = SaveSystem.LoadTotalCoins();
                    bool canAfford = totalCoins >= cost;
                    
                    row.upgradeButton.interactable = true;
                    if (row.costText != null)
                    {
                        row.costText.text = cost + " COINS";
                        row.costText.color = canAfford ? Color.white : Color.red;
                    }

                    // Gán sự kiện click nâng cấp
                    row.upgradeButton.onClick.AddListener(() =>
                    {
                        BuyUpgrade(selectedVehicleId, row.upgradeType, currentLevel, cost);
                    });
                }
            }
        }
    }

    private void BuyUpgrade(string vehicleId, string upgradeType, int currentLevel, int cost)
    {
        if (SaveSystem.SpendCoins(cost))
        {
            // Tăng cấp độ lên 1
            SaveSystem.SaveUpgradeLevel(vehicleId, upgradeType, currentLevel + 1);
            
            // Phát âm thanh thành công
            AudioService.PlayBuySuccess();
            
            Debug.Log($"Upgrade: Nâng cấp {upgradeType} cho xe {vehicleId} lên Level {currentLevel + 1} thành công!");

            // Refresh lại toàn bộ Garage và bảng UI này
            if (garageController != null)
            {
                garageController.RefreshGarage();
            }
            RefreshUI();
        }
        else
        {
            // Không đủ xu
            AudioService.PlayErrorNotEnoughCoin();
            Debug.LogWarning("Upgrade: Không đủ xu để nâng cấp!");
        }
    }
}
