#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;

namespace Watermelon.SquadShooter
{
    public class EquipmentUIBuilder : EditorWindow
    {
        private const string PREFAB_FOLDER = "Assets/Project Data/Content/Data/Equipment";
        private const string PREFAB_PATH = "Assets/Project Data/Content/Data/Equipment/EquipmentSystem.prefab";
        private const string FRAME_PREFAB_PATH = "Assets/Project Data/Content/Data/Equipment/EquipmentItemFrame.prefab";

        [MenuItem("Tools/Squad Shooter/Equipment UI Builder")]
        public static void ShowWindow()
        {
            GetWindow<EquipmentUIBuilder>("Equipment UI Builder").Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("TRANG BI - UI BUILDER (PREMIUM 4 SLOTS)", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH) != null;
            var existingInScene = Object.FindFirstObjectByType<EquipmentPanelUI>();
            bool existsInScene = existingInScene != null;

            if (existsInScene)
            {
                EditorGUILayout.HelpBox(
                    "[EQUIPMENT SYSTEM] da co trong scene!\n" +
                    "Neu muon tao lai, xoa no trong Hierarchy truoc.",
                    MessageType.Warning);

                EditorGUILayout.Space(5);
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("CHON EQUIPMENT SYSTEM TRONG SCENE", GUILayout.Height(30)))
                {
                    Selection.activeObject = existingInScene.transform.root.gameObject;
                }
                GUI.backgroundColor = Color.white;
            }
            else if (prefabExists)
            {
                EditorGUILayout.HelpBox(
                    "Da co Prefab san!\n" +
                    "Bam nut ben duoi de dat vao scene ngay lap tuc.",
                    MessageType.Info);

                EditorGUILayout.Space(10);

                GUI.backgroundColor = new Color(0.3f, 0.9f, 0.3f);
                if (GUILayout.Button("DAT VAO SCENE (tu Prefab)", GUILayout.Height(45)))
                {
                    PlacePrefabInScene();
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.Space(5);

                GUI.backgroundColor = new Color(1f, 0.6f, 0.3f);
                if (GUILayout.Button("BUILD LAI TU DAU (ghi de Prefab)", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Xac nhan",
                        "Ban co chac muon build lai tu dau?\nPrefab cu se bi ghi de!", "Build lai", "Huy"))
                    {
                        BuildEquipmentUI();
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Chua co Prefab. Bam nut ben duoi de tao UI trang bi.",
                    MessageType.Info);

                EditorGUILayout.Space(10);

                GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
                if (GUILayout.Button("TAO UI TRANG BI", GUILayout.Height(45)))
                {
                    BuildEquipmentUI();
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("DATABASE MANAGEMENT", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("RESET DATABASE & TAO LAI 4 TRANG BI MAU CHUAN", GUILayout.Height(35)))
            {
                if (EditorUtility.DisplayDialog("Xac nhan Reset",
                    "Hanh dong nay se xoa sach cac file trang bi cu tren dia va tao lai 4 trang bi mau moi phu hop voi enum 4 slots (Mu, Giap, Gang, Giay).\nBan co chac chan muon thuc hien?", "Dong y", "Huy"))
                {
                    ResetEquipmentDatabaseAndAssets();
                }
            }
            GUI.backgroundColor = Color.white;
        }

        private void PlacePrefabInScene()
        {
            Canvas mainCanvas = Object.FindFirstObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                EditorUtility.DisplayDialog("Loi", "Khong tim thay Canvas trong scene!", "OK");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("Loi", "Khong tim thay Prefab!", "OK");
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, mainCanvas.transform);
            instance.transform.SetAsLastSibling();

            Undo.RegisterCreatedObjectUndo(instance, "Dat Equipment System vao Scene");
            Selection.activeObject = instance;
            Debug.Log("[Equipment UI Builder] Da dat [EQUIPMENT SYSTEM] vao scene!");
        }

        private void ResetEquipmentDatabaseAndAssets()
        {
            var db = AssetDatabase.LoadAssetAtPath<EquipmentDatabase>(
                "Assets/Project Data/Content/Data/Equipment/Equipment Database.asset");
            if (db == null)
            {
                EditorUtility.DisplayDialog("Loi", "Khong tim thay Equipment Database.asset tai duong dan mac dinh!", "OK");
                return;
            }

            string itemsFolder = "Assets/Project Data/Content/Data/Equipment/Items";
            if (AssetDatabase.IsValidFolder(itemsFolder))
            {
                AssetDatabase.DeleteAsset(itemsFolder);
            }
            AssetDatabase.CreateFolder("Assets/Project Data/Content/Data/Equipment", "Items");

            db.AllEquipment.Clear();

            CreateSampleItem(db, "mu_chien_binh", "Mu Chien Binh", EquipmentType.Hat,
                EquipmentRarity.Common, new EquipmentBonusStats(20, 0, 0, 0), new EquipmentBonusStats(5, 0, 0, 0));

            CreateSampleItem(db, "ao_giap_sat", "Ao Giap Sat", EquipmentType.Armor,
                EquipmentRarity.Rare, new EquipmentBonusStats(0, 0, 5, 0), new EquipmentBonusStats(0, 0, 2, 0));

            CreateSampleItem(db, "gang_tay_chien_dau", "Gang Tay Chien Dau", EquipmentType.Gloves,
                EquipmentRarity.Common, new EquipmentBonusStats(0, 0, 0, 3), new EquipmentBonusStats(0, 0, 0, 1));

            CreateSampleItem(db, "giay_toc_hanh", "Giay Toc Hanh", EquipmentType.Shoes,
                EquipmentRarity.Epic, new EquipmentBonusStats(0, 5, 0, 5), new EquipmentBonusStats(0, 2, 0, 1));

            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Equipment UI Builder] Da reset database va tao lai 4 trang bi mau chuan!");
            EditorUtility.DisplayDialog("Thanh cong", "Da reset database va tao moi 4 trang bi mau chuan!", "OK");
        }

