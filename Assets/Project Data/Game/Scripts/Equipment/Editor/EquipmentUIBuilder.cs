#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class EquipmentUIBuilder : EditorWindow
    {
        private const string PREFAB_FOLDER = "Assets/Project Data/Content/Data/Equipment";
        private const string PREFAB_PATH = "Assets/Project Data/Content/Data/Equipment/EquipmentSystem.prefab";
        private const string FRAME_PREFAB_PATH = "Assets/Project Data/Content/Data/Equipment/EquipmentItemFrame.prefab";
        private const string ACTIVE_ITEM_PREFAB_PATH = "Assets/Project Data/Content/Data/Equipment/EquipmentItemActive.prefab";

        // ───────────────────────────────────────────────
        // ASSET PATHS — GUI Pro-SurvivalClean
        // ───────────────────────────────────────────────
        private const string SC_COMPONENTS = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/";
        private const string SC_DEMO = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Demo/";

        // Frames (9-sliced)
        private const string SP_PANEL_BG        = SC_COMPONENTS + "Frame_Custom/Frame_Frame01_White1.png";
        private const string SP_INNER_PANEL     = SC_COMPONENTS + "Frame_Custom/Frame_Frame02_White1.png";
        private const string SP_STAT_ROW_BG     = SC_COMPONENTS + "Frame_Custom/Frame_ListFrame01_White1.png";
        private const string SP_DARK_NAVY_BG    = SC_COMPONENTS + "Frame_Demo/Frame_ListFrame03_DarkNavy.png";
        private const string SP_POPUP_FRAME     = SC_COMPONENTS + "Popup/Popup_Frame02_Navy1.png";
        private const string SP_GLOW_LINE       = SC_COMPONENTS + "Frame_Custom/Frame_LineFrame03_White4_Glow.png";
        private const string SP_DIVIDER_LINE    = SC_COMPONENTS + "Frame_Custom/Frame_LineFrame05_White1.png";
        private const string SP_LABEL_PILL      = SC_COMPONENTS + "Label/Label_Label01_White1.png";

        // Slot empty frames
        private const string SP_SLOT_FRAME      = SC_COMPONENTS + "Frame_Custom/Frame_ItemFrame01_n_White1.png";
        private const string SP_SLOT_BG         = SC_COMPONENTS + "Frame_Custom/Frame_ItemFrame01_00_White.png";

        // Item rarity frames
        private const string SP_RARITY_COMMON   = SC_COMPONENTS + "Frame_Demo/Frame_ItemFrame03_Gray.png";
        private const string SP_RARITY_RARE     = SC_COMPONENTS + "Frame_Demo/Frame_ItemFrame03_Blue.png";
        private const string SP_RARITY_EPIC     = SC_COMPONENTS + "Frame_Demo/Frame_ItemFrame03_Purple.png";
        private const string SP_RARITY_BG       = SC_COMPONENTS + "Frame_Demo/Frame_ItemFrame03_00.png";

        // Buttons
        private const string SP_BTN_WHITE       = SC_COMPONENTS + "Button_Custom/Btn_TextButton_Square01_White1.png";
        private const string SP_BTN_ORANGE      = SC_COMPONENTS + "Button_Demo/Btn_TextButton_Square01_Orange.Png";
        private const string SP_BTN_GREEN       = SC_COMPONENTS + "Button_Demo/Btn_TextButton_Square01_Green.Png";
        private const string SP_BTN_RED         = SC_COMPONENTS + "Button_Demo/Btn_TextButton_Square01_Red.Png";
        private const string SP_BTN_BLUE        = SC_COMPONENTS + "Button_Demo/Btn_TextButton_Square01_Blue.Png";
        private const string SP_BTN_NAVY        = SC_COMPONENTS + "Button_Demo/Btn_TextButton_Square01_Navy.Png";
        private const string SP_BTN_CLOSE_BOX   = SC_COMPONENTS + "Button_Demo/Btn_IconButton_Square02_Blue.png";

        // Icons
        private const string SP_ICON_CHECK      = SC_DEMO + "Demo_Icon/Icon_PictoIcon_Check_White.png";
        private const string SP_ICON_CLOSE      = SC_DEMO + "Demo_Icon/Icon_PictoIcon_Close_White.png";
        private const string SP_ICON_COIN       = SC_DEMO + "Demo_ItemIcon/Item_ShopIcon_Coin01.png";
        private const string SP_ICON_HEART      = SC_DEMO + "Demo_Icon/Icon_PictoIcon_Heart.png";
        private const string SP_ICON_FIRE       = SC_DEMO + "Demo_Icon/Icon_PictoIcon_Fire.png";
        private const string SP_ICON_STAR       = SC_COMPONENTS + "Icon_PictoIcons(x2)/128/Icon_Star.Png";

        // Fonts
        private const string FONT_BEVIETNAM     = "Assets/Project Data/Game/Fonts/BeVietnamPro-Bold SDF.asset";
        private const string FONT_OXANIUM       = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Fonts/Oxanium-ExtraBold_Extended ASCII SDF.asset";

        // ───────────────────────────────────────────────
        // COLOR PALETTE — SurvivalClean Style
        // ───────────────────────────────────────────────
        private static readonly Color COL_PANEL_BG       = new Color(0.06f, 0.09f, 0.16f, 0.98f);
        private static readonly Color COL_INNER_BG       = new Color(0.08f, 0.11f, 0.19f, 0.96f);
        private static readonly Color COL_CARD_BG        = new Color(0.07f, 0.10f, 0.17f, 0.88f);
        private static readonly Color COL_SLOT_BG        = new Color(0.05f, 0.08f, 0.13f, 0.95f);
        private static readonly Color COL_SLOT_BORDER    = new Color(0.25f, 0.33f, 0.48f, 1.0f);
        private static readonly Color COL_ACCENT_CYAN    = new Color(0f, 0.85f, 1f, 1f);
        private static readonly Color COL_ACCENT_GOLD    = new Color(1f, 0.78f, 0.20f, 1f);
        private static readonly Color COL_STAT_GREEN     = new Color(0.18f, 0.85f, 0.45f, 1f);
        private static readonly Color COL_STAT_RED       = new Color(1f, 0.35f, 0.35f, 1f);
        private static readonly Color COL_TEXT_PRIMARY   = Color.white;
        private static readonly Color COL_TEXT_SECONDARY = new Color(0.68f, 0.74f, 0.84f, 1f);
        private static readonly Color COL_TEXT_MUTED     = new Color(0.45f, 0.52f, 0.64f, 1f);

        private static TMP_FontAsset _cachedFont;

        [MenuItem("Tools/Gun Shooter/Equipment UI Builder")]
        public static void ShowWindow()
        {
            GetWindow<EquipmentUIBuilder>("Equipment UI Builder").Show();
        }

        [MenuItem("Tools/Gun Shooter/Rebuild Equipment UI Prefab (SurvivalClean)")]
        public static void RebuildPrefabDirectly()
        {
            BuildEquipmentUI(true);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("TRANG BỊ - UI BUILDER (GUI PRO-SURVIVALCLEAN)", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH) != null;
            var existingInScene = Object.FindAnyObjectByType<EquipmentPanelUI>();
            bool existsInScene = existingInScene != null;

            if (existsInScene)
            {
                EditorGUILayout.HelpBox(
                    "[EQUIPMENT SYSTEM] đã có trong scene!\n" +
                    "Nếu muốn tạo lại, hãy xoá đối tượng trong Hierarchy trước hoặc bấm Build lại bên dưới.",
                    MessageType.Warning);

                EditorGUILayout.Space(5);
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("CHỌN [EQUIPMENT SYSTEM] TRONG SCENE", GUILayout.Height(30)))
                {
                    Selection.activeObject = existingInScene.transform.root.gameObject;
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.Space(10);

            GUI.backgroundColor = new Color(0.2f, 0.8f, 1.0f);
            if (GUILayout.Button("BUILD LẠI UI (GUI PRO-SURVIVALCLEAN PREFAB)", GUILayout.Height(45)))
            {
                if (EditorUtility.DisplayDialog("Xác nhận Build",
                    "Bạn có chắc muốn build lại toàn bộ UI Trang Bị theo phong cách GUI Pro-SurvivalClean?\nCác Prefab sẽ được cập nhật đồng bộ!", "Đồng ý", "Huỷ"))
                {
                    BuildEquipmentUI();
                }
            }
            GUI.backgroundColor = Color.white;

            if (prefabExists && !existsInScene)
            {
                EditorGUILayout.Space(5);
                GUI.backgroundColor = new Color(0.3f, 0.9f, 0.3f);
                if (GUILayout.Button("ĐẶT VÀO SCENE (từ Prefab)", GUILayout.Height(35)))
                {
                    PlacePrefabInScene();
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("DATABASE MANAGEMENT", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("RESET DATABASE & TẠO 4 TRANG BỊ MẪU", GUILayout.Height(35)))
            {
                if (EditorUtility.DisplayDialog("Xác nhận Reset",
                    "Hành động này sẽ tạo lại 4 trang bị mẫu chuẩn (Mũ, Giáp, Găng, Giày).\nBạn có muốn tiếp tục?", "Đồng ý", "Huỷ"))
                {
                    ResetEquipmentDatabaseAndAssets();
                }
            }
            GUI.backgroundColor = Color.white;
        }

        private static void PlacePrefabInScene()
        {
            Canvas mainCanvas = Object.FindAnyObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy Canvas trong scene!", "OK");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy Prefab tại " + PREFAB_PATH, "OK");
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, mainCanvas.transform);
            instance.transform.SetAsLastSibling();

            Undo.RegisterCreatedObjectUndo(instance, "Đặt Equipment System vào Scene");
            Selection.activeObject = instance;
            Debug.Log("[Equipment UI Builder] Đã đặt [EQUIPMENT SYSTEM] vào scene!");
        }

        private static void ResetEquipmentDatabaseAndAssets()
        {
            var db = AssetDatabase.LoadAssetAtPath<EquipmentDatabase>(
                "Assets/Project Data/Content/Data/Equipment/Equipment Database.asset");
            if (db == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy Equipment Database.asset!", "OK");
                return;
            }

            string itemsFolder = "Assets/Project Data/Content/Data/Equipment/Items";
            if (AssetDatabase.IsValidFolder(itemsFolder))
            {
                AssetDatabase.DeleteAsset(itemsFolder);
            }
            AssetDatabase.CreateFolder("Assets/Project Data/Content/Data/Equipment", "Items");

            db.AllEquipment.Clear();

            CreateSampleItem(db, "mu_chien_binh", "Mũ Chiến Binh", EquipmentType.Hat,
                EquipmentRarity.Common, new EquipmentBonusStats(20, 0, 0, 0), new EquipmentBonusStats(5, 0, 0, 0));

            CreateSampleItem(db, "ao_giap_sat", "Áo Giáp Sắt", EquipmentType.Armor,
                EquipmentRarity.Rare, new EquipmentBonusStats(0, 0, 5, 0), new EquipmentBonusStats(0, 0, 2, 0));

            CreateSampleItem(db, "gang_tay_chien_dau", "Găng Tay Chiến Đấu", EquipmentType.Gloves,
                EquipmentRarity.Common, new EquipmentBonusStats(0, 0, 0, 3), new EquipmentBonusStats(0, 0, 0, 1));

            CreateSampleItem(db, "giay_toc_hanh", "Giày Tốc Hành", EquipmentType.Shoes,
                EquipmentRarity.Epic, new EquipmentBonusStats(0, 5, 0, 5), new EquipmentBonusStats(0, 2, 0, 1));

            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Equipment UI Builder] Đã reset database và tạo lại 4 trang bị mẫu!");
            EditorUtility.DisplayDialog("Thành công", "Đã reset database và tạo mới 4 trang bị mẫu chuẩn!", "OK");
        }

        private static void CreateSampleItem(EquipmentDatabase db, string id, string name, EquipmentType type,
            EquipmentRarity rarity, EquipmentBonusStats baseStats, EquipmentBonusStats perLevel)
        {
            string assetPath = $"Assets/Project Data/Content/Data/Equipment/Items/{id}.asset";
            var item = CreateInstance<EquipmentData>();
            item.SetupInEditor(id, name, null, type, rarity, baseStats, perLevel, 5, new int[] { 50, 100, 200, 400, 800 }, 15);
            AssetDatabase.CreateAsset(item, assetPath);
            db.AddEquipment(item);
        }

        // ───────────────────────────────────────────────
        // PREFAB: EquipmentItemFrame.prefab (Khung Slot Trống)
        // ───────────────────────────────────────────────
        public static GameObject GetOrCreateItemFramePrefab()
        {
            EnsureDirectoryExists(PREFAB_FOLDER);

            GameObject frameObj = CreateUIObject("EquipmentItemFrame", null);
            RectTransform rect = frameObj.GetComponent<RectTransform>();
            SetAnchorsStretch(rect);

            // Background phía sau (ô tối có bo góc)
            var bgImg = CreateImage(frameObj.transform, "Background", LoadSprite(SP_SLOT_BG), COL_SLOT_BG, Image.Type.Sliced);
            SetAnchorsStretch(bgImg.rectTransform);

            // Border khung rỗng viền SurvivalClean
            var borderImg = CreateImage(frameObj.transform, "Border", LoadSprite(SP_SLOT_FRAME), COL_SLOT_BORDER, Image.Type.Sliced);
            SetAnchorsStretch(borderImg.rectTransform);

            // Icon ở giữa
            var iconImg = CreateImage(frameObj.transform, "Icon", null, Color.white, Image.Type.Simple, true);
            RectTransform iconRect = iconImg.rectTransform;
            iconRect.anchorMin = new Vector2(0.15f, 0.15f);
            iconRect.anchorMax = new Vector2(0.85f, 0.85f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(frameObj, FRAME_PREFAB_PATH);
            DestroyImmediate(frameObj);
            Debug.Log("[Equipment UI Builder] Đã lưu " + FRAME_PREFAB_PATH);
            return prefab;
        }

        // ───────────────────────────────────────────────
        // PREFAB: EquipmentItemActive.prefab (Thẻ Item Đang Mặc & Trong Kho)
        // ───────────────────────────────────────────────
        public static GameObject GetOrCreateActiveItemPrefab()
        {
            EnsureDirectoryExists(PREFAB_FOLDER);

            GameObject itemObj = CreateUIObject("EquipmentItemActive", null);
            RectTransform itemRect = itemObj.GetComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(92f, 108f);

            var clickImg = itemObj.AddComponent<Image>();
            clickImg.color = new Color(1f, 1f, 1f, 0f); // Trong suốt nhận raycast
            clickImg.raycastTarget = true;
            var button = itemObj.AddComponent<Button>();

            // Nền độ hiếm bên trong
            var bgImg = CreateImage(itemObj.transform, "Background", LoadSprite(SP_RARITY_BG), COL_SLOT_BG, Image.Type.Sliced);
            SetAnchorsStretch(bgImg.rectTransform);

            // Khung viền độ hiếm
            var borderImg = CreateImage(itemObj.transform, "Border", LoadSprite(SP_RARITY_COMMON), Color.white, Image.Type.Sliced);
            SetAnchorsStretch(borderImg.rectTransform);

            // Icon trang bị
            var iconImg = CreateImage(itemObj.transform, "Icon", null, Color.white, Image.Type.Simple, true);
            RectTransform iconRect = iconImg.rectTransform;
            iconRect.anchorMin = new Vector2(0.12f, 0.14f);
            iconRect.anchorMax = new Vector2(0.88f, 0.86f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            // Level Badge (Góc trên trái)
            var levelBadgeObj = CreateUIObject("LevelBadge", itemObj.transform);
            RectTransform levelBadgeRect = levelBadgeObj.GetComponent<RectTransform>();
            levelBadgeRect.anchorMin = new Vector2(0.04f, 0.74f);
            levelBadgeRect.anchorMax = new Vector2(0.48f, 0.96f);
            levelBadgeRect.offsetMin = Vector2.zero;
            levelBadgeRect.offsetMax = Vector2.zero;

            var levelBadgeImg = levelBadgeObj.AddComponent<Image>();
            levelBadgeImg.sprite = LoadSprite(SP_LABEL_PILL);
            levelBadgeImg.type = Image.Type.Sliced;
            levelBadgeImg.color = new Color(0.05f, 0.08f, 0.14f, 0.88f);
            levelBadgeImg.raycastTarget = false;

            var levelText = CreateTMP(levelBadgeObj.transform, "LevelText", "Lv.1", 10.5f, TextAlignmentOptions.Center, COL_ACCENT_GOLD, FontStyles.Bold);
            SetAnchorsStretch(levelText.rectTransform);

            // Tên item (Góc dưới)
            var nameText = CreateTMP(itemObj.transform, "Name", "", 10f, TextAlignmentOptions.Center, COL_TEXT_SECONDARY, FontStyles.Bold);
            RectTransform nameRect = nameText.rectTransform;
            nameRect.anchorMin = new Vector2(0.04f, 0.02f);
            nameRect.anchorMax = new Vector2(0.96f, 0.22f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            nameText.overflowMode = TextOverflowModes.Ellipsis;
            nameText.maxVisibleLines = 1;

            // Badge "Đang mặc" (Góc trên phải)
            var equippedBadge = CreateUIObject("EquippedBadge", itemObj.transform);
            RectTransform badgeRect = equippedBadge.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0.68f, 0.72f);
            badgeRect.anchorMax = new Vector2(0.96f, 0.96f);
            badgeRect.offsetMin = Vector2.zero;
            badgeRect.offsetMax = Vector2.zero;

            var badgeBg = equippedBadge.AddComponent<Image>();
            badgeBg.sprite = LoadSprite(SP_LABEL_PILL);
            badgeBg.type = Image.Type.Sliced;
            badgeBg.color = new Color(0.15f, 0.75f, 0.38f, 0.95f);
            badgeBg.raycastTarget = false;

            var checkIcon = CreateImage(equippedBadge.transform, "CheckIcon", LoadSprite(SP_ICON_CHECK), Color.white, Image.Type.Simple, true);
            RectTransform checkRect = checkIcon.rectTransform;
            checkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;

            equippedBadge.SetActive(false);

            // Component EquipmentItemUI
            var itemUI = itemObj.AddComponent<EquipmentItemUI>();
            var so = new SerializedObject(itemUI);
            so.FindProperty("iconImage").objectReferenceValue = iconImg;
            so.FindProperty("borderImage").objectReferenceValue = borderImg;
            so.FindProperty("backgroundImage").objectReferenceValue = bgImg;
            so.FindProperty("levelText").objectReferenceValue = levelText;
            so.FindProperty("nameText").objectReferenceValue = nameText;
            so.FindProperty("equippedBadge").objectReferenceValue = equippedBadge;
            so.FindProperty("button").objectReferenceValue = button;

            so.FindProperty("commonBorderSprite").objectReferenceValue = LoadSprite(SP_RARITY_COMMON);
            so.FindProperty("rareBorderSprite").objectReferenceValue = LoadSprite(SP_RARITY_RARE);
            so.FindProperty("epicBorderSprite").objectReferenceValue = LoadSprite(SP_RARITY_EPIC);
            so.FindProperty("commonBgSprite").objectReferenceValue = LoadSprite(SP_RARITY_BG);
            so.FindProperty("rareBgSprite").objectReferenceValue = LoadSprite(SP_RARITY_BG);
            so.FindProperty("epicBgSprite").objectReferenceValue = LoadSprite(SP_RARITY_BG);
            so.ApplyModifiedProperties();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(itemObj, ACTIVE_ITEM_PREFAB_PATH);
            DestroyImmediate(itemObj);
            Debug.Log("[Equipment UI Builder] Đã lưu " + ACTIVE_ITEM_PREFAB_PATH);
            return prefab;
        }

        // ───────────────────────────────────────────────
        // BUILD EQUIPMENT SYSTEM (Full GUI Pro-SurvivalClean)
        // ───────────────────────────────────────────────
        public static void BuildEquipmentUI(bool showDialog = true)
        {
            Canvas mainCanvas = Object.FindAnyObjectByType<Canvas>();
            bool createdTempCanvas = false;
            if (mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("TempCanvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
                mainCanvas = canvasObj.GetComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                createdTempCanvas = true;
            }

            // Xoá instance cũ nếu đang có trong Scene
            var oldSystem = GameObject.Find("[EQUIPMENT SYSTEM]");
            if (oldSystem != null)
            {
                DestroyImmediate(oldSystem);
            }

            // Tạo các sub-prefab chuẩn
            GameObject framePrefab = GetOrCreateItemFramePrefab();
            GameObject activeItemPrefab = GetOrCreateActiveItemPrefab();

            // 1. ROOT GAMEOBJECT
            GameObject rootObj = CreateUIObject("[EQUIPMENT SYSTEM]", mainCanvas.transform);
            RectTransform rootRect = rootObj.GetComponent<RectTransform>();
            SetAnchorsStretch(rootRect);

            // 2. NÚT MỞ HUD TRANG BỊ (Floating Menu Button)
            var openBtn = CreateButton(rootObj.transform, "OpenEquipmentButton", LoadSprite(SP_BTN_ORANGE), Color.white);
            RectTransform openBtnRect = openBtn.GetComponent<RectTransform>();
            openBtnRect.anchorMin = new Vector2(0f, 0.42f);
            openBtnRect.anchorMax = new Vector2(0f, 0.42f);
            openBtnRect.pivot = new Vector2(0f, 0.5f);
            openBtnRect.sizeDelta = new Vector2(85f, 85f);
            openBtnRect.anchoredPosition = new Vector2(15f, 0f);

            var openBtnText = CreateTMP(openBtn.transform, "Text", "TRANG BỊ", 12.5f, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            SetAnchorsStretch(openBtnText.rectTransform);

            // 3. TOÀN BỘ PANEL TRANG BỊ
            GameObject panelObj = CreateUIObject("EquipmentPanel", rootObj.transform);
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            SetAnchorsStretch(panelRect);

            Canvas panelCanvas = panelObj.AddComponent<Canvas>();
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingOrder = 1100;
            panelObj.AddComponent<GraphicRaycaster>();
            panelObj.AddComponent<CanvasGroup>();

            var panelUI = panelObj.AddComponent<EquipmentPanelUI>();

            // Nền tối phủ toàn màn hình
            var darkOverlay = CreateImage(panelObj.transform, "DarkOverlay", null, new Color(0.04f, 0.05f, 0.08f, 0.88f));
            SetAnchorsStretch(darkOverlay.rectTransform);
            darkOverlay.raycastTarget = true;

            // Panel nội dung chính
            GameObject contentPanel = CreateUIObject("ContentPanel", panelObj.transform);
            RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.035f, 0.035f);
            contentRect.anchorMax = new Vector2(0.965f, 0.965f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var contentBg = contentPanel.AddComponent<Image>();
            contentBg.sprite = LoadSprite(SP_PANEL_BG);
            contentBg.type = Image.Type.Sliced;
            contentBg.color = COL_PANEL_BG;
            contentBg.raycastTarget = true;

            // Dải viền Neon Glow phía trên
            var glowTop = CreateImage(contentPanel.transform, "GlowTop", LoadSprite(SP_GLOW_LINE), new Color(0f, 0.85f, 1f, 0.75f), Image.Type.Sliced);
            RectTransform glowRect = glowTop.rectTransform;
            glowRect.anchorMin = new Vector2(0.02f, 0.985f);
            glowRect.anchorMax = new Vector2(0.98f, 1.002f);
            glowRect.offsetMin = Vector2.zero;
            glowRect.offsetMax = Vector2.zero;

            // ==========================================
            // TOP BAR (TIÊU ĐỀ, VÀNG & NÚT ĐÓNG)
            // ==========================================
            GameObject topBar = CreateUIObject("TopBar", contentPanel.transform);
            RectTransform topBarRect = topBar.GetComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0.02f, 0.895f);
            topBarRect.anchorMax = new Vector2(0.98f, 0.985f);
            topBarRect.offsetMin = Vector2.zero;
            topBarRect.offsetMax = Vector2.zero;

            // Tiêu đề
            var titleText = CreateTMP(topBar.transform, "TitleText", "KHO TRANG BỊ", 25f, TextAlignmentOptions.Left, COL_TEXT_PRIMARY, FontStyles.Bold);
            RectTransform titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0.01f, 0.2f);
            titleRect.anchorMax = new Vector2(0.50f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            var titleLine = CreateImage(topBar.transform, "TitleLine", LoadSprite(SP_DIVIDER_LINE), new Color(0f, 0.85f, 1f, 0.45f), Image.Type.Sliced);
            RectTransform titleLineRect = titleLine.rectTransform;
            titleLineRect.anchorMin = new Vector2(0.01f, 0.05f);
            titleLineRect.anchorMax = new Vector2(0.38f, 0.12f);
            titleLineRect.offsetMin = Vector2.zero;
            titleLineRect.offsetMax = Vector2.zero;

            // Ô hiển thị Vàng
            GameObject coinsPanel = CreateUIObject("CoinsPanel", topBar.transform);
            RectTransform coinsRect = coinsPanel.GetComponent<RectTransform>();
            coinsRect.anchorMin = new Vector2(0.72f, 0.15f);
            coinsRect.anchorMax = new Vector2(0.89f, 0.85f);
            coinsRect.offsetMin = Vector2.zero;
            coinsRect.offsetMax = Vector2.zero;

            var coinsBg = coinsPanel.AddComponent<Image>();
            coinsBg.sprite = LoadSprite(SP_STAT_ROW_BG);
            coinsBg.type = Image.Type.Sliced;
            coinsBg.color = COL_CARD_BG;
            coinsBg.raycastTarget = false;

            var coinIcon = CreateImage(coinsPanel.transform, "Icon", LoadSprite(SP_ICON_COIN), Color.white, Image.Type.Simple, true);
            RectTransform coinIconRect = coinIcon.rectTransform;
            coinIconRect.anchorMin = new Vector2(0.05f, 0.12f);
            coinIconRect.anchorMax = new Vector2(0.24f, 0.88f);
            coinIconRect.offsetMin = Vector2.zero;
            coinIconRect.offsetMax = Vector2.zero;

            var coinsText = CreateTMP(coinsPanel.transform, "CoinsText", "0", 15.5f, TextAlignmentOptions.Left, COL_ACCENT_GOLD, FontStyles.Bold);
            RectTransform coinsTextRect = coinsText.rectTransform;
            coinsTextRect.anchorMin = new Vector2(0.28f, 0.10f);
            coinsTextRect.anchorMax = new Vector2(0.95f, 0.90f);
            coinsTextRect.offsetMin = Vector2.zero;
            coinsTextRect.offsetMax = Vector2.zero;

            // Nút Đóng CloseButton
            var closeBtn = CreateButton(topBar.transform, "CloseButton", LoadSprite(SP_BTN_CLOSE_BOX), Color.white);
            RectTransform closeBtnRect = closeBtn.GetComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(0.92f, 0.12f);
            closeBtnRect.anchorMax = new Vector2(0.99f, 0.88f);
            closeBtnRect.offsetMin = Vector2.zero;
            closeBtnRect.offsetMax = Vector2.zero;

            var closeIcon = CreateImage(closeBtn.transform, "CloseIcon", LoadSprite(SP_ICON_CLOSE), Color.white, Image.Type.Simple, true);
            RectTransform closeIconRect = closeIcon.rectTransform;
            closeIconRect.anchorMin = new Vector2(0.25f, 0.25f);
            closeIconRect.anchorMax = new Vector2(0.75f, 0.75f);
            closeIconRect.offsetMin = Vector2.zero;
            closeIconRect.offsetMax = Vector2.zero;

            // ==========================================
            // CỘT TRÁI: NHÂN VẬT & 4 SLOTS TRANG BỊ
            // ==========================================
            GameObject slotsArea = CreateUIObject("SlotsArea", contentPanel.transform);
            RectTransform slotsAreaRect = slotsArea.GetComponent<RectTransform>();
            slotsAreaRect.anchorMin = new Vector2(0.025f, 0.025f);
            slotsAreaRect.anchorMax = new Vector2(0.465f, 0.885f);
            slotsAreaRect.offsetMin = Vector2.zero;
            slotsAreaRect.offsetMax = Vector2.zero;

            var slotsAreaBg = slotsArea.AddComponent<Image>();
            slotsAreaBg.sprite = LoadSprite(SP_INNER_PANEL);
            slotsAreaBg.type = Image.Type.Sliced;
            slotsAreaBg.color = COL_INNER_BG;
            slotsAreaBg.raycastTarget = false;

            // Khung thông tin nhân vật phía trên cột trái
            GameObject charInfoGroup = CreateUIObject("CharInfoGroup", slotsArea.transform);
            RectTransform charInfoRect = charInfoGroup.GetComponent<RectTransform>();
            charInfoRect.anchorMin = new Vector2(0.04f, 0.83f);
            charInfoRect.anchorMax = new Vector2(0.96f, 0.97f);
            charInfoRect.offsetMin = Vector2.zero;
            charInfoRect.offsetMax = Vector2.zero;

            var nameText = CreateTMP(charInfoGroup.transform, "CharNameText", "Chiến Binh", 21f, TextAlignmentOptions.Left, COL_TEXT_PRIMARY, FontStyles.Bold);
            RectTransform nameRect = nameText.rectTransform;
            nameRect.anchorMin = new Vector2(0.02f, 0.48f);
            nameRect.anchorMax = new Vector2(0.45f, 0.98f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            // Nhóm Cấp độ & Biểu tượng Sao
            var starGroup = CreateUIObject("StarGroup", charInfoGroup.transform);
            RectTransform starGroupRect = starGroup.GetComponent<RectTransform>();
            starGroupRect.anchorMin = new Vector2(0.02f, 0.05f);
            starGroupRect.anchorMax = new Vector2(0.48f, 0.45f);
            starGroupRect.offsetMin = Vector2.zero;
            starGroupRect.offsetMax = Vector2.zero;

            var starIcon = CreateImage(starGroup.transform, "StarIcon", LoadSprite(SP_ICON_STAR), COL_ACCENT_GOLD, Image.Type.Simple, true);
            RectTransform starIconRect = starIcon.rectTransform;
            starIconRect.anchorMin = new Vector2(0f, 0.15f);
            starIconRect.anchorMax = new Vector2(0f, 0.85f);
            starIconRect.pivot = new Vector2(0f, 0.5f);
            starIconRect.sizeDelta = new Vector2(16f, 16f);
            starIconRect.anchoredPosition = Vector2.zero;

            var starsText = CreateTMP(starGroup.transform, "CharStarsText", "CẤP 1 / 5", 13f, TextAlignmentOptions.Left, COL_ACCENT_GOLD, FontStyles.Bold);
            RectTransform starsRect = starsText.rectTransform;
            starsRect.anchorMin = new Vector2(0f, 0f);
            starsRect.anchorMax = new Vector2(1f, 1f);
            starsRect.offsetMin = new Vector2(20f, 0f);
            starsRect.offsetMax = Vector2.zero;

            // Thẻ HP
            GameObject hpCard = CreateUIObject("HPCard", charInfoGroup.transform);
            RectTransform hpCardRect = hpCard.GetComponent<RectTransform>();
            hpCardRect.anchorMin = new Vector2(0.50f, 0.52f);
            hpCardRect.anchorMax = new Vector2(0.98f, 0.98f);
            hpCardRect.offsetMin = Vector2.zero;
            hpCardRect.offsetMax = Vector2.zero;

            var hpCardBg = hpCard.AddComponent<Image>();
            hpCardBg.sprite = LoadSprite(SP_STAT_ROW_BG);
            hpCardBg.type = Image.Type.Sliced;
            hpCardBg.color = COL_CARD_BG;
            hpCardBg.raycastTarget = false;

            var hpIcon = CreateImage(hpCard.transform, "Icon", LoadSprite(SP_ICON_HEART), new Color(1f, 0.35f, 0.35f), Image.Type.Simple, true);
            RectTransform hpIconRect = hpIcon.rectTransform;
            hpIconRect.anchorMin = new Vector2(0.06f, 0.18f);
            hpIconRect.anchorMax = new Vector2(0.24f, 0.82f);
            hpIconRect.offsetMin = Vector2.zero;
            hpIconRect.offsetMax = Vector2.zero;

            var hpLabel = CreateTMP(hpCard.transform, "HPLabel", "MÁU", 10.5f, TextAlignmentOptions.Left, COL_TEXT_SECONDARY, FontStyles.Bold);
            RectTransform hpLabelRect = hpLabel.rectTransform;
            hpLabelRect.anchorMin = new Vector2(0.28f, 0.1f);
            hpLabelRect.anchorMax = new Vector2(0.55f, 0.9f);
            hpLabelRect.offsetMin = Vector2.zero;
            hpLabelRect.offsetMax = Vector2.zero;

            var hpValue = CreateTMP(hpCard.transform, "HPValueText", "560", 12.5f, TextAlignmentOptions.Right, COL_STAT_GREEN, FontStyles.Bold);
            RectTransform hpValueRect = hpValue.rectTransform;
            hpValueRect.anchorMin = new Vector2(0.55f, 0.1f);
            hpValueRect.anchorMax = new Vector2(0.94f, 0.9f);
            hpValueRect.offsetMin = Vector2.zero;
            hpValueRect.offsetMax = Vector2.zero;

            // Thẻ Dame
            GameObject dmgCard = CreateUIObject("DMGCard", charInfoGroup.transform);
            RectTransform dmgCardRect = dmgCard.GetComponent<RectTransform>();
            dmgCardRect.anchorMin = new Vector2(0.50f, 0.02f);
            dmgCardRect.anchorMax = new Vector2(0.98f, 0.48f);
            dmgCardRect.offsetMin = Vector2.zero;
            dmgCardRect.offsetMax = Vector2.zero;

            var dmgCardBg = dmgCard.AddComponent<Image>();
            dmgCardBg.sprite = LoadSprite(SP_STAT_ROW_BG);
            dmgCardBg.type = Image.Type.Sliced;
            dmgCardBg.color = COL_CARD_BG;
            dmgCardBg.raycastTarget = false;

            var dmgIcon = CreateImage(dmgCard.transform, "Icon", LoadSprite(SP_ICON_FIRE), new Color(1f, 0.55f, 0.2f), Image.Type.Simple, true);
            RectTransform dmgIconRect = dmgIcon.rectTransform;
            dmgIconRect.anchorMin = new Vector2(0.06f, 0.18f);
            dmgIconRect.anchorMax = new Vector2(0.24f, 0.82f);
            dmgIconRect.offsetMin = Vector2.zero;
            dmgIconRect.offsetMax = Vector2.zero;

            var dmgLabel = CreateTMP(dmgCard.transform, "DMGLabel", "CÔNG", 10.5f, TextAlignmentOptions.Left, COL_TEXT_SECONDARY, FontStyles.Bold);
            RectTransform dmgLabelRect = dmgLabel.rectTransform;
            dmgLabelRect.anchorMin = new Vector2(0.28f, 0.1f);
            dmgLabelRect.anchorMax = new Vector2(0.55f, 0.9f);
            dmgLabelRect.offsetMin = Vector2.zero;
            dmgLabelRect.offsetMax = Vector2.zero;

            var dmgValue = CreateTMP(dmgCard.transform, "DMGValueText", "156", 12.5f, TextAlignmentOptions.Right, COL_STAT_RED, FontStyles.Bold);
            RectTransform dmgValueRect = dmgValue.rectTransform;
            dmgValueRect.anchorMin = new Vector2(0.55f, 0.1f);
            dmgValueRect.anchorMax = new Vector2(0.94f, 0.9f);
            dmgValueRect.offsetMin = Vector2.zero;
            dmgValueRect.offsetMax = Vector2.zero;

            // Ảnh xem trước nhân vật ở trung tâm
            var charPreview = CreateImage(slotsArea.transform, "CharacterPreviewImage", null, Color.white, Image.Type.Simple, true);
            RectTransform charPreviewRect = charPreview.rectTransform;
            charPreviewRect.anchorMin = new Vector2(0.5f, 0.5f);
            charPreviewRect.anchorMax = new Vector2(0.5f, 0.5f);
            charPreviewRect.pivot = new Vector2(0.5f, 0.5f);
            charPreviewRect.anchoredPosition = new Vector2(0f, -22f);
            charPreviewRect.sizeDelta = new Vector2(190f, 270f);

            // 4 Slots bao quanh nhân vật
            Vector2 slotSize = new Vector2(98f, 98f);
            EquipmentSlotUI hatSlot = CreateSlotWithSurvivalCleanFrame(slotsArea.transform, "Hat", "MŨ",
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, 68f), slotSize, framePrefab, activeItemPrefab);

            EquipmentSlotUI glovesSlot = CreateSlotWithSurvivalCleanFrame(slotsArea.transform, "Gloves", "GĂNG",
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, -72f), slotSize, framePrefab, activeItemPrefab);

            EquipmentSlotUI armorSlot = CreateSlotWithSurvivalCleanFrame(slotsArea.transform, "Armor", "GIÁP",
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 68f), slotSize, framePrefab, activeItemPrefab);

            EquipmentSlotUI shoesSlot = CreateSlotWithSurvivalCleanFrame(slotsArea.transform, "Shoes", "GIÀY",
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, -72f), slotSize, framePrefab, activeItemPrefab);

            // ==========================================
            // CỘT PHẢI: KHO ĐỒ & BỘ LỌC CATEGORY
            // ==========================================
            GameObject inventoryArea = CreateUIObject("InventoryArea", contentPanel.transform);
            RectTransform invAreaRect = inventoryArea.GetComponent<RectTransform>();
            invAreaRect.anchorMin = new Vector2(0.485f, 0.025f);
            invAreaRect.anchorMax = new Vector2(0.975f, 0.885f);
            invAreaRect.offsetMin = Vector2.zero;
            invAreaRect.offsetMax = Vector2.zero;

            var invAreaBg = inventoryArea.AddComponent<Image>();
            invAreaBg.sprite = LoadSprite(SP_INNER_PANEL);
            invAreaBg.type = Image.Type.Sliced;
            invAreaBg.color = COL_INNER_BG;
            invAreaBg.raycastTarget = false;

            // Hàng Tabs Lọc (FilterRow)
            GameObject filterRow = CreateUIObject("FilterRow", inventoryArea.transform);
            RectTransform filterRowRect = filterRow.GetComponent<RectTransform>();
            filterRowRect.anchorMin = new Vector2(0.015f, 0.875f);
            filterRowRect.anchorMax = new Vector2(0.985f, 0.980f);
            filterRowRect.offsetMin = Vector2.zero;
            filterRowRect.offsetMax = Vector2.zero;

            Button filterAllBtn    = CreateSurvivalCleanFilterButton(filterRow.transform, "ALL", "TẤT CẢ", 0.00f, 0.18f);
            Button filterHatBtn    = CreateSurvivalCleanFilterButton(filterRow.transform, "HAT", "MŨ",     0.20f, 0.38f);
            Button filterArmorBtn  = CreateSurvivalCleanFilterButton(filterRow.transform, "ARM", "GIÁP",   0.40f, 0.58f);
            Button filterGlovesBtn = CreateSurvivalCleanFilterButton(filterRow.transform, "GLV", "GĂNG",   0.60f, 0.78f);
            Button filterShoesBtn  = CreateSurvivalCleanFilterButton(filterRow.transform, "SHS", "GIÀY",   0.80f, 0.98f);

            // Scroll View Kho đồ (RectMask2D)
            GameObject scrollView = CreateUIObject("InventoryScroll", inventoryArea.transform);
            RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.015f, 0.015f);
            scrollRect.anchorMax = new Vector2(0.985f, 0.855f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;

            var scrollBg = scrollView.AddComponent<Image>();
            scrollBg.sprite = LoadSprite(SP_DARK_NAVY_BG);
            scrollBg.type = Image.Type.Sliced;
            scrollBg.color = new Color(0.05f, 0.07f, 0.12f, 0.7f);
            scrollBg.raycastTarget = true;

            // Tách riêng Sub-Canvas cho ScrollView để việc cuộn và cập nhật item không làm dirty toàn bộ panel cha
            Canvas scrollCanvas = scrollView.AddComponent<Canvas>();
            scrollView.AddComponent<GraphicRaycaster>();

            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            GameObject viewport = CreateUIObject("Viewport", scrollView.transform);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            SetAnchorsStretch(viewportRect);
            viewport.AddComponent<RectMask2D>();

            GameObject content = CreateUIObject("Content", viewport.transform);
            RectTransform contentR = content.GetComponent<RectTransform>();
            contentR.anchorMin = new Vector2(0f, 1f);
            contentR.anchorMax = new Vector2(1f, 1f);
            contentR.pivot = new Vector2(0.5f, 1f);
            contentR.offsetMin = Vector2.zero;
            contentR.offsetMax = Vector2.zero;

            var gridLayout = content.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(92f, 108f);
            gridLayout.spacing = new Vector2(8f, 8f);
            gridLayout.padding = new RectOffset(8, 8, 8, 8);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 4;

            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewportRect;
            scroll.content = contentR;

            // ==========================================
            // POPUP HÀNH ĐỘNG (ACTION POPUP)
            // ==========================================
            EquipmentActionPopup popup = CreateSurvivalCleanActionPopup(panelObj.transform);

            // ==========================================
            // LIÊN KẾT SERIALIZED PROPERTIES VÀO PANEL
            // ==========================================
            var so = new SerializedObject(panelUI);
            so.FindProperty("hatSlot").objectReferenceValue = hatSlot;
            so.FindProperty("armorSlot").objectReferenceValue = armorSlot;
            so.FindProperty("glovesSlot").objectReferenceValue = glovesSlot;
            so.FindProperty("shoesSlot").objectReferenceValue = shoesSlot;

            so.FindProperty("inventoryContainer").objectReferenceValue = content.transform;
            so.FindProperty("inventoryItemPrefab").objectReferenceValue = activeItemPrefab;

            so.FindProperty("charPreviewImage").objectReferenceValue = charPreview;
            so.FindProperty("charNameText").objectReferenceValue = nameText;
            so.FindProperty("charStarsText").objectReferenceValue = starsText;
            so.FindProperty("charHpValueText").objectReferenceValue = hpValue;
            so.FindProperty("charDmgValueText").objectReferenceValue = dmgValue;
            so.FindProperty("coinsText").objectReferenceValue = coinsText;

            so.FindProperty("filterAllBtn").objectReferenceValue = filterAllBtn;
            so.FindProperty("filterHatBtn").objectReferenceValue = filterHatBtn;
            so.FindProperty("filterArmorBtn").objectReferenceValue = filterArmorBtn;
            so.FindProperty("filterGlovesBtn").objectReferenceValue = filterGlovesBtn;
            so.FindProperty("filterShoesBtn").objectReferenceValue = filterShoesBtn;

            so.FindProperty("actionPopup").objectReferenceValue = popup;
            so.FindProperty("closeButton").objectReferenceValue = closeBtn;

            var database = AssetDatabase.LoadAssetAtPath<EquipmentDatabase>(
                "Assets/Project Data/Content/Data/Equipment/Equipment Database.asset");
            if (database != null)
                so.FindProperty("database").objectReferenceValue = database;

            so.ApplyModifiedProperties();

            // Cấu hình script nút mở HUD
            var opener = openBtn.gameObject.AddComponent<EquipmentOpenButton>();
            var openerSo = new SerializedObject(opener);
            openerSo.FindProperty("equipmentPanel").objectReferenceValue = panelObj;
            openerSo.ApplyModifiedProperties();

            // Lưu Prefab hoàn thiện
            EnsureDirectoryExists(PREFAB_FOLDER);
            PrefabUtility.SaveAsPrefabAssetAndConnect(rootObj, PREFAB_PATH, InteractionMode.AutomatedAction);

            if (createdTempCanvas && mainCanvas != null)
            {
                DestroyImmediate(mainCanvas.gameObject);
            }

            Selection.activeObject = rootObj;
            Debug.Log("[Equipment UI Builder] Đã xây dựng hoàn thành giao diện GUI Pro-SurvivalClean!");
            if (showDialog)
            {
                EditorUtility.DisplayDialog("Xây dựng thành công!",
                    "Giao diện trang bị phong cách GUI Pro-SurvivalClean đã được tạo & lưu thành công tại:\n" + PREFAB_PATH, "OK");
            }
        }

        // ───────────────────────────────────────────────
        // HELPER: TẠO SLOT TRANG BỊ VỚI PREFAB LỒNG NHAU
        // ───────────────────────────────────────────────
        private static EquipmentSlotUI CreateSlotWithSurvivalCleanFrame(
            Transform parent, string slotName, string label,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size,
            GameObject framePrefab, GameObject activeItemPrefab)
        {
            GameObject slotObj = CreateUIObject("Slot_" + slotName, parent);
            RectTransform slotRect = slotObj.GetComponent<RectTransform>();
            slotRect.anchorMin = anchorMin;
            slotRect.anchorMax = anchorMax;
            slotRect.pivot = pivot;
            slotRect.anchoredPosition = anchoredPosition;
            slotRect.sizeDelta = size;

            var clickImg = slotObj.AddComponent<Image>();
            clickImg.color = new Color(1f, 1f, 1f, 0f);
            clickImg.raycastTarget = true;
            var button = slotObj.AddComponent<Button>();

            // Lồng Prefab Khung slot rỗng vào bên trong
            GameObject frameInstance = (GameObject)PrefabUtility.InstantiatePrefab(framePrefab, slotObj.transform);
            RectTransform frameRect = frameInstance.GetComponent<RectTransform>();
            SetAnchorsStretch(frameRect);

            var bgImg = frameInstance.transform.Find("Background").GetComponent<Image>();
            var border = frameInstance.transform.Find("Border").GetComponent<Image>();
            var icon = frameInstance.transform.Find("Icon").GetComponent<Image>();

            // Nhãn tên slot kiểu pill tag ở đáy
            var nameBadgeObj = CreateUIObject("SlotNameBadge", slotObj.transform);
            RectTransform nameBadgeRect = nameBadgeObj.GetComponent<RectTransform>();
            nameBadgeRect.anchorMin = new Vector2(0.10f, 0.04f);
            nameBadgeRect.anchorMax = new Vector2(0.90f, 0.26f);
            nameBadgeRect.offsetMin = Vector2.zero;
            nameBadgeRect.offsetMax = Vector2.zero;

            var nameBadgeImg = nameBadgeObj.AddComponent<Image>();
            nameBadgeImg.sprite = LoadSprite(SP_LABEL_PILL);
            nameBadgeImg.type = Image.Type.Sliced;
            nameBadgeImg.color = new Color(0.06f, 0.09f, 0.16f, 0.92f);
            nameBadgeImg.raycastTarget = false;

            var nameText = CreateTMP(nameBadgeObj.transform, "SlotNameText", label, 10.5f, TextAlignmentOptions.Center, COL_TEXT_SECONDARY, FontStyles.Bold);
            SetAnchorsStretch(nameText.rectTransform);

            // Level text ở góc trên phải
            var levelObj = CreateUIObject("LevelBadge", slotObj.transform);
            RectTransform levelRect = levelObj.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.50f, 0.74f);
            levelRect.anchorMax = new Vector2(0.96f, 0.96f);
            levelRect.offsetMin = Vector2.zero;
            levelRect.offsetMax = Vector2.zero;

            var levelText = CreateTMP(levelObj.transform, "LevelText", "", 10.5f, TextAlignmentOptions.Center, COL_ACCENT_GOLD, FontStyles.Bold);
            SetAnchorsStretch(levelText.rectTransform);

            var slotUI = slotObj.AddComponent<EquipmentSlotUI>();
            var so = new SerializedObject(slotUI);
            so.FindProperty("iconImage").objectReferenceValue = icon;
            so.FindProperty("borderImage").objectReferenceValue = border;
            so.FindProperty("backgroundImage").objectReferenceValue = bgImg;
            so.FindProperty("slotNameText").objectReferenceValue = nameText;
            so.FindProperty("levelText").objectReferenceValue = levelText;
            so.FindProperty("button").objectReferenceValue = button;
            so.FindProperty("activeItemPrefab").objectReferenceValue = activeItemPrefab;
            so.ApplyModifiedProperties();

            return slotUI;
        }

        // ───────────────────────────────────────────────
        // HELPER: TẠO NÚT BỘ LỌC CATEGORY
        // ───────────────────────────────────────────────
        private static Button CreateSurvivalCleanFilterButton(Transform parent, string id, string label, float xMin, float xMax)
        {
            GameObject obj = CreateUIObject("Filter_" + id, parent);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(xMin, 0f);
            rect.anchorMax = new Vector2(xMax, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = obj.AddComponent<Image>();
            img.sprite = LoadSprite(SP_BTN_NAVY);
            img.type = Image.Type.Sliced;
            img.color = new Color(0.10f, 0.14f, 0.22f, 0.95f);
            img.raycastTarget = true;

            var btn = obj.AddComponent<Button>();

            var text = CreateTMP(obj.transform, "Text", label, 12f, TextAlignmentOptions.Center, COL_TEXT_SECONDARY, FontStyles.Bold);
            SetAnchorsStretch(text.rectTransform);

            return btn;
        }

        // ───────────────────────────────────────────────
        // HELPER: TẠO ACTION POPUP (SURVIVALCLEAN)
        // ───────────────────────────────────────────────
        private static EquipmentActionPopup CreateSurvivalCleanActionPopup(Transform parent)
        {
            GameObject popupRoot = CreateUIObject("ActionPopup", parent);
            RectTransform rootRect = popupRoot.GetComponent<RectTransform>();
            SetAnchorsStretch(rootRect);

            var blockerImg = popupRoot.AddComponent<Image>();
            blockerImg.color = new Color(0f, 0f, 0f, 0.75f);
            blockerImg.raycastTarget = true;
            var blockerBtn = popupRoot.AddComponent<Button>();

            var popup = popupRoot.AddComponent<EquipmentActionPopup>();

            // Hộp thoại Popup
            GameObject panel = CreateUIObject("PopupPanel", popupRoot.transform);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(420f, 360f);

            var panelBg = panel.AddComponent<Image>();
            panelBg.sprite = LoadSprite(SP_POPUP_FRAME);
            panelBg.type = Image.Type.Sliced;
            panelBg.color = new Color(0.07f, 0.10f, 0.18f, 0.98f);
            panelBg.raycastTarget = true;

            // Dải Neon Glow phía trên của popup
            var glow = CreateImage(panel.transform, "GlowTop", LoadSprite(SP_GLOW_LINE), new Color(0f, 0.85f, 1f, 0.7f), Image.Type.Sliced);
            RectTransform glowRect = glow.rectTransform;
            glowRect.anchorMin = new Vector2(0.05f, 0.97f);
            glowRect.anchorMax = new Vector2(0.95f, 1.01f);
            glowRect.offsetMin = Vector2.zero;
            glowRect.offsetMax = Vector2.zero;

            // Nút X đóng popup
            var popupCloseBtn = CreateButton(panel.transform, "CloseButton", LoadSprite(SP_BTN_CLOSE_BOX), Color.white);
            RectTransform popupCloseRect = popupCloseBtn.GetComponent<RectTransform>();
            popupCloseRect.anchorMin = new Vector2(0.88f, 0.87f);
            popupCloseRect.anchorMax = new Vector2(0.97f, 0.97f);
            popupCloseRect.offsetMin = Vector2.zero;
            popupCloseRect.offsetMax = Vector2.zero;

            var popupCloseIcon = CreateImage(popupCloseBtn.transform, "Icon", LoadSprite(SP_ICON_CLOSE), Color.white, Image.Type.Simple, true);
            RectTransform popupCloseIconRect = popupCloseIcon.rectTransform;
            popupCloseIconRect.anchorMin = new Vector2(0.2f, 0.2f);
            popupCloseIconRect.anchorMax = new Vector2(0.8f, 0.8f);
            popupCloseIconRect.offsetMin = Vector2.zero;
            popupCloseIconRect.offsetMax = Vector2.zero;

            // Tên trang bị
            var nameText = CreateTMP(panel.transform, "ItemName", "Silver Armor", 20f, TextAlignmentOptions.Center, COL_TEXT_PRIMARY, FontStyles.Bold);
            RectTransform nameRect = nameText.rectTransform;
            nameRect.anchorMin = new Vector2(0.05f, 0.85f);
            nameRect.anchorMax = new Vector2(0.86f, 0.97f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            // Đường kẻ ngăn cách tiêu đề
            var divLine = CreateImage(panel.transform, "Divider", LoadSprite(SP_DIVIDER_LINE), new Color(0f, 0.85f, 1f, 0.4f), Image.Type.Sliced);
            RectTransform divLineRect = divLine.rectTransform;
            divLineRect.anchorMin = new Vector2(0.06f, 0.82f);
            divLineRect.anchorMax = new Vector2(0.94f, 0.84f);
            divLineRect.offsetMin = Vector2.zero;
            divLineRect.offsetMax = Vector2.zero;

            // Khung hiển thị Item Preview bên trái
            GameObject previewBox = CreateUIObject("PreviewBox", panel.transform);
            RectTransform previewBoxRect = previewBox.GetComponent<RectTransform>();
            previewBoxRect.anchorMin = new Vector2(0.07f, 0.45f);
            previewBoxRect.anchorMax = new Vector2(0.33f, 0.80f);
            previewBoxRect.offsetMin = Vector2.zero;
            previewBoxRect.offsetMax = Vector2.zero;

            var rarityBg = CreateImage(previewBox.transform, "RarityBg", LoadSprite(SP_RARITY_BG), new Color(0.1f, 0.15f, 0.25f, 0.95f), Image.Type.Sliced);
            SetAnchorsStretch(rarityBg.rectTransform);

            var rarityBorder = CreateImage(previewBox.transform, "Border", LoadSprite(SP_RARITY_COMMON), Color.white, Image.Type.Sliced);
            SetAnchorsStretch(rarityBorder.rectTransform);

            var icon = CreateImage(previewBox.transform, "Icon", null, Color.white, Image.Type.Simple, true);
            RectTransform iconRect = icon.rectTransform;
            iconRect.anchorMin = new Vector2(0.12f, 0.12f);
            iconRect.anchorMax = new Vector2(0.88f, 0.88f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            // Khu vực chỉ số bên phải
            GameObject infoGroup = CreateUIObject("InfoGroup", panel.transform);
            RectTransform infoGroupRect = infoGroup.GetComponent<RectTransform>();
            infoGroupRect.anchorMin = new Vector2(0.36f, 0.45f);
            infoGroupRect.anchorMax = new Vector2(0.93f, 0.80f);
            infoGroupRect.offsetMin = Vector2.zero;
            infoGroupRect.offsetMax = Vector2.zero;

            var levelText = CreateTMP(infoGroup.transform, "LevelText", "Cấp 1/120", 13.5f, TextAlignmentOptions.Left, COL_ACCENT_GOLD, FontStyles.Bold);
            RectTransform levelRect = levelText.rectTransform;
            levelRect.anchorMin = new Vector2(0.02f, 0.65f);
            levelRect.anchorMax = new Vector2(1.00f, 0.98f);
            levelRect.offsetMin = Vector2.zero;
            levelRect.offsetMax = Vector2.zero;

            // Thẻ Stat Card bo viền
            GameObject statCard = CreateUIObject("StatCard", infoGroup.transform);
            RectTransform statCardRect = statCard.GetComponent<RectTransform>();
            statCardRect.anchorMin = new Vector2(0.00f, 0.02f);
            statCardRect.anchorMax = new Vector2(1.00f, 0.60f);
            statCardRect.offsetMin = Vector2.zero;
            statCardRect.offsetMax = Vector2.zero;

            var statCardBg = statCard.AddComponent<Image>();
            statCardBg.sprite = LoadSprite(SP_STAT_ROW_BG);
            statCardBg.type = Image.Type.Sliced;
            statCardBg.color = new Color(0.05f, 0.08f, 0.14f, 0.92f);
            statCardBg.raycastTarget = false;

            var statsText = CreateTMP(statCard.transform, "StatsText", "Máu: +200 ➔ +250", 14f, TextAlignmentOptions.Center, COL_STAT_GREEN, FontStyles.Bold);
            SetAnchorsStretch(statsText.rectTransform);

            // ==========================================
            // NHÓM NÚT KHI CHƯA MẶC (EQUIP GROUP)
            // ==========================================
            GameObject equipGroup = CreateUIObject("EquipGroup", panel.transform);
            RectTransform equipGroupRect = equipGroup.GetComponent<RectTransform>();
            equipGroupRect.anchorMin = new Vector2(0.07f, 0.06f);
            equipGroupRect.anchorMax = new Vector2(0.93f, 0.38f);
            equipGroupRect.offsetMin = Vector2.zero;
            equipGroupRect.offsetMax = Vector2.zero;

            var equipBtn = CreateButton(equipGroup.transform, "EquipButton", LoadSprite(SP_BTN_ORANGE), Color.white);
            RectTransform equipBtnRect = equipBtn.GetComponent<RectTransform>();
            equipBtnRect.anchorMin = new Vector2(0.15f, 0.12f);
            equipBtnRect.anchorMax = new Vector2(0.85f, 0.88f);
            equipBtnRect.offsetMin = Vector2.zero;
            equipBtnRect.offsetMax = Vector2.zero;

            var equipBtnText = CreateTMP(equipBtn.transform, "Text", "MANG", 16f, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            SetAnchorsStretch(equipBtnText.rectTransform);

            // ==========================================
            // NHÓM NÚT KHI ĐÃ MẶC (UPGRADE GROUP)
            // ==========================================
            GameObject upgradeGroup = CreateUIObject("UpgradeGroup", panel.transform);
            RectTransform upgradeGroupRect = upgradeGroup.GetComponent<RectTransform>();
            upgradeGroupRect.anchorMin = new Vector2(0.07f, 0.06f);
            upgradeGroupRect.anchorMax = new Vector2(0.93f, 0.40f);
            upgradeGroupRect.offsetMin = Vector2.zero;
            upgradeGroupRect.offsetMax = Vector2.zero;

            // Hàng chi phí vàng
            GameObject costRow = CreateUIObject("CostRow", upgradeGroup.transform);
            RectTransform costRowRect = costRow.GetComponent<RectTransform>();
            costRowRect.anchorMin = new Vector2(0.15f, 0.58f);
            costRowRect.anchorMax = new Vector2(0.85f, 0.98f);
            costRowRect.offsetMin = Vector2.zero;
            costRowRect.offsetMax = Vector2.zero;

            var costRowBg = costRow.AddComponent<Image>();
            costRowBg.sprite = LoadSprite(SP_STAT_ROW_BG);
            costRowBg.type = Image.Type.Sliced;
            costRowBg.color = COL_CARD_BG;
            costRowBg.raycastTarget = false;

            var coinIco = CreateImage(costRow.transform, "CoinIcon", LoadSprite(SP_ICON_COIN), Color.white, Image.Type.Simple, true);
            RectTransform coinIcoRect = coinIco.rectTransform;
            coinIcoRect.anchorMin = new Vector2(0.24f, 0.15f);
            coinIcoRect.anchorMax = new Vector2(0.38f, 0.85f);
            coinIcoRect.offsetMin = Vector2.zero;
            coinIcoRect.offsetMax = Vector2.zero;

            var coinValText = CreateTMP(costRow.transform, "CoinCostText", "0/200", 14f, TextAlignmentOptions.Left, COL_ACCENT_GOLD, FontStyles.Bold);
            RectTransform coinValRect = coinValText.rectTransform;
            coinValRect.anchorMin = new Vector2(0.42f, 0.10f);
            coinValRect.anchorMax = new Vector2(0.90f, 0.90f);
            coinValRect.offsetMin = Vector2.zero;
            coinValRect.offsetMax = Vector2.zero;

            // Nút Tháo (Unequip)
            var unequipBtn = CreateButton(upgradeGroup.transform, "UnequipButton", LoadSprite(SP_BTN_RED), Color.white);
            RectTransform unequipBtnRect = unequipBtn.GetComponent<RectTransform>();
            unequipBtnRect.anchorMin = new Vector2(0.02f, 0.04f);
            unequipBtnRect.anchorMax = new Vector2(0.47f, 0.52f);
            unequipBtnRect.offsetMin = Vector2.zero;
            unequipBtnRect.offsetMax = Vector2.zero;

            var unequipText = CreateTMP(unequipBtn.transform, "Text", "THÁO", 14f, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            SetAnchorsStretch(unequipText.rectTransform);

            // Nút Nâng Cấp (Upgrade)
            var upgradeBtn = CreateButton(upgradeGroup.transform, "UpgradeButton", LoadSprite(SP_BTN_GREEN), Color.white);
            RectTransform upgradeBtnRect = upgradeBtn.GetComponent<RectTransform>();
            upgradeBtnRect.anchorMin = new Vector2(0.53f, 0.04f);
            upgradeBtnRect.anchorMax = new Vector2(0.98f, 0.52f);
            upgradeBtnRect.offsetMin = Vector2.zero;
            upgradeBtnRect.offsetMax = Vector2.zero;

            var upgradeText = CreateTMP(upgradeBtn.transform, "Text", "NÂNG CẤP", 14f, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            SetAnchorsStretch(upgradeText.rectTransform);

            // Gán Serialized Properties vào EquipmentActionPopup
            var so = new SerializedObject(popup);
            so.FindProperty("popupPanel").objectReferenceValue = panel;
            so.FindProperty("itemNameText").objectReferenceValue = nameText;
            so.FindProperty("itemLevelText").objectReferenceValue = levelText;
            so.FindProperty("itemIconImage").objectReferenceValue = icon;
            so.FindProperty("itemStatsText").objectReferenceValue = statsText;
            so.FindProperty("rarityBgImage").objectReferenceValue = rarityBg;
            so.FindProperty("blockerButton").objectReferenceValue = blockerBtn;
            so.FindProperty("closeButton").objectReferenceValue = popupCloseBtn;

            so.FindProperty("equipGroup").objectReferenceValue = equipGroup;
            so.FindProperty("equipButton").objectReferenceValue = equipBtn;
            so.FindProperty("equipButtonText").objectReferenceValue = equipBtnText;

            so.FindProperty("upgradeGroup").objectReferenceValue = upgradeGroup;
            so.FindProperty("unequipButton").objectReferenceValue = unequipBtn;
            so.FindProperty("upgradeButton").objectReferenceValue = upgradeBtn;
            so.FindProperty("coinCostText").objectReferenceValue = coinValText;
            so.FindProperty("coinIcon").objectReferenceValue = coinIco;
            so.ApplyModifiedProperties();

            popupRoot.SetActive(false);
            return popup;
        }

        // ───────────────────────────────────────────────
        // COMMON UI HELPERS (Layer 5 enforcement & scale 1)
        // ───────────────────────────────────────────────
        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.layer = 5; // UI Layer
            if (parent != null) obj.transform.SetParent(parent, false);
            obj.transform.localScale = Vector3.one;
            return obj;
        }

        private static Image CreateImage(Transform parent, string name, Sprite sprite, Color color, Image.Type type = Image.Type.Simple, bool preserveAspect = false)
        {
            GameObject obj = CreateUIObject(name, parent);
            var img = obj.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.type = type;
            img.preserveAspect = preserveAspect;
            img.raycastTarget = false;
            return img;
        }

        private static Button CreateButton(Transform parent, string name, Sprite sprite, Color color)
        {
            GameObject obj = CreateUIObject(name, parent);
            var img = obj.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            img.raycastTarget = true;
            return obj.AddComponent<Button>();
        }

        private static TextMeshProUGUI CreateTMP(Transform parent, string name, string text, float fontSize, TextAlignmentOptions alignment, Color color, FontStyles style = FontStyles.Normal)
        {
            GameObject obj = CreateUIObject(name, parent);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.font = GetFont();
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void SetAnchorsStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static TMP_FontAsset GetFont()
        {
            if (_cachedFont != null) return _cachedFont;

            _cachedFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_BEVIETNAM);
            if (_cachedFont == null)
                _cachedFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_OXANIUM);
            return _cachedFont;
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = Path.GetFileName(path);
                EnsureDirectoryExists(parent);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
#endif
