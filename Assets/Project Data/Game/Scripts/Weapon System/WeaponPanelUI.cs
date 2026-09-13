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

        public override bool IsUnlocked => Upgrade != null && Upgrade.UpgradeLevel > 0;
        private int weaponIndex;
        public int WeaponIndex => weaponIndex;

        private UIGamepadButton gamepadButton;
        public UIGamepadButton GamepadButton => gamepadButton;

        public Transform UpgradeButtonTransform => upgradesBuyButton != null ? upgradesBuyButton.transform : transform;

        private WeaponsController weaponController;
        private UIWeaponPage weaponPage;
        private GameObject equippedBadgeObject;

        public void Init(WeaponsController weaponController, BaseWeaponUpgrade upgrade, WeaponData data, int weaponIndex)
        {
            Data = data;
            Upgrade = upgrade;
            panelRectTransform = (RectTransform)transform;
            if (upgradesBuyButton != null)
                gamepadButton = upgradesBuyButton.GetComponent<UIGamepadButton>();

            this.weaponIndex = weaponIndex;
            this.weaponController = weaponController;
            this.weaponPage = UIController.GetPage<UIWeaponPage>();

            FormatCardLayout(data);

            if (weaponName != null) weaponName.text = data.Name;
            if (weaponImage != null)
            {
                weaponImage.sprite = data.Icon;
                weaponImage.preserveAspect = true;
            }
            if (weaponBackImage != null) weaponBackImage.color = new Color(data.RarityData.MainColor.r, data.RarityData.MainColor.g, data.RarityData.MainColor.b, 0.35f);
            if (rarityText != null)
            {
                rarityText.text = data.RarityData.Name;
                rarityText.color = data.RarityData.TextColor;
            }

            // An nut nang cap trong the vi thao tac da chuyen sang Cot Trai
            if (upgradesBuyButton != null)
            {
                upgradesBuyButton.gameObject.SetActive(false);
            }

            UpdateUI();
            UpdateSelectionState();

            WeaponsController.OnNewWeaponSelected -= UpdateSelectionState;
            WeaponsController.OnNewWeaponSelected += UpdateSelectionState;
        }

        private void FormatCardLayout(WeaponData data)
        {
            // Set kích thước card theo dạng thanh ngang gọn gàng (88px height)
            if (panelRectTransform != null)
            {
                panelRectTransform.sizeDelta = new Vector2(0f, 88f);
            }

            LayoutElement le = GetComponent<LayoutElement>();
            if (le == null) le = gameObject.AddComponent<LayoutElement>();
            le.minHeight = 88f;
            le.preferredHeight = 88f;
            le.flexibleWidth = 1f;

            // Background nền thẻ
            if (backgroundTransform != null)
            {
                RectTransform bgRt = (RectTransform)backgroundTransform;
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.offsetMin = Vector2.zero;
                bgRt.offsetMax = Vector2.zero;
                bgRt.sizeDelta = Vector2.zero;
                bgRt.anchoredPosition = Vector2.zero;
                bgRt.localScale = Vector3.one;

                Image bgImg = backgroundTransform.GetComponent<Image>();
                if (bgImg != null)
                {
                    bgImg.color = new Color(0.08f, 0.11f, 0.18f, 0.95f);
                }
            }

            // Selection Highlight viền neon khi súng được chọn
            if (selectionImage != null)
            {
                RectTransform selRt = selectionImage.rectTransform;
                selRt.anchorMin = Vector2.zero;
                selRt.anchorMax = Vector2.one;
                selRt.offsetMin = new Vector2(-2f, -2f);
                selRt.offsetMax = new Vector2(2f, 2f);
                selRt.sizeDelta = new Vector2(4f, 4f);
                selRt.anchoredPosition = Vector2.zero;
                selectionImage.color = new Color(0f, 0.85f, 1f, 0.95f);
            }

            // Icon nền bên trái
            if (weaponBackImage != null)
            {
                RectTransform backRt = weaponBackImage.rectTransform;
                backRt.anchorMin = new Vector2(0f, 0.5f);
                backRt.anchorMax = new Vector2(0f, 0.5f);
                backRt.pivot = new Vector2(0f, 0.5f);
                backRt.anchoredPosition = new Vector2(10f, 0f);
                backRt.sizeDelta = new Vector2(68f, 68f);
            }

            // Icon súng bên trái
            if (weaponImage != null)
            {
                RectTransform imgRt = weaponImage.rectTransform;
                imgRt.anchorMin = new Vector2(0.5f, 0.5f);
                imgRt.anchorMax = new Vector2(0.5f, 0.5f);
                imgRt.pivot = new Vector2(0.5f, 0.5f);
                imgRt.anchoredPosition = Vector2.zero;
                imgRt.sizeDelta = new Vector2(60f, 60f);
                weaponImage.preserveAspect = true;
                weaponImage.color = Color.white;
            }

            // Tên súng
            if (weaponName != null)
            {
                RectTransform nameRt = weaponName.rectTransform;
                nameRt.anchorMin = new Vector2(0f, 1f);
                nameRt.anchorMax = new Vector2(0f, 1f);
                nameRt.pivot = new Vector2(0f, 1f);
                nameRt.anchoredPosition = new Vector2(86f, -12f);
                nameRt.sizeDelta = new Vector2(120f, 22f);
                weaponName.fontSize = 16f;
                weaponName.fontStyle = FontStyles.Bold;
                weaponName.alignment = TextAlignmentOptions.Left;
                weaponName.color = Color.white;
            }

            // Rarity
            if (rarityText != null)
            {
                RectTransform rarRt = rarityText.rectTransform;
                rarRt.anchorMin = new Vector2(0f, 1f);
                rarRt.anchorMax = new Vector2(0f, 1f);
                rarRt.pivot = new Vector2(0f, 1f);
                rarRt.anchoredPosition = new Vector2(86f, -36f);
                rarRt.sizeDelta = new Vector2(100f, 18f);
                rarityText.fontSize = 12f;
                rarityText.fontStyle = FontStyles.Bold;
                rarityText.alignment = TextAlignmentOptions.Left;
            }

            // Level text
            if (levelText != null)
            {
                RectTransform lvlRt = levelText.rectTransform;
                lvlRt.anchorMin = new Vector2(0f, 0f);
                lvlRt.anchorMax = new Vector2(0f, 0f);
                lvlRt.pivot = new Vector2(0f, 0f);
                lvlRt.anchoredPosition = new Vector2(86f, 10f);
                lvlRt.sizeDelta = new Vector2(90f, 18f);
                levelText.fontSize = 12f;
                levelText.fontStyle = FontStyles.Normal;
                levelText.alignment = TextAlignmentOptions.Left;
                levelText.color = new Color(0.4f, 0.85f, 1f);
            }

            // An cac thanh phan cu khong can thiet trong the item
            if (powerObject != null) powerObject.SetActive(false);
            if (upgradesMaxObject != null) upgradesMaxObject.SetActive(false);
            if (upgradesBuyButton != null) upgradesBuyButton.gameObject.SetActive(false);
        }

        public bool IsNextUpgradeCanBePurchased()
        {
            if (IsUnlocked && Upgrade != null)
            {
                if (!Upgrade.IsMaxedOut && Upgrade.NextStage != null)
                {
                    if (CurrenciesController.HasAmount(CurrencyType.Coins, Upgrade.NextStage.Price))
                        return true;
                }
            }

            return false;
        }

        public void UpdateUI()
        {
            if (Upgrade == null) return;

            if (IsUnlocked)
            {
                UpdateUpgradeState();
            }
            else
            {
                UpdateLockedState();
            }

            UpdateEquippedBadge();
        }

        public void UpdateSelectionState()
        {
            if (weaponPage == null)
                weaponPage = UIController.GetPage<UIWeaponPage>();

            int previewIdx = (weaponPage != null) ? weaponPage.PreviewWeaponIndex : WeaponsController.SelectedWeaponIndex;

            if (selectionImage != null)
            {
                selectionImage.gameObject.SetActive(weaponIndex == previewIdx);
            }

            if (backgroundTransform != null)
            {
                backgroundTransform.localScale = Vector3.one;
            }

            UpdateEquippedBadge();
        }

        private void UpdateEquippedBadge()
        {
            bool isEquipped = (weaponIndex == WeaponsController.SelectedWeaponIndex);

            if (equippedBadgeObject == null)
            {
                Transform badgeTr = transform.Find("Equipped Badge");
                if (badgeTr != null)
                {
                    equippedBadgeObject = badgeTr.gameObject;
                }
                else
                {
                    equippedBadgeObject = new GameObject("Equipped Badge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    equippedBadgeObject.transform.SetParent(transform, false);
                    RectTransform rt = equippedBadgeObject.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(1f, 0.5f);
                    rt.anchorMax = new Vector2(1f, 0.5f);
                    rt.pivot = new Vector2(1f, 0.5f);
                    rt.anchoredPosition = new Vector2(-10f, 0f);
                    rt.sizeDelta = new Vector2(85f, 26f);

                    Image badgeImg = equippedBadgeObject.GetComponent<Image>();
                    badgeImg.color = new Color(0.12f, 0.65f, 0.35f, 0.95f);

                    GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                    textObj.transform.SetParent(equippedBadgeObject.transform, false);
                    RectTransform textRt = textObj.GetComponent<RectTransform>();
                    textRt.anchorMin = Vector2.zero;
                    textRt.anchorMax = Vector2.one;
                    textRt.sizeDelta = Vector2.zero;

                    TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
                    if (weaponName != null) tmp.font = weaponName.font;
                    tmp.text = "ĐANG DÙNG";
                    tmp.fontSize = 11f;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.color = Color.white;
                    tmp.alignment = TextAlignmentOptions.Center;
                }
            }

            if (equippedBadgeObject != null)
            {
                equippedBadgeObject.SetActive(isEquipped);
            }
        }

        private void UpdateLockedState()
        {
            if (lockedStateObject != null) lockedStateObject.SetActive(true);
            if (upgradeStateObject != null) upgradeStateObject.SetActive(false);

            if (Data != null && Upgrade != null && Upgrade.NextStage != null)
            {
                int currentAmount = Data.CardsAmount;
                int target = Upgrade.NextStage.Price;

                if (cardsFillImage != null) cardsFillImage.fillAmount = (float)currentAmount / target;
                if (cardsAmountText != null)
                {
                    cardsAmountText.text = currentAmount + "/" + target;
                    RectTransform cardAmtRt = cardsAmountText.rectTransform;
                    cardAmtRt.anchorMin = new Vector2(1f, 0.5f);
                    cardAmtRt.anchorMax = new Vector2(1f, 0.5f);
                    cardAmtRt.pivot = new Vector2(1f, 0.5f);
                    cardAmtRt.anchoredPosition = new Vector2(-10f, 0f);
                    cardAmtRt.sizeDelta = new Vector2(75f, 22f);
                    cardsAmountText.fontSize = 12f;
                    cardsAmountText.alignment = TextAlignmentOptions.Right;
                    cardsAmountText.color = new Color(1f, 0.85f, 0.3f);
                }
            }

            if (powerObject != null) powerObject.SetActive(false);
            if (powerText != null) powerText.gameObject.SetActive(false);
        }

        private void UpdateUpgradeState()
        {
            if (lockedStateObject != null) lockedStateObject.SetActive(false);
            if (upgradeStateObject != null) upgradeStateObject.SetActive(true);

            if (Upgrade.NextStage != null)
            {
                if (upgradePriceText != null) upgradePriceText.text = Upgrade.NextStage.Price.ToString();
                if (upgradeCurrencyImage != null)
                {
                    upgradeCurrencyImage.gameObject.SetActive(true);
                    upgradeCurrencyImage.sprite = CurrenciesController.GetCurrency(Upgrade.NextStage.CurrencyType).Icon;
                }
            }
            else
            {
                if (upgradePriceText != null) upgradePriceText.text = "MAX";
                if (upgradeCurrencyImage != null) upgradeCurrencyImage.gameObject.SetActive(false);
            }

            if (powerObject != null) powerObject.SetActive(true);
            if (powerText != null)
            {
                powerText.gameObject.SetActive(true);

                float bonusDmg = 0f;
                if (Application.isPlaying)
                {
                    bonusDmg = EquipmentController.GetTotalBonusStats().bonusDamagePercent;
                }
                int finalPower = Mathf.RoundToInt(Upgrade.GetCurrentStage().Power * (1f + bonusDmg / 100f));
                powerText.text = finalPower.ToString();
            }

            RedrawUpgradeElements();
        }

        private void RedrawUpgradeElements()
        {
            if (levelText != null) levelText.text = "CẤP " + Upgrade.UpgradeLevel;

            if (upgradesMaxObject != null)
            {
                upgradesMaxObject.SetActive(Upgrade.IsMaxedOut);
            }

            if (upgradesBuyButton != null)
            {
                // Luon an nut buy trong the danh sach vi cot trai da dam nhan
                upgradesBuyButton.gameObject.SetActive(false);
            }
        }

        protected override void RedrawUpgradeButton()
        {
            // Redundant for cards in preview list
        }

        public override void Select()
        {
            // Pure Preview Only: Cap nhat xem truoc o man hinh 3D va cot trai!
            // Nhan vat ben ngoai chi trang bi khi nguoi choi an "TRANG BI" o cot trai.
            if (weaponPage == null) weaponPage = UIController.GetPage<UIWeaponPage>();
            if (weaponPage != null)
            {
                AudioController.PlaySound(AudioController.Sounds.buttonSound);
                weaponPage.SelectWeaponForPreview(weaponIndex);
            }
        }

        public void UpgradeButton()
        {
            // Forwarded to left panel
        }

        private void OnDisable()
        {
            WeaponsController.OnNewWeaponSelected -= UpdateSelectionState;
        }
    }
}
