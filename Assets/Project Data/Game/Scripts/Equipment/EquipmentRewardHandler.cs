using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// Xử lý logic nhận trang bị khi qua màn.
    /// Gắn vào scene Game.
    /// </summary>
    public class EquipmentRewardHandler : MonoBehaviour
    {
        [Header("Cấu hình tỉ lệ rơi")]
        [SerializeField] EquipmentDatabase database;

        [Tooltip("Tỉ lệ rơi trang bị khi qua màn (0-1)")]
        [SerializeField, Range(0f, 1f)] float dropChance = 0.3f;

        [Header("Tỉ lệ độ hiếm")]
        [SerializeField, Range(0f, 1f)] float commonChance = 0.70f;
        [SerializeField, Range(0f, 1f)] float rareChance = 0.25f;
        [SerializeField, Range(0f, 1f)] float epicChance = 0.05f;

        [Header("UI")]
        [SerializeField] GameObject rewardPopupPrefab;

        private static EquipmentRewardHandler instance;

        private void Awake()
        {
            instance = this;
        }

        /// <summary>
        /// Gọi khi người chơi qua màn để thử rơi trang bị
        /// </summary>
        public static EquipmentData TryDropEquipment()
        {
            if (instance == null || instance.database == null) return null;

            float roll = Random.value;
            if (roll > instance.dropChance) return null; // Không rơi

            // Chọn trang bị ngẫu nhiên theo tỉ lệ rarity
            EquipmentData droppedItem = instance.database.GetRandomEquipmentWeighted(
                instance.commonChance, instance.rareChance, instance.epicChance);

            if (droppedItem != null)
            {
                EquipmentController.AddToInventory(droppedItem);
                SaveController.MarkAsSaveIsRequired();

                Debug.Log($"[Equipment Reward] Rơi trang bị: {droppedItem.ItemName} ({droppedItem.Rarity})");
            }

            return droppedItem;
        }

        /// <summary>
        /// Thêm trang bị cụ thể vào inventory (dùng cho shop/gacha)
        /// </summary>
        public static void GiveEquipment(EquipmentData item)
        {
            if (item == null) return;

            EquipmentController.AddToInventory(item);
            SaveController.MarkAsSaveIsRequired();
        }
    }
}
