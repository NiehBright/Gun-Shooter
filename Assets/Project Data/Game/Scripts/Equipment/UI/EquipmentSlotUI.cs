using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// Hiển thị 1 ô slot trang bị (Mũ/Áo/Quần/Giày)
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour
    {
        [SerializeField] Image iconImage;
        [SerializeField] Image borderImage;
        [SerializeField] Image backgroundImage;
        [SerializeField] TMPro.TMP_Text slotNameText;
        [SerializeField] TMPro.TMP_Text levelText;
        [SerializeField] Button button;
        [SerializeField] GameObject activeItemPrefab; // Prefab UI đại diện cho vật phẩm hoạt động (khung đổi màu/độ hiếm)

        private EquipmentData currentItem;
        private GameObject spawnedItemInstance; // Đối tượng vật phẩm đã spawn trong ô slot
        private EquipmentType slotType;
        private System.Action<EquipmentSlotUI, EquipmentData, EquipmentType> onClickCallback;

        private static readonly Color[] RARITY_BORDER_COLORS = {
            new Color(0.7f, 0.7f, 0.7f, 1f),  // Thường - xám
            new Color(0.2f, 0.6f, 1.0f, 1f),   // Hiếm - xanh
            new Color(0.7f, 0.2f, 0.9f, 1f),   // Sử thi - tím
        };

        private static readonly string[] SLOT_NAMES = { "Mũ", "Giáp", "Găng tay", "Giày" };

        private void Awake()
        {
            if (button != null)
                button.onClick.AddListener(OnClick);
        }

        public void Setup(EquipmentData item, EquipmentType type, int level,
            System.Action<EquipmentSlotUI, EquipmentData, EquipmentType> callback)
        {
            currentItem = item;
            slotType = type;
            onClickCallback = callback;

            // Xóa bỏ đối tượng đã spawn cũ trước khi thiết lập lại
            if (spawnedItemInstance != null)
            {
                Destroy(spawnedItemInstance);
                spawnedItemInstance = null;
            }

            // Tên slot
            if (slotNameText != null)
                slotNameText.text = SLOT_NAMES[(int)type];

            if (item != null)
            {
                // Có trang bị -> Bật iconImage làm vùng chứa cha và spawn prefab của vật phẩm vào trong
                if (iconImage != null)
                {
                    iconImage.sprite = null;
                    iconImage.enabled = true; // Bật lên làm cha chứa RectTransform con
                    iconImage.color = new Color(1, 1, 1, 0); // Ẩn hình ảnh gốc của iconImage nhưng giữ nó hoạt động

                    if (activeItemPrefab != null)
                    {
                        spawnedItemInstance = Instantiate(activeItemPrefab, iconImage.transform);
                        RectTransform rect = spawnedItemInstance.GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            rect.anchorMin = Vector2.zero;
                            rect.anchorMax = Vector2.one;
                            rect.offsetMin = Vector2.zero;
                            rect.offsetMax = Vector2.zero;
                        }

                        // Cấu hình vật phẩm hoạt động vừa spawn
                        EquipmentItemUI activeItemUI = spawnedItemInstance.GetComponent<EquipmentItemUI>();
                        if (activeItemUI != null)
                        {
                            activeItemUI.Setup(item, level, true, null);
                        }
                    }
                }

                if (levelText != null)
                {
                    levelText.enabled = false; // Ẩn levelText gốc của slot (đã dùng levelText trong prefab active)
                }

                if (slotNameText != null)
                {
                    slotNameText.enabled = false; // Ẩn chữ tên slot khi đã mặc đồ
                }
            }
            else
            {
                // Slot trống -> Ẩn iconImage gốc
                if (iconImage != null)
                {
                    iconImage.sprite = null;
                    iconImage.enabled = false;
                }

                if (levelText != null)
                    levelText.enabled = false;

                if (slotNameText != null)
                {
                    slotNameText.enabled = true; // Hiện lại chữ tên slot khi trống
                }
            }
        }

        private void OnClick()
        {
            onClickCallback?.Invoke(this, currentItem, slotType);
        }
    }
}
