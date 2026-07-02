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

            if (EquipmentController.SaveData == null || inventoryItemPrefab == null || inventoryContainer == null) return;

            var ownedItems = EquipmentController.SaveData.OwnedItems;
            foreach (var saveItem in ownedItems)
            {
                var data = EquipmentController.Database?.GetEquipmentByID(saveItem.itemID);
                if (data == null) continue;

                GameObject itemObj = Instantiate(inventoryItemPrefab, inventoryContainer);
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
