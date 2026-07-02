using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public struct EquipmentBonusStats
    {
        [Tooltip("Cộng thêm máu (HP)")]
        public float bonusHP;

        [Tooltip("Cộng thêm % sát thương")]
        public float bonusDamagePercent;

        [Tooltip("Giảm % sát thương nhận (giáp)")]
        public float bonusArmor;

        [Tooltip("Cộng thêm % tốc độ di chuyển")]
        public float bonusMoveSpeed;

        public EquipmentBonusStats(float hp, float dmg, float armor, float speed)
        {
            bonusHP = hp;
            bonusDamagePercent = dmg;
            bonusArmor = armor;
            bonusMoveSpeed = speed;
        }

        public static EquipmentBonusStats operator +(EquipmentBonusStats a, EquipmentBonusStats b)
        {
            return new EquipmentBonusStats(
                a.bonusHP + b.bonusHP,
                a.bonusDamagePercent + b.bonusDamagePercent,
                a.bonusArmor + b.bonusArmor,
                a.bonusMoveSpeed + b.bonusMoveSpeed
            );
        }

        public static EquipmentBonusStats Zero => new EquipmentBonusStats(0, 0, 0, 0);
    }
}