        private void CreateSampleItem(EquipmentDatabase db, string id, string name, EquipmentType type,
            EquipmentRarity rarity, EquipmentBonusStats baseStats, EquipmentBonusStats perLevel)
        {
            string assetPath = $"Assets/Project Data/Content/Data/Equipment/Items/{id}.asset";
            var item = CreateInstance<EquipmentData>();
            item.SetupInEditor(id, name, null, type, rarity, baseStats, perLevel, 5, new int[] { 50, 100, 200, 400, 800 }, 15);
            AssetDatabase.CreateAsset(item, assetPath);
            db.AddEquipment(item);
        }

        private static GameObject GetOrCreateItemFramePrefab()
        {
            if (!Directory.Exists(PREFAB_FOLDER))
            {
                Directory.CreateDirectory(PREFAB_FOLDER);
            }

            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(FRAME_PREFAB_PATH);
            if (existing != null) return existing;

            GameObject frameObj = new GameObject("EquipmentItemFrame");
            RectTransform rect = frameObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Background o phia sau
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(frameObj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.18f, 0.18f, 0.22f, 0.8f);

            // Border (Khung vien)
            GameObject borderObj = new GameObject("Border");
            borderObj.transform.SetParent(frameObj.transform, false);
            var borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;
            var borderImg = borderObj.AddComponent<Image>();
            borderImg.color = new Color(0.4f, 0.4f, 0.4f, 0.6f);

            // Icon o giua
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(frameObj.transform, false);
            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.15f, 0.15f);
            iconRect.anchorMax = new Vector2(0.85f, 0.85f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            var iconImg = iconObj.AddComponent<Image>();
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(frameObj, FRAME_PREFAB_PATH);
            DestroyImmediate(frameObj);
            return prefab;
        }

        private void BuildEquipmentUI()
        {
            Canvas mainCanvas = Object.FindFirstObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                EditorUtility.DisplayDialog("Loi", "Khong tim thay Canvas!", "OK");
                return;
            }

            GameObject rootObj = new GameObject("[EQUIPMENT SYSTEM]");
            rootObj.transform.SetParent(mainCanvas.transform, false);
            RectTransform rootRect = rootObj.AddComponent<RectTransform>();
            SetAnchorsStretch(rootRect);

            // Frame Prefab
            GameObject framePrefab = GetOrCreateItemFramePrefab();

