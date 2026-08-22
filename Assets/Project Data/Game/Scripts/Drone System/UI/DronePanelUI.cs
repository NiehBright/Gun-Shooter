using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class DronePanelUI : UIUpgradeAbstractPanel
    {
        [SerializeField] TextMeshProUGUI droneName;
        [SerializeField] Image droneImage;
        [SerializeField] Image droneBackImage;
        [SerializeField] TextMeshProUGUI rarityText;

        [Header("Locked State")]
        [SerializeField] GameObject lockedStateObject;
        [SerializeField] SlicedFilledImage cardsFillImage;
        [SerializeField] TextMeshProUGUI cardsAmountText;

        [Header("Upgrade State")]
        [SerializeField] TextMeshProUGUI levelText;
        [SerializeField] GameObject upgradeStateObject;
        [SerializeField] TextMeshProUGUI upgradePriceText;
        [SerializeField] Image upgradeCurrencyImage;

        [Space]
        [SerializeField] Color upgradeStateActiveColor = Color.white;
        [SerializeField] Color upgradeStateUnactiveColor = Color.white;
        [SerializeField] Image[] upgradesStatesImages;

        public DroneData Data { get; private set; }

        private BaseDroneUpgrade Upgrade { get; set; }

        [Space]
        [SerializeField] Button upgradesBuyButton;
        [SerializeField] Image upgradesBuyButtonImage;
        [SerializeField] TextMeshProUGUI upgradesBuyButtonText;
        [SerializeField] Sprite upgradesBuyButtonActiveSprite;
        [SerializeField] Sprite upgradesBuyButtonDisableSprite;

        [Space]
        [SerializeField] GameObject upgradesMaxObject;



        public override bool IsUnlocked => Data != null && Data.Save != null && Data.Save.IsOwned;
        private int droneIndex;
        public int DroneIndex => droneIndex;

        private UIGamepadButton gamepadButton;
        public UIGamepadButton GamepadButton => gamepadButton;

        public Transform UpgradeButtonTransform => upgradesBuyButton.transform;

        public void Init(BaseDroneUpgrade upgrade, DroneData data, int droneIndex)
        {
            Data = data;
            Upgrade = upgrade;
            panelRectTransform = (RectTransform)transform;
            if (upgradesBuyButton != null)
            {
                gamepadButton = upgradesBuyButton.GetComponent<UIGamepadButton>();
                upgradesBuyButton.onClick.AddListener(UpgradeButton);
            }

            this.droneIndex = droneIndex;

            if (droneName != null) droneName.text = data.Name;
            if (droneImage != null) droneImage.sprite = data.Icon;
            if (droneBackImage != null && data.RarityData != null) droneBackImage.color = data.RarityData.MainColor;
            if (rarityText != null && data.RarityData != null)
            {
                rarityText.text = data.RarityData.Name;
                rarityText.color = data.RarityData.TextColor;
            }

            UpdateUI();
            UpdateSelectionState();

            DronesController.OnNewDroneSelected += UpdateSelectionState;
            DronesController.OnDroneCardsAmountChanged += UpdateUI;
            DronesController.OnDroneUpgraded += UpdateUI;
        }

        public bool IsNextUpgradeCanBePurchased()
        {
            if (IsUnlocked)
            {
                if (!Upgrade.IsMaxedOut)
                {
                    if (CurrenciesController.HasAmount(Upgrade.NextStage.CurrencyType, Upgrade.NextStage.Price))
                        return true;
                }
            }

            return false;
        }

        public void UpdateUI()
        {
            if (IsUnlocked)
            {
                UpdateUpgradeState();
            }
            else
            {
                UpdateLockedState();
            }
        }

        private void UpdateSelectionState()
        {
            if (droneIndex == DronesController.SelectedDroneIndex)
            {
                if (selectionImage != null) selectionImage.gameObject.SetActive(true);
                if (backgroundTransform != null) backgroundTransform.localScale = Vector3.one;
            }
            else
            {
                if (selectionImage != null) selectionImage.gameObject.SetActive(false);
                if (backgroundTransform != null) backgroundTransform.localScale = Vector3.one;
            }

            UpdateUI();
        }

        private void UpdateLockedState()
        {
            if (lockedStateObject != null) lockedStateObject.SetActive(true);
            if (upgradeStateObject != null) upgradeStateObject.SetActive(false);

            int currentAmount = Data != null ? Data.CardsAmount : 0;
            int target = 1;
            if (Upgrade != null && Upgrade.NextStage != null && Upgrade.NextStage is BaseDroneUpgradeStage)
            {
                target = ((BaseDroneUpgradeStage)Upgrade.NextStage).CardsRequired;
            }

            if (cardsFillImage != null) cardsFillImage.fillAmount = target > 0 ? (float)currentAmount / target : 1f;
            if (cardsAmountText != null) cardsAmountText.text = currentAmount + "/" + target;

            if (powerObject != null) powerObject.SetActive(false);
            if (powerText != null) powerText.gameObject.SetActive(false);
        }

        private void UpdateUpgradeState()
        {
            if (lockedStateObject != null) lockedStateObject.SetActive(false);
            if (upgradeStateObject != null) upgradeStateObject.SetActive(true);

            if (Upgrade != null && Upgrade.NextStage != null)
            {
                BaseDroneUpgradeStage nextStage = Upgrade.NextStage as BaseDroneUpgradeStage;
                if (upgradePriceText != null) upgradePriceText.text = nextStage != null ? nextStage.CardsRequired.ToString() : Upgrade.NextStage.Price.ToString();
                
                if (upgradeCurrencyImage != null)
                {
                    // If we use cards, maybe we hide currency image or change it to card icon
                    // For now, let's just hide it since it uses Cards
                    upgradeCurrencyImage.gameObject.SetActive(false);
                }
            }
            else
            {
                if (upgradePriceText != null) upgradePriceText.text = "MAXED OUT";
                if (upgradeCurrencyImage != null) upgradeCurrencyImage.gameObject.SetActive(false);
            }

            if (powerObject != null) powerObject.SetActive(true);
            if (powerText != null)
            {
                powerText.gameObject.SetActive(true);
                int finalPower = Mathf.RoundToInt(Upgrade.GetCurrentStage().Power);
                powerText.text = finalPower.ToString();
            }

            RedrawUpgradeElements();
        }

        private void RedrawUpgradeElements()
        {
            if (levelText != null) levelText.text = "LEVEL " + Upgrade.UpgradeLevel;

            if (!Upgrade.IsMaxedOut)
            {
                if (upgradesMaxObject != null) upgradesMaxObject.SetActive(false);
                if (upgradesBuyButton != null) upgradesBuyButton.gameObject.SetActive(true);

                RedrawUpgradeButton();
            }
            else
            {
                if (upgradesMaxObject != null) upgradesMaxObject.SetActive(true);
                if (upgradesBuyButton != null) upgradesBuyButton.gameObject.SetActive(false);

                if (gamepadButton != null)
                    gamepadButton.SetFocus(false);
            }
        }

        protected override void RedrawUpgradeButton()
        {
            if (!Upgrade.IsMaxedOut)
            {
                int price = Upgrade.NextStage.Price;
                CurrencyType currencyType = Upgrade.NextStage.CurrencyType;

                if (CurrenciesController.HasAmount(currencyType, price))
                {
                    if (upgradesBuyButtonImage != null && upgradesBuyButtonActiveSprite != null) upgradesBuyButtonImage.sprite = upgradesBuyButtonActiveSprite;

                    if (gamepadButton != null)
                        gamepadButton.SetFocus(droneIndex == DronesController.SelectedDroneIndex);
                }
                else
                {
                    if (upgradesBuyButtonImage != null && upgradesBuyButtonDisableSprite != null) upgradesBuyButtonImage.sprite = upgradesBuyButtonDisableSprite;

                    if (gamepadButton != null)
                        gamepadButton.SetFocus(false);
                }

                if (upgradesBuyButtonText != null) upgradesBuyButtonText.text = CurrenciesHelper.Format(price);
            }
        }

        public override void Select()
        {
            if (IsUnlocked)
            {
                if (droneIndex != DronesController.SelectedDroneIndex)
                {
                    AudioController.PlaySound(AudioController.Sounds.buttonSound);
                    DronesController.SelectDrone(Data.Type);
                }

                UIGeneralPowerIndicator.UpdateText();
            }
        }

        public void UpgradeButton()
        {
            if (CurrenciesController.HasAmount(Upgrade.NextStage.CurrencyType, Upgrade.NextStage.Price))
            {
                Select();

                DronesController.OnUpgradeBuyed(Data);

                AudioController.PlaySound(AudioController.Sounds.buttonSound);

                UIGeneralPowerIndicator.UpdateText(true);
            }
            else
            {
                Debug.LogWarning("[DronePanelUI] Không đủ tiền để nâng cấp! Giá: " + Upgrade.NextStage.Price);
            }
        }

        private void OnDestroy()
        {
            DronesController.OnNewDroneSelected -= UpdateSelectionState;
            DronesController.OnDroneCardsAmountChanged -= UpdateUI;
            DronesController.OnDroneUpgraded -= UpdateUI;
        }
    }
}
