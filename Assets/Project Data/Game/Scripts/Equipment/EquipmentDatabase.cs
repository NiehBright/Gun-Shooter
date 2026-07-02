using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [CreateAssetMenu(fileName = "Equipment Database", menuName = "Content/Equipment/Equipment Database")]
    public class EquipmentDatabase : ScriptableObject
    {
        [SerializeField] List<EquipmentData> allEquipment = new List<EquipmentData>();
        public List<EquipmentData> AllEquipment => allEquipment;

        /// <summary>
        /// Lấy trang bị theo ID
        /// </summary>
        public EquipmentData GetEquipmentByID(string id)
        {
            return allEquipment.Find(e => e.ItemID == id);
        }

        /// <summary>
        /// Lấy danh sách trang bị theo loại
        /// </summary>
        public List<EquipmentData> GetEquipmentByType(EquipmentType type)
        {
            return allEquipment.FindAll(e => e.EquipmentType == type);
        }

        /// <summary>
        /// Lấy danh sách trang bị theo độ hiếm
        /// </summary>
        public List<EquipmentData> GetEquipmentByRarity(EquipmentRarity rarity)
        {
            return allEquipment.FindAll(e => e.Rarity == rarity);
        }

        /// <summary>
        /// Lấy trang bị ngẫu nhiên theo độ hiếm
        /// </summary>
        public EquipmentData GetRandomEquipment(EquipmentRarity rarity)
        {
            var filtered = GetEquipmentByRarity(rarity);
            if (filtered.Count == 0) return null;
            return filtered[Random.Range(0, filtered.Count)];
        }

        /// <summary>
        /// Lấy trang bị ngẫu nhiên với tỉ lệ rarity
        /// </summary>
        public EquipmentData GetRandomEquipmentWeighted(float commonChance = 0.7f, float rareChance = 0.25f, float epicChance = 0.05f)
        {
            float roll = Random.value;
            EquipmentRarity selectedRarity;

            if (roll < epicChance)
                selectedRarity = EquipmentRarity.Epic;
            else if (roll < epicChance + rareChance)
                selectedRarity = EquipmentRarity.Rare;
            else
                selectedRarity = EquipmentRarity.Common;

            var result = GetRandomEquipment(selectedRarity);

            // Fallback nếu không có trang bị ở rarity đó
            if (result == null && allEquipment.Count > 0)
                result = allEquipment[Random.Range(0, allEquipment.Count)];

            return result;
        }

#if UNITY_EDITOR
        public void AddEquipment(EquipmentData data)
        {
            if (!allEquipment.Contains(data))
            {
                allEquipment.Add(data);
            }
        }

        public void RemoveEquipment(EquipmentData data)
        {
            allEquipment.Remove(data);
        }
#endif
    }
}
