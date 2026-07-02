#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// Tool tự động tạo toàn bộ UI trang bị trong scene.
    /// Chạy từ menu: Tools > Equipment UI Builder
    /// </summary>
    public class EquipmentUIBuilder : EditorWindow
    {
        [MenuItem("Tools/Equipment UI Builder")]
        public static void ShowWindow()
        {
            GetWindow<EquipmentUIBuilder>("Equipment UI Builder").Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("🏗️ TẠO UI TRANG BỊ", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Nút này sẽ tự động tạo toàn bộ UI trang bị trong scene hiện tại:\n" +
                "• Canvas + Panel chính\n" +
                "• 4 slot trang bị (Mũ, Áo, Quần, Giày)\n" +
                "• Kho đồ (Scroll View)\n" +
                "• Popup hành động\n" +
                "• Nút mở/đóng\n" +
                "• EquipmentController (DontDestroyOnLoad)",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("🔨 TẠO UI TRANG BỊ", GUILayout.Height(40)))
            {
                BuildEquipmentUI();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(0.3f, 0.6f, 1.0f);
            if (GUILayout.Button("🎮 TẠO EQUIPMENT CONTROLLER", GUILayout.Height(30)))
            {
                BuildEquipmentController();
            }
            GUI.backgroundColor = Color.white;
        }

        private void BuildEquipmentController()
        {
            // Kiểm tra đã có chưa
            var existing = Object.FindObjectOfType<EquipmentController>();
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Thông báo", "EquipmentController đã tồn tại trong scene!", "OK");
                Selection.activeObject = existing.gameObject;
                return;
            }

            GameObject controllerObj = new GameObject("[EQUIPMENT CONTROLLER]");
            controllerObj.AddComponent<EquipmentController>();
            controllerObj.AddComponent<EquipmentStatsApplier>();

            // Load database
            var database = AssetDatabase.LoadAssetAtPath<EquipmentDatabase>(
                "Assets/Project Data/Content/Data/Equipment/Equipment Database.asset");

            if (database != null)
            {
                var controller = controllerObj.GetComponent<EquipmentController>();
                var serializedObj = new SerializedObject(controller);
                serializedObj.FindProperty("database").objectReferenceValue = database;
                serializedObj.ApplyModifiedProperties();
            }

            Undo.RegisterCreatedObjectUndo(controllerObj, "Tạo Equipment Controller");
            Selection.activeObject = controllerObj;

            Debug.Log("[Equipment UI Builder] Đã tạo Equipment Controller!");
        }

        private void BuildEquipmentUI()
        {
            // Tìm Canvas chính
            Canvas mainCanvas = Object.FindObjectOfType<Canvas>();
            if (mainCanvas == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy Canvas trong scene!", "OK");
                return;
            }

            // === Tạo Empty root chứa tất cả ===
            GameObject rootObj = new GameObject("[EQUIPMENT SYSTEM]");
            rootObj.transform.SetParent(mainCanvas.transform, false);
            RectTransform rootRect = rootObj.AddComponent<RectTransform>();
            SetAnchorsStretch(rootRect);

            // ============================
            // 1. PANEL TRANG BỊ (tạo trước, để nút mở nằm trên)
            // ============================
            GameObject panelObj = new GameObject("EquipmentPanel");
            panelObj.transform.SetParent(rootObj.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            SetAnchorsStretch(panelRect);

            // Canvas riêng cho panel (overrideSorting để nằm trên cùng)
            Canvas panelCanvas = panelObj.AddComponent<Canvas>();
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingOrder = 100;
            panelObj.AddComponent<GraphicRaycaster>();
            panelObj.AddComponent<CanvasGroup>();

            // Thêm EquipmentPanelUI component
            var panelUI = panelObj.AddComponent<EquipmentPanelUI>();

            // === Background tối (che toàn bộ màn hình) ===
            var bg = CreateImage(panelObj.transform, "Background", new Color(0, 0, 0, 0.7f));
            SetAnchorsStretch(bg.GetComponent<RectTransform>());

            // === Panel nội dung (ở giữa) ===
            GameObject contentPanel = CreatePanel(panelObj.transform, "ContentPanel");
            RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.1f, 0.1f);
            contentRect.anchorMax = new Vector2(0.9f, 0.9f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var contentBg = contentPanel.AddComponent<Image>();
            contentBg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // === Tiêu đề ===
            var titleObj = CreateText(contentPanel.transform, "Title", "TRANG BI", 24, TextAnchor.MiddleCenter);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.9f);
            titleRect.anchorMax = new Vector2(1, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // === Khu vực 4 slot (bên trái) ===
            GameObject slotsArea = CreatePanel(contentPanel.transform, "SlotsArea");
            RectTransform slotsRect = slotsArea.GetComponent<RectTransform>();
            slotsRect.anchorMin = new Vector2(0.02f, 0.15f);
            slotsRect.anchorMax = new Vector2(0.35f, 0.88f);
            slotsRect.offsetMin = Vector2.zero;
            slotsRect.offsetMax = Vector2.zero;

            // Label cho slots
            var slotsLabel = CreateText(slotsArea.transform, "SlotsLabel", "Dang Trang Bi", 16, TextAnchor.UpperCenter);
            RectTransform slotsLabelRect = slotsLabel.GetComponent<RectTransform>();
            slotsLabelRect.anchorMin = new Vector2(0, 0.9f);
            slotsLabelRect.anchorMax = new Vector2(1, 1f);
            slotsLabelRect.offsetMin = Vector2.zero;
            slotsLabelRect.offsetMax = Vector2.zero;

            // Tạo 4 slot
            string[] slotNames = { "Mu", "Ao", "Quan", "Giay" };
            EquipmentSlotUI[] slots = new EquipmentSlotUI[4];

            for (int i = 0; i < 4; i++)
            {
                float yMax = 0.88f - (i * 0.22f);
                float yMin = yMax - 0.20f;
                slots[i] = CreateSlot(slotsArea.transform, slotNames[i], yMin, yMax);
            }

            // === Khu vực kho đồ (bên phải) ===
            GameObject inventoryArea = CreatePanel(contentPanel.transform, "InventoryArea");
            RectTransform invRect = inventoryArea.GetComponent<RectTransform>();
            invRect.anchorMin = new Vector2(0.37f, 0.15f);
            invRect.anchorMax = new Vector2(0.98f, 0.88f);
            invRect.offsetMin = Vector2.zero;
            invRect.offsetMax = Vector2.zero;

            var invBg = inventoryArea.AddComponent<Image>();
            invBg.color = new Color(0.1f, 0.1f, 0.15f, 0.5f);

            // Label cho inventory
            var invLabel = CreateText(inventoryArea.transform, "InvLabel", "Kho Do", 16, TextAnchor.UpperCenter);
            RectTransform invLabelRect = invLabel.GetComponent<RectTransform>();
            invLabelRect.anchorMin = new Vector2(0, 0.93f);
            invLabelRect.anchorMax = new Vector2(1, 1f);
            invLabelRect.offsetMin = Vector2.zero;
            invLabelRect.offsetMax = Vector2.zero;

            // ScrollView cho inventory
            GameObject scrollView = CreateScrollView(inventoryArea.transform, "InventoryScroll");

            // === Tổng chỉ số ===
            var statsObj = CreateText(contentPanel.transform, "TotalStats", "Chua trang bi gi", 14, TextAnchor.MiddleCenter);
            RectTransform statsRect = statsObj.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0, 0.02f);
            statsRect.anchorMax = new Vector2(1, 0.12f);
            statsRect.offsetMin = Vector2.zero;
            statsRect.offsetMax = Vector2.zero;

            // === Nút đóng ===
            var closeBtn = CreateButton(contentPanel.transform, "CloseButton", "X", 20);
            RectTransform closeBtnRect = closeBtn.GetComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(0.92f, 0.92f);
            closeBtnRect.anchorMax = new Vector2(1f, 1f);
            closeBtnRect.offsetMin = Vector2.zero;
            closeBtnRect.offsetMax = Vector2.zero;
            closeBtn.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f);

            // === Popup hành động ===
            var popup = CreateActionPopup(panelObj.transform);

            // === Item Prefab (template) ===
            var itemPrefab = CreateItemPrefab(panelObj.transform);

            // === Gán SerializedObject references ===
            var so = new SerializedObject(panelUI);
            so.FindProperty("hatSlot").objectReferenceValue = slots[0];
            so.FindProperty("armorSlot").objectReferenceValue = slots[1];
            so.FindProperty("pantsSlot").objectReferenceValue = slots[2];
            so.FindProperty("shoesSlot").objectReferenceValue = slots[3];
            so.FindProperty("inventoryContainer").objectReferenceValue = scrollView.transform.Find("Viewport/Content");
            so.FindProperty("inventoryItemPrefab").objectReferenceValue = itemPrefab;
            so.FindProperty("actionPopup").objectReferenceValue = popup;
            so.FindProperty("totalStatsText").objectReferenceValue = statsObj.GetComponent<Text>();
            so.FindProperty("closeButton").objectReferenceValue = closeBtn.GetComponent<Button>();

            // Load database
            var database = AssetDatabase.LoadAssetAtPath<EquipmentDatabase>(
                "Assets/Project Data/Content/Data/Equipment/Equipment Database.asset");
            if (database != null)
                so.FindProperty("database").objectReferenceValue = database;

            so.ApplyModifiedProperties();

            // Ẩn item prefab template
            itemPrefab.SetActive(false);

            // ============================
            // 2. NÚT MỞ TRANG BỊ (tạo sau panel để render trên cùng)
            // ============================
            var openBtn = CreateButton(rootObj.transform, "OpenEquipmentButton", "TB", 20);
            RectTransform openBtnRect = openBtn.GetComponent<RectTransform>();
            openBtnRect.anchorMin = new Vector2(0, 0.5f);
            openBtnRect.anchorMax = new Vector2(0, 0.5f);
            openBtnRect.pivot = new Vector2(0, 0.5f);
            openBtnRect.sizeDelta = new Vector2(80, 80);
            openBtnRect.anchoredPosition = new Vector2(10, 0);

            // Màu nền nút rõ ràng
            Image openBtnImg = openBtn.GetComponent<Image>();
            openBtnImg.color = new Color(0.15f, 0.4f, 0.75f, 1f);

            // Đổi màu text cho dễ đọc
            Text openBtnText = openBtn.GetComponentInChildren<Text>();
            if (openBtnText != null)
            {
                openBtnText.color = Color.white;
                openBtnText.fontStyle = FontStyle.Bold;
            }

            // === Gắn script mở panel ===
            var opener = openBtn.AddComponent<EquipmentOpenButton>();
            var openerSo = new SerializedObject(opener);
            openerSo.FindProperty("equipmentPanel").objectReferenceValue = panelObj;
            openerSo.ApplyModifiedProperties();

            // Đăng ký Undo
            Undo.RegisterCreatedObjectUndo(rootObj, "Tạo Equipment System UI");
            Selection.activeObject = rootObj;

            Debug.Log("[Equipment UI Builder] Đã tạo [EQUIPMENT SYSTEM] chứa nút mở + panel trang bị!");
            EditorUtility.DisplayDialog("Thành công!",
                "Đã tạo xong UI trang bị!\n\n" +
                "Cấu trúc trong Hierarchy:\n" +
                "[EQUIPMENT SYSTEM]\n" +
                "  ├── EquipmentPanel (panel trang bị)\n" +
                "  └── OpenEquipmentButton (nút mở - chữ 'TB')\n\n" +
                "Nút mở nằm ở giữa bên trái màn hình.\n" +
                "Nhớ tạo Equipment Controller nếu chưa có!",
                "OK");
        }


        // === Helper Methods ===


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
            img.color = new Color(0.3f, 0.3f, 0.4f);
            obj.AddComponent<Button>();

            // Text con
            var textObj = CreateText(obj.transform, "Text", text, fontSize, TextAnchor.MiddleCenter);
            SetAnchorsStretch(textObj.GetComponent<RectTransform>());

            return obj;
        }

        private EquipmentSlotUI CreateSlot(Transform parent, string slotName, float yMin, float yMax)
        {
            GameObject slotObj = new GameObject($"Slot_{slotName}");
            slotObj.transform.SetParent(parent, false);
            RectTransform slotRect = slotObj.AddComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0.05f, yMin);
            slotRect.anchorMax = new Vector2(0.95f, yMax);
            slotRect.offsetMin = Vector2.zero;
            slotRect.offsetMax = Vector2.zero;

            // Background
            Image bgImg = slotObj.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.3f, 0.5f);

            // Button
            slotObj.AddComponent<Button>();

            // Border
            var border = CreateImage(slotObj.transform, "Border", new Color(0.5f, 0.5f, 0.5f));
            RectTransform borderRect = border.GetComponent<RectTransform>();
            SetAnchorsStretch(borderRect);

            // Icon
            var icon = CreateImage(slotObj.transform, "Icon", Color.white);
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.02f, 0.1f);
            iconRect.anchorMax = new Vector2(0.35f, 0.9f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            // Slot name
            var nameObj = CreateText(slotObj.transform, "SlotName", slotName, 14, TextAnchor.MiddleLeft);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.38f, 0.5f);
            nameRect.anchorMax = new Vector2(0.85f, 1f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            // Level text
            var levelObj = CreateText(slotObj.transform, "Level", "", 12, TextAnchor.MiddleRight);
            RectTransform levelRect = levelObj.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.85f, 0.5f);
            levelRect.anchorMax = new Vector2(0.98f, 1f);
            levelRect.offsetMin = Vector2.zero;
            levelRect.offsetMax = Vector2.zero;

            // Add component
            var slotUI = slotObj.AddComponent<EquipmentSlotUI>();
            var so = new SerializedObject(slotUI);
            so.FindProperty("iconImage").objectReferenceValue = icon;
            so.FindProperty("borderImage").objectReferenceValue = border;
            so.FindProperty("backgroundImage").objectReferenceValue = bgImg;
            so.FindProperty("slotNameText").objectReferenceValue = nameObj.GetComponent<Text>();
            so.FindProperty("levelText").objectReferenceValue = levelObj.GetComponent<Text>();
            so.FindProperty("button").objectReferenceValue = slotObj.GetComponent<Button>();
            so.ApplyModifiedProperties();

            return slotUI;
        }

        private GameObject CreateScrollView(Transform parent, string name)
        {
            GameObject scrollObj = new GameObject(name);
            scrollObj.transform.SetParent(parent, false);
            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 0.92f);
            scrollRect.offsetMin = new Vector2(5, 5);
            scrollRect.offsetMax = new Vector2(-5, -5);

            scrollObj.AddComponent<Image>().color = Color.clear;
            ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            SetAnchorsStretch(viewportRect);
            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var gridLayout = content.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(90, 110);
            gridLayout.spacing = new Vector2(8, 8);
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
            // Stretch toàn màn để popup luôn hiện đúng
            SetAnchorsStretch(rootRect);
            var popup = popupRoot.AddComponent<EquipmentActionPopup>();

            // Panel popup - neo ở giữa màn hình
            GameObject panel = new GameObject("PopupPanel");
            panel.transform.SetParent(popupRoot.transform, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(300, 220);
            panelRect.anchoredPosition = Vector2.zero;
            panel.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.18f, 0.98f);

            // Item name
            var nameObj = CreateText(panel.transform, "ItemName", "", 16, TextAnchor.MiddleCenter);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.75f);
            nameRect.anchorMax = new Vector2(1, 1f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            // Top button
            var topBtn = CreateButton(panel.transform, "TopButton", "Deo trang bi", 14);
            RectTransform topRect = topBtn.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0.05f, 0.4f);
            topRect.anchorMax = new Vector2(0.95f, 0.7f);
            topRect.offsetMin = Vector2.zero;
            topRect.offsetMax = Vector2.zero;
            topBtn.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.3f);

            // Bottom button
            var bottomBtn = CreateButton(panel.transform, "BottomButton", "Ban", 14);
            RectTransform bottomRect = bottomBtn.GetComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(0.05f, 0.05f);
            bottomRect.anchorMax = new Vector2(0.95f, 0.35f);
            bottomRect.offsetMin = Vector2.zero;
            bottomRect.offsetMax = Vector2.zero;
            bottomBtn.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f);

            // Setup serialized references
            var so = new SerializedObject(popup);
            so.FindProperty("popupPanel").objectReferenceValue = panel;
            so.FindProperty("topButton").objectReferenceValue = topBtn.GetComponent<Button>();
            so.FindProperty("topButtonText").objectReferenceValue = topBtn.transform.Find("Text").GetComponent<Text>();
            so.FindProperty("bottomButton").objectReferenceValue = bottomBtn.GetComponent<Button>();
            so.FindProperty("bottomButtonText").objectReferenceValue = bottomBtn.transform.Find("Text").GetComponent<Text>();
            so.FindProperty("itemNameText").objectReferenceValue = nameObj.GetComponent<Text>();
            so.ApplyModifiedProperties();

            return popup;
        }

        private GameObject CreateItemPrefab(Transform parent)
        {
            GameObject itemObj = new GameObject("InventoryItemTemplate");
            itemObj.transform.SetParent(parent, false);
            RectTransform itemRect = itemObj.AddComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(90, 110);

            // Background
            Image bgImg = itemObj.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.3f, 0.5f);

            // Button
            itemObj.AddComponent<Button>();

            // Border
            var border = CreateImage(itemObj.transform, "Border", Color.gray);
            SetAnchorsStretch(border.GetComponent<RectTransform>());

            // Icon
            var icon = CreateImage(itemObj.transform, "Icon", Color.white);
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.25f);
            iconRect.anchorMax = new Vector2(0.9f, 0.85f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            // Name
            var nameObj = CreateText(itemObj.transform, "Name", "", 10, TextAnchor.MiddleCenter);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(1, 0.22f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            // Level
            var levelObj = CreateText(itemObj.transform, "Level", "", 10, TextAnchor.UpperRight);
            RectTransform levelRect = levelObj.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.6f, 0.8f);
            levelRect.anchorMax = new Vector2(1f, 1f);
            levelRect.offsetMin = Vector2.zero;
            levelRect.offsetMax = Vector2.zero;
            levelObj.GetComponent<Text>().color = Color.yellow;

            // Equipped badge
            var equippedBadge = CreateText(itemObj.transform, "EquippedBadge", "✓", 12, TextAnchor.UpperLeft);
            RectTransform badgeRect = equippedBadge.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0, 0.8f);
            badgeRect.anchorMax = new Vector2(0.3f, 1f);
            badgeRect.offsetMin = Vector2.zero;
            badgeRect.offsetMax = Vector2.zero;
            equippedBadge.GetComponent<Text>().color = Color.green;

            // Add component
            var itemUI = itemObj.AddComponent<EquipmentItemUI>();
            var so = new SerializedObject(itemUI);
            so.FindProperty("iconImage").objectReferenceValue = icon;
            so.FindProperty("borderImage").objectReferenceValue = border;
            so.FindProperty("backgroundImage").objectReferenceValue = bgImg;
            so.FindProperty("levelText").objectReferenceValue = levelObj.GetComponent<Text>();
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
