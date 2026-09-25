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

        // Cho Tutorial: Con trỏ chỉ dẫn trỏ thẳng vào nút Upgrade ở Cột Thông Số Bên Trái
        public Transform UpgradeButtonTransform
        {
            get
            {
                if (weaponPage != null && weaponPage.DetailsPanel != null && weaponPage.DetailsPanel.CoinUpgradeButton != null && weaponPage.DetailsPanel.CoinUpgradeButton.gameObject.activeInHierarchy)
                {
                    return weaponPage.DetailsPanel.CoinUpgradeButton.transform;
                }
                return upgradesBuyButton != null ? upgradesBuyButton.transform : transform;
            }
        }

        private WeaponsController weaponController;
        private UIWeaponPage weaponPage;
        private GameObject equipActionObject;

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

            UpdateUI();
            UpdateSelectionState();

            WeaponsController.OnNewWeaponSelected -= UpdateSelectionState;
            WeaponsController.OnNewWeaponSelected += UpdateSelectionState;
        }

        private void FormatCardLayout(WeaponData data)
        {
            if (panelRectTransform == null) panelRectTransform = (RectTransform)transform;
            if (panelRectTransform != null)
            {
                panelRectTransform.sizeDelta = new Vector2(0f, 155f);
            }

            LayoutElement le = GetComponent<LayoutElement>();
            if (le == null) le = gameObject.AddComponent<LayoutElement>();
            le.minHeight = 155f;
            le.preferredHeight = 155f;
            le.flexibleWidth = 1f;

            Color rarityColor = (data != null && data.RarityData != null) ? data.RarityData.MainColor : new Color(0f, 0.85f, 1f);

            // 1. Background — SurvivalClean dark navy
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
                    Sprite cardBg = UIWeaponPageBuilder.GetSurvivalCleanSprite("Frame_ListFrame03_White1");
                    if (cardBg != null)
                    {
                        bgImg.sprite = cardBg;
                        bgImg.type = Image.Type.Sliced;
                    }
                    bgImg.color = new Color(0.06f, 0.09f, 0.16f, 0.98f);
                }
            }

            // 2. Selection Highlight — SurvivalClean cyan glow
            if (selectionImage != null)
            {
                RectTransform selRt = selectionImage.rectTransform;
                selRt.anchorMin = Vector2.zero;
                selRt.anchorMax = Vector2.one;
                selRt.offsetMin = new Vector2(-4f, -4f);
                selRt.offsetMax = new Vector2(4f, 4f);
                selRt.sizeDelta = new Vector2(8f, 8f);
                selRt.anchoredPosition = Vector2.zero;
                Sprite glowSp = UIWeaponPageBuilder.GetSurvivalCleanSprite("Frame_LineFrame03_White4_Glow");
                if (glowSp != null)
                {
                    selectionImage.sprite = glowSp;
                    selectionImage.type = Image.Type.Sliced;
                }
                selectionImage.color = new Color(0f, 0.85f, 1f, 0.95f);
            }

            // 3. Left Rarity Accent Stripe
            SetupRarityStripe(rarityColor);

            // 4. Slot (Beveled Frame, Inner BG, Weapon Icon, Back Glow)
            SetupSlotLayout(rarityColor);

            // 5. Typography and Middle Info
            TMP_FontAsset font = UIWeaponPageBuilder.GetFont();

            // Weapon Name — Bold White with auto-sizing
            if (weaponName != null)
            {
                if (font != null) weaponName.font = font;
                RectTransform nameRt = weaponName.rectTransform;
                nameRt.anchorMin = new Vector2(0f, 0.5f);
                nameRt.anchorMax = new Vector2(0f, 0.5f);
                nameRt.pivot = new Vector2(0f, 0.5f);
                nameRt.anchoredPosition = new Vector2(155f, 18f);
                nameRt.sizeDelta = new Vector2(280f, 48f);
                weaponName.fontSize = 46f;
                weaponName.fontStyle = FontStyles.Bold;
                weaponName.alignment = TextAlignmentOptions.Left;
                weaponName.color = Color.white;
                weaponName.enableAutoSizing = true;
                weaponName.fontSizeMin = 30f;
                weaponName.fontSizeMax = 46f;
            }

            // Rarity Badge Pill Tag
            SetupRarityBadge(data, font, rarityColor);

            // 6. TUYỆT ĐỐI ẨN TẤT CẢ CÁC NÚT VÀ ELEMENT UPGRADE/POWER TRÊN THẺ
            HideAllUpgradeAndStatsElements();

            // 7. Cập nhật Nút mang vũ khí bên phải
            UpdateEquipAction();
        }

        private void HideAllUpgradeAndStatsElements()
        {
            if (levelText != null) levelText.gameObject.SetActive(false);
            if (upgradeStateObject != null) upgradeStateObject.SetActive(false);
            if (upgradesBuyButton != null) upgradesBuyButton.gameObject.SetActive(false);
            if (powerObject != null) powerObject.SetActive(false);
            if (powerText != null) powerText.gameObject.SetActive(false);
            if (upgradesMaxObject != null) upgradesMaxObject.SetActive(false);
            if (lockedStateObject != null) lockedStateObject.SetActive(false);
            if (cardsFillImage != null) cardsFillImage.gameObject.SetActive(false);
            if (cardsAmountText != null) cardsAmountText.gameObject.SetActive(false);

            Transform parent = backgroundTransform != null ? backgroundTransform : transform;
            string[] toHide = { "Power Panel", "Upgrade State", "Max Panel", "Lock State", "Level Text" };
            foreach (var name in toHide)
            {
                Transform t = parent.Find(name);
                if (t != null) t.gameObject.SetActive(false);
                Transform tRoot = transform.Find(name);
                if (tRoot != null) tRoot.gameObject.SetActive(false);
            }
        }

        private void SetupRarityStripe(Color rarityColor)
        {
            Transform parent = backgroundTransform != null ? backgroundTransform : transform;
            Transform stripeTr = parent.Find("Rarity Stripe");
            GameObject stripeObj;
            if (stripeTr != null)
            {
                stripeObj = stripeTr.gameObject;
            }
            else
            {
                stripeObj = UIWeaponPageBuilder.CreateUIObject("Rarity Stripe", parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            }

            RectTransform rt = stripeObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(5f, 0f);
            rt.sizeDelta = new Vector2(5f, 145f);

            Image img = stripeObj.GetComponent<Image>();
            if (img != null)
            {
                Sprite stripeSp = UIWeaponPageBuilder.GetSurvivalCleanSprite("Frame_LineFrame05_White1");
                if (stripeSp != null)
                {
                    img.sprite = stripeSp;
                    img.type = Image.Type.Sliced;
                }
                img.color = rarityColor;
                img.raycastTarget = false;
            }
        }

        private void SetupSlotLayout(Color rarityColor)
        {
            Transform iconBgTr = (backgroundTransform != null ? backgroundTransform : transform).Find("Icon Background");
            if (iconBgTr == null && weaponBackImage != null)
            {
                iconBgTr = weaponBackImage.transform;
            }

            if (iconBgTr != null)
            {
                RectTransform slotRt = (RectTransform)iconBgTr;
                slotRt.anchorMin = new Vector2(0f, 0.5f);
                slotRt.anchorMax = new Vector2(0f, 0.5f);
                slotRt.pivot = new Vector2(0f, 0.5f);
                slotRt.anchoredPosition = new Vector2(20f, 0f);
                slotRt.sizeDelta = new Vector2(120f, 120f);

                // Slot Inner BG (dark sci-fi backdrop)
                Transform slotBgTr = iconBgTr.Find("Slot BG");
                GameObject slotBgObj;
                if (slotBgTr != null)
                {
                    slotBgObj = slotBgTr.gameObject;
                }
                else
                {
                    slotBgObj = UIWeaponPageBuilder.CreateUIObject("Slot BG", iconBgTr, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    slotBgObj.transform.SetAsFirstSibling();
                }
                RectTransform sbRt = slotBgObj.GetComponent<RectTransform>();
                sbRt.anchorMin = Vector2.zero;
                sbRt.anchorMax = Vector2.one;
                sbRt.sizeDelta = Vector2.zero;
                sbRt.anchoredPosition = Vector2.zero;
                Image sbImg = slotBgObj.GetComponent<Image>();
                if (sbImg != null)
                {
                    Sprite sBg = UIWeaponPageBuilder.GetSurvivalCleanSprite("Frame_ItemFrame01_00_White");
                    if (sBg != null) { sbImg.sprite = sBg; sbImg.type = Image.Type.Sliced; }
                    sbImg.color = new Color(0.04f, 0.06f, 0.11f, 0.95f);
                    sbImg.raycastTarget = false;
                }

                // weaponBackImage (rarity glow)
                if (weaponBackImage != null)
                {
                    RectTransform backRt = weaponBackImage.rectTransform;
                    backRt.anchorMin = Vector2.zero;
                    backRt.anchorMax = Vector2.one;
                    backRt.sizeDelta = Vector2.zero;
                    backRt.anchoredPosition = Vector2.zero;
                    weaponBackImage.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.35f);
                }

                // weaponImage (centered weapon icon)
                if (weaponImage != null)
                {
                    RectTransform imgRt = weaponImage.rectTransform;
                    imgRt.anchorMin = new Vector2(0.5f, 0.5f);
                    imgRt.anchorMax = new Vector2(0.5f, 0.5f);
                    imgRt.pivot = new Vector2(0.5f, 0.5f);
                    imgRt.anchoredPosition = Vector2.zero;
                    imgRt.sizeDelta = new Vector2(96f, 96f);
                    weaponImage.preserveAspect = true;
                    weaponImage.color = Color.white;
                }

                // Slot Outer Frame (beveled metallic frame on top)
                Transform frameTr = iconBgTr.Find("Slot Frame");
                GameObject frameObj;
                if (frameTr != null)
                {
                    frameObj = frameTr.gameObject;
                }
                else
                {
                    frameObj = UIWeaponPageBuilder.CreateUIObject("Slot Frame", iconBgTr, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    frameObj.transform.SetAsLastSibling();
                }
                RectTransform sfRt = frameObj.GetComponent<RectTransform>();
                sfRt.anchorMin = Vector2.zero;
                sfRt.anchorMax = Vector2.one;
                sfRt.sizeDelta = Vector2.zero;
                sfRt.anchoredPosition = Vector2.zero;
                Image sfImg = frameObj.GetComponent<Image>();
                if (sfImg != null)
                {
                    Sprite sFrame = UIWeaponPageBuilder.GetSurvivalCleanSprite("Frame_ItemFrame01_n_White1");
                    if (sFrame != null) { sfImg.sprite = sFrame; sfImg.type = Image.Type.Sliced; }
                    sfImg.color = Color.Lerp(rarityColor, Color.white, 0.35f);
                    sfImg.raycastTarget = false;
                }
            }
        }

        private void SetupRarityBadge(WeaponData data, TMP_FontAsset font, Color rarityColor)
        {
            Transform parent = backgroundTransform != null ? backgroundTransform : transform;
            Transform badgeTr = parent.Find("Rarity Badge");
            GameObject badgeObj;
            if (badgeTr != null)
            {
                badgeObj = badgeTr.gameObject;
            }
            else
            {
                badgeObj = UIWeaponPageBuilder.CreateUIObject("Rarity Badge", parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            }

            RectTransform badgeRt = badgeObj.GetComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(0f, 0.5f);
            badgeRt.anchorMax = new Vector2(0f, 0.5f);
            badgeRt.pivot = new Vector2(0f, 0.5f);
            badgeRt.anchoredPosition = new Vector2(155f, -22f);
            badgeRt.sizeDelta = new Vector2(130f, 28f);

            Image badgeImg = badgeObj.GetComponent<Image>();
            if (badgeImg != null)
            {
                Sprite badgeSp = UIWeaponPageBuilder.GetSurvivalCleanSprite("Label_Label01_White1");
                if (badgeSp != null)
                {
                    badgeImg.sprite = badgeSp;
                    badgeImg.type = Image.Type.Sliced;
                }
                badgeImg.color = new Color(rarityColor.r * 0.35f, rarityColor.g * 0.35f, rarityColor.b * 0.35f, 0.95f);
                badgeImg.raycastTarget = false;
            }

            if (rarityText != null)
            {
                rarityText.transform.SetParent(badgeObj.transform, false);
                RectTransform rarRt = rarityText.rectTransform;
                rarRt.anchorMin = Vector2.zero;
                rarRt.anchorMax = Vector2.one;
                rarRt.offsetMin = Vector2.zero;
                rarRt.offsetMax = Vector2.zero;
                rarRt.sizeDelta = Vector2.zero;
                rarRt.anchoredPosition = Vector2.zero;

                if (font != null) rarityText.font = font;
                rarityText.fontSize = 20f;
                rarityText.fontStyle = FontStyles.Bold;
                rarityText.alignment = TextAlignmentOptions.Center;
                rarityText.color = Color.Lerp(rarityColor, Color.white, 0.65f);
            }
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
            HideAllUpgradeAndStatsElements();
            UpdateEquipAction();
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

            UpdateEquipAction();
        }

        private void UpdateEquipAction()
        {
            bool isEquipped = (weaponIndex == WeaponsController.SelectedWeaponIndex);
            bool isUnlocked = IsUnlocked;

            if (equipActionObject == null)
            {
                Transform badgeTr = transform.Find("Equip Action");
                if (badgeTr == null) badgeTr = transform.Find("Equipped Badge");
                if (badgeTr != null)
                {
                    equipActionObject = badgeTr.gameObject;
                    equipActionObject.name = "Equip Action";
                }
                else
                {
                    equipActionObject = UIWeaponPageBuilder.CreateUIObject("Equip Action", transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                }
            }

            if (equipActionObject != null)
            {
                RectTransform rt = equipActionObject.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1f, 0.5f);
                rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(1f, 0.5f);
                rt.anchoredPosition = new Vector2(-18f, 0f);
                rt.sizeDelta = new Vector2(175f, 52f);

                Image badgeImg = equipActionObject.GetComponent<Image>();
                Button equipBtn = equipActionObject.GetComponent<Button>();
                if (equipBtn == null) equipBtn = equipActionObject.AddComponent<Button>();

                equipBtn.onClick.RemoveAllListeners();

                // Icon (Checkmark or Lock)
                Transform iconTr = equipActionObject.transform.Find("Action Icon");
                if (iconTr == null) iconTr = equipActionObject.transform.Find("Check Icon");
                GameObject iconObj;
                if (iconTr != null)
                {
                    iconObj = iconTr.gameObject;
                    iconObj.name = "Action Icon";
                }
                else
                {
                    iconObj = UIWeaponPageBuilder.CreateUIObject("Action Icon", equipActionObject.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                }
                RectTransform iconRt = iconObj.GetComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0f, 0.5f);
                iconRt.anchorMax = new Vector2(0f, 0.5f);
                iconRt.pivot = new Vector2(0.5f, 0.5f);
                iconRt.anchoredPosition = new Vector2(24f, 0f);
                iconRt.sizeDelta = new Vector2(24f, 24f);
                Image iconImg = iconObj.GetComponent<Image>();
                if (iconImg != null) iconImg.raycastTarget = false;

                // Text
                Transform textTr = equipActionObject.transform.Find("Text");
                GameObject textObj;
                if (textTr != null)
                {
                    textObj = textTr.gameObject;
                }
                else
                {
                    textObj = UIWeaponPageBuilder.CreateUIObject("Text", equipActionObject.transform, typeof(RectTransform), typeof(TextMeshProUGUI));
                }
                RectTransform textRt = textObj.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;

                TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
                TMP_FontAsset font = UIWeaponPageBuilder.GetFont();
                if (font != null && tmp != null) tmp.font = font;

                if (isEquipped)
                {
                    // TRẠNG THÁI 1: ĐANG DÙNG (Equipped)
                    if (badgeImg != null)
                    {
                        Sprite badgeSp = UIWeaponPageBuilder.GetSurvivalCleanSprite("Label_Label01_White1");
                        if (badgeSp != null) { badgeImg.sprite = badgeSp; badgeImg.type = Image.Type.Sliced; }
                        badgeImg.color = new Color(0.12f, 0.65f, 0.33f, 0.95f);
                    }
                    if (iconImg != null)
                    {
                        iconObj.SetActive(true);
                        Sprite checkSp = UIWeaponPageBuilder.GetSurvivalCleanSprite("Icon_Check");
                        if (checkSp != null) iconImg.sprite = checkSp;
                        iconImg.color = Color.white;
                    }
                    if (tmp != null)
                    {
                        tmp.text = "ĐANG DÙNG";
                        tmp.fontSize = 24f;
                        tmp.fontStyle = FontStyles.Bold;
                        tmp.color = Color.white;
                        tmp.alignment = TextAlignmentOptions.Center;
                    }
                    textRt.offsetMin = new Vector2(36f, 0f);
                    textRt.offsetMax = new Vector2(-8f, 0f);

                    equipBtn.interactable = false;
                }
                else if (isUnlocked)
                {
                    // TRẠNG THÁI 2: ĐÃ MỞ KHÓA -> NÚT "MANG" (Equip)
                    if (badgeImg != null)
                    {
                        Sprite btnSp = UIWeaponPageBuilder.GetSurvivalCleanSprite("Label_Label01_White1");
                        if (btnSp != null) { badgeImg.sprite = btnSp; badgeImg.type = Image.Type.Sliced; }
                        badgeImg.color = new Color(0f, 0.62f, 0.88f, 1f);
                    }
                    if (iconObj != null) iconObj.SetActive(false);

                    if (tmp != null)
                    {
                        tmp.text = "MANG";
                        tmp.fontSize = 26f;
                        tmp.fontStyle = FontStyles.Bold;
                        tmp.color = Color.white;
                        tmp.alignment = TextAlignmentOptions.Center;
                    }
                    textRt.offsetMin = Vector2.zero;
                    textRt.offsetMax = Vector2.zero;

                    equipBtn.interactable = true;
                    equipBtn.onClick.AddListener(() =>
                    {
                        EquipThisWeapon();
                    });
                }
                else
                {
                    // TRẠNG THÁI 3: CHƯA MỞ KHÓA (Locked)
                    if (badgeImg != null)
                    {
                        Sprite badgeSp = UIWeaponPageBuilder.GetSurvivalCleanSprite("Label_Label01_White1");
                        if (badgeSp != null) { badgeImg.sprite = badgeSp; badgeImg.type = Image.Type.Sliced; }
                        badgeImg.color = new Color(0.18f, 0.22f, 0.3f, 0.85f);
                    }
                    if (iconImg != null)
                    {
                        iconObj.SetActive(true);
                        Sprite lockSp = UIWeaponPageBuilder.GetSurvivalCleanSprite("Icon_Lock");
                        if (lockSp != null) iconImg.sprite = lockSp;
                        iconImg.color = new Color(0.95f, 0.75f, 0.2f, 1f);
                    }
                    if (tmp != null)
                    {
                        int currentCards = (Data != null) ? Data.CardsAmount : 0;
                        int targetCards = (Upgrade != null && Upgrade.NextStage != null) ? Upgrade.NextStage.Price : 10;
                        tmp.text = currentCards > 0 ? $"{currentCards}/{targetCards}" : "CHƯA MỞ";
                        tmp.fontSize = 24f;
                        tmp.fontStyle = FontStyles.Bold;
                        tmp.color = new Color(0.85f, 0.88f, 0.95f, 1f);
                        tmp.alignment = TextAlignmentOptions.Center;
                    }
                    textRt.offsetMin = new Vector2(36f, 0f);
                    textRt.offsetMax = new Vector2(-8f, 0f);

                    equipBtn.interactable = false;
                }

                equipActionObject.SetActive(true);
            }
        }

        public void EquipThisWeapon()
        {
            if (weaponPage == null) weaponPage = UIController.GetPage<UIWeaponPage>();
            if (weaponPage != null)
            {
                weaponPage.EquipWeapon(weaponIndex);
            }
        }

        public override void Select()
        {
            // Preview on 3D character and Left Details Panel
            if (weaponPage == null) weaponPage = UIController.GetPage<UIWeaponPage>();
            if (weaponPage != null)
            {
                AudioController.PlaySound(AudioController.Sounds.buttonSound);
                weaponPage.SelectWeaponForPreview(weaponIndex);
            }
        }

        public void UpgradeButton()
        {
            // Forwarded to left details panel
        }

        private void OnDisable()
        {
            WeaponsController.OnNewWeaponSelected -= UpdateSelectionState;
        }
    }
}