            // ============================
            // 1. PANEL TRANG BI
            // ============================
            GameObject panelObj = new GameObject("EquipmentPanel");
            panelObj.transform.SetParent(rootObj.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            SetAnchorsStretch(panelRect);

            Canvas panelCanvas = panelObj.AddComponent<Canvas>();
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingOrder = 100;
            panelObj.AddComponent<GraphicRaycaster>();
            panelObj.AddComponent<CanvasGroup>();

            var panelUI = panelObj.AddComponent<EquipmentPanelUI>();

            // Background toi
            var bg = CreateImage(panelObj.transform, "Background", new Color(0.05f, 0.05f, 0.08f, 0.85f));
            SetAnchorsStretch(bg.GetComponent<RectTransform>());

            // Panel noi dung chinh (o giua)
            GameObject contentPanel = CreatePanel(panelObj.transform, "ContentPanel");
            RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.03f, 0.03f);
            contentRect.anchorMax = new Vector2(0.97f, 0.97f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var contentBg = contentPanel.AddComponent<Image>();
            contentBg.color = new Color(0.12f, 0.12f, 0.16f, 0.98f);

            // Nút Close to rộng rãi dễ bấm (x: 0.91 -> 0.99)
            var closeBtn = CreateButton(contentPanel.transform, "CloseButton", "X", 20);
            RectTransform closeBtnRect = closeBtn.GetComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(0.91f, 0.90f);
            closeBtnRect.anchorMax = new Vector2(0.99f, 0.98f);
            closeBtnRect.offsetMin = Vector2.zero;
            closeBtnRect.offsetMax = Vector2.zero;
            closeBtn.GetComponent<Image>().color = new Color(0.7f, 0.2f, 0.2f);
            closeBtn.transform.Find("Text").GetComponent<Text>().fontStyle = FontStyle.Bold;

            // Coins Panel (x: 0.72 -> 0.89)
            GameObject coinsPanel = CreatePanel(contentPanel.transform, "CoinsPanel");
            RectTransform coinsPanelRect = coinsPanel.GetComponent<RectTransform>();
            coinsPanelRect.anchorMin = new Vector2(0.72f, 0.90f);
            coinsPanelRect.anchorMax = new Vector2(0.89f, 0.98f);
            coinsPanelRect.offsetMin = Vector2.zero;
            coinsPanelRect.offsetMax = Vector2.zero;

            var coinsBg = coinsPanel.AddComponent<Image>();
            coinsBg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

            // Icon dong tien vang
            var coinIcon = CreateImage(coinsPanel.transform, "Icon", Color.yellow);
            RectTransform coinIconRect = coinIcon.GetComponent<RectTransform>();
            coinIconRect.anchorMin = new Vector2(0.06f, 0.15f);
            coinIconRect.anchorMax = new Vector2(0.22f, 0.85f);
            coinIconRect.offsetMin = Vector2.zero;
            coinIconRect.offsetMax = Vector2.zero;

            // Chu so luong vang
            var coinsTextObj = CreateText(coinsPanel.transform, "CoinsText", "0", 13, TextAnchor.MiddleLeft);
            RectTransform coinsTextRect = coinsTextObj.GetComponent<RectTransform>();
            coinsTextRect.anchorMin = new Vector2(0.28f, 0.15f);
            coinsTextRect.anchorMax = new Vector2(0.95f, 0.85f);
            coinsTextRect.offsetMin = Vector2.zero;
            coinsTextRect.offsetMax = Vector2.zero;
            coinsTextObj.GetComponent<Text>().fontStyle = FontStyle.Bold;

            // Title
            var titleObj = CreateText(contentPanel.transform, "TitleText", "UPGRADE & EQUIPMENTS", 22, TextAnchor.MiddleLeft);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.03f, 0.90f);
            titleRect.anchorMax = new Vector2(0.5f, 0.98f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // ============================
            // AREA TRAI: CHAR PREVIEW & 4 SLOTS
            // ============================
            GameObject slotsArea = CreatePanel(contentPanel.transform, "SlotsArea");
            RectTransform slotsAreaRect = slotsArea.GetComponent<RectTransform>();
            slotsAreaRect.anchorMin = new Vector2(0.02f, 0.02f);
            slotsAreaRect.anchorMax = new Vector2(0.46f, 0.88f);
            slotsAreaRect.offsetMin = Vector2.zero;
            slotsAreaRect.offsetMax = Vector2.zero;

            // Name & Star
            var nameObj = CreateText(slotsArea.transform, "CharNameText", "Kaelith", 18, TextAnchor.MiddleLeft);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.90f);
            nameRect.anchorMax = new Vector2(0.50f, 0.98f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            var starObj = CreateText(slotsArea.transform, "CharStarsText", "★★★★★", 14, TextAnchor.MiddleLeft);
            RectTransform starRect = starObj.GetComponent<RectTransform>();
            starRect.anchorMin = new Vector2(0.05f, 0.83f);
            starRect.anchorMax = new Vector2(0.50f, 0.89f);
            starRect.offsetMin = Vector2.zero;
            starRect.offsetMax = Vector2.zero;
            starObj.GetComponent<Text>().color = new Color(1f, 0.8f, 0.1f);

            // Stats (HP & DMG)
            GameObject statsGroup = CreatePanel(slotsArea.transform, "StatsGroup");
            RectTransform statsGroupRect = statsGroup.GetComponent<RectTransform>();
            statsGroupRect.anchorMin = new Vector2(0.55f, 0.83f);
            statsGroupRect.anchorMax = new Vector2(0.95f, 0.98f);
            statsGroupRect.offsetMin = Vector2.zero;
            statsGroupRect.offsetMax = Vector2.zero;

            // HP Label & Value
            var hpLabel = CreateText(statsGroup.transform, "HPLabel", "Mau:", 12, TextAnchor.MiddleLeft);
            RectTransform hpLabelRect = hpLabel.GetComponent<RectTransform>();
            hpLabelRect.anchorMin = new Vector2(0, 0.5f);
            hpLabelRect.anchorMax = new Vector2(0.4f, 1f);
            hpLabelRect.offsetMin = Vector2.zero;
            hpLabelRect.offsetMax = Vector2.zero;

            var hpValue = CreateText(statsGroup.transform, "HPValueText", "560", 12, TextAnchor.MiddleRight);
            RectTransform hpValueRect = hpValue.GetComponent<RectTransform>();
            hpValueRect.anchorMin = new Vector2(0.45f, 0.5f);
            hpValueRect.anchorMax = new Vector2(1f, 1f);
            hpValueRect.offsetMin = Vector2.zero;
            hpValueRect.offsetMax = Vector2.zero;
            hpValue.GetComponent<Text>().color = new Color(0.3f, 0.9f, 0.4f);

            // DMG Label & Value
            var dmgLabel = CreateText(statsGroup.transform, "DMGLabel", "Dame:", 12, TextAnchor.MiddleLeft);
            RectTransform dmgLabelRect = dmgLabel.GetComponent<RectTransform>();
            dmgLabelRect.anchorMin = new Vector2(0, 0);
            dmgLabelRect.anchorMax = new Vector2(0.4f, 0.5f);
            dmgLabelRect.offsetMin = Vector2.zero;
            dmgLabelRect.offsetMax = Vector2.zero;

            var dmgValue = CreateText(statsGroup.transform, "DMGValueText", "156", 12, TextAnchor.MiddleRight);
            RectTransform dmgValueRect = dmgValue.GetComponent<RectTransform>();
            dmgValueRect.anchorMin = new Vector2(0.45f, 0);
            dmgValueRect.anchorMax = new Vector2(1f, 0.5f);
            dmgValueRect.offsetMin = Vector2.zero;
            dmgValueRect.offsetMax = Vector2.zero;
            dmgValue.GetComponent<Text>().color = new Color(1f, 0.4f, 0.4f);

            // Character Center Preview
            var charPreview = CreateImage(slotsArea.transform, "CharacterPreviewImage", Color.white);
            RectTransform charPreviewRect = charPreview.GetComponent<RectTransform>();
            charPreviewRect.anchorMin = new Vector2(0.32f, 0.08f);
            charPreviewRect.anchorMax = new Vector2(0.68f, 0.78f);
            charPreviewRect.offsetMin = Vector2.zero;
            charPreviewRect.offsetMax = Vector2.zero;
            charPreview.preserveAspect = true;

            // 4 Slots surrounding the preview (Top-Left: Hat, Bottom-Left: Gloves, Top-Right: Armor, Bottom-Right: Shoes)
            EquipmentSlotUI hatSlot = CreateSlotWithFrame(slotsArea.transform, "Hat", 0.03f, 0.28f, 0.52f, 0.75f, framePrefab);
            EquipmentSlotUI glovesSlot = CreateSlotWithFrame(slotsArea.transform, "Gloves", 0.03f, 0.28f, 0.15f, 0.38f, framePrefab);
            EquipmentSlotUI armorSlot = CreateSlotWithFrame(slotsArea.transform, "Armor", 0.72f, 0.97f, 0.52f, 0.75f, framePrefab);
            EquipmentSlotUI shoesSlot = CreateSlotWithFrame(slotsArea.transform, "Shoes", 0.72f, 0.97f, 0.15f, 0.38f, framePrefab);

            // ============================
            // AREA PHAI: INVENTORY & FILTERS
            // ============================
            GameObject inventoryArea = CreatePanel(contentPanel.transform, "InventoryArea");
            RectTransform invAreaRect = inventoryArea.GetComponent<RectTransform>();
            invAreaRect.anchorMin = new Vector2(0.48f, 0.02f);
            invAreaRect.anchorMax = new Vector2(0.98f, 0.88f);
            invAreaRect.offsetMin = Vector2.zero;
            invAreaRect.offsetMax = Vector2.zero;

            // Filter tab row (All, Hat, Armor, Gloves, Shoes)
            GameObject filterRow = CreatePanel(inventoryArea.transform, "FilterRow");
            RectTransform filterRowRect = filterRow.GetComponent<RectTransform>();
            filterRowRect.anchorMin = new Vector2(0.01f, 0.85f);
            filterRowRect.anchorMax = new Vector2(0.99f, 0.98f);
            filterRowRect.offsetMin = Vector2.zero;
            filterRowRect.offsetMax = Vector2.zero;

            Button filterAllBtn = CreateFilterButton(filterRow.transform, "ALL", 0.00f, 0.18f);
            Button filterHatBtn = CreateFilterButton(filterRow.transform, "HAT", 0.20f, 0.38f);
            Button filterArmorBtn = CreateFilterButton(filterRow.transform, "ARM", 0.40f, 0.58f);
            Button filterGlovesBtn = CreateFilterButton(filterRow.transform, "GLV", 0.60f, 0.78f);
            Button filterShoesBtn = CreateFilterButton(filterRow.transform, "SHS", 0.80f, 0.98f);

            // Scroll View for inventory items
            GameObject scrollView = CreateScrollView(inventoryArea.transform, "InventoryScroll");
            RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 0.82f);
            scrollRect.offsetMin = new Vector2(5, 5);
            scrollRect.offsetMax = new Vector2(-5, -5);

