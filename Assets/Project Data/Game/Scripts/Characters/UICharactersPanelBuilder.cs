using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public static class UICharactersPanelBuilder
    {
        public static void BuildLayout(UICharactersPanel panel)
        {
            if (panel == null) return;

            Sprite defaultSlicedSprite = null;
            Image[] existingImages = panel.GetComponentsInChildren<Image>(true);
            foreach (var img in existingImages)
            {
                if (img.sprite != null && img.type == Image.Type.Sliced)
                {
                    defaultSlicedSprite = img.sprite;
                    break;
                }
            }

            TMP_FontAsset defaultFont = null;
            TextMeshProUGUI[] existingTexts = panel.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in existingTexts)
            {
                if (t.font != null)
                {
                    defaultFont = t.font;
                    break;
                }
            }

            // 1. RECONFIGURE RIGHT PANEL (Back Panel / backgroundPanelRectTransform) - ENLARGED
            RectTransform backPanelRect = panel.BackgroundPanelRectTransform;
            if (backPanelRect == null)
            {
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var bgPanelField = typeof(UICharactersPanel).BaseType.GetField("backgroundPanelRectTransform", flags);
                if (bgPanelField != null)
                    backPanelRect = bgPanelField.GetValue(panel) as RectTransform;
            }

            if (backPanelRect != null)
            {
                backPanelRect.anchorMin = new Vector2(1f, 0.5f);
                backPanelRect.anchorMax = new Vector2(1f, 0.5f);
                backPanelRect.pivot = new Vector2(1f, 0.5f);
                backPanelRect.anchoredPosition = new Vector2(-70f, 0f);
                backPanelRect.sizeDelta = new Vector2(440f, 980f); // Enlarged width & height

                // Make the background image sleek and fitted
                Image bgImg = backPanelRect.GetComponent<Image>();
                if (bgImg != null)
                {
                    bgImg.color = new Color(0.06f, 0.08f, 0.13f, 0.94f);
                }

                // Center Title ("CHARACTERS") - Wide enough so it never wraps to 2 lines
                Transform titleTr = backPanelRect.Find("Title");
                if (titleTr != null)
                {
                    RectTransform titleRt = titleTr.GetComponent<RectTransform>();
                    titleRt.anchorMin = new Vector2(0.5f, 1f);
                    titleRt.anchorMax = new Vector2(0.5f, 1f);
                    titleRt.pivot = new Vector2(0.5f, 1f);
                    titleRt.anchoredPosition = new Vector2(0f, -24f);
                    titleRt.sizeDelta = new Vector2(300f, 55f);

                    TextMeshProUGUI titleTMP = titleTr.GetComponentInChildren<TextMeshProUGUI>();
                    if (titleTMP != null)
                    {
                        titleTMP.fontSize = 30f;
                        titleTMP.textWrappingMode = TextWrappingModes.NoWrap;
                    }
                }

                // Reposition Close Button ("X")
                Transform closeBtnTr = backPanelRect.Find("Back Button");
                if (closeBtnTr != null)
                {
                    RectTransform closeRt = closeBtnTr.GetComponent<RectTransform>();
                    closeRt.anchorMin = new Vector2(1f, 1f);
                    closeRt.anchorMax = new Vector2(1f, 1f);
                    closeRt.pivot = new Vector2(1f, 1f);
                    closeRt.anchoredPosition = new Vector2(-18f, -22f);
                    closeRt.sizeDelta = new Vector2(56f, 56f);
                }

                // Panel container holding the scrollview
                Transform innerPanelTr = backPanelRect.Find("Panel");
                if (innerPanelTr != null)
                {
                    RectTransform innerPanelRt = innerPanelTr.GetComponent<RectTransform>();
                    innerPanelRt.anchorMin = Vector2.zero;
                    innerPanelRt.anchorMax = Vector2.one;
                    innerPanelRt.pivot = new Vector2(0.5f, 0.5f);
                    innerPanelRt.anchoredPosition = new Vector2(0f, -44f);
                    innerPanelRt.sizeDelta = new Vector2(-16f, -100f);

                    Image innerBg = innerPanelTr.GetComponent<Image>();
                    if (innerBg != null)
                    {
                        innerBg.color = new Color(0.06f, 0.08f, 0.13f, 0.94f);
                    }

                    // Hide Outline Top if any
                    Transform outlineTop = innerPanelTr.Find("Outline Top");
                    if (outlineTop != null) outlineTop.gameObject.SetActive(false);
                }
            }

            // ScrollRect adjustments
            ScrollRect scrollView = panel.ScrollView;
            if (scrollView == null) scrollView = panel.GetComponentInChildren<ScrollRect>(true);
            if (scrollView != null)
            {
                RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
                scrollRect.anchorMin = Vector2.zero;
                scrollRect.anchorMax = Vector2.one;
                scrollRect.pivot = new Vector2(0.5f, 0.5f);
                scrollRect.anchoredPosition = Vector2.zero;
                scrollRect.sizeDelta = new Vector2(-12f, -12f);

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

                Transform containerTr = panel.PanelsContainer;
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

                    GridLayoutGroup grid = containerTr.GetComponent<GridLayoutGroup>();
                    if (grid != null)
                    {
                        grid.cellSize = new Vector2(400f, 270f); // Enlarged card cell
                        grid.spacing = new Vector2(0f, 18f);
                        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                        grid.constraintCount = 1;
                        grid.childAlignment = TextAnchor.UpperCenter;
                        grid.padding = new RectOffset(0, 0, 10, 10);
                    }

                    ContentSizeFitter fitter = containerTr.GetComponent<ContentSizeFitter>();
                    if (fitter == null) fitter = containerTr.gameObject.AddComponent<ContentSizeFitter>();
                    fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                }

                if (scrollView.horizontalScrollbar != null)
                    scrollView.horizontalScrollbar.gameObject.SetActive(false);
            }

            // 2. DESTROY ANY PREVIOUS / OBSOLETE LEFT PANEL OR TOOLTIP INSTANCES
            for (int i = panel.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = panel.transform.GetChild(i);
                if (child.name == "Left Details Panel" || child.name == "Skill Tooltip")
                {
                    if (Application.isPlaying) Object.Destroy(child.gameObject);
                    else Object.DestroyImmediate(child.gameObject);
                }
            }

            // 3. CREATE SKILL TOOLTIP (Enlarged)
            UICharacterSkillTooltip tooltip = CreateSkillTooltip(panel.transform, defaultSlicedSprite, defaultFont);

            // 4. CREATE LEFT DETAILS PANEL (Enlarged)
            UICharacterDetailsPanel detailsPanel = CreateDetailsPanel(panel, tooltip, defaultSlicedSprite, defaultFont);

            // 5. LINK REFERENCES INTO UICHARACTERSPANEL VIA REFLECTION
            var flagsRefl = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var detailsField = typeof(UICharactersPanel).GetField("detailsPanel", flagsRefl);
            if (detailsField != null)
                detailsField.SetValue(panel, detailsPanel);

            var leftRectField = typeof(UICharactersPanel).GetField("leftPanelRectTransform", flagsRefl);
            if (leftRectField != null && detailsPanel != null)
                leftRectField.SetValue(panel, detailsPanel.GetComponent<RectTransform>());

            var rightRectField = typeof(UICharactersPanel).GetField("rightPanelRectTransform", flagsRefl);
            if (rightRectField != null && backPanelRect != null)
                rightRectField.SetValue(panel, backPanelRect);
        }

        private static UICharacterDetailsPanel CreateDetailsPanel(UICharactersPanel panel, UICharacterSkillTooltip tooltip, Sprite bgSprite, TMP_FontAsset font)
        {
            GameObject detailsObj = new GameObject("Left Details Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(UICharacterDetailsPanel));
            detailsObj.transform.SetParent(panel.transform, false);

            RectTransform rootRect = detailsObj.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 0.5f);
            rootRect.anchorMax = new Vector2(0f, 0.5f);
            rootRect.pivot = new Vector2(0f, 0.5f);
            rootRect.anchoredPosition = new Vector2(90f, 0f); // 90px safe offset for iPhone 12 notch
            rootRect.sizeDelta = new Vector2(500f, 980f); // Enlarged width & height

            Image bgImage = detailsObj.GetComponent<Image>();
            if (bgSprite != null)
            {
                bgImage.sprite = bgSprite;
                bgImage.type = Image.Type.Sliced;
            }
            bgImage.color = new Color(0.06f, 0.08f, 0.13f, 0.94f);

            VerticalLayoutGroup vlg = detailsObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.spacing = 14;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // --- HEADER ---
            GameObject headerObj = new GameObject("Header", typeof(RectTransform), typeof(VerticalLayoutGroup));
            headerObj.transform.SetParent(detailsObj.transform, false);
            VerticalLayoutGroup headerVlg = headerObj.GetComponent<VerticalLayoutGroup>();
            headerVlg.spacing = 4;
            headerVlg.childAlignment = TextAnchor.MiddleCenter;
            headerVlg.childControlWidth = true;
            headerVlg.childControlHeight = false;
            headerObj.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 105);

            TextMeshProUGUI charName = CreateTMP(headerObj.transform, "CharName", "CHARACTER", 34, font, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
            TextMeshProUGUI stageLevel = CreateTMP(headerObj.transform, "StageLevel", "CẤP 1 • GIAI ĐOẠN 1", 18, font, FontStyles.Normal, new Color(0.7f, 0.85f, 1f, 1f), TextAlignmentOptions.Center);

            // Stars
            GameObject starsCont = new GameObject("Stars Container", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            starsCont.transform.SetParent(headerObj.transform, false);
            HorizontalLayoutGroup starsHlg = starsCont.GetComponent<HorizontalLayoutGroup>();
            starsHlg.spacing = 10;
            starsHlg.childAlignment = TextAnchor.MiddleCenter;
            starsHlg.childControlWidth = false;
            starsHlg.childControlHeight = false;
            starsCont.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 28);

            List<Image> starList = new List<Image>();
            Sprite starSprite = GetStageStarSprite(panel);
            for (int i = 0; i < 5; i++)
            {
                GameObject starObj = new GameObject($"Star_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                starObj.transform.SetParent(starsCont.transform, false);
                RectTransform starRt = starObj.GetComponent<RectTransform>();
                starRt.sizeDelta = new Vector2(26, 26);
                Image starImg = starObj.GetComponent<Image>();
                if (starSprite != null) starImg.sprite = starSprite;
                starImg.color = new Color(1f, 0.85f, 0.2f, 1f);
                starList.Add(starImg);
            }

            // --- STATS CARD ---
            GameObject statsObj = new GameObject("Stats Container", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image));
            statsObj.transform.SetParent(detailsObj.transform, false);
            statsObj.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 230);
            Image statsBg = statsObj.GetComponent<Image>();
            if (bgSprite != null) { statsBg.sprite = bgSprite; statsBg.type = Image.Type.Sliced; }
            statsBg.color = new Color(0.1f, 0.14f, 0.22f, 0.9f);

            VerticalLayoutGroup statsVlg = statsObj.GetComponent<VerticalLayoutGroup>();
            statsVlg.padding = new RectOffset(18, 18, 14, 14);
            statsVlg.spacing = 8;
            statsVlg.childAlignment = TextAnchor.UpperCenter;
            statsVlg.childControlWidth = true;
            statsVlg.childControlHeight = false;

            // Clean text labels without unsupported unicode emoji characters
            CreateStatRow(statsObj.transform, "LỰC CHIẾN", font, out TextMeshProUGUI powerText, out TextMeshProUGUI powerBonusText);
            CreateStatRow(statsObj.transform, "MÁU (HP)", font, out TextMeshProUGUI hpText, out TextMeshProUGUI hpBonusText);
            CreateStatRow(statsObj.transform, "SÁT THƯƠNG", font, out TextMeshProUGUI dmgText, out _);
            CreateStatRow(statsObj.transform, "TỐC ĐỘ", font, out TextMeshProUGUI speedText, out _);

            // --- SKILL SECTION ---
            GameObject skillObj = new GameObject("Skill Card", typeof(RectTransform), typeof(Image), typeof(SkillTooltipTrigger));
            skillObj.transform.SetParent(detailsObj.transform, false);
            skillObj.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 135);
            Image skillBg = skillObj.GetComponent<Image>();
            if (bgSprite != null) { skillBg.sprite = bgSprite; skillBg.type = Image.Type.Sliced; }
            skillBg.color = new Color(0.11f, 0.16f, 0.25f, 0.95f);
            skillBg.raycastTarget = true; // Crucial for catching pointer events

            HorizontalLayoutGroup skillHlg = skillObj.AddComponent<HorizontalLayoutGroup>();
            skillHlg.padding = new RectOffset(16, 16, 14, 14);
            skillHlg.spacing = 16;
            skillHlg.childAlignment = TextAnchor.MiddleLeft;
            skillHlg.childControlWidth = false;
            skillHlg.childControlHeight = false;

            // Skill Icon
            GameObject iconObj = new GameObject("Skill Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.transform.SetParent(skillObj.transform, false);
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(80, 80);
            Image skillIconImg = iconObj.GetComponent<Image>();
            skillIconImg.preserveAspect = true;
            skillIconImg.raycastTarget = false; // Don't block parent trigger

            // Skill text column
            GameObject skillTextCol = new GameObject("Skill Info", typeof(RectTransform), typeof(VerticalLayoutGroup));
            skillTextCol.transform.SetParent(skillObj.transform, false);
            skillTextCol.GetComponent<RectTransform>().sizeDelta = new Vector2(310, 95);
            VerticalLayoutGroup stcVlg = skillTextCol.GetComponent<VerticalLayoutGroup>();
            stcVlg.spacing = 4;
            stcVlg.childAlignment = TextAnchor.MiddleLeft;
            stcVlg.childControlWidth = true;
            stcVlg.childControlHeight = false;

            TextMeshProUGUI skillName = CreateTMP(skillTextCol.transform, "SkillName", "KỸ NĂNG", 24, font, FontStyles.Bold, new Color(0.35f, 0.85f, 1f), TextAlignmentOptions.Left);
            skillName.raycastTarget = false;

            TextMeshProUGUI skillTag = CreateTMP(skillTextCol.transform, "SkillTag", "Khống Chế & Hút Quái", 16, font, FontStyles.Italic, new Color(0.8f, 0.9f, 1f), TextAlignmentOptions.Left);
            skillTag.raycastTarget = false;

            TextMeshProUGUI hintText = CreateTMP(skillTextCol.transform, "Hint", "(Đè giữ để xem thông tin chi tiết)", 13, font, FontStyles.Normal, new Color(0.7f, 0.75f, 0.85f, 0.8f), TextAlignmentOptions.Left);
            hintText.raycastTarget = false;

            SkillTooltipTrigger trigger = skillObj.GetComponent<SkillTooltipTrigger>();

            // --- UPGRADES SECTION ---
            GameObject upgradesCont = new GameObject("Upgrades Container", typeof(RectTransform), typeof(VerticalLayoutGroup));
            upgradesCont.transform.SetParent(detailsObj.transform, false);
            upgradesCont.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 190);
            VerticalLayoutGroup upgVlg = upgradesCont.GetComponent<VerticalLayoutGroup>();
            upgVlg.spacing = 12;
            upgVlg.childAlignment = TextAnchor.UpperCenter;
            upgVlg.childControlWidth = true;
            upgVlg.childControlHeight = false;

            // Coin Upgrade Button
            Button coinsBtn = CreateUpgradeButton(upgradesCont.transform, "Upgrade Coins Button", "NÂNG CẤP BẰNG VÀNG",
                new Color(0.95f, 0.74f, 0.18f, 1f), CurrenciesController.GetCurrency(CurrencyType.Coins)?.Icon, bgSprite, font, out Image coinsImg, out TextMeshProUGUI coinsPrice);

            // Gem Upgrade Button
            Button gemsBtn = CreateUpgradeButton(upgradesCont.transform, "Upgrade Gems Button", "NÂNG CẤP BẰNG GEM",
                new Color(0.25f, 0.8f, 0.95f, 1f), CurrenciesController.GetCurrency(CurrencyType.Gems)?.Icon, bgSprite, font, out Image gemsImg, out TextMeshProUGUI gemsPrice);

            // Max Level Banner
            GameObject maxLvlObj = new GameObject("Max Level Object", typeof(RectTransform), typeof(Image));
            maxLvlObj.transform.SetParent(detailsObj.transform, false);
            maxLvlObj.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 70);
            Image maxLvlBg = maxLvlObj.GetComponent<Image>();
            if (bgSprite != null) { maxLvlBg.sprite = bgSprite; maxLvlBg.type = Image.Type.Sliced; }
            maxLvlBg.color = new Color(0.2f, 0.25f, 0.35f, 0.9f);
            CreateTMP(maxLvlObj.transform, "MaxLevelText", "★ ĐẠT CẤP TỐI ĐA ★", 22, font, FontStyles.Bold, new Color(1f, 0.85f, 0.2f), TextAlignmentOptions.Center);
            maxLvlObj.SetActive(false);

            // Locked Object
            GameObject lockedObj = new GameObject("Locked Object", typeof(RectTransform), typeof(Image));
            lockedObj.transform.SetParent(detailsObj.transform, false);
            lockedObj.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 70);
            Image lockedBg = lockedObj.GetComponent<Image>();
            if (bgSprite != null) { lockedBg.sprite = bgSprite; lockedBg.type = Image.Type.Sliced; }
            lockedBg.color = new Color(0.25f, 0.15f, 0.15f, 0.9f);
            TextMeshProUGUI lockedNotice = CreateTMP(lockedObj.transform, "LockedNoticeText", "MỞ KHÓA Ở MÀN 2", 20, font, FontStyles.Bold, new Color(1f, 0.5f, 0.5f), TextAlignmentOptions.Center);
            lockedObj.SetActive(false);

            // Bind everything
            UICharacterDetailsPanel detailsPanel = detailsObj.GetComponent<UICharacterDetailsPanel>();
            detailsPanel.BindReferences(
                charName,
                stageLevel,
                starsCont,
                starList.ToArray(),
                powerText,
                powerBonusText,
                hpText,
                hpBonusText,
                dmgText,
                speedText,
                skillIconImg,
                skillName,
                skillTag,
                trigger,
                tooltip,
                coinsBtn,
                coinsImg,
                coinsPrice,
                gemsBtn,
                gemsImg,
                gemsPrice,
                upgradesCont,
                maxLvlObj,
                lockedObj,
                lockedNotice
            );

            return detailsPanel;
        }

        private static void CreateStatRow(Transform parent, string title, TMP_FontAsset font, out TextMeshProUGUI valueText, out TextMeshProUGUI bonusText)
        {
            GameObject row = new GameObject($"Row_{title}", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(414, 42);

            HorizontalLayoutGroup hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            // Clean Label without unicode emoji
            CreateTMP(row.transform, "Label", title, 20, font, FontStyles.Bold, new Color(0.8f, 0.85f, 0.9f), TextAlignmentOptions.Left, 190);

            // Value
            valueText = CreateTMP(row.transform, "Value", "100", 22, font, FontStyles.Bold, Color.white, TextAlignmentOptions.Right, 100);

            // Bonus
            bonusText = CreateTMP(row.transform, "Bonus", "+10", 18, font, FontStyles.Bold, new Color(0.2f, 0.9f, 0.4f), TextAlignmentOptions.Left, 65);
            bonusText.gameObject.SetActive(false);
        }

        private static Button CreateUpgradeButton(Transform parent, string name, string subText, Color mainColor, Sprite currencyIcon, Sprite bgSprite, TMP_FontAsset font, out Image btnImage, out TextMeshProUGUI priceText)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 84);

            btnImage = btnObj.GetComponent<Image>();
            if (bgSprite != null) { btnImage.sprite = bgSprite; btnImage.type = Image.Type.Sliced; }
            btnImage.color = mainColor;

            Button btn = btnObj.GetComponent<Button>();

            HorizontalLayoutGroup hlg = btnObj.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 10, 10);
            hlg.spacing = 16;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            // Currency Icon
            GameObject iconObj = new GameObject("Currency Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.transform.SetParent(btnObj.transform, false);
            iconObj.GetComponent<RectTransform>().sizeDelta = new Vector2(44, 44);
            Image curImg = iconObj.GetComponent<Image>();
            if (currencyIcon != null) curImg.sprite = currencyIcon;
            curImg.preserveAspect = true;
            curImg.raycastTarget = false;

            // Texts column
            GameObject textCol = new GameObject("Texts", typeof(RectTransform), typeof(VerticalLayoutGroup));
            textCol.transform.SetParent(btnObj.transform, false);
            textCol.GetComponent<RectTransform>().sizeDelta = new Vector2(330, 60);
            VerticalLayoutGroup vlg = textCol.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 2;
            vlg.childAlignment = TextAnchor.MiddleLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            priceText = CreateTMP(textCol.transform, "Price", "500", 26, font, FontStyles.Bold, Color.white, TextAlignmentOptions.Left);
            priceText.raycastTarget = false;

            TextMeshProUGUI subTMP = CreateTMP(textCol.transform, "Subtitle", subText, 15, font, FontStyles.Normal, new Color(1f, 1f, 1f, 0.9f), TextAlignmentOptions.Left);
            subTMP.raycastTarget = false;

            return btn;
        }

        private static UICharacterSkillTooltip CreateSkillTooltip(Transform parent, Sprite bgSprite, TMP_FontAsset font)
        {
            GameObject tooltipObj = new GameObject("Skill Tooltip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup), typeof(UICharacterSkillTooltip));
            tooltipObj.transform.SetParent(parent, false);

            RectTransform rt = tooltipObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 360); // Enlarged tooltip
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(610f, 0f); // Floats to the right of the Left Details Panel

            Image bg = tooltipObj.GetComponent<Image>();
            if (bgSprite != null) { bg.sprite = bgSprite; bg.type = Image.Type.Sliced; }
            bg.color = new Color(0.05f, 0.07f, 0.11f, 0.98f);
            bg.raycastTarget = false; // Don't block raycasts

            CanvasGroup cg = tooltipObj.GetComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            VerticalLayoutGroup vlg = tooltipObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(22, 22, 22, 22);
            vlg.spacing = 14;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Header row
            GameObject header = new GameObject("Header Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            header.transform.SetParent(tooltipObj.transform, false);
            header.GetComponent<RectTransform>().sizeDelta = new Vector2(456, 65);
            HorizontalLayoutGroup hlg = header.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.transform.SetParent(header.transform, false);
            iconObj.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
            Image skillIcon = iconObj.GetComponent<Image>();
            skillIcon.preserveAspect = true;
            skillIcon.raycastTarget = false;

            GameObject nameCol = new GameObject("NameCol", typeof(RectTransform), typeof(VerticalLayoutGroup));
            nameCol.transform.SetParent(header.transform, false);
            nameCol.GetComponent<RectTransform>().sizeDelta = new Vector2(380, 60);
            VerticalLayoutGroup nVlg = nameCol.GetComponent<VerticalLayoutGroup>();
            nVlg.spacing = 2;
            nVlg.childAlignment = TextAnchor.MiddleLeft;
            nVlg.childControlWidth = true;
            nVlg.childControlHeight = false;

            TextMeshProUGUI nameText = CreateTMP(nameCol.transform, "SkillName", "TÊN KỸ NĂNG", 26, font, FontStyles.Bold, new Color(0.35f, 0.85f, 1f), TextAlignmentOptions.Left);
            nameText.raycastTarget = false;

            TextMeshProUGUI tagText = CreateTMP(nameCol.transform, "SkillTag", "Khống Chế & Hút Quái", 17, font, FontStyles.Italic, new Color(0.8f, 0.9f, 1f), TextAlignmentOptions.Left);
            tagText.raycastTarget = false;

            // Description
            TextMeshProUGUI descText = CreateTMP(tooltipObj.transform, "Desc", "Mô tả chi tiết kỹ năng xuất hiện ở đây...", 18, font, FontStyles.Normal, new Color(0.9f, 0.93f, 0.98f), TextAlignmentOptions.Left);
            descText.GetComponent<RectTransform>().sizeDelta = new Vector2(456, 85);
            descText.raycastTarget = false;

            // Attributes Grid
            GameObject attrObj = new GameObject("Attributes Grid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(Image));
            attrObj.transform.SetParent(tooltipObj.transform, false);
            attrObj.GetComponent<RectTransform>().sizeDelta = new Vector2(456, 90);
            Image attrBg = attrObj.GetComponent<Image>();
            if (bgSprite != null) { attrBg.sprite = bgSprite; attrBg.type = Image.Type.Sliced; }
            attrBg.color = new Color(0.1f, 0.14f, 0.2f, 0.7f);
            attrBg.raycastTarget = false;

            GridLayoutGroup glg = attrObj.GetComponent<GridLayoutGroup>();
            glg.padding = new RectOffset(14, 14, 10, 10);
            glg.cellSize = new Vector2(210, 32);
            glg.spacing = new Vector2(14, 6);
            glg.childAlignment = TextAnchor.MiddleCenter;

            // Text labels without unicode emoji
            CreateAttrItem(attrObj.transform, "Hồi chiêu:", font, out TextMeshProUGUI cdText);
            CreateAttrItem(attrObj.transform, "Thời lượng:", font, out TextMeshProUGUI durText);
            CreateAttrItem(attrObj.transform, "Bán kính:", font, out TextMeshProUGUI radText);
            CreateAttrItem(attrObj.transform, "Sát thương:", font, out TextMeshProUGUI dmgText);

            UICharacterSkillTooltip tooltipComp = tooltipObj.GetComponent<UICharacterSkillTooltip>();
            tooltipComp.BindReferences(cg, rt, skillIcon, nameText, tagText, descText, cdText, durText, radText, dmgText);

            return tooltipComp;
        }

        private static void CreateAttrItem(Transform parent, string label, TMP_FontAsset font, out TextMeshProUGUI val)
        {
            GameObject item = new GameObject(label, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            item.transform.SetParent(parent, false);
            HorizontalLayoutGroup hlg = item.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            TextMeshProUGUI lbl = CreateTMP(item.transform, "Label", label, 17, font, FontStyles.Normal, new Color(0.75f, 0.8f, 0.88f), TextAlignmentOptions.Left, 110);
            lbl.raycastTarget = false;

            val = CreateTMP(item.transform, "Val", "-", 18, font, FontStyles.Bold, Color.white, TextAlignmentOptions.Left, 80);
            val.raycastTarget = false;
        }

        private static TextMeshProUGUI CreateTMP(Transform parent, string name, string text, float size, TMP_FontAsset font, FontStyles style, Color color, TextAlignmentOptions align, float width = -1)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = align;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            if (width > 0)
            {
                RectTransform rt = obj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(width, size + 10);
            }
            return tmp;
        }

        private static Sprite GetStageStarSprite(UICharactersPanel panel)
        {
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var starField = typeof(UICharactersPanel).GetField("stageStarPrefab", flags);
            if (starField != null)
            {
                GameObject starPrefab = starField.GetValue(panel) as GameObject;
                if (starPrefab != null)
                {
                    Image img = starPrefab.GetComponentInChildren<Image>();
                    if (img != null) return img.sprite;
                }
            }
            return null;
        }
    }
}
