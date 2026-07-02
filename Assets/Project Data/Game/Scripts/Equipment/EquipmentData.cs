using UnityEngine;

namespace Watermelon.SquadShooter
{
    [CreateAssetMenu(fileName = "Equipment Item", menuName = "Content/Equipment/Equipment Item")]
    public class EquipmentData : ScriptableObject
    {
        [Header("Thông tin cơ bản")]
        [SerializeField] string itemName;
        public string ItemName => itemName;

        [SerializeField] string itemID;
        public string ItemID => itemID;

        [SerializeField] Sprite icon;
        public Sprite Icon => icon;

        [SerializeField] EquipmentType equipmentType;
        public EquipmentType EquipmentType => equipmentType;

        [SerializeField] EquipmentRarity rarity;
        public EquipmentRarity Rarity => rarity;

        [Header("Chỉ số")]
        [SerializeField] EquipmentBonusStats baseStats;
        public EquipmentBonusStats BaseStats => baseStats;

        [Tooltip("Chỉ số cộng thêm mỗi cấp nâng")]
        [SerializeField] EquipmentBonusStats statsPerLevel;
        public EquipmentBonusStats StatsPerLevel => statsPerLevel;

        [Header("Nâng cấp")]
        [SerializeField] int maxLevel = 5;
        public int MaxLevel => maxLevel;

        [SerializeField] int[] upgradeCosts;
        public int[] UpgradeCosts => upgradeCosts;

        [Header("Bán")]
        [SerializeField] int sellPrice = 10;
        public int SellPrice => sellPrice;

        /// <summary>
        /// Tính tổng bonus stats tại một cấp nâng cụ thể
        /// </summary>
        public EquipmentBonusStats GetStatsAtLevel(int level)
        {
            return new EquipmentBonusStats(
                baseStats.bonusHP + statsPerLevel.bonusHP * level,
                baseStats.bonusDamagePercent + statsPerLevel.bonusDamagePercent * level,
                baseStats.bonusArmor + statsPerLevel.bonusArmor * level,
                baseStats.bonusMoveSpeed + statsPerLevel.bonusMoveSpeed * level
            );
        }

        /// <summary>
        /// Lấy chi phí nâng cấp lên cấp tiếp theo
        /// </summary>
        public int GetUpgradeCost(int currentLevel)
        {
            if (upgradeCosts == null || currentLevel >= upgradeCosts.Length)
                return -1; // Không thể nâng cấp

            return upgradeCosts[currentLevel];
        }

#if UNITY_EDITOR
        public void SetupInEditor(string id, string name, Sprite iconSprite, EquipmentType type,
            EquipmentRarity rarityLevel, EquipmentBonusStats baseBonus, EquipmentBonusStats perLevel,
            int maxLvl, int[] costs, int sell)
        {
            itemID = id;
            itemName = name;
            icon = iconSprite;
            equipmentType = type;
            rarity = rarityLevel;
            baseStats = baseBonus;
            statsPerLevel = perLevel;
            maxLevel = maxLvl;
            upgradeCosts = costs;
            sellPrice = sell;
        }
#endif
    }
}
