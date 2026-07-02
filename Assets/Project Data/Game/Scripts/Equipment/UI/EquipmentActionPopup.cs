using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// Popup nhỏ hiển thị khi click vào trang bị:
    /// - Chưa trang bị: "Đeo trang bị" / "Bán"
    /// - Đang trang bị: "Nâng cấp" / "Tháo trang bị"
    /// </summary>
    public class EquipmentActionPopup : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] GameObject popupPanel;
        [SerializeField] Button topButton;
        [SerializeField] Text topButtonText;
        [SerializeField] Button bottomButton;
        [SerializeField] Text bottomButtonText;
        [SerializeField] Text itemNameText;
        [SerializeField] Image itemIconImage;

        [Header("Popup xác nhận bán")]
        [SerializeField] GameObject sellConfirmPanel;
        [SerializeField] Text sellConfirmText;
        [SerializeField] Button sellConfirmYes;
        [SerializeField] Button sellConfirmNo;

        private EquipmentData currentItem;
        private EquipmentType currentSlotType;
        private bool isForEquipped;

        private void Start()
        {
            Hide();

            if (sellConfirmYes != null)
                sellConfirmYes.onClick.AddListener(OnConfirmSell);
            if (sellConfirmNo != null)
                sellConfirmNo.onClick.AddListener(() => { if (sellConfirmPanel != null) sellConfirmPanel.SetActive(false); });
        }

        /// <summary>
        /// Hiện popup cho item CHƯA trang bị: "Đeo trang bị" / "Bán"
        /// </summary>
        public void ShowForUnequipped(EquipmentData item, Vector3 position)
        {
            if (item == null || popupPanel == null) return;

            currentItem = item;
            isForEquipped = false;

            // Tên và icon
            if (itemNameText != null) itemNameText.text = item.ItemName;
            if (itemIconImage != null)
            {
                itemIconImage.sprite = item.Icon;
                itemIconImage.enabled = item.Icon != null;
            }

            // Nút trên: Đeo trang bị
            if (topButtonText != null) topButtonText.text = "Deo trang bi";
            topButton.onClick.RemoveAllListeners();
            topButton.onClick.AddListener(OnEquipClicked);

            // Nút dưới: Bán
            if (bottomButtonText != null) bottomButtonText.text = $"Ban ({item.SellPrice} coins)";
            bottomButton.onClick.RemoveAllListeners();
            bottomButton.onClick.AddListener(OnSellClicked);

            ShowPopup();
        }

        /// <summary>
        /// Hiện popup cho item ĐANG trang bị: "Nâng cấp" / "Tháo trang bị"
        /// </summary>
        public void ShowForEquipped(EquipmentData item, EquipmentType slotType, Vector3 position)
        {
            if (item == null || popupPanel == null) return;

            currentItem = item;
            currentSlotType = slotType;
            isForEquipped = true;

            // Tên và icon
            if (itemNameText != null) itemNameText.text = item.ItemName;
            if (itemIconImage != null)
            {
                itemIconImage.sprite = item.Icon;
                itemIconImage.enabled = item.Icon != null;
            }

            // Nút trên: Nâng cấp
            string upgradeCostText = "";
            if (EquipmentController.SaveData != null)
            {
                var saveItem = EquipmentController.SaveData.GetItem(item.ItemID);
                if (saveItem != null)
                {
                    int cost = item.GetUpgradeCost(saveItem.level);
                    if (cost >= 0)
                        upgradeCostText = $" ({cost} coins)";
                    else
                        upgradeCostText = " (MAX)";
                }
            }

            if (topButtonText != null) topButtonText.text = $"Nang cap{upgradeCostText}";
            topButton.onClick.RemoveAllListeners();
            topButton.onClick.AddListener(OnUpgradeClicked);

            // Nút dưới: Tháo trang bị
            if (bottomButtonText != null) bottomButtonText.text = "Thao trang bi";
            bottomButton.onClick.RemoveAllListeners();
            bottomButton.onClick.AddListener(OnUnequipClicked);

            ShowPopup();
        }

        private void ShowPopup()
        {
            // Đặt popup ở giữa màn hình
            RectTransform popupRect = popupPanel.GetComponent<RectTransform>();
            if (popupRect != null)
            {
                popupRect.anchoredPosition = Vector2.zero;
            }

            popupPanel.SetActive(true);

            if (sellConfirmPanel != null)
                sellConfirmPanel.SetActive(false);

            Debug.Log($"[Equipment] Popup mở cho: {currentItem?.ItemName}");
        }

        public void Hide()
        {
            if (popupPanel != null) popupPanel.SetActive(false);
            if (sellConfirmPanel != null) sellConfirmPanel.SetActive(false);
        }

        // === Button Handlers ===

        private void OnEquipClicked()
        {
            if (currentItem == null) return;

            EquipmentController.Equip(currentItem);
            Hide();

            // Refresh UI
            if (EquipmentPanelUI.Instance != null)
                EquipmentPanelUI.Instance.RefreshUI();
        }

        private void OnSellClicked()
        {
            if (currentItem == null) return;

            // Hiện popup xác nhận
            if (sellConfirmPanel != null)
            {
                if (sellConfirmText != null)
                    sellConfirmText.text = $"Ban \"{currentItem.ItemName}\" voi gia {currentItem.SellPrice} coins?";
                sellConfirmPanel.SetActive(true);
            }
            else
            {
                // Không có popup xác nhận → bán luôn
                ConfirmSell();
            }
        }

        private void OnConfirmSell()
        {
            ConfirmSell();
        }

        private void ConfirmSell()
        {
            if (currentItem == null) return;

            EquipmentController.SellEquipment(currentItem.ItemID);
            Hide();

            if (EquipmentPanelUI.Instance != null)
                EquipmentPanelUI.Instance.RefreshUI();
        }

        private void OnUpgradeClicked()
        {
            if (currentItem == null) return;

            bool success = EquipmentController.UpgradeEquipment(currentItem.ItemID);

            if (success)
            {
                Hide();

                if (EquipmentPanelUI.Instance != null)
                    EquipmentPanelUI.Instance.RefreshUI();
            }
            else
            {
                Debug.Log("[Equipment] Không thể nâng cấp - kiểm tra tiền hoặc cấp tối đa!");
            }
        }

        private void OnUnequipClicked()
        {
            if (currentItem == null) return;

            EquipmentController.Unequip(currentSlotType);
            Hide();

            if (EquipmentPanelUI.Instance != null)
                EquipmentPanelUI.Instance.RefreshUI();
        }
    }
}
