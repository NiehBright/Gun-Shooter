using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public static class UIWeaponPageBuilder
    {
        public static void BuildLayout(UIWeaponPage page)
        {
            if (page == null) return;

            Sprite defaultSlicedSprite = null;
            Image[] existingImages = page.GetComponentsInChildren<Image>(true);
            foreach (var img in existingImages)
            {
                if (img.sprite != null && img.type == Image.Type.Sliced)
                {
                    defaultSlicedSprite = img.sprite;
                    break;
                }
            }

            TMP_FontAsset defaultFont = null;
            TextMeshProUGUI[] existingTexts = page.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in existingTexts)
            {
                if (t.font != null)
                {
                    defaultFont = t.font;
                    break;
                }
            }

            // 1. RECONFIGURE RIGHT PANEL (Back Panel / Weapon Selector List) - 320px WIDTH FULL HEIGHT
            RectTransform backPanelRect = page.BackgroundPanelRectTransform;
            if (backPanelRect == null)
            {
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var bgPanelField = typeof(UIWeaponPage).BaseType.GetField("backgroundPanelRectTransform", flags);
                if (bgPanelField != null)
                    backPanelRect = bgPanelField.GetValue(page) as RectTransform;
            }

            if (backPanelRect != null)
            {
                backPanelRect.anchorMin = new Vector2(1f, 0f);
                backPanelRect.anchorMax = new Vector2(1f, 1f);
                backPanelRect.pivot = new Vector2(1f, 0.5f);
                backPanelRect.anchoredPosition = new Vector2(-65f, 0f);
                backPanelRect.sizeDelta = new Vector2(310f, 0f);
                backPanelRect.offsetMin = new Vector2(-375f, 0f);
                backPanelRect.offsetMax = new Vector2(-65f, 0f);

                Image bgImg = backPanelRect.GetComponent<Image>();
                if (bgImg != null)
                {
                    if (defaultSlicedSprite != null) { bgImg.sprite = defaultSlicedSprite; bgImg.type = Image.Type.Sliced; }
                    bgImg.color = new Color(0.05f, 0.07f, 0.12f, 0.96f);
                }

                // Left Neon Divider Line
                Transform oldDivider = backPanelRect.Find("Neon Divider");
                if (oldDivider != null) Object.DestroyImmediate(oldDivider.gameObject);
                GameObject dividerObj = new GameObject("Neon Divider", typeof(RectTransform), typeof(Image));
                dividerObj.transform.SetParent(backPanelRect, false);
                RectTransform divRt = dividerObj.GetComponent<RectTransform>();
                divRt.anchorMin = new Vector2(0f, 0f);
                divRt.anchorMax = new Vector2(0f, 1f);
                divRt.pivot = new Vector2(0f, 0.5f);
                divRt.anchoredPosition = Vector2.zero;
                divRt.sizeDelta = new Vector2(2f, 0f);
                dividerObj.GetComponent<Image>().color = new Color(0f, 0.85f, 1f, 0.35f);

                // Title ("KHO VŨ KHÍ")
                Transform titleTr = backPanelRect.Find("Title");
                if (titleTr != null)
                {
                    RectTransform titleRt = titleTr.GetComponent<RectTransform>();
                    titleRt.anchorMin = new Vector2(0f, 1f);
                    titleRt.anchorMax = new Vector2(1f, 1f);
                    titleRt.pivot = new Vector2(0.5f, 1f);
                    titleRt.anchoredPosition = new Vector2(-20f, -14f);
                    titleRt.sizeDelta = new Vector2(-70f, 40f);

                    TextMeshProUGUI titleTMP = titleTr.GetComponentInChildren<TextMeshProUGUI>();
                    if (titleTMP != null)
                    {
                        titleTMP.text = "KHO VŨ KHÍ";
                        titleTMP.fontSize = 20f;
                        titleTMP.fontStyle = FontStyles.Bold;
                        titleTMP.alignment = TextAlignmentOptions.Center;
                        titleTMP.textWrappingMode = TextWrappingModes.NoWrap;
                    }
                }

                // Close Button ("X")
                Transform closeBtnTr = backPanelRect.Find("Back Button");
                if (closeBtnTr != null)
                {
                    RectTransform closeRt = closeBtnTr.GetComponent<RectTransform>();
                    closeRt.anchorMin = new Vector2(1f, 1f);
                    closeRt.anchorMax = new Vector2(1f, 1f);
                    closeRt.pivot = new Vector2(1f, 1f);
                    closeRt.anchoredPosition = new Vector2(-12f, -12f);
                    closeRt.sizeDelta = new Vector2(40f, 40f);
                }

                // Inner panel holding the ScrollView
                Transform innerPanelTr = backPanelRect.Find("Panel");
                if (innerPanelTr != null)
                {
                    RectTransform innerPanelRt = innerPanelTr.GetComponent<RectTransform>();
                    innerPanelRt.anchorMin = Vector2.zero;
                    innerPanelRt.anchorMax = Vector2.one;
                    innerPanelRt.pivot = new Vector2(0.5f, 0.5f);
                    innerPanelRt.anchoredPosition = new Vector2(0f, -28f);
                    innerPanelRt.sizeDelta = new Vector2(-10f, -65f);

                    Image innerBg = innerPanelTr.GetComponent<Image>();
                    if (innerBg != null)
                    {
                        innerBg.color = new Color(0.06f, 0.08f, 0.13f, 0.94f);
                    }

                    Transform outlineTop = innerPanelTr.Find("Outline Top");
                    if (outlineTop != null) outlineTop.gameObject.SetActive(false);
                }
            }

            // ScrollRect adjustments for Vertical List
            ScrollRect scrollView = page.ScrollView;
            if (scrollView == null) scrollView = page.GetComponentInChildren<ScrollRect>(true);
            if (scrollView != null)
            {
                RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
                scrollRect.anchorMin = Vector2.zero;
                scrollRect.anchorMax = Vector2.one;
                scrollRect.pivot = new Vector2(0.5f, 0.5f);
                scrollRect.anchoredPosition = Vector2.zero;
                scrollRect.sizeDelta = new Vector2(-10f, -10f);

                scrollView.horizontal = false;
                scrollView.vertical = true;
                scrollView.movementType = ScrollRect.MovementType.Elastic;

                if (scrollView.viewport != null)
                {
                    scrollView.viewport.anchorMin = Vector2.zero;
                    scrollView.viewport.anchorMax = Vector2.one;
                    scrollView.viewport.anchoredPosition = Vector2.zero;
                    scrollView.viewport.sizeDelta = Vector2.zero;
                }

                Transform containerTr = page.PanelsContainer;
                if (containerTr == null && scrollView.content != null)
                    containerTr = scrollView.content;

                if (containerTr != null)
                {
                    RectTransform contentRt = containerTr.GetComponent<RectTransform>();
                    contentRt.anchorMin = new Vector2(0f, 1f);
                    contentRt.anchorMax = new Vector2(1f, 1f);
                    contentRt.pivot = new Vector2(0.5f, 1f);
                    contentRt.anchoredPosition = Vector2.zero;
                    contentRt.sizeDelta = Vector2.zero;

                    // Xoa bo toan bo LayoutGroup cu (dac biet la HorizontalLayoutGroup) de dung VerticalLayoutGroup danh sach doc
                    HorizontalLayoutGroup oldHlg = containerTr.GetComponent<HorizontalLayoutGroup>();
                    if (oldHlg != null)
                    {
                        Object.DestroyImmediate(oldHlg);
                    }

                    GridLayoutGroup oldGlg = containerTr.GetComponent<GridLayoutGroup>();
                    if (oldGlg != null)
                    {
                        Object.DestroyImmediate(oldGlg);
                    }

                    VerticalLayoutGroup vlg = containerTr.GetComponent<VerticalLayoutGroup>();
                    if (vlg == null) vlg = containerTr.gameObject.AddComponent<VerticalLayoutGroup>();
                    if (vlg != null)
                    {
                        vlg.spacing = 8f;
                        vlg.padding = new RectOffset(6, 6, 8, 8);
                        vlg.childAlignment = TextAnchor.UpperCenter;
                        vlg.childControlWidth = true;
                        vlg.childControlHeight = false;
                        vlg.childForceExpandWidth = true;
                        vlg.childForceExpandHeight = false;
                    }

                    ContentSizeFitter fitter = containerTr.GetComponent<ContentSizeFitter>();
                    if (fitter == null) fitter = containerTr.gameObject.AddComponent<ContentSizeFitter>();
                    if (fitter != null)
                    {
                        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    }
                }

                if (scrollView.horizontalScrollbar != null)
                    scrollView.horizontalScrollbar.gameObject.SetActive(false);
            }

            // 2. DESTROY ANY PREVIOUS LEFT PANEL
            for (int i = page.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = page.transform.GetChild(i);
                if (child.name == "Left Weapon Details Panel")
                {
                    if (Application.isPlaying) Object.Destroy(child.gameObject);
                    else Object.DestroyImmediate(child.gameObject);
                }
            }

            // 3. CREATE FULL-HEIGHT DOCKED LEFT DETAILS PANEL
            UIWeaponDetailsPanel detailsPanel = CreateDetailsPanel(page, defaultSlicedSprite, defaultFont);

            // 4. LINK REFERENCES INTO UIWEAPONPAGE
            var flagsRefl = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var detailsField = typeof(UIWeaponPage).GetField("detailsPanel", flagsRefl);
            if (detailsField != null)
                detailsField.SetValue(page, detailsPanel);

            var leftRectField = typeof(UIWeaponPage).GetField("leftPanelRectTransform", flagsRefl);
            if (leftRectField != null && detailsPanel != null)
                leftRectField.SetValue(page, detailsPanel.GetComponent<RectTransform>());

            var rightRectField = typeof(UIWeaponPage).GetField("rightPanelRectTransform", flagsRefl);
            if (rightRectField != null && backPanelRect != null)
                rightRectField.SetValue(page, backPanelRect);
        }

        private static UIWeaponDetailsPanel CreateDetailsPanel(UIWeaponPage page, Sprite bgSprite, TMP_FontAsset font)
        {
            GameObject detailsObj = new GameObject("Left Weapon Details Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(UIWeaponDetailsPanel));
            detailsObj.transform.SetParent(page.transform, false);

            UIWeaponDetailsPanel detailsPanel = detailsObj.GetComponent<UIWeaponDetailsPanel>();

            RectTransform rootRect = detailsObj.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 0f);
            rootRect.anchorMax = new Vector2(0f, 1f);
            rootRect.pivot = new Vector2(0f, 0.5f);
            rootRect.anchoredPosition = new Vector2(75f, 0f);
            rootRect.sizeDelta = new Vector2(350f, 0f);
            rootRect.offsetMin = new Vector2(75f, 0f);
            rootRect.offsetMax = new Vector2(425f, 0f);

            Image bgImage = detailsObj.GetComponent<Image>();
            if (bgSprite != null)
            {
                bgImage.sprite = bgSprite;
                bgImage.type = Image.Type.Sliced;
            }
            bgImage.color = new Color(0.05f, 0.07f, 0.12f, 0.96f);

            // Right Neon Divider Line
            GameObject dividerObj = new GameObject("Neon Divider", typeof(RectTransform), typeof(Image));
            dividerObj.transform.SetParent(detailsObj.transform, false);
            RectTransform divRt = dividerObj.GetComponent<RectTransform>();
            divRt.anchorMin = new Vector2(1f, 0f);
            divRt.anchorMax = new Vector2(1f, 1f);
            divRt.pivot = new Vector2(1f, 0.5f);
            divRt.anchoredPosition = Vector2.zero;
            divRt.sizeDelta = new Vector2(2f, 0f);
            dividerObj.GetComponent<Image>().color = new Color(0f, 0.85f, 1f, 0.35f);

            // Content container with safe padding (42px safe area for notch, 14px right, 16px top/bottom)
            VerticalLayoutGroup vlg = detailsObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(42, 14, 16, 16);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // --- HEADER ---
            GameObject headerObj = new GameObject("Header", typeof(RectTransform), typeof(VerticalLayoutGroup));
            headerObj.transform.SetParent(detailsObj.transform, false);
            VerticalLayoutGroup headerVlg = headerObj.GetComponent<VerticalLayoutGroup>();
            headerVlg.spacing = 4;
            headerVlg.childAlignment = TextAnchor.MiddleLeft;
            headerVlg.childControlWidth = true;
            headerVlg.childControlHeight = false;
            headerObj.GetComponent<RectTransform>().sizeDelta = new Vector2(304, 54);

            TextMeshProUGUI weaponName = CreateTMP(headerObj.transform, "WeaponName", "MINIGUN", 24, font, FontStyles.Bold, Color.white, TextAlignmentOptions.Left);

            GameObject subHeader = new GameObject("SubHeader", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            subHeader.transform.SetParent(headerObj.transform, false);
            HorizontalLayoutGroup subHlg = subHeader.GetComponent<HorizontalLayoutGroup>();
            subHlg.spacing = 8;
            subHlg.childAlignment = TextAnchor.MiddleLeft;
            subHlg.childControlWidth = false;
            subHlg.childControlHeight = false;
            subHeader.GetComponent<RectTransform>().sizeDelta = new Vector2(304, 24);

            // Rarity Badge
            GameObject rarityBadge = new GameObject("Rarity Badge", typeof(RectTransform), typeof(Image));
            rarityBadge.transform.SetParent(subHeader.transform, false);
            rarityBadge.GetComponent<RectTransform>().sizeDelta = new Vector2(95, 22);
            Image rarityImg = rarityBadge.GetComponent<Image>();
            if (bgSprite != null) { rarityImg.sprite = bgSprite; rarityImg.type = Image.Type.Sliced; }
            rarityImg.color = new Color(0.2f, 0.6f, 1f);

            TextMeshProUGUI rarityTMP = CreateTMP(rarityBadge.transform, "RarityText", "LEGENDARY", 12, font, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
            rarityTMP.rectTransform.anchorMin = Vector2.zero;
            rarityTMP.rectTransform.anchorMax = Vector2.one;
            rarityTMP.rectTransform.sizeDelta = Vector2.zero;

            TextMeshProUGUI levelTMP = CreateTMP(subHeader.transform, "LevelText", "CẤP 1", 15, font, FontStyles.Bold, new Color(0.4f, 0.9f, 1f), TextAlignmentOptions.Left, 110);

            // --- CARDS PROGRESS CONTAINER ---
            GameObject cardsObj = new GameObject("Cards Container", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image));
            cardsObj.transform.SetParent(detailsObj.transform, false);
            cardsObj.GetComponent<RectTransform>().sizeDelta = new Vector2(304, 52);
            Image cardsBg = cardsObj.GetComponent<Image>();
            if (bgSprite != null) { cardsBg.sprite = bgSprite; cardsBg.type = Image.Type.Sliced; }
            cardsBg.color = new Color(0.09f, 0.13f, 0.2f, 0.9f);

            VerticalLayoutGroup cardsVlg = cardsObj.GetComponent<VerticalLayoutGroup>();
            cardsVlg.padding = new RectOffset(10, 10, 6, 6);
            cardsVlg.spacing = 3;
            cardsVlg.childAlignment = TextAnchor.UpperCenter;
            cardsVlg.childControlWidth = true;
            cardsVlg.childControlHeight = false;

            GameObject cardRow = new GameObject("Card Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            cardRow.transform.SetParent(cardsObj.transform, false);
            HorizontalLayoutGroup cardRowHlg = cardRow.GetComponent<HorizontalLayoutGroup>();
            cardRowHlg.childAlignment = TextAnchor.MiddleCenter;
            cardRowHlg.childControlWidth = false;
            cardRowHlg.childControlHeight = false;
            cardRow.GetComponent<RectTransform>().sizeDelta = new Vector2(284, 18);

            CreateTMP(cardRow.transform, "Label", "TIẾN ĐỘ THẺ", 13, font, FontStyles.Bold, new Color(0.8f, 0.85f, 0.95f), TextAlignmentOptions.Left, 120);
            TextMeshProUGUI cardsAmount = CreateTMP(cardRow.transform, "Amount", "12 / 20 THẺ", 13, font, FontStyles.Bold, Color.white, TextAlignmentOptions.Right, 120);

            // Card Fill Bar
            GameObject barBg = new GameObject("Bar Bg", typeof(RectTransform), typeof(Image));
            barBg.transform.SetParent(cardsObj.transform, false);
            barBg.GetComponent<RectTransform>().sizeDelta = new Vector2(284, 8);
            Image barBgImg = barBg.GetComponent<Image>();
            if (bgSprite != null) { barBgImg.sprite = bgSprite; barBgImg.type = Image.Type.Sliced; }
            barBgImg.color = new Color(0.04f, 0.06f, 0.1f, 0.9f);

            GameObject barFill = new GameObject("Bar Fill", typeof(RectTransform), typeof(Image));
            barFill.transform.SetParent(barBg.transform, false);
            RectTransform fillRt = barFill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            Image fillImg = barFill.GetComponent<Image>();
            if (bgSprite != null) { fillImg.sprite = bgSprite; fillImg.type = Image.Type.Filled; }
            else fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.color = new Color(0.55f, 0.25f, 0.95f, 1f);

            TextMeshProUGUI cardsStatus = CreateTMP(cardsObj.transform, "Status", "Cần thêm thẻ để nâng cấp", 11, font, FontStyles.Italic, new Color(1f, 0.8f, 0.3f), TextAlignmentOptions.Left);

            // --- STATS HUD CONTAINER ---
            GameObject statsObj = new GameObject("Stats Container", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image));
            statsObj.transform.SetParent(detailsObj.transform, false);
            statsObj.GetComponent<RectTransform>().sizeDelta = new Vector2(304, 126);
            Image statsBg = statsObj.GetComponent<Image>();
            if (bgSprite != null) { statsBg.sprite = bgSprite; statsBg.type = Image.Type.Sliced; }
            statsBg.color = new Color(0.08f, 0.11f, 0.18f, 0.9f);

            VerticalLayoutGroup statsVlg = statsObj.GetComponent<VerticalLayoutGroup>();
            statsVlg.padding = new RectOffset(10, 10, 8, 8);
            statsVlg.spacing = 4;
            statsVlg.childAlignment = TextAnchor.UpperCenter;
            statsVlg.childControlWidth = true;
            statsVlg.childControlHeight = false;

            CreateStatRow(statsObj.transform, "SÁT THƯƠNG", font, out TextMeshProUGUI dmgVal, out TextMeshProUGUI dmgBonus);
            CreateStatRow(statsObj.transform, "TỐC ĐỘ BẮN", font, out TextMeshProUGUI fireVal, out TextMeshProUGUI fireBonus);
            CreateStatRow(statsObj.transform, "TẦM BẮN", font, out TextMeshProUGUI rangeVal, out _);
            CreateStatRow(statsObj.transform, "LỰC CHIẾN", font, out TextMeshProUGUI powerVal, out TextMeshProUGUI powerBonus);

            // Dummy stat rows de phuc vu BindReferences day du
            CreateStatRow(statsObj.transform, "ĐỘ GIẬT", font, out TextMeshProUGUI spreadVal, out _);
            spreadVal.transform.parent.gameObject.SetActive(false);
            CreateStatRow(statsObj.transform, "SỐ ĐẠN BẮN", font, out TextMeshProUGUI bulletsVal, out _);
            bulletsVal.transform.parent.gameObject.SetActive(false);

            // --- UPGRADES CONTAINER ---
            GameObject upgradesObj = new GameObject("Upgrades Container", typeof(RectTransform), typeof(VerticalLayoutGroup));
            upgradesObj.transform.SetParent(detailsObj.transform, false);
            upgradesObj.GetComponent<RectTransform>().sizeDelta = new Vector2(304, 46);
            VerticalLayoutGroup upgVlg = upgradesObj.GetComponent<VerticalLayoutGroup>();
            upgVlg.spacing = 6;
            upgVlg.childAlignment = TextAnchor.UpperCenter;
            upgVlg.childControlWidth = true;
            upgVlg.childControlHeight = false;

            // Coin Upgrade Button
            Button coinBtn = CreateActionButton(upgradesObj.transform, "Coin Upgrade Button", "NÂNG CẤP BẰNG VÀNG",
                new Color(0.95f, 0.74f, 0.18f, 1f), GetCoinIcon(page), bgSprite, font, out Image coinBtnImg, out TextMeshProUGUI coinPriceText);

            // Card Unlock Button
            Button unlockBtn = CreateActionButton(upgradesObj.transform, "Card Unlock Button", "MỞ KHÓA VŨ KHÍ",
                new Color(0.18f, 0.75f, 0.35f, 1f), null, bgSprite, font, out Image unlockBtnImg, out TextMeshProUGUI unlockBtnText);
            unlockBtn.gameObject.SetActive(false);

            // Max Level Banner
            GameObject maxBanner = new GameObject("Max Level Banner", typeof(RectTransform), typeof(Image));
            maxBanner.transform.SetParent(upgradesObj.transform, false);
            maxBanner.GetComponent<RectTransform>().sizeDelta = new Vector2(304, 44);
            Image maxBg = maxBanner.GetComponent<Image>();
            if (bgSprite != null) { maxBg.sprite = bgSprite; maxBg.type = Image.Type.Sliced; }
            maxBg.color = new Color(0.2f, 0.24f, 0.35f, 0.95f);
            CreateTMP(maxBanner.transform, "MaxText", "ĐẠT CẤP TỐI ĐA", 16, font, FontStyles.Bold, new Color(1f, 0.85f, 0.2f), TextAlignmentOptions.Center);
            maxBanner.SetActive(false);

            // --- EQUIP BUTTON ("TRANG BỊ") ---
            GameObject equipCont = new GameObject("Equip Container", typeof(RectTransform), typeof(VerticalLayoutGroup));
            equipCont.transform.SetParent(detailsObj.transform, false);
            equipCont.GetComponent<RectTransform>().sizeDelta = new Vector2(304, 48);
            VerticalLayoutGroup eqVlg = equipCont.GetComponent<VerticalLayoutGroup>();
            eqVlg.childAlignment = TextAnchor.MiddleCenter;
            eqVlg.childControlWidth = true;
            eqVlg.childControlHeight = false;

            Button equipBtn = CreateActionButton(equipCont.transform, "Equip Button", "TRANG BỊ",
                new Color(0.15f, 0.55f, 0.95f, 1f), null, bgSprite, font, out Image equipBtnImg, out TextMeshProUGUI equipBtnText);
            equipBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(304, 46);
            equipBtnText.fontSize = 18f;

            // Bind References
            detailsPanel.BindReferences(
                weaponName,
                rarityTMP,
                rarityImg,
                levelTMP,
                cardsObj,
                fillImg,
                cardsAmount,
                cardsStatus,
                powerVal,
                powerBonus,
                dmgVal,
                dmgBonus,
                fireVal,
                fireBonus,
                rangeVal,
                spreadVal,
                bulletsVal,
                upgradesObj,
                coinBtn,
                coinBtnImg,
                coinPriceText,
                unlockBtn,
                unlockBtnImg,
                unlockBtnText,
                maxBanner,
                equipBtn,
                equipBtnImg,
                equipBtnText
            );

            return detailsPanel;
        }

        private static void CreateStatRow(Transform parent, string title, TMP_FontAsset font, out TextMeshProUGUI valText, out TextMeshProUGUI bonusText)
        {
            GameObject row = new GameObject($"Row_{title}", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(284, 22);

            HorizontalLayoutGroup hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            CreateTMP(row.transform, "Label", title, 13, font, FontStyles.Bold, new Color(0.75f, 0.82f, 0.9f), TextAlignmentOptions.Left, 130);
            valText = CreateTMP(row.transform, "Value", "100", 14, font, FontStyles.Bold, Color.white, TextAlignmentOptions.Right, 75);
            bonusText = CreateTMP(row.transform, "Bonus", "+10", 13, font, FontStyles.Bold, new Color(0.2f, 0.95f, 0.45f), TextAlignmentOptions.Left, 55);
            bonusText.gameObject.SetActive(false);
        }

        private static Button CreateActionButton(Transform parent, string name, string label, Color color, Sprite icon, Sprite bgSprite, TMP_FontAsset font, out Image btnImg, out TextMeshProUGUI labelTmp)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(304, 44);

            btnImg = btnObj.GetComponent<Image>();
            if (bgSprite != null) { btnImg.sprite = bgSprite; btnImg.type = Image.Type.Sliced; }
            btnImg.color = color;

            Button btn = btnObj.GetComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = btn.colors;
            cb.highlightedColor = color * 1.15f;
            cb.pressedColor = color * 0.85f;
            cb.disabledColor = new Color(0.35f, 0.35f, 0.4f, 0.6f);
            btn.colors = cb;

            GameObject cont = new GameObject("Container", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            cont.transform.SetParent(btnObj.transform, false);
            RectTransform contRt = cont.GetComponent<RectTransform>();
            contRt.anchorMin = Vector2.zero;
            contRt.anchorMax = Vector2.one;
            contRt.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup hlg = cont.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            if (icon != null)
            {
                GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                iconObj.transform.SetParent(cont.transform, false);
                iconObj.GetComponent<RectTransform>().sizeDelta = new Vector2(24, 24);
                Image iconImg = iconObj.GetComponent<Image>();
                iconImg.sprite = icon;
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
            }

            labelTmp = CreateTMP(cont.transform, "Label", label, 16, font, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, 200);
            labelTmp.raycastTarget = false;

            return btn;
        }

        private static TextMeshProUGUI CreateTMP(Transform parent, string name, string text, float size, TMP_FontAsset font, FontStyles style, Color color, TextAlignmentOptions align, float width = 0)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.GetComponent<RectTransform>();
            if (width > 0) rt.sizeDelta = new Vector2(width, size + 10);

            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = align;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;

            return tmp;
        }

        private static Sprite GetCoinIcon(UIWeaponPage page)
        {
            if (Application.isPlaying)
            {
                try
                {
                    var curr = CurrenciesController.GetCurrency(CurrencyType.Coins);
                    if (curr != null) return curr.Icon;
                }
                catch {}
            }

            if (page != null)
            {
                Image[] allImages = page.GetComponentsInChildren<Image>(true);
                foreach (var img in allImages)
                {
                    if (img.sprite != null && img.sprite.name.ToLower().Contains("coin"))
                    {
                        return img.sprite;
                    }
                }
            }

            #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite Coin");
            if (guids != null && guids.Length > 0)
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                    Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (s != null) return s;
                }
            }
            #endif

            return null;
        }
    }
}
