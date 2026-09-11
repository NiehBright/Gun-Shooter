#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    [InitializeOnLoad]
    public static class UIGachaPageSetup
    {
        private const string RESULT_ITEM_PREFAB_PATH = "Assets/Project Data/Game/Prefabs/UI/Pages/ResultItem.prefab";
        private const string GACHA_PAGE_PREFAB_PATH = "Assets/Project Data/Game/Prefabs/UI/Pages/UI Gacha Page.prefab";
        private const string SCENE_PATH = "Assets/Project Data/Game/Scenes/Game.unity";
        private const string SETUP_KEY = "GunShooter_GachaPageSetup_v3";

        // Sprite Assets
        private const string CARD_FRAME_OUTER = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Frame_Custom/Frame_CardFrame01_White1.png";
        private const string CARD_FRAME_INNER = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Frame_Custom/Frame_CardFrame01_White2.png";
        private const string BANNER_FRAME = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Frame_Custom/Frame_BannerFrame02_White1.png";
        private const string BTN_SKY = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Button_Demo/Btn_TextButton_Square01_Sky.Png";
        private const string BTN_ORANGE = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Button_Demo/Btn_TextButton_Square01_Orange.Png";
        private const string BTN_ICON_BLUE = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Button_Demo/Btn_IconButton_Square01_Blue.png";
        private const string ICON_ARROW_LEFT = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Icon_PictoIcons(x2)/128/Icon_Arrow_1_Left.Png";
        private const string ICON_GEM = "Assets/ThirdParty/Space_Exploration_GUI_Kit/Shop_Assets/gems-tier-1-extra-large.png";
        private const string ICON_CRATE = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Demo/Demo_ItemIcon/Item_ShopIcon_BulletBox02.png";
        private const string POPUP_FRAME = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Popup/Popup_Frame01_Navy1.png";
        private const string TAG_RED = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Label/Label_Tag01_Red.png";
        private const string TAG_SKY = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Label/Label_Tag01_SkyBlue.png";
        private const string TAG_ORANGE = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Label/Label_Tag01_Orange.png";
        private const string LABEL_PILL = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Label/Label_Label01_White1.png";
        private const string SCREEN_GLOW = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Demo/Demo_Backgound/Background_ScreenGlow.png";

        // Fonts
        private const string FONT_OXANIUM = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Fonts/Oxanium-ExtraBold_Extended ASCII SDF.asset";
        private const string FONT_FALLBACK = "Assets/Project Data/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset";
        private const string FONT_BEVIETNAM = "Assets/Project Data/Game/Fonts/BeVietnamPro-Bold SDF.asset";
        private const string FONT_CHAKRA = "Assets/Project Data/Game/Fonts/ChakraPetch-Bold SDF.asset";

        static UIGachaPageSetup()
        {
            EditorApplication.delayCall += AutoRunCheck;
        }

        private static void AutoRunCheck()
        {
            if (!EditorPrefs.GetBool(SETUP_KEY, false))
            {
                RunSetup(false);
                EditorPrefs.SetBool(SETUP_KEY, true);
            }
        }

        [MenuItem("GunShooter/Setup Beautiful Gacha UI")]
        public static void ForceSetup()
        {
            RunSetup(true);
        }

        public static void RunSetup(bool isManual)
        {
            Debug.Log("[GachaSetup] Bắt đầu thiết lập UI Gacha mới với bộ asset GUI Pro-SurvivalClean & Font tiếng Việt...");

            TMP_FontAsset fontBeVietnam = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_BEVIETNAM);
            TMP_FontAsset fontChakra = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_CHAKRA);
            TMP_FontAsset fontOxanium = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_OXANIUM);
            TMP_FontAsset fontFallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_FALLBACK);

            TMP_FontAsset mainFont = fontChakra != null ? fontChakra : (fontOxanium != null ? fontOxanium : fontFallback);
            TMP_FontAsset textFont = fontBeVietnam != null ? fontBeVietnam : (fontChakra != null ? fontChakra : fontFallback);

            // 1. Build ResultItem Prefab
            GameObject resultItemPrefab = BuildResultItemPrefab(mainFont, textFont);

            // 2. Build UI Gacha Page Prefab
            GameObject gachaPagePrefab = BuildGachaPagePrefab(resultItemPrefab, mainFont, textFont);

            // 3. Clean up scene instance rect transforms if Game.unity is active
            SyncSceneInstance(gachaPagePrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[GachaSetup] Hoàn tất thiết lập UI Gacha thành công!");

            if (isManual)
            {
                EditorUtility.DisplayDialog("Gacha UI Remake",
                    "Đã tạo mới và thiết lập thành công giao diện Gacha:\n\n" +
                    "1. ResultItem.prefab:\n" +
                    "   - Thẻ hiển thị Drone kích thước chuẩn 160x210\n" +
                    "   - Frame Sci-Fi 2 lớp (Outer Frame + Inner Slot)\n" +
                    "   - Icon Drone to rõ ràng kèm hiệu ứng Glow phía sau\n" +
                    "   - Tên Drone & Nhãn trạng thái (MỞ KHOÁ! hoặc +X Thẻ)\n" +
                    "   - Huy hiệu góc (MỚI! màu đỏ / THẺ màu xanh sky)\n\n" +
                    "2. UI Gacha Page.prefab:\n" +
                    "   - Header bar hiện đại: Nút quay lại, Banner tiêu đề, Thanh hiển thị Gems\n" +
                    "   - Kho tiếp tế trung tâm: Hòm tiếp tế podium, khung viền Sci-Fi, hiệu ứng ánh sáng\n" +
                    "   - Nút Quay x1 (Sky Blue) & Quay x10 (Cam Vàng) kèm tag giảm giá -10%\n" +
                    "   - Popup kết quả triệu hồi với lưới Grid 5x2 cực kỳ cân đối và nút TIẾP TỤC\n\n" +
                    "Tất cả logic & liên kết hoạt động trơn tru 100%!", "Tuyệt vời!");
            }
        }

        private static GameObject BuildResultItemPrefab(TMP_FontAsset titleFont, TMP_FontAsset textFont)
        {
            Sprite cardOuterSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CARD_FRAME_OUTER);
            Sprite cardInnerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CARD_FRAME_INNER);
            Sprite glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SCREEN_GLOW);
            Sprite tagRedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TAG_RED);
            Sprite tagSkySprite = AssetDatabase.LoadAssetAtPath<Sprite>(TAG_SKY);

            GameObject root = new GameObject("ResultItem");
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(160, 210);
            rootRect.pivot = new Vector2(0.5f, 0.5f);

            CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;

            UIGachaResultItem itemScript = root.AddComponent<UIGachaResultItem>();

            // 1. Card Outer Frame
            GameObject outerObj = new GameObject("Card_Outer");
            outerObj.transform.SetParent(root.transform, false);
            RectTransform outerRect = outerObj.AddComponent<RectTransform>();
            outerRect.anchorMin = Vector2.zero;
            outerRect.anchorMax = Vector2.one;
            outerRect.offsetMin = Vector2.zero;
            outerRect.offsetMax = Vector2.zero;
            Image outerImg = outerObj.AddComponent<Image>();
            if (cardOuterSprite != null)
            {
                outerImg.sprite = cardOuterSprite;
                outerImg.type = Image.Type.Sliced;
            }
            outerImg.color = new Color(0.24f, 0.32f, 0.44f, 1f);

            // 2. Card Inner Frame
            GameObject innerObj = new GameObject("Card_Inner");
            innerObj.transform.SetParent(root.transform, false);
            RectTransform innerRect = innerObj.AddComponent<RectTransform>();
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.offsetMin = new Vector2(5, 5);
            innerRect.offsetMax = new Vector2(-5, -5);
            Image innerImg = innerObj.AddComponent<Image>();
            if (cardInnerSprite != null)
            {
                innerImg.sprite = cardInnerSprite;
                innerImg.type = Image.Type.Sliced;
            }
            innerImg.color = new Color(0.08f, 0.11f, 0.16f, 0.98f);

            // 3. Icon Glow
            if (glowSprite != null)
            {
                GameObject glowObj = new GameObject("Icon_Glow");
                glowObj.transform.SetParent(root.transform, false);
                RectTransform glowRect = glowObj.AddComponent<RectTransform>();
                glowRect.anchorMin = glowRect.anchorMax = glowRect.pivot = new Vector2(0.5f, 0.5f);
                glowRect.anchoredPosition = new Vector2(0, 20);
                glowRect.sizeDelta = new Vector2(120, 120);
                Image glowImg = glowObj.AddComponent<Image>();
                glowImg.sprite = glowSprite;
                glowImg.color = new Color(0.22f, 0.74f, 0.96f, 0.25f);
                glowImg.raycastTarget = false;
            }

            // 4. Drone Icon
            GameObject iconObj = new GameObject("Drone_Icon");
            iconObj.transform.SetParent(root.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = iconRect.anchorMax = iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(0, 22);
            iconRect.sizeDelta = new Vector2(105, 105);
            Image droneIconImg = iconObj.AddComponent<Image>();
            droneIconImg.preserveAspect = true;

            // 5. Drone Name Text
            GameObject nameObj = new GameObject("Drone_Name");
            nameObj.transform.SetParent(root.transform, false);
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(1, 0);
            nameRect.pivot = new Vector2(0.5f, 0);
            nameRect.anchoredPosition = new Vector2(0, 32);
            nameRect.sizeDelta = new Vector2(-16, 24);
            TextMeshProUGUI droneNameText = nameObj.AddComponent<TextMeshProUGUI>();
            droneNameText.font = textFont;
            droneNameText.text = "TÊN DRONE";
            droneNameText.fontSize = 13;
            droneNameText.fontStyle = FontStyles.Bold;
            droneNameText.alignment = TextAlignmentOptions.Center;
            droneNameText.color = new Color(0.92f, 0.95f, 0.98f, 1f);
            droneNameText.overflowMode = TextOverflowModes.Ellipsis;

            // 6. Status Text
            GameObject statusObj = new GameObject("Status_Text");
            statusObj.transform.SetParent(root.transform, false);
            RectTransform statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 0);
            statusRect.anchorMax = new Vector2(1, 0);
            statusRect.pivot = new Vector2(0.5f, 0);
            statusRect.anchoredPosition = new Vector2(0, 10);
            statusRect.sizeDelta = new Vector2(-16, 20);
            TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
            statusText.font = textFont;
            statusText.text = "MỞ KHOÁ!";
            statusText.fontSize = 12;
            statusText.fontStyle = FontStyles.Bold;
            statusText.alignment = TextAlignmentOptions.Center;
            statusText.color = new Color(0.22f, 0.82f, 0.98f, 1f);

            // 7. New Badge (Top-left)
            GameObject newBadgeObj = new GameObject("New_Badge");
            newBadgeObj.transform.SetParent(root.transform, false);
            RectTransform newBadgeRect = newBadgeObj.AddComponent<RectTransform>();
            newBadgeRect.anchorMin = new Vector2(0, 1);
            newBadgeRect.anchorMax = new Vector2(0, 1);
            newBadgeRect.pivot = new Vector2(0, 1);
            newBadgeRect.anchoredPosition = new Vector2(4, -4);
            newBadgeRect.sizeDelta = new Vector2(62, 24);
            Image newBadgeImg = newBadgeObj.AddComponent<Image>();
            if (tagRedSprite != null)
            {
                newBadgeImg.sprite = tagRedSprite;
                newBadgeImg.type = Image.Type.Sliced;
            }
            newBadgeImg.color = Color.white;

            GameObject newBadgeTextObj = new GameObject("Text");
            newBadgeTextObj.transform.SetParent(newBadgeObj.transform, false);
            RectTransform newBadgeTextRect = newBadgeTextObj.AddComponent<RectTransform>();
            newBadgeTextRect.anchorMin = Vector2.zero;
            newBadgeTextRect.anchorMax = Vector2.one;
            newBadgeTextRect.offsetMin = Vector2.zero;
            newBadgeTextRect.offsetMax = Vector2.zero;
            TextMeshProUGUI newBadgeText = newBadgeTextObj.AddComponent<TextMeshProUGUI>();
            newBadgeText.font = textFont;
            newBadgeText.text = "MỚI!";
            newBadgeText.fontSize = 11;
            newBadgeText.fontStyle = FontStyles.Bold;
            newBadgeText.alignment = TextAlignmentOptions.Center;
            newBadgeText.color = Color.white;

            // 8. Duplicate Badge (Top-right)
            GameObject dupBadgeObj = new GameObject("Duplicate_Badge");
            dupBadgeObj.transform.SetParent(root.transform, false);
            RectTransform dupBadgeRect = dupBadgeObj.AddComponent<RectTransform>();
            dupBadgeRect.anchorMin = new Vector2(1, 1);
            dupBadgeRect.anchorMax = new Vector2(1, 1);
            dupBadgeRect.pivot = new Vector2(1, 1);
            dupBadgeRect.anchoredPosition = new Vector2(-4, -4);
            dupBadgeRect.sizeDelta = new Vector2(56, 22);
            Image dupBadgeImg = dupBadgeObj.AddComponent<Image>();
            if (tagSkySprite != null)
            {
                dupBadgeImg.sprite = tagSkySprite;
                dupBadgeImg.type = Image.Type.Sliced;
            }
            dupBadgeImg.color = Color.white;

            GameObject dupBadgeTextObj = new GameObject("Text");
            dupBadgeTextObj.transform.SetParent(dupBadgeObj.transform, false);
            RectTransform dupBadgeTextRect = dupBadgeTextObj.AddComponent<RectTransform>();
            dupBadgeTextRect.anchorMin = Vector2.zero;
            dupBadgeTextRect.anchorMax = Vector2.one;
            dupBadgeTextRect.offsetMin = Vector2.zero;
            dupBadgeTextRect.offsetMax = Vector2.zero;
            TextMeshProUGUI dupBadgeText = dupBadgeTextObj.AddComponent<TextMeshProUGUI>();
            dupBadgeText.font = textFont;
            dupBadgeText.text = "THẺ";
            dupBadgeText.fontSize = 10;
            dupBadgeText.fontStyle = FontStyles.Bold;
            dupBadgeText.alignment = TextAlignmentOptions.Center;
            dupBadgeText.color = Color.white;

            // Wire Serialized Properties on UIGachaResultItem
            SerializedObject itemSo = new SerializedObject(itemScript);
            itemSo.FindProperty("droneIcon").objectReferenceValue = droneIconImg;
            itemSo.FindProperty("droneNameText").objectReferenceValue = droneNameText;
            itemSo.FindProperty("statusText").objectReferenceValue = statusText;
            itemSo.FindProperty("newBadge").objectReferenceValue = newBadgeObj;
            itemSo.FindProperty("duplicateBadge").objectReferenceValue = dupBadgeObj;
            itemSo.ApplyModifiedProperties();

            string dir = Path.GetDirectoryName(RESULT_ITEM_PREFAB_PATH);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, RESULT_ITEM_PREFAB_PATH);
            Object.DestroyImmediate(root);
            Debug.Log($"[GachaSetup] Đã lưu ResultItem.prefab thành công tại: {RESULT_ITEM_PREFAB_PATH}");
            return savedPrefab;
        }

        private static GameObject BuildGachaPagePrefab(GameObject resultItemPrefab, TMP_FontAsset titleFont, TMP_FontAsset textFont)
        {
            Sprite bannerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BANNER_FRAME);
            Sprite cardInnerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CARD_FRAME_INNER);
            Sprite btnSkySprite = AssetDatabase.LoadAssetAtPath<Sprite>(BTN_SKY);
            Sprite btnOrangeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BTN_ORANGE);
            Sprite btnIconBlueSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BTN_ICON_BLUE);
            Sprite arrowLeftSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_ARROW_LEFT);
            Sprite gemSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_GEM);
            Sprite crateSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_CRATE);
            Sprite popupFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(POPUP_FRAME);
            Sprite tagRedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TAG_RED);
            Sprite tagOrangeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TAG_ORANGE);
            Sprite pillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(LABEL_PILL);
            Sprite glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SCREEN_GLOW);

            // Root GameObject
            GameObject root = new GameObject("UI Gacha Page");
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            rootRect.pivot = new Vector2(0.5f, 0.5f);

            Canvas rootCanvas = root.AddComponent<Canvas>();
            rootCanvas.overrideSorting = true;
            rootCanvas.sortingOrder = 1;

            root.AddComponent<GraphicRaycaster>();

            CanvasGroup rootCg = root.AddComponent<CanvasGroup>();
            rootCg.alpha = 1f;
            rootCg.interactable = true;
            rootCg.blocksRaycasts = true;

            UIGachaPage pageScript = root.AddComponent<UIGachaPage>();

            // Background Dark Dim
            GameObject bgObj = new GameObject("Background_Dim");
            bgObj.transform.SetParent(root.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.06f, 0.09f, 0.96f);

            // Main Panel (animated by UIGachaPage)
            GameObject mainPanel = new GameObject("Main Panel");
            mainPanel.transform.SetParent(root.transform, false);
            RectTransform mainPanelRect = mainPanel.AddComponent<RectTransform>();
            mainPanelRect.anchorMin = Vector2.zero;
            mainPanelRect.anchorMax = Vector2.one;
            mainPanelRect.offsetMin = Vector2.zero;
            mainPanelRect.offsetMax = Vector2.zero;
            mainPanelRect.pivot = new Vector2(0.5f, 0.5f);

            // -------------------------------------------------------------
            // A. HEADER BAR
            // -------------------------------------------------------------
            GameObject headerObj = new GameObject("Header_Bar");
            headerObj.transform.SetParent(mainPanel.transform, false);
            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0, 100);

            // Back Button
            GameObject backBtnObj = new GameObject("Back_Button");
            backBtnObj.transform.SetParent(headerObj.transform, false);
            RectTransform backBtnRect = backBtnObj.AddComponent<RectTransform>();
            backBtnRect.anchorMin = new Vector2(0, 0.5f);
            backBtnRect.anchorMax = new Vector2(0, 0.5f);
            backBtnRect.pivot = new Vector2(0.5f, 0.5f);
            backBtnRect.anchoredPosition = new Vector2(65, 0);
            backBtnRect.sizeDelta = new Vector2(58, 58);
            Image backBtnImg = backBtnObj.AddComponent<Image>();
            if (btnIconBlueSprite != null)
            {
                backBtnImg.sprite = btnIconBlueSprite;
                backBtnImg.type = Image.Type.Sliced;
            }
            backBtnImg.color = Color.white;
            Button backBtn = backBtnObj.AddComponent<Button>();
            backBtn.targetGraphic = backBtnImg;

            // Back Icon Arrow
            GameObject backIconObj = new GameObject("Icon");
            backIconObj.transform.SetParent(backBtnObj.transform, false);
            RectTransform backIconRect = backIconObj.AddComponent<RectTransform>();
            backIconRect.anchorMin = backIconRect.anchorMax = backIconRect.pivot = new Vector2(0.5f, 0.5f);
            backIconRect.anchoredPosition = Vector2.zero;
            backIconRect.sizeDelta = new Vector2(30, 30);
            Image backIconImg = backIconObj.AddComponent<Image>();
            if (arrowLeftSprite != null)
            {
                backIconImg.sprite = arrowLeftSprite;
            }
            backIconImg.color = Color.white;
            backIconImg.raycastTarget = false;

            // Title Banner
            GameObject titleBannerObj = new GameObject("Title_Banner");
            titleBannerObj.transform.SetParent(headerObj.transform, false);
            RectTransform titleBannerRect = titleBannerObj.AddComponent<RectTransform>();
            titleBannerRect.anchorMin = titleBannerRect.anchorMax = titleBannerRect.pivot = new Vector2(0.5f, 0.5f);
            titleBannerRect.anchoredPosition = Vector2.zero;
            titleBannerRect.sizeDelta = new Vector2(380, 58);
            Image titleBannerImg = titleBannerObj.AddComponent<Image>();
            if (bannerSprite != null)
            {
                titleBannerImg.sprite = bannerSprite;
                titleBannerImg.type = Image.Type.Sliced;
            }
            titleBannerImg.color = new Color(0.12f, 0.17f, 0.25f, 0.95f);

            GameObject titleTextObj = new GameObject("Title_Text");
            titleTextObj.transform.SetParent(titleBannerObj.transform, false);
            RectTransform titleTextRect = titleTextObj.AddComponent<RectTransform>();
            titleTextRect.anchorMin = Vector2.zero;
            titleTextRect.anchorMax = Vector2.one;
            titleTextRect.offsetMin = Vector2.zero;
            titleTextRect.offsetMax = Vector2.zero;
            TextMeshProUGUI titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
            titleText.font = titleFont;
            titleText.text = "DRONE GACHA";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.characterSpacing = 2;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            // Gems Counter Container (Right)
            GameObject gemsContainerObj = new GameObject("Gems_Counter");
            gemsContainerObj.transform.SetParent(headerObj.transform, false);
            RectTransform gemsContainerRect = gemsContainerObj.AddComponent<RectTransform>();
            gemsContainerRect.anchorMin = new Vector2(1, 0.5f);
            gemsContainerRect.anchorMax = new Vector2(1, 0.5f);
            gemsContainerRect.pivot = new Vector2(0.5f, 0.5f);
            gemsContainerRect.anchoredPosition = new Vector2(-110, 0);
            gemsContainerRect.sizeDelta = new Vector2(160, 44);
            Image gemsPillImg = gemsContainerObj.AddComponent<Image>();
            if (pillSprite != null)
            {
                gemsPillImg.sprite = pillSprite;
                gemsPillImg.type = Image.Type.Sliced;
            }
            gemsPillImg.color = new Color(0.08f, 0.12f, 0.18f, 0.95f);

            GameObject gemIconObj = new GameObject("Gem_Icon");
            gemIconObj.transform.SetParent(gemsContainerObj.transform, false);
            RectTransform gemIconRect = gemIconObj.AddComponent<RectTransform>();
            gemIconRect.anchorMin = new Vector2(0, 0.5f);
            gemIconRect.anchorMax = new Vector2(0, 0.5f);
            gemIconRect.pivot = new Vector2(0, 0.5f);
            gemIconRect.anchoredPosition = new Vector2(10, 0);
            gemIconRect.sizeDelta = new Vector2(28, 28);
            Image gemIconImg = gemIconObj.AddComponent<Image>();
            if (gemSprite != null)
            {
                gemIconImg.sprite = gemSprite;
                gemIconImg.preserveAspect = true;
            }

            GameObject gemsAmountTextObj = new GameObject("Gems_Amount_Text");
            gemsAmountTextObj.transform.SetParent(gemsContainerObj.transform, false);
            RectTransform gemsAmountTextRect = gemsAmountTextObj.AddComponent<RectTransform>();
            gemsAmountTextRect.anchorMin = new Vector2(0, 0);
            gemsAmountTextRect.anchorMax = new Vector2(1, 1);
            gemsAmountTextRect.offsetMin = new Vector2(44, 0);
            gemsAmountTextRect.offsetMax = new Vector2(-12, 0);
            TextMeshProUGUI gemsAmountText = gemsAmountTextObj.AddComponent<TextMeshProUGUI>();
            gemsAmountText.font = titleFont;
            gemsAmountText.text = "0";
            gemsAmountText.fontSize = 20;
            gemsAmountText.fontStyle = FontStyles.Bold;
            gemsAmountText.alignment = TextAlignmentOptions.Right;
            gemsAmountText.color = new Color(0.24f, 0.82f, 0.98f, 1f);

            // -------------------------------------------------------------
            // B. CENTER SHOWCASE PODIUM
            // -------------------------------------------------------------
            GameObject showcaseObj = new GameObject("Showcase_Card");
            showcaseObj.transform.SetParent(mainPanel.transform, false);
            RectTransform showcaseRect = showcaseObj.AddComponent<RectTransform>();
            showcaseRect.anchorMin = showcaseRect.anchorMax = showcaseRect.pivot = new Vector2(0.5f, 0.5f);
            showcaseRect.anchoredPosition = new Vector2(0, 30);
            showcaseRect.sizeDelta = new Vector2(680, 470);

            // Outer Frame
            Image showcaseOuter = showcaseObj.AddComponent<Image>();
            if (bannerSprite != null)
            {
                showcaseOuter.sprite = bannerSprite;
                showcaseOuter.type = Image.Type.Sliced;
            }
            showcaseOuter.color = new Color(0.12f, 0.16f, 0.24f, 0.95f);

            // Inner Frame
            GameObject showcaseInnerObj = new GameObject("Inner_Panel");
            showcaseInnerObj.transform.SetParent(showcaseObj.transform, false);
            RectTransform showcaseInnerRect = showcaseInnerObj.AddComponent<RectTransform>();
            showcaseInnerRect.anchorMin = Vector2.zero;
            showcaseInnerRect.anchorMax = Vector2.one;
            showcaseInnerRect.offsetMin = new Vector2(6, 6);
            showcaseInnerRect.offsetMax = new Vector2(-6, -6);
            Image showcaseInnerImg = showcaseInnerObj.AddComponent<Image>();
            if (cardInnerSprite != null)
            {
                showcaseInnerImg.sprite = cardInnerSprite;
                showcaseInnerImg.type = Image.Type.Sliced;
            }
            showcaseInnerImg.color = new Color(0.06f, 0.08f, 0.12f, 0.98f);

            // Soft Glow
            if (glowSprite != null)
            {
                GameObject centerGlowObj = new GameObject("Showcase_Glow");
                centerGlowObj.transform.SetParent(showcaseInnerObj.transform, false);
                RectTransform centerGlowRect = centerGlowObj.AddComponent<RectTransform>();
                centerGlowRect.anchorMin = centerGlowRect.anchorMax = centerGlowRect.pivot = new Vector2(0.5f, 0.5f);
                centerGlowRect.anchoredPosition = new Vector2(0, 10);
                centerGlowRect.sizeDelta = new Vector2(360, 360);
                Image centerGlowImg = centerGlowObj.AddComponent<Image>();
                centerGlowImg.sprite = glowSprite;
                centerGlowImg.color = new Color(0.18f, 0.58f, 0.95f, 0.22f);
                centerGlowImg.raycastTarget = false;
            }

            // Tagline Banner
            GameObject tagObj = new GameObject("Tagline_Banner");
            tagObj.transform.SetParent(showcaseInnerObj.transform, false);
            RectTransform tagRect = tagObj.AddComponent<RectTransform>();
            tagRect.anchorMin = new Vector2(0.5f, 1);
            tagRect.anchorMax = new Vector2(0.5f, 1);
            tagRect.pivot = new Vector2(0.5f, 1);
            tagRect.anchoredPosition = new Vector2(0, -18);
            tagRect.sizeDelta = new Vector2(460, 36);
            Image tagImg = tagObj.AddComponent<Image>();
            if (tagOrangeSprite != null)
            {
                tagImg.sprite = tagOrangeSprite;
                tagImg.type = Image.Type.Sliced;
            }
            tagImg.color = Color.white;

            GameObject tagTextObj = new GameObject("Tag_Text");
            tagTextObj.transform.SetParent(tagObj.transform, false);
            RectTransform tagTextRect = tagTextObj.AddComponent<RectTransform>();
            tagTextRect.anchorMin = Vector2.zero;
            tagTextRect.anchorMax = Vector2.one;
            tagTextRect.offsetMin = Vector2.zero;
            tagTextRect.offsetMax = Vector2.zero;
            TextMeshProUGUI tagText = tagTextObj.AddComponent<TextMeshProUGUI>();
            tagText.font = textFont;
            tagText.text = "KHO TIẾP TẾ CHIẾN THUẬT - TỈ LỆ NHẬN DRONE HIẾM X2";
            tagText.fontSize = 13;
            tagText.fontStyle = FontStyles.Bold;
            tagText.alignment = TextAlignmentOptions.Center;
            tagText.color = Color.white;

            // Crate Box Image
            GameObject crateObj = new GameObject("Crate_Graphic");
            crateObj.transform.SetParent(showcaseInnerObj.transform, false);
            RectTransform crateRect = crateObj.AddComponent<RectTransform>();
            crateRect.anchorMin = crateRect.anchorMax = crateRect.pivot = new Vector2(0.5f, 0.5f);
            crateRect.anchoredPosition = new Vector2(0, 15);
            crateRect.sizeDelta = new Vector2(230, 230);
            Image crateImg = crateObj.AddComponent<Image>();
            if (crateSprite != null)
            {
                crateImg.sprite = crateSprite;
                crateImg.preserveAspect = true;
            }

            // Description Label
            GameObject descObj = new GameObject("Desc_Text");
            descObj.transform.SetParent(showcaseInnerObj.transform, false);
            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.5f, 0);
            descRect.anchorMax = new Vector2(0.5f, 0);
            descRect.pivot = new Vector2(0.5f, 0);
            descRect.anchoredPosition = new Vector2(0, 24);
            descRect.sizeDelta = new Vector2(520, 48);
            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.font = textFont;
            descText.text = "Mở hòm tiếp tế để nhận Drone chiến đấu ngẫu nhiên\nhoặc tích luỹ mảnh thẻ để nâng cấp sức mạnh!";
            descText.fontSize = 14;
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = new Color(0.65f, 0.74f, 0.85f, 1f);

            // -------------------------------------------------------------
            // C. BOTTOM PULL BUTTONS
            // -------------------------------------------------------------
            GameObject bottomBar = new GameObject("Bottom_Bar");
            bottomBar.transform.SetParent(mainPanel.transform, false);
            RectTransform bottomBarRect = bottomBar.AddComponent<RectTransform>();
            bottomBarRect.anchorMin = new Vector2(0, 0);
            bottomBarRect.anchorMax = new Vector2(1, 0);
            bottomBarRect.pivot = new Vector2(0.5f, 0);
            bottomBarRect.anchoredPosition = Vector2.zero;
            bottomBarRect.sizeDelta = new Vector2(0, 150);

            // Single Pull Button (Sky Blue)
            GameObject singleBtnObj = new GameObject("Single_Pull_Button");
            singleBtnObj.transform.SetParent(bottomBar.transform, false);
            RectTransform singleBtnRect = singleBtnObj.AddComponent<RectTransform>();
            singleBtnRect.anchorMin = singleBtnRect.anchorMax = singleBtnRect.pivot = new Vector2(0.5f, 0.5f);
            singleBtnRect.anchoredPosition = new Vector2(-165, 0);
            singleBtnRect.sizeDelta = new Vector2(270, 84);
            Image singleBtnImg = singleBtnObj.AddComponent<Image>();
            if (btnSkySprite != null)
            {
                singleBtnImg.sprite = btnSkySprite;
                singleBtnImg.type = Image.Type.Sliced;
            }
            singleBtnImg.color = Color.white;
            Button singlePullButton = singleBtnObj.AddComponent<Button>();
            singlePullButton.targetGraphic = singleBtnImg;

            // Single Pull Title
            GameObject singleTitleObj = new GameObject("Title");
            singleTitleObj.transform.SetParent(singleBtnObj.transform, false);
            RectTransform singleTitleRect = singleTitleObj.AddComponent<RectTransform>();
            singleTitleRect.anchorMin = singleTitleRect.anchorMax = singleTitleRect.pivot = new Vector2(0.5f, 0.5f);
            singleTitleRect.anchoredPosition = new Vector2(0, 14);
            singleTitleRect.sizeDelta = new Vector2(200, 30);
            TextMeshProUGUI singleTitleText = singleTitleObj.AddComponent<TextMeshProUGUI>();
            singleTitleText.font = textFont;
            singleTitleText.text = "QUAY x1";
            singleTitleText.fontSize = 22;
            singleTitleText.fontStyle = FontStyles.Bold;
            singleTitleText.alignment = TextAlignmentOptions.Center;
            singleTitleText.color = Color.white;

            // Single Pull Cost Group (Gem + Price)
            GameObject singleCostGroup = new GameObject("Cost_Group");
            singleCostGroup.transform.SetParent(singleBtnObj.transform, false);
            RectTransform singleCostRect = singleCostGroup.AddComponent<RectTransform>();
            singleCostRect.anchorMin = singleCostRect.anchorMax = singleCostRect.pivot = new Vector2(0.5f, 0.5f);
            singleCostRect.anchoredPosition = new Vector2(0, -16);
            singleCostRect.sizeDelta = new Vector2(120, 26);

            GameObject singleGemObj = new GameObject("Gem");
            singleGemObj.transform.SetParent(singleCostGroup.transform, false);
            RectTransform singleGemRect = singleGemObj.AddComponent<RectTransform>();
            singleGemRect.anchorMin = singleGemRect.anchorMax = singleGemRect.pivot = new Vector2(0.5f, 0.5f);
            singleGemRect.anchoredPosition = new Vector2(-22, 0);
            singleGemRect.sizeDelta = new Vector2(24, 24);
            Image singleGemImg = singleGemObj.AddComponent<Image>();
            if (gemSprite != null)
            {
                singleGemImg.sprite = gemSprite;
                singleGemImg.preserveAspect = true;
            }

            GameObject singlePriceObj = new GameObject("Price_Text");
            singlePriceObj.transform.SetParent(singleCostGroup.transform, false);
            RectTransform singlePriceRect = singlePriceObj.AddComponent<RectTransform>();
            singlePriceRect.anchorMin = singlePriceRect.anchorMax = singlePriceRect.pivot = new Vector2(0.5f, 0.5f);
            singlePriceRect.anchoredPosition = new Vector2(16, 0);
            singlePriceRect.sizeDelta = new Vector2(60, 24);
            TextMeshProUGUI singlePriceText = singlePriceObj.AddComponent<TextMeshProUGUI>();
            singlePriceText.font = titleFont;
            singlePriceText.text = "50";
            singlePriceText.fontSize = 19;
            singlePriceText.fontStyle = FontStyles.Bold;
            singlePriceText.alignment = TextAlignmentOptions.Left;
            singlePriceText.color = Color.white;

            // Multi Pull Button (Orange/Gold)
            GameObject multiBtnObj = new GameObject("Multi_Pull_Button");
            multiBtnObj.transform.SetParent(bottomBar.transform, false);
            RectTransform multiBtnRect = multiBtnObj.AddComponent<RectTransform>();
            multiBtnRect.anchorMin = multiBtnRect.anchorMax = multiBtnRect.pivot = new Vector2(0.5f, 0.5f);
            multiBtnRect.anchoredPosition = new Vector2(165, 0);
            multiBtnRect.sizeDelta = new Vector2(270, 84);
            Image multiBtnImg = multiBtnObj.AddComponent<Image>();
            if (btnOrangeSprite != null)
            {
                multiBtnImg.sprite = btnOrangeSprite;
                multiBtnImg.type = Image.Type.Sliced;
            }
            multiBtnImg.color = Color.white;
            Button multiPullButton = multiBtnObj.AddComponent<Button>();
            multiPullButton.targetGraphic = multiBtnImg;

            // Discount Badge on Multi Pull
            GameObject discTagObj = new GameObject("Discount_Tag");
            discTagObj.transform.SetParent(multiBtnObj.transform, false);
            RectTransform discTagRect = discTagObj.AddComponent<RectTransform>();
            discTagRect.anchorMin = new Vector2(1, 1);
            discTagRect.anchorMax = new Vector2(1, 1);
            discTagRect.pivot = new Vector2(1, 1);
            discTagRect.anchoredPosition = new Vector2(6, 6);
            discTagRect.sizeDelta = new Vector2(88, 25);
            Image discTagImg = discTagObj.AddComponent<Image>();
            if (tagRedSprite != null)
            {
                discTagImg.sprite = tagRedSprite;
                discTagImg.type = Image.Type.Sliced;
            }
            discTagImg.color = Color.white;

            GameObject discTextObj = new GameObject("Text");
            discTextObj.transform.SetParent(discTagObj.transform, false);
            RectTransform discTextRect = discTextObj.AddComponent<RectTransform>();
            discTextRect.anchorMin = Vector2.zero;
            discTextRect.anchorMax = Vector2.one;
            discTextRect.offsetMin = Vector2.zero;
            discTextRect.offsetMax = Vector2.zero;
            TextMeshProUGUI discText = discTextObj.AddComponent<TextMeshProUGUI>();
            discText.font = textFont;
            discText.text = "-10% GIẢM";
            discText.fontSize = 11;
            discText.fontStyle = FontStyles.Bold;
            discText.alignment = TextAlignmentOptions.Center;
            discText.color = Color.white;

            // Multi Pull Title
            GameObject multiTitleObj = new GameObject("Title");
            multiTitleObj.transform.SetParent(multiBtnObj.transform, false);
            RectTransform multiTitleRect = multiTitleObj.AddComponent<RectTransform>();
            multiTitleRect.anchorMin = multiTitleRect.anchorMax = multiTitleRect.pivot = new Vector2(0.5f, 0.5f);
            multiTitleRect.anchoredPosition = new Vector2(0, 14);
            multiTitleRect.sizeDelta = new Vector2(200, 30);
            TextMeshProUGUI multiTitleText = multiTitleObj.AddComponent<TextMeshProUGUI>();
            multiTitleText.font = textFont;
            multiTitleText.text = "QUAY x10";
            multiTitleText.fontSize = 22;
            multiTitleText.fontStyle = FontStyles.Bold;
            multiTitleText.alignment = TextAlignmentOptions.Center;
            multiTitleText.color = Color.white;

            // Multi Pull Cost Group
            GameObject multiCostGroup = new GameObject("Cost_Group");
            multiCostGroup.transform.SetParent(multiBtnObj.transform, false);
            RectTransform multiCostRect = multiCostGroup.AddComponent<RectTransform>();
            multiCostRect.anchorMin = multiCostRect.anchorMax = multiCostRect.pivot = new Vector2(0.5f, 0.5f);
            multiCostRect.anchoredPosition = new Vector2(0, -16);
            multiCostRect.sizeDelta = new Vector2(120, 26);

            GameObject multiGemObj = new GameObject("Gem");
            multiGemObj.transform.SetParent(multiCostGroup.transform, false);
            RectTransform multiGemRect = multiGemObj.AddComponent<RectTransform>();
            multiGemRect.anchorMin = multiGemRect.anchorMax = multiGemRect.pivot = new Vector2(0.5f, 0.5f);
            multiGemRect.anchoredPosition = new Vector2(-22, 0);
            multiGemRect.sizeDelta = new Vector2(24, 24);
            Image multiGemImg = multiGemObj.AddComponent<Image>();
            if (gemSprite != null)
            {
                multiGemImg.sprite = gemSprite;
                multiGemImg.preserveAspect = true;
            }

            GameObject multiPriceObj = new GameObject("Price_Text");
            multiPriceObj.transform.SetParent(multiCostGroup.transform, false);
            RectTransform multiPriceRect = multiPriceObj.AddComponent<RectTransform>();
            multiPriceRect.anchorMin = multiPriceRect.anchorMax = multiPriceRect.pivot = new Vector2(0.5f, 0.5f);
            multiPriceRect.anchoredPosition = new Vector2(16, 0);
            multiPriceRect.sizeDelta = new Vector2(60, 24);
            TextMeshProUGUI multiPriceText = multiPriceObj.AddComponent<TextMeshProUGUI>();
            multiPriceText.font = titleFont;
            multiPriceText.text = "450";
            multiPriceText.fontSize = 19;
            multiPriceText.fontStyle = FontStyles.Bold;
            multiPriceText.alignment = TextAlignmentOptions.Left;
            multiPriceText.color = Color.white;

            // -------------------------------------------------------------
            // D. RESULT POPUP
            // -------------------------------------------------------------
            GameObject popupObj = new GameObject("Result_Popup");
            popupObj.transform.SetParent(root.transform, false);
            RectTransform popupRect = popupObj.AddComponent<RectTransform>();
            popupRect.anchorMin = Vector2.zero;
            popupRect.anchorMax = Vector2.one;
            popupRect.offsetMin = Vector2.zero;
            popupRect.offsetMax = Vector2.zero;
            popupRect.pivot = new Vector2(0.5f, 0.5f);

            CanvasGroup popupCg = popupObj.AddComponent<CanvasGroup>();
            popupCg.alpha = 0f;
            popupCg.interactable = true;
            popupCg.blocksRaycasts = true;

            UIGachaResultPopup popupScript = popupObj.AddComponent<UIGachaResultPopup>();

            // Modal Dim
            GameObject modalDimObj = new GameObject("Modal_Dim");
            modalDimObj.transform.SetParent(popupObj.transform, false);
            RectTransform modalDimRect = modalDimObj.AddComponent<RectTransform>();
            modalDimRect.anchorMin = Vector2.zero;
            modalDimRect.anchorMax = Vector2.one;
            modalDimRect.offsetMin = Vector2.zero;
            modalDimRect.offsetMax = Vector2.zero;
            Image modalDimImg = modalDimObj.AddComponent<Image>();
            modalDimImg.color = new Color(0f, 0f, 0f, 0.85f);

            // Modal Content Window
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(popupObj.transform, false);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = contentRect.anchorMax = contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(940, 620);
            Image contentImg = contentObj.AddComponent<Image>();
            if (popupFrameSprite != null)
            {
                contentImg.sprite = popupFrameSprite;
                contentImg.type = Image.Type.Sliced;
            }
            contentImg.color = new Color(0.12f, 0.16f, 0.24f, 0.98f);

            // Popup Title
            GameObject popTitleObj = new GameObject("Title_Text");
            popTitleObj.transform.SetParent(contentObj.transform, false);
            RectTransform popTitleRect = popTitleObj.AddComponent<RectTransform>();
            popTitleRect.anchorMin = new Vector2(0.5f, 1);
            popTitleRect.anchorMax = new Vector2(0.5f, 1);
            popTitleRect.pivot = new Vector2(0.5f, 1);
            popTitleRect.anchoredPosition = new Vector2(0, -30);
            popTitleRect.sizeDelta = new Vector2(480, 42);
            TextMeshProUGUI popTitleText = popTitleObj.AddComponent<TextMeshProUGUI>();
            popTitleText.font = titleFont;
            popTitleText.text = "KẾT QUẢ TRIỆU HỒI";
            popTitleText.fontSize = 26;
            popTitleText.fontStyle = FontStyles.Bold;
            popTitleText.alignment = TextAlignmentOptions.Center;
            popTitleText.color = Color.white;

            // Items Container (Grid 5x2)
            GameObject itemsContainerObj = new GameObject("Items_Container");
            itemsContainerObj.transform.SetParent(contentObj.transform, false);
            RectTransform itemsContainerRect = itemsContainerObj.AddComponent<RectTransform>();
            itemsContainerRect.anchorMin = itemsContainerRect.anchorMax = itemsContainerRect.pivot = new Vector2(0.5f, 0.5f);
            itemsContainerRect.anchoredPosition = new Vector2(0, -5);
            itemsContainerRect.sizeDelta = new Vector2(870, 450);

            GridLayoutGroup grid = itemsContainerObj.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(155, 205);
            grid.spacing = new Vector2(16, 16);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;

            // Continue Button
            GameObject continueBtnObj = new GameObject("Continue_Button");
            continueBtnObj.transform.SetParent(contentObj.transform, false);
            RectTransform continueBtnRect = continueBtnObj.AddComponent<RectTransform>();
            continueBtnRect.anchorMin = new Vector2(0.5f, 0);
            continueBtnRect.anchorMax = new Vector2(0.5f, 0);
            continueBtnRect.pivot = new Vector2(0.5f, 0);
            continueBtnRect.anchoredPosition = new Vector2(0, 30);
            continueBtnRect.sizeDelta = new Vector2(230, 56);
            Image continueBtnImg = continueBtnObj.AddComponent<Image>();
            if (btnSkySprite != null)
            {
                continueBtnImg.sprite = btnSkySprite;
                continueBtnImg.type = Image.Type.Sliced;
            }
            continueBtnImg.color = Color.white;
            Button continueBtn = continueBtnObj.AddComponent<Button>();
            continueBtn.targetGraphic = continueBtnImg;

            GameObject continueTextObj = new GameObject("Text");
            continueTextObj.transform.SetParent(continueBtnObj.transform, false);
            RectTransform continueTextRect = continueTextObj.AddComponent<RectTransform>();
            continueTextRect.anchorMin = Vector2.zero;
            continueTextRect.anchorMax = Vector2.one;
            continueTextRect.offsetMin = Vector2.zero;
            continueTextRect.offsetMax = Vector2.zero;
            TextMeshProUGUI continueText = continueTextObj.AddComponent<TextMeshProUGUI>();
            continueText.font = textFont;
            continueText.text = "TIẾP TỤC";
            continueText.fontSize = 20;
            continueText.fontStyle = FontStyles.Bold;
            continueText.alignment = TextAlignmentOptions.Center;
            continueText.color = Color.white;

            // Wire Serialized Properties on UIGachaResultPopup
            SerializedObject popupSo = new SerializedObject(popupScript);
            popupSo.FindProperty("titleText").objectReferenceValue = popTitleText;
            popupSo.FindProperty("continueButton").objectReferenceValue = continueBtn;
            popupSo.FindProperty("itemsContainer").objectReferenceValue = itemsContainerObj.transform;
            popupSo.FindProperty("itemPrefab").objectReferenceValue = resultItemPrefab;
            popupSo.FindProperty("canvasGroup").objectReferenceValue = popupCg;
            popupSo.FindProperty("contentRect").objectReferenceValue = contentRect;
            popupSo.ApplyModifiedProperties();

            popupObj.SetActive(false);

            // Wire Serialized Properties on UIGachaPage
            SerializedObject pageSo = new SerializedObject(pageScript);
            pageSo.FindProperty("singlePullButton").objectReferenceValue = singlePullButton;
            pageSo.FindProperty("multiPullButton").objectReferenceValue = multiPullButton;
            pageSo.FindProperty("backButton").objectReferenceValue = backBtn;
            pageSo.FindProperty("singlePriceText").objectReferenceValue = singlePriceText;
            pageSo.FindProperty("multiPriceText").objectReferenceValue = multiPriceText;
            pageSo.FindProperty("gemsAmountText").objectReferenceValue = gemsAmountText;
            pageSo.FindProperty("resultPopup").objectReferenceValue = popupScript;
            pageSo.FindProperty("mainPanelRect").objectReferenceValue = mainPanelRect;
            pageSo.ApplyModifiedProperties();

            // Save Prefab
            string pageDir = Path.GetDirectoryName(GACHA_PAGE_PREFAB_PATH);
            if (!Directory.Exists(pageDir)) Directory.CreateDirectory(pageDir);

            // Deactivate root so UI Gacha Page is initially hidden in hierarchy
            root.SetActive(false);

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, GACHA_PAGE_PREFAB_PATH);
            Object.DestroyImmediate(root);
            Debug.Log($"[GachaSetup] Đã lưu UI Gacha Page.prefab thành công tại: {GACHA_PAGE_PREFAB_PATH}");
            return savedPrefab;
        }

        private static void SyncSceneInstance(GameObject gachaPagePrefab)
        {
            if (gachaPagePrefab == null) return;

            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != SCENE_PATH)
            {
                // If Game.unity is not currently open, no need to force open and disrupt user
                return;
            }

            UIGachaPage pageInScene = Object.FindAnyObjectByType<UIGachaPage>(FindObjectsInactive.Include);
            if (pageInScene != null)
            {
                Undo.RecordObject(pageInScene.gameObject, "Set Gacha Page Inactive");
                pageInScene.gameObject.SetActive(false);

                RectTransform rect = pageInScene.GetComponent<RectTransform>();
                if (rect != null)
                {
                    Undo.RecordObject(rect, "Reset Gacha Page Rect");
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    rect.anchoredPosition = Vector2.zero;
                }

                EditorSceneManager.MarkSceneDirty(activeScene);
                Debug.Log("[GachaSetup] Đã đồng bộ RectTransform và ẩn UI Gacha Page trong Scene Game.unity!");
            }
        }
    }
}
#endif
