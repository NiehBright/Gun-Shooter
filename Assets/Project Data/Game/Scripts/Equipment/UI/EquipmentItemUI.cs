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
        [SerializeField] TMPro.TMP_Text levelText;
        [SerializeField] TMPro.TMP_Text nameText;
        [SerializeField] GameObject equippedBadge; // Badge "Đang mặc"
        [SerializeField] Button button;

        [Header("Rarity Frame Customisation")]
        [SerializeField] Sprite commonBorderSprite;
        [SerializeField] Sprite rareBorderSprite;
        [SerializeField] Sprite epicBorderSprite;
        [SerializeField] Sprite commonBgSprite;
        [SerializeField] Sprite rareBgSprite;
        [SerializeField] Sprite epicBgSprite;

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

            // Cấu hình khung viền (Border) theo độ hiếm bằng Sprite tùy chọn của người dùng
            if (borderImage != null)
            {
                if (data.Rarity == EquipmentRarity.Common && commonBorderSprite != null)
                    borderImage.sprite = commonBorderSprite;
                else if (data.Rarity == EquipmentRarity.Rare && rareBorderSprite != null)
                    borderImage.sprite = rareBorderSprite;
                else if (data.Rarity == EquipmentRarity.Epic && epicBorderSprite != null)
                    borderImage.sprite = epicBorderSprite;
            }

            // Cấu hình nền (Background) theo độ hiếm bằng Sprite tùy chọn của người dùng
            if (backgroundImage != null)
            {
                if (data.Rarity == EquipmentRarity.Common && commonBgSprite != null)
                    backgroundImage.sprite = commonBgSprite;
                else if (data.Rarity == EquipmentRarity.Rare && rareBgSprite != null)
                    backgroundImage.sprite = rareBgSprite;
                else if (data.Rarity == EquipmentRarity.Epic && epicBgSprite != null)
                    backgroundImage.sprite = epicBgSprite;
            }

            if (levelText != null)
            {
                levelText.text = level > 0 ? level.ToString() : "";
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