            // Action Popup Overlay Panel
            EquipmentActionPopup popup = CreateActionPopup(panelObj.transform);

            // Item Prefab Template
            GameObject itemPrefab = CreateItemPrefabWithFrame(panelObj.transform, framePrefab);
            itemPrefab.SetActive(false);

            // ============================
            // SETUP SERIALIZED PROPERTIES ON PANEL
            // ============================
            var so = new SerializedObject(panelUI);
            so.FindProperty("hatSlot").objectReferenceValue = hatSlot;
            so.FindProperty("armorSlot").objectReferenceValue = armorSlot;
            so.FindProperty("glovesSlot").objectReferenceValue = glovesSlot;
            so.FindProperty("shoesSlot").objectReferenceValue = shoesSlot;

            so.FindProperty("inventoryContainer").objectReferenceValue = scrollView.transform.Find("Viewport/Content");
            so.FindProperty("inventoryItemPrefab").objectReferenceValue = itemPrefab;

            so.FindProperty("charPreviewImage").objectReferenceValue = charPreview;
            so.FindProperty("charNameText").objectReferenceValue = nameObj.GetComponent<Text>();
            so.FindProperty("charStarsText").objectReferenceValue = starObj.GetComponent<Text>();
            so.FindProperty("charHpValueText").objectReferenceValue = hpValue.GetComponent<Text>();
            so.FindProperty("charDmgValueText").objectReferenceValue = dmgValue.GetComponent<Text>();
            so.FindProperty("coinsText").objectReferenceValue = coinsTextObj.GetComponent<Text>();

