using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [Serializable]
    public class EquipmentSaveData : Watermelon.ISaveObject
    {
        // Danh sách trang bị đã sở hữu (inventory)
        [SerializeField] List<EquipmentSaveItem> ownedItems = new List<EquipmentSaveItem>();
        public List<EquipmentSaveItem> OwnedItems => ownedItems;

        // Trang bị đang mặc trên 4 slot (lưu ItemID, rỗng = chưa trang bị)
        [SerializeField] string equippedHat = "";
        [SerializeField] string equippedArmor = "";
        [SerializeField] string equippedPants = "";
        [SerializeField] string equippedShoes = "";

        public void Flush() { }

        /// <summary>
        /// Lấy ID trang bị đang mặc theo slot
        /// </summary>
        public string GetEquippedID(EquipmentType slot)
        {
            switch (slot)
            {
                case EquipmentType.Hat: return equippedHat;
                case EquipmentType.Armor: return equippedArmor;
                case EquipmentType.Pants: return equippedPants;
                case EquipmentType.Shoes: return equippedShoes;
                default: return "";
            }
        }

        /// <summary>
        /// Đặt trang bị vào slot
        /// </summary>
        public void SetEquipped(EquipmentType slot, string itemID)
        {
            switch (slot)
            {
                case EquipmentType.Hat: equippedHat = itemID; break;
                case EquipmentType.Armor: equippedArmor = itemID; break;
                case EquipmentType.Pants: equippedPants = itemID; break;
                case EquipmentType.Shoes: equippedShoes = itemID; break;
            }

            SaveController.MarkAsSaveIsRequired();
        }

        /// <summary>
        /// Thêm trang bị vào kho
        /// </summary>
        public void AddItem(string itemID)
        {
            var existing = ownedItems.Find(i => i.itemID == itemID);
            if (existing != null)
            {
                existing.count++;
            }
            else
            {
                ownedItems.Add(new EquipmentSaveItem(itemID));
            }

            SaveController.MarkAsSaveIsRequired();
        }

        /// <summary>
        /// Xóa trang bị khỏi kho
        /// </summary>
        public bool RemoveItem(string itemID)
        {
            var existing = ownedItems.Find(i => i.itemID == itemID);
            if (existing == null) return false;

            existing.count--;
            if (existing.count <= 0)
            {
                ownedItems.Remove(existing);

                // Nếu đang trang bị item này thì tháo ra
                if (equippedHat == itemID) equippedHat = "";
                if (equippedArmor == itemID) equippedArmor = "";
                if (equippedPants == itemID) equippedPants = "";
                if (equippedShoes == itemID) equippedShoes = "";
            }

            SaveController.MarkAsSaveIsRequired();
            return true;
        }

        /// <summary>
        /// Kiểm tra có sở hữu trang bị không
        /// </summary>
        public bool HasItem(string itemID)
        {
            return ownedItems.Exists(i => i.itemID == itemID && i.count > 0);
        }

        /// <summary>
        /// Lấy thông tin save của 1 item
        /// </summary>
        public EquipmentSaveItem GetItem(string itemID)
        {
            return ownedItems.Find(i => i.itemID == itemID);
        }

        /// <summary>
        /// Kiểm tra trang bị có đang được mặc không
        /// </summary>
        public bool IsEquipped(string itemID)
        {
            return equippedHat == itemID || equippedArmor == itemID
                || equippedPants == itemID || equippedShoes == itemID;
        }
    }

    [Serializable]
    public class EquipmentSaveItem
    {
        public string itemID;
        public int level;
        public int count;

        public EquipmentSaveItem(string id)
        {
            itemID = id;
            level = 0;
            count = 1;
        }
    }
}
