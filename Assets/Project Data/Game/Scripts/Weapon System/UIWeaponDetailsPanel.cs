using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class UIWeaponDetailsPanel : MonoBehaviour
    {
        private UIWeaponPage weaponPage;

        private WeaponData currentWeaponData;
        private BaseWeaponUpgrade currentUpgrade;
        private int currentWeaponIndex;

        // UI Element References
        private TextMeshProUGUI weaponNameText;
        private TextMeshProUGUI rarityText;
        private Image rarityBadgeImage;
        private TextMeshProUGUI levelText;

        // Cards Progress
        private Image cardsFillImage;
        private TextMeshProUGUI cardsAmountText;
        private TextMeshProUGUI cardsStatusText;
        private GameObject cardsContainer;

        // Stats HUD
        private TextMeshProUGUI powerValueText;
        private TextMeshProUGUI powerBonusText;
        private TextMeshProUGUI dmgValueText;
        private TextMeshProUGUI dmgDeltaText;
        private TextMeshProUGUI fireRateValueText;
        private TextMeshProUGUI fireRateDeltaText;
        private TextMeshProUGUI rangeValueText;
        private TextMeshProUGUI spreadValueText;
        private TextMeshProUGUI bulletsValueText;

        // Actions: Upgrades
        private GameObject upgradeSectionContainer;
        private Button coinUpgradeButton;
        private Image coinUpgradeButtonImage;
        private TextMeshProUGUI coinUpgradePriceText;
        private Button cardUnlockButton;
        private Image cardUnlockButtonImage;
        private TextMeshProUGUI cardUnlockButtonText;
        private GameObject maxLevelBanner;

        // Actions: Equip
        private Button equipButton;
        private Image equipButtonImage;
        private TextMeshProUGUI equipButtonText;

        public void BindReferences(
            TextMeshProUGUI weaponNameText,
            TextMeshProUGUI rarityText,
            Image rarityBadgeImage,
            TextMeshProUGUI levelText,
            GameObject cardsContainer,
            Image cardsFillImage,
            TextMeshProUGUI cardsAmountText,
            TextMeshProUGUI cardsStatusText,
            TextMeshProUGUI powerValueText,
            TextMeshProUGUI powerBonusText,
            TextMeshProUGUI dmgValueText,
            TextMeshProUGUI dmgDeltaText,
            TextMeshProUGUI fireRateValueText,
            TextMeshProUGUI fireRateDeltaText,
            TextMeshProUGUI rangeValueText,
            TextMeshProUGUI spreadValueText,
            TextMeshProUGUI bulletsValueText,
            GameObject upgradeSectionContainer,
            Button coinUpgradeButton,
            Image coinUpgradeButtonImage,
            TextMeshProUGUI coinUpgradePriceText,
            Button cardUnlockButton,
            Image cardUnlockButtonImage,
            TextMeshProUGUI cardUnlockButtonText,
            GameObject maxLevelBanner,
            Button equipButton,
            Image equipButtonImage,
            TextMeshProUGUI equipButtonText
        )
        {
            this.weaponNameText = weaponNameText;
            this.rarityText = rarityText;
            this.rarityBadgeImage = rarityBadgeImage;
            this.levelText = levelText;

            this.cardsContainer = cardsContainer;
            this.cardsFillImage = cardsFillImage;
            this.cardsAmountText = cardsAmountText;
            this.cardsStatusText = cardsStatusText;

            this.powerValueText = powerValueText;
            this.powerBonusText = powerBonusText;
            this.dmgValueText = dmgValueText;
            this.dmgDeltaText = dmgDeltaText;
            this.fireRateValueText = fireRateValueText;
            this.fireRateDeltaText = fireRateDeltaText;
            this.rangeValueText = rangeValueText;
            this.spreadValueText = spreadValueText;
            this.bulletsValueText = bulletsValueText;

            this.upgradeSectionContainer = upgradeSectionContainer;
            this.coinUpgradeButton = coinUpgradeButton;
            this.coinUpgradeButtonImage = coinUpgradeButtonImage;
            this.coinUpgradePriceText = coinUpgradePriceText;
            this.cardUnlockButton = cardUnlockButton;
            this.cardUnlockButtonImage = cardUnlockButtonImage;
            this.cardUnlockButtonText = cardUnlockButtonText;
            this.maxLevelBanner = maxLevelBanner;

            this.equipButton = equipButton;
            this.equipButtonImage = equipButtonImage;
            this.equipButtonText = equipButtonText;
        }

        public void Initialise(UIWeaponPage page)
        {
            this.weaponPage = page;

            if (coinUpgradeButton != null)
            {
                coinUpgradeButton.onClick.RemoveAllListeners();
                coinUpgradeButton.onClick.AddListener(OnCoinUpgradeClicked);
            }

            if (cardUnlockButton != null)
            {
                cardUnlockButton.onClick.RemoveAllListeners();
                cardUnlockButton.onClick.AddListener(OnCardUnlockClicked);
            }

            if (equipButton != null)
            {
                equipButton.onClick.RemoveAllListeners();
                equipButton.onClick.AddListener(OnEquipClicked);
            }
        }

        public void DisplayWeapon(WeaponData weaponData, BaseWeaponUpgrade upgrade, int weaponIndex)
        {
            currentWeaponData = weaponData;
            currentUpgrade = upgrade;
            currentWeaponIndex = weaponIndex;

            if (weaponData == null || upgrade == null) return;

            // 1. HEADER
            if (weaponNameText != null) weaponNameText.text = weaponData.Name.ToUpper();
            if (rarityText != null)
            {
                rarityText.text = weaponData.RarityData.Name.ToUpper();
                rarityText.color = weaponData.RarityData.TextColor;
            }
            if (rarityBadgeImage != null)
            {
                rarityBadgeImage.color = weaponData.RarityData.MainColor;
            }

            bool isUnlocked = upgrade.UpgradeLevel > 0;
            if (levelText != null)
            {
                levelText.text = isUnlocked ? $"CẤP {upgrade.UpgradeLevel}" : "CHƯA MỞ KHÓA";
                levelText.color = isUnlocked ? new Color(0.4f, 0.9f, 1f) : new Color(1f, 0.5f, 0.5f);
            }

            // 2. CARDS PROGRESS
            UpdateCardsSection(isUnlocked);

            // 3. STATS HUD
            UpdateStatsSection();

            // 4. UPGRADE ACTIONS
            UpdateUpgradeSection(isUnlocked);

            // 5. EQUIP BUTTON
            UpdateEquipButton(isUnlocked);
        }

        private void UpdateCardsSection(bool isUnlocked)
        {
            if (cardsContainer == null) return;

            int currentCards = currentWeaponData.CardsAmount;

            if (!isUnlocked)
            {
                cardsContainer.SetActive(true);
                int target = currentUpgrade.NextStage != null ? currentUpgrade.NextStage.Price : 10;
                float fill = target > 0 ? Mathf.Clamp01((float)currentCards / target) : 1f;

                if (cardsFillImage != null) cardsFillImage.fillAmount = fill;
                if (cardsAmountText != null) cardsAmountText.text = $"{currentCards} / {target} THẺ";

                if (cardsStatusText != null)
                {
                    if (currentCards >= target)
                    {
                        cardsStatusText.text = "ĐÃ ĐỦ THẺ! SẴN SÀNG MỞ KHÓA";
                        cardsStatusText.color = new Color(0.2f, 1f, 0.45f);
                    }
                    else
                    {
                        cardsStatusText.text = $"CẦN THÊM {target - currentCards} THẺ ĐỂ MỞ KHÓA";
                        cardsStatusText.color = new Color(1f, 0.8f, 0.3f);
                    }
                }
            }
            else
            {
                if (!currentUpgrade.IsMaxedOut)
                {
                    cardsContainer.SetActive(true);
                    if (cardsFillImage != null) cardsFillImage.fillAmount = 1f;
                    if (cardsAmountText != null) cardsAmountText.text = $"{currentCards} THẺ SỞ HỮU";
                    if (cardsStatusText != null)
                    {
                        cardsStatusText.text = "TÍCH LŨY THẺ CHO CÁC MỐC ĐẶC BIỆT";
                        cardsStatusText.color = new Color(0.7f, 0.85f, 1f);
                    }
                }
                else
                {
                    cardsContainer.SetActive(false);
                }
            }
        }

        private void UpdateStatsSection()
        {
            BaseWeaponUpgradeStage currentStage = currentUpgrade.GetCurrentStage();
            BaseWeaponUpgradeStage nextStage = currentUpgrade.NextStage as BaseWeaponUpgradeStage;

            // Power
            float bonusDmgPercent = 0f;
            if (Application.isPlaying)
            {
                bonusDmgPercent = EquipmentController.GetTotalBonusStats().bonusDamagePercent;
            }
            int currentPower = Mathf.RoundToInt(currentStage.Power * (1f + bonusDmgPercent / 100f));
            if (powerValueText != null) powerValueText.text = currentPower.ToString();

            if (powerBonusText != null)
            {
                if (nextStage != null)
                {
                    int nextPower = Mathf.RoundToInt(nextStage.Power * (1f + bonusDmgPercent / 100f));
                    int delta = nextPower - currentPower;
                    powerBonusText.text = $"+{delta}";
                    powerBonusText.gameObject.SetActive(delta > 0);
                }
                else
                {
                    powerBonusText.gameObject.SetActive(false);
                }
            }

            // Damage
            if (dmgValueText != null)
            {
                if (currentStage.Damage.firstValue == currentStage.Damage.secondValue)
                    dmgValueText.text = currentStage.Damage.firstValue.ToString();
                else
                    dmgValueText.text = $"{currentStage.Damage.firstValue}-{currentStage.Damage.secondValue}";
            }
            if (dmgDeltaText != null)
            {
                if (nextStage != null)
                {
                    int deltaDmg = nextStage.Damage.firstValue - currentStage.Damage.firstValue;
                    dmgDeltaText.text = $"+{deltaDmg}";
                    dmgDeltaText.gameObject.SetActive(deltaDmg > 0);
                }
                else
                {
                    dmgDeltaText.gameObject.SetActive(false);
                }
            }

            // Fire Rate
            if (fireRateValueText != null) fireRateValueText.text = $"{currentStage.FireRate:0.0} v/s";
            if (fireRateDeltaText != null)
            {
                if (nextStage != null)
                {
                    float deltaRate = nextStage.FireRate - currentStage.FireRate;
                    if (Mathf.Abs(deltaRate) > 0.01f)
                    {
                        fireRateDeltaText.text = deltaRate > 0 ? $"+{deltaRate:0.0}" : $"{deltaRate:0.0}";
                        fireRateDeltaText.gameObject.SetActive(true);
                    }
                    else
                    {
                        fireRateDeltaText.gameObject.SetActive(false);
                    }
                }
                else
                {
                    fireRateDeltaText.gameObject.SetActive(false);
                }
            }

            // Range
            if (rangeValueText != null) rangeValueText.text = $"{currentStage.RangeRadius:0.0} m";

            // Spread
            if (spreadValueText != null) spreadValueText.text = $"{currentStage.Spread:0}°";

            // Bullets
            if (bulletsValueText != null) bulletsValueText.text = currentStage.BulletsPerShot.firstValue.ToString();
        }

        private void UpdateUpgradeSection(bool isUnlocked)
        {
            if (upgradeSectionContainer == null) return;

            if (!isUnlocked)
            {
                // Che do khoa: chi hien nut mo khoa bang the
                if (coinUpgradeButton != null) coinUpgradeButton.gameObject.SetActive(false);
                if (maxLevelBanner != null) maxLevelBanner.SetActive(false);

                if (cardUnlockButton != null)
                {
                    cardUnlockButton.gameObject.SetActive(true);
                    int targetCards = currentUpgrade.NextStage != null ? currentUpgrade.NextStage.Price : 10;
                    bool canUnlock = currentWeaponData.CardsAmount >= targetCards;

                    cardUnlockButton.interactable = canUnlock;
                    if (cardUnlockButtonImage != null)
                    {
                        cardUnlockButtonImage.color = canUnlock ? new Color(0.18f, 0.75f, 0.35f, 1f) : new Color(0.35f, 0.35f, 0.4f, 0.7f);
                    }
                    if (cardUnlockButtonText != null)
                    {
                        cardUnlockButtonText.text = canUnlock ? "MỞ KHÓA VŨ KHÍ" : "CHƯA ĐỦ THẺ";
                    }
                }
            }
            else
            {
                // Che do da mo khoa: hien nut nang cap bang vang hoac max banner
                if (cardUnlockButton != null) cardUnlockButton.gameObject.SetActive(false);

                if (currentUpgrade.IsMaxedOut)
                {
                    if (coinUpgradeButton != null) coinUpgradeButton.gameObject.SetActive(false);
                    if (maxLevelBanner != null) maxLevelBanner.SetActive(true);
                }
                else
                {
                    if (maxLevelBanner != null) maxLevelBanner.SetActive(false);
                    if (coinUpgradeButton != null)
                    {
                        coinUpgradeButton.gameObject.SetActive(true);
                        int price = currentUpgrade.NextStage.Price;
                        bool hasEnoughCoins = CurrenciesController.HasAmount(CurrencyType.Coins, price);

                        coinUpgradeButton.interactable = hasEnoughCoins;
                        if (coinUpgradeButtonImage != null)
                        {
                            coinUpgradeButtonImage.color = hasEnoughCoins ? new Color(0.95f, 0.74f, 0.18f, 1f) : new Color(0.5f, 0.45f, 0.3f, 0.7f);
                        }
                        if (coinUpgradePriceText != null)
                        {
                            coinUpgradePriceText.text = CurrenciesHelper.Format(price);
                        }
                    }
                }
            }
        }

        public void UpdateEquipButton(bool isUnlocked)
        {
            if (equipButton == null) return;

            if (!isUnlocked)
            {
                equipButton.interactable = false;
                if (equipButtonText != null) equipButtonText.text = "CHƯA MỞ KHÓA";
                if (equipButtonImage != null) equipButtonImage.color = new Color(0.25f, 0.25f, 0.3f, 0.8f);
            }
            else if (currentWeaponIndex == WeaponsController.SelectedWeaponIndex)
            {
                equipButton.interactable = false;
                if (equipButtonText != null) equipButtonText.text = "ĐANG TRANG BỊ";
                if (equipButtonImage != null) equipButtonImage.color = new Color(0.18f, 0.65f, 0.35f, 0.95f);
            }
            else
            {
                equipButton.interactable = true;
                if (equipButtonText != null) equipButtonText.text = "TRANG BỊ";
                if (equipButtonImage != null) equipButtonImage.color = new Color(0.15f, 0.55f, 0.95f, 1f);
            }
        }

        private void OnEquipClicked()
        {
            if (weaponPage == null || currentWeaponData == null) return;

            weaponPage.EquipWeapon(currentWeaponIndex);
            UpdateEquipButton(currentUpgrade.UpgradeLevel > 0);
        }

        private void OnCoinUpgradeClicked()
        {
            if (weaponPage == null || currentWeaponData == null || currentUpgrade == null) return;

            weaponPage.UpgradeWeaponWithCoins(currentWeaponData, currentUpgrade, currentWeaponIndex);
            DisplayWeapon(currentWeaponData, currentUpgrade, currentWeaponIndex);
        }

        private void OnCardUnlockClicked()
        {
            if (weaponPage == null || currentWeaponData == null || currentUpgrade == null) return;

            weaponPage.UnlockWeaponWithCards(currentWeaponData, currentUpgrade, currentWeaponIndex);
            DisplayWeapon(currentWeaponData, currentUpgrade, currentWeaponIndex);
        }
    }
}
