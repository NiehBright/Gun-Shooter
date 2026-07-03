using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// Panel UI chính hiển thị 4 slot trang bị và kho đồ.
    /// Tự quản lý Canvas riêng, không can thiệp vào hệ thống UIPage gốc.
    /// </summary>
    public class EquipmentPanelUI : MonoBehaviour
    {
        [Header("Tham chiếu")]
        [SerializeField] EquipmentDatabase database;

        [Header("Slot trang bị")]
        [SerializeField] EquipmentSlotUI hatSlot;
        [SerializeField] EquipmentSlotUI armorSlot;
        [SerializeField] EquipmentSlotUI pantsSlot;
        [SerializeField] EquipmentSlotUI shoesSlot;

        [Header("Kho đồ")]
        [SerializeField] Transform inventoryContainer;
        [SerializeField] GameObject inventoryItemPrefab;

        [Header("Popup hành động")]
        [SerializeField] EquipmentActionPopup actionPopup;

        [Header("Tổng chỉ số")]
        [SerializeField] Text totalStatsText;

        [Header("Nút đóng")]
        [SerializeField] Button closeButton;

        private List<EquipmentItemUI> inventoryItems = new List<EquipmentItemUI>();

        private static EquipmentPanelUI instance;
        public static EquipmentPanelUI Instance => instance;

        // Canvas riêng để bật/tắt mà không ảnh hưởng UI gốc
        private Canvas panelCanvas;
        private GraphicRaycaster panelRaycaster;
        private CanvasGroup panelCanvasGroup;

        private void Awake()
        {
            instance = this;

            // Lấy hoặc thêm Canvas riêng
            panelCanvas = GetComponent<Canvas>();
            if (panelCanvas == null) panelCanvas = gameObject.AddComponent<Canvas>();

            panelRaycaster = GetComponent<GraphicRaycaster>();
            if (panelRaycaster == null) panelRaycaster = gameObject.AddComponent<GraphicRaycaster>();

            panelCanvasGroup = GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null) panelCanvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Close);
            }

            // Tự động khởi tạo Controller nếu chưa có trong scene
            if (EquipmentController.Instance == null)
            {
                GameObject controllerObj = new GameObject("[EQUIPMENT CONTROLLER]");
                var controller = controllerObj.AddComponent<EquipmentController>();
                controllerObj.AddComponent<EquipmentStatsApplier>();

#if UNITY_EDITOR
                var db = UnityEditor.AssetDatabase.LoadAssetAtPath<EquipmentDatabase>(
                    "Assets/Project Data/Content/Data/Equipment/Equipment Database.asset");
                if (db != null)
                {
                    var serializedObj = new UnityEditor.SerializedObject(controller);
                    serializedObj.FindProperty("database").objectReferenceValue = db;
                    serializedObj.ApplyModifiedProperties();
                }
#endif
            }

            // Mặc định ẩn
            HideImmediate();
        }

        private void OnEnable()
        {
            EquipmentController.OnEquipmentChanged += RefreshUI;
        }

        private void OnDisable()
        {
            EquipmentController.OnEquipmentChanged -= RefreshUI;
        }

        /// <summary>
        /// Mở panel trang bị
        /// </summary>
        public static void Show()
        {
            if (instance == null) return;

            instance.panelCanvas.enabled = true;
            instance.panelRaycaster.enabled = true;
            instance.panelCanvasGroup.alpha = 1f;
            instance.panelCanvasGroup.blocksRaycasts = true;
            instance.panelCanvasGroup.interactable = true;

            instance.RefreshUI();
        }

        /// <summary>
        /// Đóng panel trang bị
        /// </summary>
        public void Close()
        {
            if (actionPopup != null)
                actionPopup.Hide();

            HideImmediate();
        }

        private void HideImmediate()
        {
            if (panelCanvas != null) panelCanvas.enabled = false;
            if (panelRaycaster != null) panelRaycaster.enabled = false;
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.blocksRaycasts = false;
                panelCanvasGroup.interactable = false;
            }
        }

        public void RefreshUI()
        {
            // Cập nhật 4 slot
            RefreshSlot(hatSlot, EquipmentType.Hat);
            RefreshSlot(armorSlot, EquipmentType.Armor);
            RefreshSlot(pantsSlot, EquipmentType.Pants);
            RefreshSlot(shoesSlot, EquipmentType.Shoes);

            // Cập nhật kho đồ
            RefreshInventory();

            // Cập nhật tổng chỉ số
            RefreshTotalStats();
        }

        private void RefreshSlot(EquipmentSlotUI slot, EquipmentType type)
        {
            if (slot == null) return;

            var equippedItem = EquipmentController.GetEquippedItem(type);
            int level = 0;

            if (equippedItem != null && EquipmentController.SaveData != null)
            {
                var saveItem = EquipmentController.SaveData.GetItem(equippedItem.ItemID);
                if (saveItem != null) level = saveItem.level;
            }

            slot.Setup(equippedItem, type, level, OnSlotClicked);
        }

        private void RefreshInventory()
        {
            // Xóa items cũ
            foreach (var item in inventoryItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            inventoryItems.Clear();

            Debug.Log($"[Equipment UI] RefreshInventory. SaveData: {(EquipmentController.SaveData != null ? "Not Null" : "Null")}, Database: {(EquipmentController.Database != null ? "Not Null" : "Null")}, Prefab: {(inventoryItemPrefab != null ? "Not Null" : "Null")}, Container: {(inventoryContainer != null ? "Not Null" : "Null")}");

            if (EquipmentController.SaveData == null || inventoryItemPrefab == null || inventoryContainer == null) return;

            // Sao chép và sắp xếp kho đồ theo Loại (Type), rồi đến Độ hiếm (Rarity), rồi đến Cấp độ (Level)
            var sortedItems = new List<EquipmentSaveItem>(EquipmentController.SaveData.OwnedItems);
            sortedItems.Sort((a, b) =>
            {
                var dataA = EquipmentController.Database?.GetEquipmentByID(a.itemID);
                var dataB = EquipmentController.Database?.GetEquipmentByID(b.itemID);
                if (dataA == null && dataB == null) return 0;
                if (dataA == null) return 1;
                if (dataB == null) return -1;

                // 1. Sắp xếp theo Loại (Mũ -> Áo -> Quần -> Giày)
                int typeCompare = dataA.EquipmentType.CompareTo(dataB.EquipmentType);
                if (typeCompare != 0) return typeCompare;

                // 2. Sắp xếp theo Độ hiếm giảm dần (Sử thi -> Hiếm -> Thường)
                int rarityCompare = dataB.Rarity.CompareTo(dataA.Rarity);
                if (rarityCompare != 0) return rarityCompare;

                // 3. Sắp xếp theo Cấp độ giảm dần
                return b.level.CompareTo(a.level);
            });

            Debug.Log($"[Equipment UI] Owned Items Count: {sortedItems.Count}");

            foreach (var saveItem in sortedItems)
            {
                var data = EquipmentController.Database?.GetEquipmentByID(saveItem.itemID);
                Debug.Log($"[Equipment UI] Item ID in save: '{saveItem.itemID}', Data in database: {(data != null ? data.ItemName : "Null")}");
                if (data == null) continue;

                GameObject itemObj = Instantiate(inventoryItemPrefab, inventoryContainer);
                itemObj.transform.localScale = Vector3.one;
                itemObj.transform.localPosition = Vector3.zero;
                itemObj.SetActive(true);
                EquipmentItemUI itemUI = itemObj.GetComponent<EquipmentItemUI>();
                if (itemUI != null)
                {
                    bool isEquipped = EquipmentController.SaveData.IsEquipped(saveItem.itemID);
                    itemUI.Setup(data, saveItem.level, isEquipped, OnInventoryItemClicked);
                    inventoryItems.Add(itemUI);
                }
            }
        }

        private void RefreshTotalStats()
        {
            if (totalStatsText == null) return;

            var total = EquipmentController.GetTotalBonusStats();
            List<string> parts = new List<string>();

            if (total.bonusHP != 0) parts.Add($"HP+{total.bonusHP}");
            if (total.bonusDamagePercent != 0) parts.Add($"DMG+{total.bonusDamagePercent}%");
            if (total.bonusArmor != 0) parts.Add($"Giap {total.bonusArmor}%");
            if (total.bonusMoveSpeed != 0) parts.Add($"Speed+{total.bonusMoveSpeed}%");

            totalStatsText.text = parts.Count > 0 ? string.Join("  |  ", parts) : "Chua trang bi gi";
        }

        private void OnSlotClicked(EquipmentSlotUI slot, EquipmentData item, EquipmentType type)
        {
            if (item == null) return;
            actionPopup.ShowForEquipped(item, type, slot.transform.position);
        }

        private void OnInventoryItemClicked(EquipmentItemUI itemUI, EquipmentData item, bool isEquipped)
        {
            if (isEquipped)
            {
                actionPopup.ShowForEquipped(item, item.EquipmentType, itemUI.transform.position);
            }
            else
            {
                actionPopup.ShowForUnequipped(item, itemUI.transform.position);
            }
        }
    }
}
