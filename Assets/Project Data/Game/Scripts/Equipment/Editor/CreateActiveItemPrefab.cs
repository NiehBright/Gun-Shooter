using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace Watermelon.SquadShooter
{
    public static class CreateActiveItemPrefab
    {
        private const string ACTIVE_PREFAB_PATH = "Assets/Project Data/Content/Data/Equipment/EquipmentItemActive.prefab";
        private const string PANEL_PREFAB_PATH = "Assets/Project Data/Content/Data/Equipment/EquipmentSystem.prefab";

        [MenuItem("Tools/Equipment/Create and Link Active Item Prefab")]
        public static void BuildAndLink()
        {
            // 1. Tạo GameObject làm khung của EquipmentItemActive
            GameObject root = new GameObject("EquipmentItemActive");
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(85, 100);

            // Add Background Image
            Image bgImg = root.AddComponent<Image>();
            bgImg.color = Color.white;
            bgImg.raycastTarget = true;

            // Add Border child
            GameObject borderObj = new GameObject("Border");
            borderObj.transform.SetParent(root.transform, false);
            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;
            Image borderImg = borderObj.AddComponent<Image>();
            borderImg.color = Color.white;
            borderImg.raycastTarget = false;

            // Add Icon child (chứa Icon của vật phẩm với lề trong 12px)
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(root.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(12, 12);
            iconRect.offsetMax = new Vector2(-12, -12);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.color = Color.white;
            iconImg.raycastTarget = false;
            iconImg.preserveAspect = true;

            // Add LevelText child (góc trên bên phải)
            GameObject levelObj = new GameObject("LevelText");
            levelObj.transform.SetParent(root.transform, false);
            RectTransform levelRect = levelObj.AddComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.4f, 0.7f);
            levelRect.anchorMax = new Vector2(0.95f, 0.95f);
            levelRect.offsetMin = Vector2.zero;
            levelRect.offsetMax = Vector2.zero;
            TextMeshProUGUI lvlText = levelObj.AddComponent<TextMeshProUGUI>();
            lvlText.fontSize = 11;
            lvlText.fontStyle = FontStyles.Bold;
            lvlText.color = Color.yellow;
            lvlText.alignment = TextAlignmentOptions.TopRight;
            lvlText.raycastTarget = false;

            // Add NameText child (nằm ở dưới)
            GameObject nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(root.transform, false);
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(1, 0.2f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            TextMeshProUGUI nmText = nameObj.AddComponent<TextMeshProUGUI>();
            nmText.fontSize = 9;
            nmText.color = Color.white;
            nmText.alignment = TextAlignmentOptions.Center;
            nmText.raycastTarget = false;

            // Add EquippedBadge child (badge "Đang mặc" màu xanh lục)
            GameObject badgeObj = new GameObject("EquippedBadge");
            badgeObj.transform.SetParent(root.transform, false);
            RectTransform badgeRect = badgeObj.AddComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0, 0.8f);
            badgeRect.anchorMax = new Vector2(0.4f, 1.0f);
            badgeRect.offsetMin = Vector2.zero;
            badgeRect.offsetMax = Vector2.zero;
            Image badgeImg = badgeObj.AddComponent<Image>();
            badgeImg.color = Color.green;
            badgeImg.raycastTarget = false;
            badgeObj.SetActive(false);

            // Add Button component on root
            Button btn = root.AddComponent<Button>();

            // Add EquipmentItemUI component on root và liên kết các trường
            EquipmentItemUI itemUI = root.AddComponent<EquipmentItemUI>();
            SerializedObject so = new SerializedObject(itemUI);
            so.FindProperty("iconImage").objectReferenceValue = iconImg;
            so.FindProperty("borderImage").objectReferenceValue = borderImg;
            so.FindProperty("backgroundImage").objectReferenceValue = bgImg;
            so.FindProperty("levelText").objectReferenceValue = lvlText;
            so.FindProperty("nameText").objectReferenceValue = nmText;
            so.FindProperty("equippedBadge").objectReferenceValue = badgeObj;
            so.FindProperty("button").objectReferenceValue = btn;
            so.ApplyModifiedProperties();

            // Lưu thành Prefab
            GameObject activePrefab = PrefabUtility.SaveAsPrefabAsset(root, ACTIVE_PREFAB_PATH);
            Object.DestroyImmediate(root);
            Debug.Log("[CreateActiveItemPrefab] Da tao prefab EquipmentItemActive tai " + ACTIVE_PREFAB_PATH);

            // 2. Tải và sửa đổi EquipmentPanel.prefab để liên kết prefab mới
            GameObject panelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PANEL_PREFAB_PATH);
            if (panelPrefab == null)
            {
                Debug.LogError("[CreateActiveItemPrefab] Khong tim thay EquipmentPanel.prefab tai " + PANEL_PREFAB_PATH);
                return;
            }

            // Mở prefab để chỉnh sửa
            GameObject panelInstance = PrefabUtility.LoadPrefabContents(PANEL_PREFAB_PATH);
            EquipmentPanelUI panelUI = panelInstance.GetComponentInChildren<EquipmentPanelUI>(true);
            EquipmentActionPopup popupUI = panelInstance.GetComponentInChildren<EquipmentActionPopup>(true);

            if (panelUI != null)
            {
                // Gán vào inventoryItemPrefab
                SerializedObject panelSo = new SerializedObject(panelUI);
                panelSo.FindProperty("inventoryItemPrefab").objectReferenceValue = activePrefab;
                panelSo.ApplyModifiedProperties();

                SerializedObject popupSo = popupUI != null ? new SerializedObject(popupUI) : null;

                // Tìm và gán activeItemPrefab cho tất cả ô slots
                EquipmentSlotUI[] slots = panelInstance.GetComponentsInChildren<EquipmentSlotUI>(true);

                // Tự động quét và chuyển đổi toàn bộ UnityEngine.UI.Text thành TextMeshProUGUI
                ConvertTextToTMPRecursive(panelInstance, panelSo, popupSo, slots);

                foreach (var slot in slots)
                {
                    SerializedObject slotSo = new SerializedObject(slot);
                    slotSo.FindProperty("activeItemPrefab").objectReferenceValue = activePrefab;
                    slotSo.ApplyModifiedProperties();
                    Debug.Log("[CreateActiveItemPrefab] Da lien ket activePrefab vao o slot: " + slot.gameObject.name);
                }

                // Lưu lại thay đổi vào prefab
                PrefabUtility.SaveAsPrefabAsset(panelInstance, PANEL_PREFAB_PATH);
                Debug.Log("[CreateActiveItemPrefab] Da luu va lien ket tat ca vao EquipmentSystem.prefab thanh cong!");
            }
            else
            {
                Debug.LogError("[CreateActiveItemPrefab] Khong tim thay component EquipmentPanelUI trong prefab.");
            }

            // Dọn dẹp bộ nhớ prefab
            PrefabUtility.UnloadPrefabContents(panelInstance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Thành Công", "Đã nâng cấp toàn bộ chữ sang TextMeshPro, tạo prefab mới và liên kết tự động thành công!", "OK");
        }

        private static void ConvertTextToTMPRecursive(GameObject obj, SerializedObject panelSo, SerializedObject popupSo, EquipmentSlotUI[] slots)
        {
            Text oldText = obj.GetComponent<Text>();
            if (oldText != null)
            {
                string txtValue = oldText.text;
                int fontSize = oldText.fontSize;
                Color txtColor = oldText.color;
                bool raycast = oldText.raycastTarget;
                TextAnchor alignment = oldText.alignment;

                // Dò xem trường nào trong panelUI đang trỏ tới text này
                bool isReferencedInPanel = false;
                string panelPropName = "";
                if (panelSo != null)
                {
                    string[] panelProps = { "charNameText", "charStarsText", "charHpValueText", "charDmgValueText", "coinsText" };
                    foreach (var prop in panelProps)
                    {
                        var sp = panelSo.FindProperty(prop);
                        if (sp != null && sp.objectReferenceValue == oldText)
                        {
                            isReferencedInPanel = true;
                            panelPropName = prop;
                            break;
                        }
                    }
                }

                // Dò xem trường nào trong popupUI đang trỏ tới text này
                bool isReferencedInPopup = false;
                string popupPropName = "";
                if (popupSo != null)
                {
                    string[] popupProps = { "itemNameText", "itemLevelText", "itemStatsText", "equipButtonText", "coinCostText" };
                    foreach (var prop in popupProps)
                    {
                        var sp = popupSo.FindProperty(prop);
                        if (sp != null && sp.objectReferenceValue == oldText)
                        {
                            isReferencedInPopup = true;
                            popupPropName = prop;
                            break;
                        }
                    }
                }

                // Dò xem trường nào trong các slots đang trỏ tới text này
                EquipmentSlotUI referencedSlot = null;
                string slotPropName = "";
                foreach (var slot in slots)
                {
                    SerializedObject slotSo = new SerializedObject(slot);
                    var nameProp = slotSo.FindProperty("slotNameText");
                    var lvlProp = slotSo.FindProperty("levelText");
                    if (nameProp != null && nameProp.objectReferenceValue == oldText)
                    {
                        referencedSlot = slot;
                        slotPropName = "slotNameText";
                        break;
                    }
                    if (lvlProp != null && lvlProp.objectReferenceValue == oldText)
                    {
                        referencedSlot = slot;
                        slotPropName = "levelText";
                        break;
                    }
                }

                // Xóa Text cũ và thêm TextMeshProUGUI mới
                Object.DestroyImmediate(oldText, true);
                TextMeshProUGUI newTMP = obj.AddComponent<TextMeshProUGUI>();
                newTMP.text = txtValue;
                newTMP.fontSize = fontSize;
                newTMP.color = txtColor;
                newTMP.raycastTarget = raycast;

                // Đồng bộ Alignment
                if (alignment == TextAnchor.MiddleCenter) newTMP.alignment = TextAlignmentOptions.Center;
                else if (alignment == TextAnchor.UpperRight || alignment == TextAnchor.MiddleRight || alignment == TextAnchor.LowerRight) newTMP.alignment = TextAlignmentOptions.Right;
                else if (alignment == TextAnchor.UpperLeft || alignment == TextAnchor.MiddleLeft || alignment == TextAnchor.LowerLeft) newTMP.alignment = TextAlignmentOptions.Left;
                else newTMP.alignment = TextAlignmentOptions.Center;

                // Gán lại tham chiếu
                if (isReferencedInPanel)
                {
                    panelSo.FindProperty(panelPropName).objectReferenceValue = newTMP;
                    panelSo.ApplyModifiedProperties();
                }
                if (isReferencedInPopup)
                {
                    popupSo.FindProperty(popupPropName).objectReferenceValue = newTMP;
                    popupSo.ApplyModifiedProperties();
                }
                if (referencedSlot != null)
                {
                    SerializedObject slotSo = new SerializedObject(referencedSlot);
                    slotSo.FindProperty(slotPropName).objectReferenceValue = newTMP;
                    slotSo.ApplyModifiedProperties();
                }
            }

            // Đệ quy tất cả con
            for (int i = obj.transform.childCount - 1; i >= 0; i--)
            {
                if (i < obj.transform.childCount)
                {
                    ConvertTextToTMPRecursive(obj.transform.GetChild(i).gameObject, panelSo, popupSo, slots);
                }
            }
        }
    }
}
