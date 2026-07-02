using UnityEngine;
using UnityEngine.AI;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// Gắn vào scene Game. Khi game bắt đầu, áp dụng bonus từ trang bị vào nhân vật.
    /// Không sửa code gốc - chỉ hook vào hệ thống hiện tại.
    /// </summary>
    public class EquipmentStatsApplier : MonoBehaviour
    {
        private CharacterBehaviour characterBehaviour;

        private float appliedBonusHP;
        private float appliedArmorPercent;
        private float appliedMoveSpeedPercent;
        private float appliedDamagePercent;

        private float originalSpeed = -1f;

        private void OnEnable()
        {
            EquipmentController.OnEquipmentChanged += RecalculateStats;
        }

        private void OnDisable()
        {
            EquipmentController.OnEquipmentChanged -= RecalculateStats;
        }

        /// <summary>
        /// Gọi khi game bắt đầu level mới để áp dụng bonus
        /// </summary>
        public void ApplyToCharacter(CharacterBehaviour character)
        {
            characterBehaviour = character;
            RecalculateStats();
        }

        private void RecalculateStats()
        {
            if (characterBehaviour == null) return;

            EquipmentBonusStats totalBonus = EquipmentController.GetTotalBonusStats();

            appliedBonusHP = totalBonus.bonusHP;
            appliedArmorPercent = Mathf.Clamp(totalBonus.bonusArmor, 0f, 75f); // Max 75% giảm damage
            appliedMoveSpeedPercent = totalBonus.bonusMoveSpeed;
            appliedDamagePercent = totalBonus.bonusDamagePercent;

            // Áp dụng tốc độ di chuyển
            NavMeshAgent agent = characterBehaviour.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                if (originalSpeed < 0) originalSpeed = agent.speed;
                agent.speed = originalSpeed * (1f + appliedMoveSpeedPercent / 100f);
            }

            Debug.Log($"[Equipment Stats] HP+{appliedBonusHP} | DMG+{appliedDamagePercent}% | Armor {appliedArmorPercent}% | Speed+{appliedMoveSpeedPercent}%");
        }

        /// <summary>
        /// Lấy HP bonus từ trang bị (cộng thêm vào MaxHealth)
        /// </summary>
        public float GetBonusHP()
        {
            return appliedBonusHP;
        }

        /// <summary>
        /// Lấy % giảm sát thương nhận
        /// </summary>
        public float GetArmorPercent()
        {
            return appliedArmorPercent;
        }

        /// <summary>
        /// Lấy % tăng sát thương
        /// </summary>
        public float GetDamagePercent()
        {
            return appliedDamagePercent;
        }

        /// <summary>
        /// Tính sát thương sau khi giảm bởi giáp
        /// </summary>
        public float CalculateDamageAfterArmor(float rawDamage)
        {
            return rawDamage * (1f - appliedArmorPercent / 100f);
        }

        /// <summary>
        /// Tính sát thương sau khi tăng bởi bonus
        /// </summary>
        public float CalculateBonusDamage(float rawDamage)
        {
            return rawDamage * (1f + appliedDamagePercent / 100f);
        }
    }
}
