using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class WeaponPanelUI : UIUpgradeAbstractPanel
    {
        [SerializeField] TextMeshProUGUI weaponName;
        [SerializeField] Image weaponImage;
        [SerializeField] Image weaponBackImage;
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

        public WeaponData Data { get; private set; }

        private BaseWeaponUpgrade Upgrade { get; set; }

        [Space]
        [SerializeField] Button upgradesBuyButton;
        [SerializeField] Image upgradesBuyButtonImage;
        [SerializeField] TextMeshProUGUI upgradesBuyButtonText;
        [SerializeField] Sprite upgradesBuyButtonActiveSprite;
        [SerializeField] Sprite upgradesBuyButtonDisableSprite;

        [Space]
        [SerializeField] GameObject upgradesMaxObject;

        public override bool IsUnlocked => Upgrade.UpgradeLevel > 0;
        private int weaponIndex;
        public int WeaponIndex => weaponIndex;

        private UIGamepadButton gamepadButton;
        public UIGamepadButton GamepadButton => gamepadButton;

        public Transform UpgradeButtonTransform => upgradesBuyButton.transform;

        private WeaponsController weaponController;

        public void Init(WeaponsController weaponController, BaseWeaponUpgrade upgrade, WeaponData data, int weaponIndex)
        {
            Data = data;
            Upgrade = upgrade;
            panelRectTransform = (RectTransform)transform;
            gamepadButton = upgradesBuyButton.GetComponent<UIGamepadButton>();

            this.weaponIndex = weaponIndex;
            this.weaponController = weaponController;

            weaponName.text = data.Name;
            weaponImage.sprite = data.Icon;
            weaponBackImage.color = data.RarityData.MainColor;
            rarityText.text = data.RarityData.Name;
            rarityText.color = data.RarityData.TextColor;

            UpdateUI();
            UpdateSelectionState();

            WeaponsController.OnNewWeaponSelected += UpdateSelectionState;
        }

        public bool IsNextUpgradeCanBePurchased()
        {
            if (IsUnlocked)
            {
                if (!Upgrade.IsMaxedOut)
                {
                    if (CurrenciesController.HasAmount(CurrencyType.Coins, Upgrade.NextStage.Price))
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
            if (weaponIndex == WeaponsController.SelectedWeaponIndex)
            {
                selectionImage.gameObject.SetActive(true);
                backgroundTransform.localScale = Vector3.one;
            }
            else
            {
                selectionImage.gameObject.SetActive(false);
                backgroundTransform.localScale = Vector3.one;
            }

            UpdateUI();
        }

        private void UpdateLockedState()
        {
            lockedStateObject.SetActive(true);
            upgradeStateObject.SetActive(false);

            int currentAmount = Data.CardsAmount;
            int target = Upgrade.NextStage.Price;

            cardsFillImage.fillAmount = (float)currentAmount / target;
            cardsAmountText.text = currentAmount + "/" + target;

            powerObject.SetActive(false);
            powerText.gameObject.SetActive(false);
        }

        private void UpdateUpgradeState()
        {
            lockedStateObject.SetActive(false);
            upgradeStateObject.SetActive(true);

            if (Upgrade.NextStage != null)
            {
                upgradePriceText.text = Upgrade.NextStage.Price.ToString();
                upgradeCurrencyImage.sprite = CurrenciesController.GetCurrency(Upgrade.NextStage.CurrencyType).Icon;
            }
            else
            {
                upgradePriceText.text = "MAXED OUT";
                upgradeCurrencyImage.gameObject.SetActive(false);
            }

            powerObject.SetActive(true);
            powerText.gameObject.SetActive(true);
            powerText.text = Upgrade.GetCurrentStage().Power.ToString();

            RedrawUpgradeElements();
        }

        private void RedrawUpgradeElements()
        {
            levelText.text = "LEVEL " + Upgrade.UpgradeLevel;

            if (!Upgrade.IsMaxedOut)
            {
                upgradesMaxObject.SetActive(false);
                upgradesBuyButton.gameObject.SetActive(true);

                RedrawUpgradeButton();
            }
            else
            {
                upgradesMaxObject.SetActive(true);
                upgradesBuyButton.gameObject.SetActive(false);

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
                    upgradesBuyButtonImage.sprite = upgradesBuyButtonActiveSprite;

                    if (gamepadButton != null)
                        gamepadButton.SetFocus(weaponIndex == WeaponsController.SelectedWeaponIndex);
                }
                else
                {
                    upgradesBuyButtonImage.sprite = upgradesBuyButtonDisableSprite;

                    if (gamepadButton != null)
                        gamepadButton.SetFocus(false);
                }

                upgradesBuyButtonText.text = CurrenciesHelper.Format(price);

            }
        }

        public override void Select()
        {
            if (IsUnlocked)
            {
                if (weaponIndex != WeaponsController.SelectedWeaponIndex)
                {
                    AudioController.PlaySound(AudioController.Sounds.buttonSound);

                    weaponController.OnWeaponSelected(weaponIndex);
                }

                UIGeneralPowerIndicator.UpdateText();
            }
        }

        public void UpgradeButton()
        {
            if (Upgrade.NextStage.Price <= CurrenciesController.GetCurrency(Upgrade.NextStage.CurrencyType).Amount)
            {
                Select();

                CurrenciesController.Add(Upgrade.NextStage.CurrencyType, -Upgrade.NextStage.Price);
                Upgrade.UpgradeStage();

                weaponController.WeaponUpgraded(Data);

                AudioController.PlaySound(AudioController.Sounds.buttonSound);

                UIGeneralPowerIndicator.UpdateText(true);
            }
        }

        private void OnDisable()
        {
            WeaponsController.OnNewWeaponSelected += UpdateSelectionState;
        }
    }
}