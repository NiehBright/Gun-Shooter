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
            if (panelRectTransform == null) panelRectTransform = (RectTransform)transform;
            if (panelRectTransform != null)
            {
                panelRectTransform.sizeDelta = new Vector2(0f, 165f);
            }

            LayoutElement le = GetComponent<LayoutElement>();
            if (le == null) le = gameObject.AddComponent<LayoutElement>();
            le.minHeight = 165f;
            le.preferredHeight = 165f;
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
                nameRt.anchorMin = new Vector2(0f, 1f);
                nameRt.anchorMax = new Vector2(0f, 1f);
                nameRt.pivot = new Vector2(0f, 1f);
                nameRt.anchoredPosition = new Vector2(164f, -14f);
                nameRt.sizeDelta = new Vector2(290f, 58f);
                weaponName.fontSize = 54f;
                weaponName.fontStyle = FontStyles.Bold;
                weaponName.alignment = TextAlignmentOptions.Left;
                weaponName.color = Color.white;
                weaponName.enableAutoSizing = true;
                weaponName.fontSizeMin = 34f;
                weaponName.fontSizeMax = 54f;
            }

            // Rarity Badge Pill Tag
            SetupRarityBadge(data, font, rarityColor);

            // Level Text — Cyan Neon
            if (levelText != null)
            {
                if (font != null) levelText.font = font;
                RectTransform lvlRt = levelText.rectTransform;
                lvlRt.anchorMin = new Vector2(0f, 0f);
                lvlRt.anchorMax = new Vector2(0f, 0f);
                lvlRt.pivot = new Vector2(0f, 0f);
                lvlRt.anchoredPosition = new Vector2(164f, 16f);
                lvlRt.sizeDelta = new Vector2(200f, 42f);
                levelText.fontSize = 40f;
                levelText.fontStyle = FontStyles.Bold;
                levelText.alignment = TextAlignmentOptions.Left;
                levelText.color = new Color(0f, 0.85f, 1f);
            }

            // Hide old upgrade elements
            if (powerObject != null) powerObject.SetActive(false);
            if (powerText != null) powerText.gameObject.SetActive(false);
            if (upgradesMaxObject != null) upgradesMaxObject.SetActive(false);
            if (upgradesBuyButton != null) upgradesBuyButton.gameObject.SetActive(false);
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
            rt.sizeDelta = new Vector2(5f, 153f);

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
                slotRt.sizeDelta = new Vector2(128f, 128f);

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
                    imgRt.sizeDelta = new Vector2(104f, 104f);
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
            badgeRt.anchorMin = new Vector2(0f, 1f);
            badgeRt.anchorMax = new Vector2(0f, 1f);
            badgeRt.pivot = new Vector2(0f, 1f);
            badgeRt.anchoredPosition = new Vector2(164f, -74f);
            badgeRt.sizeDelta = new Vector2(140f, 34f);

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
                rarityText.fontSize = 24f;
                rarityText.fontStyle = FontStyles.Bold;
                rarityText.alignment = TextAlignmentOptions.Center;
                rarityText.color = Color.Lerp(rarityColor, Color.white, 0.6f);
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
                    equippedBadgeObject = UIWeaponPageBuilder.CreateUIObject("Equipped Badge", transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                }
            }

            if (equippedBadgeObject != null)
            {
                RectTransform rt = equippedBadgeObject.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1f, 0.5f);
                rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(1f, 0.5f);
                rt.anchoredPosition = new Vector2(-18f, 0f);
                rt.sizeDelta = new Vector2(195f, 54f);

                Image badgeImg = equippedBadgeObject.GetComponent<Image>();
                if (badgeImg != null)
                {
                    Sprite badgeSp = UIWeaponPageBuilder.GetSurvivalCleanSprite("Label_Label01_White1");
                    if (badgeSp != null)
                    {
                        badgeImg.sprite = badgeSp;
                        badgeImg.type = Image.Type.Sliced;
                    }
                    badgeImg.color = new Color(0.12f, 0.65f, 0.33f, 0.95f);
                }

                // Check Icon
                Transform checkTr = equippedBadgeObject.transform.Find("Check Icon");
                GameObject checkObj;
                if (checkTr != null)
                {
                    checkObj = checkTr.gameObject;
                }
                else
                {
                    checkObj = UIWeaponPageBuilder.CreateUIObject("Check Icon", equippedBadgeObject.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                }
                RectTransform checkRt = checkObj.GetComponent<RectTransform>();
                checkRt.anchorMin = new Vector2(0f, 0.5f);
                checkRt.anchorMax = new Vector2(0f, 0.5f);
                checkRt.pivot = new Vector2(0.5f, 0.5f);
                checkRt.anchoredPosition = new Vector2(26f, 0f);
                checkRt.sizeDelta = new Vector2(26f, 26f);
                Image checkImg = checkObj.GetComponent<Image>();
                if (checkImg != null)
                {
                    Sprite checkSp = UIWeaponPageBuilder.GetSurvivalCleanSprite("Icon_Check");
                    if (checkSp != null) checkImg.sprite = checkSp;
                    checkImg.color = Color.white;
                    checkImg.raycastTarget = false;
                }

                // Text
                Transform textTr = equippedBadgeObject.transform.Find("Text");
                GameObject textObj;
                if (textTr != null)
                {
                    textObj = textTr.gameObject;
                }
                else
                {
                    textObj = UIWeaponPageBuilder.CreateUIObject("Text", equippedBadgeObject.transform, typeof(RectTransform), typeof(TextMeshProUGUI));
                }
                RectTransform textRt = textObj.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = new Vector2(44f, 0f);
                textRt.offsetMax = new Vector2(-10f, 0f);

                TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    TMP_FontAsset font = UIWeaponPageBuilder.GetFont();
                    if (font != null) tmp.font = font;
                    else if (weaponName != null) tmp.font = weaponName.font;
                    tmp.text = "ĐANG DÙNG";
                    tmp.fontSize = 28f;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.color = Color.white;
                    tmp.alignment = TextAlignmentOptions.Center;
                }

                equippedBadgeObject.SetActive(isEquipped);
            }
        }

        private void UpdateLockedState()
        {
            if (lockedStateObject != null)
            {
                lockedStateObject.SetActive(true);
                Image lockedBg = lockedStateObject.GetComponent<Image>();
                if (lockedBg != null) lockedBg.color = Color.clear;

                RectTransform lockRt = lockedStateObject.GetComponent<RectTransform>();
                if (lockRt != null)
                {
                    lockRt.anchorMin = new Vector2(1f, 0.5f);
                    lockRt.anchorMax = new Vector2(1f, 0.5f);
                    lockRt.pivot = new Vector2(1f, 0.5f);
                    lockRt.anchoredPosition = new Vector2(-18f, 0f);
                    lockRt.sizeDelta = new Vector2(195f, 65f);
                    lockRt.localScale = Vector3.one;
                }

                // Lock Icon
                Transform lockIconTr = lockedStateObject.transform.Find("Lock Icon");
                GameObject lockIconObj;
                if (lockIconTr != null)
                {
                    lockIconObj = lockIconTr.gameObject;
                }
                else
                {
                    lockIconObj = UIWeaponPageBuilder.CreateUIObject("Lock Icon", lockedStateObject.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                }
                RectTransform liRt = lockIconObj.GetComponent<RectTransform>();
                liRt.anchorMin = new Vector2(0f, 1f);
                liRt.anchorMax = new Vector2(0f, 1f);
                liRt.pivot = new Vector2(0f, 1f);
                liRt.anchoredPosition = new Vector2(6f, -4f);
                liRt.sizeDelta = new Vector2(26f, 26f);
                Image liImg = lockIconObj.GetComponent<Image>();
                if (liImg != null)
                {
                    Sprite lockSp = UIWeaponPageBuilder.GetSurvivalCleanSprite("Icon_Lock");
                    if (lockSp != null) liImg.sprite = lockSp;
                    liImg.color = new Color(1f, 0.78f, 0.25f, 1f);
                    liImg.raycastTarget = false;
                }

                int currentAmount = 0;
                int target = 1;
                if (Data != null && Upgrade != null && Upgrade.NextStage != null)
                {
                    currentAmount = Data.CardsAmount;
                    target = Upgrade.NextStage.Price;
                }

                // cardsAmountText
                if (cardsAmountText != null)
                {
                    cardsAmountText.text = $"{currentAmount}/{target}";
                    RectTransform cardAmtRt = cardsAmountText.rectTransform;
                    cardAmtRt.anchorMin = new Vector2(0f, 1f);
                    cardAmtRt.anchorMax = new Vector2(1f, 1f);
                    cardAmtRt.pivot = new Vector2(1f, 1f);
                    cardAmtRt.anchoredPosition = new Vector2(0f, 0f);
                    cardAmtRt.sizeDelta = new Vector2(0f, 32f);
                    TMP_FontAsset font = UIWeaponPageBuilder.GetFont();
                    if (font != null) cardsAmountText.font = font;
                    cardsAmountText.fontSize = 32f;
                    cardsAmountText.fontStyle = FontStyles.Bold;
                    cardsAmountText.alignment = TextAlignmentOptions.Right;
                    cardsAmountText.color = new Color(1f, 0.78f, 0.25f, 1f);
                }

                // Progress Bar Frame + Fill
                Transform barFrameTr = lockedStateObject.transform.Find("Progress Bar");
                GameObject barFrameObj;
                if (barFrameTr != null)
                {
                    barFrameObj = barFrameTr.gameObject;
                }
                else
                {
                    barFrameObj = UIWeaponPageBuilder.CreateUIObject("Progress Bar", lockedStateObject.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                }
                RectTransform bfRt = barFrameObj.GetComponent<RectTransform>();
                bfRt.anchorMin = new Vector2(0f, 0f);
                bfRt.anchorMax = new Vector2(1f, 0f);
                bfRt.pivot = new Vector2(0.5f, 0f);
                bfRt.anchoredPosition = new Vector2(0f, 6f);
                bfRt.sizeDelta = new Vector2(0f, 14f);
                Image bfImg = barFrameObj.GetComponent<Image>();
                if (bfImg != null)
                {
                    Sprite barFrameSp = UIWeaponPageBuilder.GetSurvivalCleanSprite("Slider06_White1_Frame");
                    if (barFrameSp != null)
                    {
                        bfImg.sprite = barFrameSp;
                        bfImg.type = Image.Type.Sliced;
                    }
                    bfImg.color = new Color(0.20f, 0.25f, 0.35f, 0.95f);
                    bfImg.raycastTarget = false;
                }

                Transform barFillTr = barFrameObj.transform.Find("Fill");
                GameObject barFillObj;
                if (barFillTr != null)
                {
                    barFillObj = barFillTr.gameObject;
                }
                else
                {
                    barFillObj = UIWeaponPageBuilder.CreateUIObject("Fill", barFrameObj.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                }
                RectTransform fillRt = barFillObj.GetComponent<RectTransform>();
                fillRt.anchorMin = Vector2.zero;
                fillRt.anchorMax = Vector2.one;
                fillRt.offsetMin = new Vector2(2f, 2f);
                fillRt.offsetMax = new Vector2(-2f, -2f);
                Image fillImg = barFillObj.GetComponent<Image>();
                if (fillImg != null)
                {
                    Sprite barFillSp = UIWeaponPageBuilder.GetSurvivalCleanSprite("Slider06_White3_Fill1");
                    if (barFillSp != null)
                    {
                        fillImg.sprite = barFillSp;
                        fillImg.type = Image.Type.Filled;
                        fillImg.fillMethod = Image.FillMethod.Horizontal;
                        fillImg.fillOrigin = 0;
                    }
                    fillImg.color = new Color(0f, 0.85f, 1f, 1f);
                    fillImg.fillAmount = target > 0 ? Mathf.Clamp01((float)currentAmount / target) : 0f;
                    fillImg.raycastTarget = false;
                }

                if (cardsFillImage != null) cardsFillImage.gameObject.SetActive(false);
            }

            if (upgradeStateObject != null) upgradeStateObject.SetActive(false);
            if (powerObject != null) powerObject.SetActive(false);
            if (powerText != null) powerText.gameObject.SetActive(false);
            if (upgradesMaxObject != null) upgradesMaxObject.SetActive(false);
            if (upgradesBuyButton != null) upgradesBuyButton.gameObject.SetActive(false);
        }

        private void UpdateUpgradeState()
        {
            if (lockedStateObject != null) lockedStateObject.SetActive(false);
            if (upgradeStateObject != null) upgradeStateObject.SetActive(false);

            if (powerObject != null) powerObject.SetActive(false);
            if (powerText != null) powerText.gameObject.SetActive(false);
            if (upgradesMaxObject != null) upgradesMaxObject.SetActive(false);
            if (upgradesBuyButton != null) upgradesBuyButton.gameObject.SetActive(false);

            RedrawUpgradeElements();
        }

        private void RedrawUpgradeElements()
        {
            if (levelText != null && Upgrade != null)
            {
                levelText.gameObject.SetActive(true);
                levelText.text = "CẤP " + Upgrade.UpgradeLevel;
            }

            if (upgradesMaxObject != null)
            {
                upgradesMaxObject.SetActive(false);
            }

            if (upgradesBuyButton != null)
            {
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