            so.FindProperty("filterAllBtn").objectReferenceValue = filterAllBtn;
            so.FindProperty("filterHatBtn").objectReferenceValue = filterHatBtn;
            so.FindProperty("filterArmorBtn").objectReferenceValue = filterArmorBtn;
            so.FindProperty("filterGlovesBtn").objectReferenceValue = filterGlovesBtn;
            so.FindProperty("filterShoesBtn").objectReferenceValue = filterShoesBtn;

            so.FindProperty("actionPopup").objectReferenceValue = popup;
            so.FindProperty("closeButton").objectReferenceValue = closeBtn.GetComponent<Button>();

            var database = AssetDatabase.LoadAssetAtPath<EquipmentDatabase>(
                "Assets/Project Data/Content/Data/Equipment/Equipment Database.asset");
            if (database != null)
                so.FindProperty("database").objectReferenceValue = database;

            so.ApplyModifiedProperties();

            // Open equipment button logic
            var openBtn = CreateButton(rootObj.transform, "OpenEquipmentButton", "TRANG BI", 14);
            RectTransform openBtnRect = openBtn.GetComponent<RectTransform>();
            openBtnRect.anchorMin = new Vector2(0, 0.4f);
            openBtnRect.anchorMax = new Vector2(0, 0.4f);
            openBtnRect.pivot = new Vector2(0, 0.5f);
            openBtnRect.sizeDelta = new Vector2(80, 80);
            openBtnRect.anchoredPosition = new Vector2(10, 0);
            openBtn.GetComponent<Image>().color = new Color(0.9f, 0.7f, 0.2f, 1f);

            var opener = openBtn.AddComponent<EquipmentOpenButton>();
            var openerSo = new SerializedObject(opener);
            openerSo.FindProperty("equipmentPanel").objectReferenceValue = panelObj;
            openerSo.ApplyModifiedProperties();

            // Prefab saving
            if (!AssetDatabase.IsValidFolder(PREFAB_FOLDER))
            {
                string parentFolder = Path.GetDirectoryName(PREFAB_FOLDER).Replace("\\", "/");
                string newFolder = Path.GetFileName(PREFAB_FOLDER);
                AssetDatabase.CreateFolder(parentFolder, newFolder);
            }
            PrefabUtility.SaveAsPrefabAssetAndConnect(rootObj, PREFAB_PATH, InteractionMode.AutomatedAction);

            Selection.activeObject = rootObj;
            Debug.Log("[Equipment UI Builder] Đã xây dựng hoàn thành giao diện Premium!");
            EditorUtility.DisplayDialog("Xây dựng thành công!", 
                "Giao diện trang bị 4 slots Premium đã được tạo & lưu thành công tại:\n" + PREFAB_PATH, "OK");
        }

        // ============================
        // HELPERS
        // ============================

        private GameObject CreatePanel(Transform parent, string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            SetAnchorsStretch(rect);
            return obj;
        }

        private Image CreateImage(Transform parent, string name, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            Image img = obj.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private GameObject CreateText(Transform parent, string name, string text, int fontSize, TextAnchor alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            Text txt = obj.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.color = Color.white;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return obj;
        }

        private GameObject CreateButton(Transform parent, string name, string text, int fontSize)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.25f);
            obj.AddComponent<Button>();

            var textObj = CreateText(obj.transform, "Text", text, fontSize, TextAnchor.MiddleCenter);
            SetAnchorsStretch(textObj.GetComponent<RectTransform>());

