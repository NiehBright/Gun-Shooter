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
        [SerializeField] Text slotNameText;
        [SerializeField] Text levelText;
        [SerializeField] Button button;

        private EquipmentData currentItem;
        private EquipmentType slotType;
        private System.Action<EquipmentSlotUI, EquipmentData, EquipmentType> onClickCallback;

        private static readonly Color[] RARITY_BORDER_COLORS = {
            new Color(0.7f, 0.7f, 0.7f, 1f),  // Thường - xám
            new Color(0.2f, 0.6f, 1.0f, 1f),   // Hiếm - xanh
            new Color(0.7f, 0.2f, 0.9f, 1f),   // Sử thi - tím
        };

        private static readonly string[] SLOT_NAMES = { "Mũ", "Áo", "Quần", "Giày" };

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

            // Tên slot
            if (slotNameText != null)
                slotNameText.text = SLOT_NAMES[(int)type];

            if (item != null)
            {
                // Có trang bị
                if (iconImage != null)
                {
                    iconImage.sprite = item.Icon;
                    iconImage.enabled = item.Icon != null;
                    iconImage.color = Color.white;
                }

                if (borderImage != null)
                    borderImage.color = RARITY_BORDER_COLORS[(int)item.Rarity];

                if (levelText != null)
                {
                    levelText.text = level > 0 ? $"+{level}" : "";
                    levelText.enabled = level > 0;
                }

                if (backgroundImage != null)
                    backgroundImage.color = new Color(RARITY_BORDER_COLORS[(int)item.Rarity].r,
                        RARITY_BORDER_COLORS[(int)item.Rarity].g,
                        RARITY_BORDER_COLORS[(int)item.Rarity].b, 0.15f);
            }
            else
            {
                // Slot trống
                if (iconImage != null)
                {
                    iconImage.sprite = null;
                    iconImage.enabled = false;
                }

                if (borderImage != null)
                    borderImage.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);

                if (levelText != null)
                    levelText.enabled = false;

                if (backgroundImage != null)
                    backgroundImage.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
            }
        }

        private void OnClick()
        {
            onClickCallback?.Invoke(this, currentItem, slotType);
        }
    }
}
