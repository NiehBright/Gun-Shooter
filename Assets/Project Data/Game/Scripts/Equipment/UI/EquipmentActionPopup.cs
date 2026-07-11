using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class EquipmentActionPopup : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] GameObject popupPanel;
        [SerializeField] Text itemNameText;
        [SerializeField] Text itemLevelText;
        [SerializeField] Image itemIconImage;
        [SerializeField] Text itemStatsText;
        [SerializeField] Image rarityBgImage;
        [SerializeField] Button blockerButton; // Nút nền để đóng khi ấn ra ngoài

        [Header("Equip Elements")]
        [SerializeField] GameObject equipGroup;
        [SerializeField] Button equipButton;
        [SerializeField] Text equipButtonText;

        [Header("Upgrade Elements")]
        [SerializeField] GameObject upgradeGroup;
        [SerializeField] Button unequipButton;
        [SerializeField] Button upgradeButton;
        [SerializeField] Text coinCostText;
        [SerializeField] Image coinIcon;

        private EquipmentData currentItem;
        private EquipmentType currentSlotType;
        private bool isForEquipped;

        private void Start()
        {
            Hide();

            if (blockerButton != null)
                blockerButton.onClick.AddListener(Hide);

            if (equipButton != null)
                equipButton.onClick.AddListener(OnEquipClicked);

            if (unequipButton != null)
                unequipButton.onClick.AddListener(OnUnequipClicked);

            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }

        public void ShowForUnequipped(EquipmentData item)
        {
            if (item == null || popupPanel == null) return;

            currentItem = item;
            isForEquipped = false;

            SetupBaseInfo(item);

            if (equipGroup != null) equipGroup.SetActive(true);
            if (upgradeGroup != null) upgradeGroup.SetActive(false);

            if (equipButtonText != null)
            {
                var equippedID = EquipmentController.SaveData.GetEquippedID(item.EquipmentType);
                equipButtonText.text = string.IsNullOrEmpty(equippedID) ? "Mang" : "Doi";
            }

            ShowPopup();
        }

        public void ShowForEquipped(EquipmentData item, EquipmentType slotType)
        {
            if (item == null || popupPanel == null) return;

            currentItem = item;
            currentSlotType = slotType;
            isForEquipped = true;

            SetupBaseInfo(item);

            if (equipGroup != null) equipGroup.SetActive(false);
            if (upgradeGroup != null) upgradeGroup.SetActive(true);

            UpdateUpgradeCost(item);

            ShowPopup();
        }

        private void SetupBaseInfo(EquipmentData item)
        {
            var saveItem = EquipmentController.SaveData.GetItem(item.ItemID);
            int level = saveItem != null ? saveItem.level : 1;

            if (itemNameText != null) itemNameText.text = item.ItemName;
            if (itemLevelText != null) itemLevelText.text = "Cap " + level + "/" + item.MaxLevel;
            if (itemIconImage != null)
            {
                itemIconImage.sprite = item.Icon;
                itemIconImage.enabled = item.Icon != null;
            }

            if (rarityBgImage != null)
            {
                Color rc = GetRarityColor(item.Rarity);
                rarityBgImage.color = new Color(rc.r, rc.g, rc.b, 0.45f);
            }

            var stats = item.GetStatsAtLevel(level);
            var nextStats = level < item.MaxLevel ? item.GetStatsAtLevel(level + 1) : stats;
            string statsStr = "";

            if (stats.bonusHP > 0)
            {
                statsStr = level < item.MaxLevel 
                    ? $"Mau: +{stats.bonusHP} \u2794 +{nextStats.bonusHP}" 
                    : $"Mau: +{stats.bonusHP} (Max)";
            }
            else if (stats.bonusDamagePercent > 0)
            {
                statsStr = level < item.MaxLevel 
                    ? $"Sat thuong: +{stats.bonusDamagePercent}% \u2794 +{nextStats.bonusDamagePercent}%" 
                    : $"Sat thuong: +{stats.bonusDamagePercent}% (Max)";
            }
            else if (stats.bonusArmor > 0)
            {
                statsStr = level < item.MaxLevel 
                    ? $"Giap: +{stats.bonusArmor}% \u2794 +{nextStats.bonusArmor}%" 
                    : $"Giap: +{stats.bonusArmor}% (Max)";
            }
            else if (stats.bonusMoveSpeed > 0)
            {
                statsStr = level < item.MaxLevel 
                    ? $"Toc do: +{stats.bonusMoveSpeed}% \u2794 +{nextStats.bonusMoveSpeed}%" 
                    : $"Toc do: +{stats.bonusMoveSpeed}% (Max)";
            }

            if (itemStatsText != null) itemStatsText.text = statsStr;
        }

        private void UpdateUpgradeCost(EquipmentData item)
        {
            var saveItem = EquipmentController.SaveData.GetItem(item.ItemID);
            if (saveItem == null) return;

            int level = saveItem.level;
            int costCoins = item.GetUpgradeCost(level);

            if (costCoins < 0 || level >= item.MaxLevel)
            {
                if (coinCostText != null) coinCostText.text = "Max";
                if (upgradeButton != null) upgradeButton.interactable = false;
                return;
            }

            int currentCoins = CurrenciesController.Get(CurrencyType.Coins);
            if (coinCostText != null)
            {
                coinCostText.text = currentCoins + "/" + costCoins;
                coinCostText.color = currentCoins >= costCoins ? Color.white : Color.red;
            }

            if (upgradeButton != null)
            {
                upgradeButton.interactable = currentCoins >= costCoins;
            }
        }

        private Color GetRarityColor(EquipmentRarity rarity)
        {
            switch (rarity)
            {
                case EquipmentRarity.Common: return new Color(0.7f, 0.7f, 0.7f);
                case EquipmentRarity.Rare: return new Color(0.2f, 0.6f, 1f);
                case EquipmentRarity.Epic: return new Color(0.7f, 0.2f, 0.9f);
                default: return Color.white;
            }
        }

        private void ShowPopup()
        {
            gameObject.SetActive(true);
            if (popupPanel != null) popupPanel.SetActive(true);
        }

        public void Hide()
        {
            if (popupPanel != null) popupPanel.SetActive(false);
            gameObject.SetActive(false);
        }

        private void OnEquipClicked()
        {
            if (currentItem == null) return;

            EquipmentController.Equip(currentItem);
            Hide();

            if (EquipmentPanelUI.Instance != null)
                EquipmentPanelUI.Instance.RefreshUI();
        }

        private void OnUnequipClicked()
        {
            if (currentItem == null) return;

            EquipmentController.Unequip(currentSlotType);
            Hide();

            if (EquipmentPanelUI.Instance != null)
                EquipmentPanelUI.Instance.RefreshUI();
        }

        private void OnUpgradeClicked()
        {
            if (currentItem == null) return;

            var saveItem = EquipmentController.SaveData.GetItem(currentItem.ItemID);
            if (saveItem == null) return;

            int costCoins = currentItem.GetUpgradeCost(saveItem.level);

            if (CurrenciesController.Get(CurrencyType.Coins) < costCoins) return;

            CurrenciesController.Substract(CurrencyType.Coins, costCoins);
            saveItem.level++;

            EquipmentController.NotifyEquipmentChanged();
            SaveController.MarkAsSaveIsRequired();

            Hide();

            if (EquipmentPanelUI.Instance != null)
                EquipmentPanelUI.Instance.RefreshUI();
        }
    }
}