            return obj;
        }

        private Button CreateFilterButton(Transform parent, string label, float xMin, float xMax)
        {
            GameObject obj = new GameObject("Filter_" + label);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(xMin, 0);
            rect.anchorMax = new Vector2(xMax, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.25f);
            Button btn = obj.AddComponent<Button>();

            var textObj = CreateText(obj.transform, "Text", label, 11, TextAnchor.MiddleCenter);
            SetAnchorsStretch(textObj.GetComponent<RectTransform>());
            textObj.GetComponent<Text>().fontStyle = FontStyle.Bold;

            return btn;
        }

        // Tạo slot thiết bị sử dụng Prefab Khung lồng nhau (Nested Prefab)
        private EquipmentSlotUI CreateSlotWithFrame(Transform parent, string slotName, float xMin, float xMax, float yMin, float yMax, GameObject framePrefab)
        {
            GameObject slotObj = new GameObject("Slot_" + slotName);
            slotObj.transform.SetParent(parent, false);
            RectTransform slotRect = slotObj.AddComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(xMin, yMin);
            slotRect.anchorMax = new Vector2(xMax, yMax);
            slotRect.offsetMin = Vector2.zero;
            slotRect.offsetMax = Vector2.zero;

            slotObj.AddComponent<Button>();

            // Lồng Prefab Khung vật phẩm vào bên trong
            GameObject frameInstance = (GameObject)PrefabUtility.InstantiatePrefab(framePrefab, slotObj.transform);
            var bgImg = frameInstance.transform.Find("Background").GetComponent<Image>();
            var border = frameInstance.transform.Find("Border").GetComponent<Image>();
            var icon = frameInstance.transform.Find("Icon").GetComponent<Image>();

            var nameObj = CreateText(slotObj.transform, "SlotNameText", slotName, 10, TextAnchor.UpperCenter);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.8f);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            nameObj.GetComponent<Text>().color = new Color(0.6f, 0.6f, 0.6f);

            // Cấp độ trang bị được đưa lên PHÍA TRÊN góc phải của icon và in đậm màu vàng nổi bật
            var levelObj = CreateText(slotObj.transform, "LevelText", "", 10, TextAnchor.UpperRight);
            RectTransform levelRect = levelObj.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.45f, 0.70f);
            levelRect.anchorMax = new Vector2(0.95f, 0.95f);
            levelRect.offsetMin = Vector2.zero;
            levelRect.offsetMax = Vector2.zero;
            var levelText = levelObj.GetComponent<Text>();
            levelText.color = Color.yellow;
            levelText.fontStyle = FontStyle.Bold;

            var slotUI = slotObj.AddComponent<EquipmentSlotUI>();
            var so = new SerializedObject(slotUI);
            so.FindProperty("iconImage").objectReferenceValue = icon;
            so.FindProperty("borderImage").objectReferenceValue = border;
            so.FindProperty("backgroundImage").objectReferenceValue = bgImg;
            so.FindProperty("slotNameText").objectReferenceValue = nameObj.GetComponent<Text>();
            so.FindProperty("levelText").objectReferenceValue = levelText;
            so.FindProperty("button").objectReferenceValue = slotObj.GetComponent<Button>();
            so.ApplyModifiedProperties();

            return slotUI;
        }

        private GameObject CreateScrollView(Transform parent, string name)
        {
            GameObject scrollObj = new GameObject(name);
            scrollObj.transform.SetParent(parent, false);
            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();

            scrollObj.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.4f);
            ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            SetAnchorsStretch(viewportRect);
            viewport.AddComponent<Image>().color = Color.white;
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var gridLayout = content.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(85, 100);
            gridLayout.spacing = new Vector2(6, 6);
            gridLayout.padding = new RectOffset(5, 5, 5, 5);

            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewportRect;
            scroll.content = contentRect;

            return scrollObj;
        }

        private EquipmentActionPopup CreateActionPopup(Transform parent)
        {
            GameObject popupRoot = new GameObject("ActionPopup");
            popupRoot.transform.SetParent(parent, false);
            RectTransform rootRect = popupRoot.AddComponent<RectTransform>();
            SetAnchorsStretch(rootRect);

            Image blockerImg = popupRoot.AddComponent<Image>();
            blockerImg.color = new Color(0, 0, 0, 0.6f);
            var blockerBtn = popupRoot.AddComponent<Button>();

            var popup = popupRoot.AddComponent<EquipmentActionPopup>();

            GameObject panel = new GameObject("PopupPanel");
            panel.transform.SetParent(popupRoot.transform, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(400, 320);
            panel.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

            blockerBtn.onClick.AddListener(popup.Hide);

            var nameObj = CreateText(panel.transform, "ItemName", "Silver Armor", 18, TextAnchor.MiddleCenter);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.85f);
            nameRect.anchorMax = new Vector2(0.95f, 0.98f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            nameObj.GetComponent<Text>().fontStyle = FontStyle.Bold;

            var rarityBg = CreateImage(panel.transform, "RarityBg", Color.gray);
            RectTransform rarityBgRect = rarityBg.GetComponent<RectTransform>();
            rarityBgRect.anchorMin = new Vector2(0.08f, 0.45f);
            rarityBgRect.anchorMax = new Vector2(0.35f, 0.82f);
            rarityBgRect.offsetMin = Vector2.zero;
            rarityBgRect.offsetMax = Vector2.zero;

            var icon = CreateImage(rarityBg.transform, "Icon", Color.white);
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.05f, 0.05f);
            iconRect.anchorMax = new Vector2(0.95f, 0.95f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            icon.preserveAspect = true;

            var levelTextObj = CreateText(panel.transform, "LevelText", "Lv.1/120", 12, TextAnchor.MiddleLeft);
            RectTransform levelTextRect = levelTextObj.GetComponent<RectTransform>();
            levelTextRect.anchorMin = new Vector2(0.38f, 0.72f);
            levelTextRect.anchorMax = new Vector2(0.92f, 0.82f);
            levelTextRect.offsetMin = Vector2.zero;
            levelTextRect.offsetMax = Vector2.zero;

            var statsTextObj = CreateText(panel.transform, "StatsText", "Mau: +200", 15, TextAnchor.MiddleLeft);
            RectTransform statsTextRect = statsTextObj.GetComponent<RectTransform>();
            statsTextRect.anchorMin = new Vector2(0.38f, 0.35f);
            statsTextRect.anchorMax = new Vector2(0.92f, 0.65f);
            statsTextRect.offsetMin = Vector2.zero;
            statsTextRect.offsetMax = Vector2.zero;
            statsTextObj.GetComponent<Text>().fontStyle = FontStyle.Bold;

            GameObject equipGroup = CreatePanel(panel.transform, "EquipGroup");
            RectTransform equipGroupRect = equipGroup.GetComponent<RectTransform>();
            equipGroupRect.anchorMin = new Vector2(0.05f, 0.05f);
            equipGroupRect.anchorMax = new Vector2(0.95f, 0.30f);
            equipGroupRect.offsetMin = Vector2.zero;
            equipGroupRect.offsetMax = Vector2.zero;

            var equipBtn = CreateButton(equipGroup.transform, "EquipButton", "Mang", 14);
            RectTransform equipBtnRect = equipBtn.GetComponent<RectTransform>();
            equipBtnRect.anchorMin = new Vector2(0.2f, 0.0f);
            equipBtnRect.anchorMax = new Vector2(0.8f, 1.0f);
            equipBtnRect.offsetMin = Vector2.zero;
            equipBtnRect.offsetMax = Vector2.zero;
            equipBtn.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.9f);

            GameObject upgradeGroup = CreatePanel(panel.transform, "UpgradeGroup");
            RectTransform upgradeGroupRect = upgradeGroup.GetComponent<RectTransform>();
            upgradeGroupRect.anchorMin = new Vector2(0.05f, 0.05f);
            upgradeGroupRect.anchorMax = new Vector2(0.95f, 0.32f);
            upgradeGroupRect.offsetMin = Vector2.zero;
            upgradeGroupRect.offsetMax = Vector2.zero;

            GameObject costRow = CreatePanel(upgradeGroup.transform, "CostRow");
            RectTransform costRowRect = costRow.GetComponent<RectTransform>();
            costRowRect.anchorMin = new Vector2(0.0f, 0.55f);
            costRowRect.anchorMax = new Vector2(1.0f, 1.0f);
            costRowRect.offsetMin = Vector2.zero;
            costRowRect.offsetMax = Vector2.zero;

            var coinIco = CreateImage(costRow.transform, "CoinIcon", Color.yellow);
            RectTransform coinIcoRect = coinIco.GetComponent<RectTransform>();
            coinIcoRect.anchorMin = new Vector2(0.35f, 0.1f);
            coinIcoRect.anchorMax = new Vector2(0.43f, 0.9f);
            coinIcoRect.offsetMin = Vector2.zero;
            coinIcoRect.offsetMax = Vector2.zero;

            var coinValText = CreateText(costRow.transform, "CoinCostText", "0/200", 13, TextAnchor.MiddleLeft);
            RectTransform coinValRect = coinValText.GetComponent<RectTransform>();
            coinValRect.anchorMin = new Vector2(0.45f, 0.1f);
            coinValRect.anchorMax = new Vector2(0.75f, 0.9f);
            coinValRect.offsetMin = Vector2.zero;
            coinValRect.offsetMax = Vector2.zero;
            coinValText.GetComponent<Text>().fontStyle = FontStyle.Bold;

            var unequipBtn = CreateButton(upgradeGroup.transform, "UnequipButton", "Go", 12);
            RectTransform unequipBtnRect = unequipBtn.GetComponent<RectTransform>();
            unequipBtnRect.anchorMin = new Vector2(0.05f, 0.0f);
            unequipBtnRect.anchorMax = new Vector2(0.45f, 0.50f);
            unequipBtnRect.offsetMin = Vector2.zero;
            unequipBtnRect.offsetMax = Vector2.zero;
            unequipBtn.GetComponent<Image>().color = new Color(0.7f, 0.3f, 0.3f);

            var upgradeBtn = CreateButton(upgradeGroup.transform, "UpgradeButton", "Nang cap", 12);
            RectTransform upgradeBtnRect = upgradeBtn.GetComponent<RectTransform>();
            upgradeBtnRect.anchorMin = new Vector2(0.55f, 0.0f);
            upgradeBtnRect.anchorMax = new Vector2(0.95f, 0.50f);
            upgradeBtnRect.offsetMin = Vector2.zero;
            upgradeBtnRect.offsetMax = Vector2.zero;
            upgradeBtn.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f);

            var so = new SerializedObject(popup);
            so.FindProperty("popupPanel").objectReferenceValue = panel;
            so.FindProperty("itemNameText").objectReferenceValue = nameObj.GetComponent<Text>();
            so.FindProperty("itemLevelText").objectReferenceValue = levelTextObj.GetComponent<Text>();
            so.FindProperty("itemIconImage").objectReferenceValue = icon;
            so.FindProperty("itemStatsText").objectReferenceValue = statsTextObj.GetComponent<Text>();
            so.FindProperty("rarityBgImage").objectReferenceValue = rarityBg;
            so.FindProperty("blockerButton").objectReferenceValue = blockerBtn; // Lien ket blocker Button vao popup

            so.FindProperty("equipGroup").objectReferenceValue = equipGroup;
            so.FindProperty("equipButton").objectReferenceValue = equipBtn.GetComponent<Button>();
            so.FindProperty("equipButtonText").objectReferenceValue = equipBtn.transform.Find("Text").GetComponent<Text>();

            so.FindProperty("upgradeGroup").objectReferenceValue = upgradeGroup;
            so.FindProperty("unequipButton").objectReferenceValue = unequipBtn.GetComponent<Button>();
            so.FindProperty("upgradeButton").objectReferenceValue = upgradeBtn.GetComponent<Button>();
            so.FindProperty("coinCostText").objectReferenceValue = coinValText.GetComponent<Text>();
            so.FindProperty("coinIcon").objectReferenceValue = coinIco;
            so.ApplyModifiedProperties();

            return popup;
        }

        // Tạo template vật phẩm trong kho đồ sử dụng Prefab Khung lồng nhau (Nested Prefab)
        private GameObject CreateItemPrefabWithFrame(Transform parent, GameObject framePrefab)
        {
            GameObject itemObj = new GameObject("InventoryItemTemplate");
            itemObj.transform.SetParent(parent, false);
            RectTransform itemRect = itemObj.AddComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(85, 100);

            itemObj.AddComponent<Button>();

            // Lồng Prefab Khung vật phẩm vào bên trong
            GameObject frameInstance = (GameObject)PrefabUtility.InstantiatePrefab(framePrefab, itemObj.transform);
            var bgImg = frameInstance.transform.Find("Background").GetComponent<Image>();
            var border = frameInstance.transform.Find("Border").GetComponent<Image>();
            var icon = frameInstance.transform.Find("Icon").GetComponent<Image>();

            var nameObj = CreateText(itemObj.transform, "Name", "", 9, TextAnchor.MiddleCenter);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(1, 0.2f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            // Cấp độ trang bị được đưa lên PHÍA TRÊN góc phải của icon và in đậm màu vàng nổi bật
            var levelObj = CreateText(itemObj.transform, "Level", "", 9, TextAnchor.UpperRight);
            RectTransform levelRect = levelObj.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.5f, 0.8f);
            levelRect.anchorMax = new Vector2(0.95f, 1.0f);
            levelRect.offsetMin = Vector2.zero;
            levelRect.offsetMax = Vector2.zero;
            var levelText = levelObj.GetComponent<Text>();
            levelText.color = Color.yellow;
            levelText.fontStyle = FontStyle.Bold;

            var equippedBadge = CreateText(itemObj.transform, "EquippedBadge", "E", 10, TextAnchor.UpperLeft);
            RectTransform badgeRect = equippedBadge.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0.05f, 0.8f);
            badgeRect.anchorMax = new Vector2(0.3f, 1f);
            badgeRect.offsetMin = Vector2.zero;
            badgeRect.offsetMax = Vector2.zero;
            equippedBadge.GetComponent<Text>().color = Color.green;

            var itemUI = itemObj.AddComponent<EquipmentItemUI>();
            var so = new SerializedObject(itemUI);
            so.FindProperty("iconImage").objectReferenceValue = icon;
            so.FindProperty("borderImage").objectReferenceValue = border;
            so.FindProperty("backgroundImage").objectReferenceValue = bgImg;
            so.FindProperty("levelText").objectReferenceValue = levelText;
            so.FindProperty("nameText").objectReferenceValue = nameObj.GetComponent<Text>();
            so.FindProperty("equippedBadge").objectReferenceValue = equippedBadge;
            so.FindProperty("button").objectReferenceValue = itemObj.GetComponent<Button>();
            so.ApplyModifiedProperties();

            return itemObj;
        }

        private void SetAnchorsStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
#endif
