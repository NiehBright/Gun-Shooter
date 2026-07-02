using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// Hiển thị 1 item trong kho đồ (inventory grid)
    /// </summary>
    public class EquipmentItemUI : MonoBehaviour
    {
        [SerializeField] Image iconImage;
        [SerializeField] Image borderImage;
        [SerializeField] Image backgroundImage;
        [SerializeField] Text levelText;
        [SerializeField] Text nameText;
        [SerializeField] GameObject equippedBadge; // Badge "Đang mặc"
        [SerializeField] Button button;

        private EquipmentData itemData;
        private bool isEquipped;
        private System.Action<EquipmentItemUI, EquipmentData, bool> onClickCallback;

        private static readonly Color[] RARITY_COLORS = {
            new Color(0.7f, 0.7f, 0.7f, 1f),
            new Color(0.2f, 0.6f, 1.0f, 1f),
            new Color(0.7f, 0.2f, 0.9f, 1f),
        };

        private void Awake()
        {
            if (button != null)
                button.onClick.AddListener(OnClick);
        }

        public void Setup(EquipmentData data, int level, bool equipped,
            System.Action<EquipmentItemUI, EquipmentData, bool> callback)
        {
            itemData = data;
            isEquipped = equipped;
            onClickCallback = callback;

            // Icon
            if (iconImage != null)
            {
                iconImage.sprite = data.Icon;
                iconImage.enabled = data.Icon != null;
            }

            // Border theo rarity
            if (borderImage != null)
                borderImage.color = RARITY_COLORS[(int)data.Rarity];

            // Background nhạt theo rarity
            if (backgroundImage != null)
            {
                Color c = RARITY_COLORS[(int)data.Rarity];
                backgroundImage.color = new Color(c.r, c.g, c.b, 0.2f);
            }

            // Level badge
            if (levelText != null)
            {
                levelText.text = level > 0 ? $"+{level}" : "";
                levelText.enabled = level > 0;
            }

            // Tên
            if (nameText != null)
                nameText.text = data.ItemName;

            // Badge "Đang mặc"
            if (equippedBadge != null)
                equippedBadge.SetActive(equipped);
        }

        private void OnClick()
        {
            onClickCallback?.Invoke(this, itemData, isEquipped);
        }
    }
}
