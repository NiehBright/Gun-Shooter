using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// Controller quản lý toàn bộ logic trang bị.
    /// Gắn vào 1 GameObject trong scene hoặc khởi tạo từ GameController.
    /// </summary>
    public class EquipmentController : MonoBehaviour
    {
        private static EquipmentController instance;
        public static EquipmentController Instance => instance;

        [SerializeField] EquipmentDatabase database;
        public static EquipmentDatabase Database => instance?.database;

        private EquipmentSaveData saveData;
        public static EquipmentSaveData SaveData => instance?.saveData;

        // Event khi trang bị thay đổi (để UI và StatsApplier lắng nghe)
        public static event SimpleCallback OnEquipmentChanged;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR
            if (database == null)
            {
                database = UnityEditor.AssetDatabase.LoadAssetAtPath<EquipmentDatabase>(
                    "Assets/Project Data/Content/Data/Equipment/Equipment Database.asset");
                if (database != null)
                {
                    Debug.Log("[Equipment] Tự động liên kết Database thành công!");
                }
            }
#endif
        }

        private void Start()
        {
            // Load save data
            saveData = SaveController.GetSaveObject<EquipmentSaveData>("Equipment");
            
            Debug.Log($"[Equipment] Start. SaveData: {(saveData != null ? "Loaded" : "Null")}, Database: {(database != null ? "Loaded" : "Null")}");

            // Để chạy thử nghiệm và Test: tự động tặng tất cả trang bị trong Database cho người chơi nếu chưa sở hữu
            if (saveData != null && database != null)
            {
#if UNITY_EDITOR
                // Fallback: nếu database rỗng nhưng có file sẵn trong folder, nạp chúng vào database trong Editor
                if (database.AllEquipment == null || database.AllEquipment.Count == 0)
                {
                    Debug.Log("[Equipment] Database rỗng, đang quét tìm file trang bị mẫu...");
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:EquipmentData");
                    foreach (string guid in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                        var eqData = UnityEditor.AssetDatabase.LoadAssetAtPath<EquipmentData>(path);
                        if (eqData != null)
                        {
                            database.AddEquipment(eqData);
                        }
                    }
                    UnityEditor.EditorUtility.SetDirty(database);
                    UnityEditor.AssetDatabase.SaveAssets();
                    Debug.Log($"[Equipment] Đã nạp {database.AllEquipment.Count} trang bị vào Database!");
                }
#endif

                Debug.Log($"[Equipment] Database items count: {database.AllEquipment.Count}");
                foreach (var eq in database.AllEquipment)
                {
                    if (eq != null && !string.IsNullOrEmpty(eq.ItemID))
                    {
                        if (!saveData.HasItem(eq.ItemID))
                        {
                            saveData.AddItem(eq.ItemID);
                            Debug.Log($"[Equipment] Tự động tặng item cho save: {eq.ItemName}");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"[Equipment] Start failed. SaveData: {saveData}, Database: {database}");
            }
        }

        /// <summary>
        /// Trang bị vật phẩm vào slot tương ứng
        /// </summary>
        public static bool Equip(EquipmentData item)
        {
            if (instance == null || instance.saveData == null) return false;
            if (item == null) return false;

            // Kiểm tra có sở hữu không
            if (!instance.saveData.HasItem(item.ItemID))
            {
                Debug.LogWarning($"[Equipment] Không sở hữu trang bị: {item.ItemName}");
                return false;
            }

            // Trang bị vào slot
            instance.saveData.SetEquipped(item.EquipmentType, item.ItemID);

            Debug.Log($"[Equipment] Đã trang bị: {item.ItemName} vào slot {item.EquipmentType}");

            NotifyEquipmentChanged();
            SaveController.MarkAsSaveIsRequired();

            return true;
        }

        /// <summary>
        /// Tháo trang bị khỏi slot
        /// </summary>
        public static void Unequip(EquipmentType slot)
        {
            if (instance == null || instance.saveData == null) return;

            instance.saveData.SetEquipped(slot, "");

            Debug.Log($"[Equipment] Đã tháo trang bị slot: {slot}");

            NotifyEquipmentChanged();
            SaveController.MarkAsSaveIsRequired();
        }

        /// <summary>
        /// Nâng cấp trang bị
        /// </summary>
        public static bool UpgradeEquipment(string itemID)
        {
            if (instance == null || instance.saveData == null) return false;

            var saveItem = instance.saveData.GetItem(itemID);
            if (saveItem == null) return false;

            var data = instance.database.GetEquipmentByID(itemID);
            if (data == null) return false;

            // Kiểm tra max level
            if (saveItem.level >= data.MaxLevel)
            {
                Debug.LogWarning($"[Equipment] {data.ItemName} đã đạt cấp tối đa!");
                return false;
            }

            // Kiểm tra chi phí
            int cost = data.GetUpgradeCost(saveItem.level);
            if (cost < 0) return false;

            // Kiểm tra tiền (dùng hệ thống CurrenciesController có sẵn)
            if (CurrenciesController.Get(CurrencyType.Coins) < cost)
            {
                Debug.LogWarning($"[Equipment] Không đủ tiền nâng cấp! Cần {cost} coins");
                return false;
            }

            // Trừ tiền và nâng cấp
            CurrenciesController.Substract(CurrencyType.Coins, cost);
            saveItem.level++;

            Debug.Log($"[Equipment] Nâng cấp {data.ItemName} lên cấp {saveItem.level}");

            NotifyEquipmentChanged();
            SaveController.MarkAsSaveIsRequired();

            return true;
        }

        /// <summary>
        /// Thêm trang bị vào kho đồ
        /// </summary>
        public static void AddToInventory(EquipmentData item)
        {
            if (instance == null || instance.saveData == null || item == null) return;

            instance.saveData.AddItem(item.ItemID);

            Debug.Log($"[Equipment] Nhận trang bị mới: {item.ItemName} ({item.Rarity})");
        }

        /// <summary>
        /// Bán trang bị
        /// </summary>
        public static bool SellEquipment(string itemID)
        {
            if (instance == null || instance.saveData == null) return false;

            // Nếu đang trang bị thì không bán được
            if (instance.saveData.IsEquipped(itemID))
            {
                Debug.LogWarning("[Equipment] Không thể bán trang bị đang mặc!");
                return false;
            }

            var data = instance.database.GetEquipmentByID(itemID);
            if (data == null) return false;

            if (instance.saveData.RemoveItem(itemID))
            {
                CurrenciesController.Add(CurrencyType.Coins, data.SellPrice);
                Debug.Log($"[Equipment] Đã bán {data.ItemName} được {data.SellPrice} coins");

                NotifyEquipmentChanged();
                SaveController.MarkAsSaveIsRequired();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Lấy trang bị đang mặc theo slot
        /// </summary>
        public static EquipmentData GetEquippedItem(EquipmentType slot)
        {
            if (instance == null || instance.saveData == null || instance.database == null) return null;

            string id = instance.saveData.GetEquippedID(slot);
            if (string.IsNullOrEmpty(id)) return null;

            return instance.database.GetEquipmentByID(id);
        }

        /// <summary>
        /// Tính tổng bonus stats từ tất cả trang bị đang mặc
        /// </summary>
        public static EquipmentBonusStats GetTotalBonusStats()
        {
            if (instance == null || instance.saveData == null || instance.database == null)
                return EquipmentBonusStats.Zero;

            EquipmentBonusStats total = EquipmentBonusStats.Zero;

            for (int i = 0; i <= 3; i++)
            {
                EquipmentType slot = (EquipmentType)i;
                string id = instance.saveData.GetEquippedID(slot);
                if (string.IsNullOrEmpty(id)) continue;

                var data = instance.database.GetEquipmentByID(id);
                if (data == null) continue;

                var saveItem = instance.saveData.GetItem(id);
                int level = saveItem != null ? saveItem.level : 0;

                total = total + data.GetStatsAtLevel(level);
            }

            return total;
        }

        public static void NotifyEquipmentChanged()
        {
            OnEquipmentChanged?.Invoke();

            // Cập nhật lại UI lực chiến tổng và các panel vũ khí bên ngoài ngay lập tức
            if (Application.isPlaying)
            {
                UIGeneralPowerIndicator.UpdateText();
                UIController.GetPage<UIWeaponPage>()?.UpdateUI();
            }
        }
    }
}
