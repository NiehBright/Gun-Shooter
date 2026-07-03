#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Watermelon.SquadShooter
{
    public class EquipmentEditorWindow : EditorWindow
    {
        private const string DATABASE_PATH = "Assets/Project Data/Content/Data/Equipment";
        private const string DATABASE_FILE = "Equipment Database.asset";
        private const string ITEMS_FOLDER = "Items";

        private EquipmentDatabase database;
        private Vector2 listScrollPos;
        private Vector2 detailScrollPos;
        private int selectedIndex = -1;

        // Tạo mới
        private bool isCreatingNew = false;
        private string newItemName = "";
        private string newItemID = "";
        private Sprite newItemIcon;
        private EquipmentType newItemType = EquipmentType.Hat;
        private EquipmentRarity newItemRarity = EquipmentRarity.Common;
        private EquipmentBonusStats newBaseStats;
        private EquipmentBonusStats newStatsPerLevel;
        private int newMaxLevel = 5;
        private int newSellPrice = 10;
        private int[] newUpgradeCosts = new int[] { 100, 200, 400, 800, 1600 };

        private static readonly string[] EQUIPMENT_TYPE_NAMES = { "🎩 Mũ", "🛡️ Áo Giáp", "👖 Quần", "👟 Giày" };
        private static readonly string[] RARITY_NAMES = { "⬜ Thường", "🟦 Hiếm", "🟪 Sử Thi" };
        private static readonly Color[] RARITY_COLORS = {
            new Color(0.8f, 0.8f, 0.8f),   // Trắng
            new Color(0.3f, 0.6f, 1.0f),    // Xanh
            new Color(0.7f, 0.3f, 0.9f),    // Tím
        };

        [MenuItem("Tools/Equipment Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<EquipmentEditorWindow>("Equipment Editor");
            window.minSize = new Vector2(700, 500);
            window.Show();
        }

        private void OnEnable()
        {
            LoadOrCreateDatabase();
        }

        private void LoadOrCreateDatabase()
        {
            string fullPath = Path.Combine(DATABASE_PATH, DATABASE_FILE);
            database = AssetDatabase.LoadAssetAtPath<EquipmentDatabase>(fullPath);

            if (database == null)
            {
                // Tạo thư mục nếu chưa có
                if (!AssetDatabase.IsValidFolder(DATABASE_PATH))
                {
                    string parent = Path.GetDirectoryName(DATABASE_PATH).Replace("\\", "/");
                    string folder = Path.GetFileName(DATABASE_PATH);
                    AssetDatabase.CreateFolder(parent, folder);
                }

                string itemsPath = Path.Combine(DATABASE_PATH, ITEMS_FOLDER);
                if (!AssetDatabase.IsValidFolder(itemsPath))
                {
                    AssetDatabase.CreateFolder(DATABASE_PATH, ITEMS_FOLDER);
                }

                database = CreateInstance<EquipmentDatabase>();
                AssetDatabase.CreateAsset(database, fullPath);
                AssetDatabase.SaveAssets();

                Debug.Log("[Equipment Editor] Đã tạo Equipment Database mới!");
            }

            if (database != null)
            {
                // Dọn dẹp các liên kết hỏng (null) trong database nếu anh lỡ tay xóa file
                database.AllEquipment.RemoveAll(item => item == null);

                // Kiểm tra và khôi phục/tạo đủ 5 trang bị mẫu
                CreateSampleEquipment();
            }
        }

        private void OnGUI()
        {
            if (database == null)
            {
                EditorGUILayout.HelpBox("Không tìm thấy Equipment Database!", MessageType.Error);
                if (GUILayout.Button("Tạo Database Mới"))
                    LoadOrCreateDatabase();
                return;
            }

            EditorGUILayout.BeginHorizontal();

            // Cột trái - Danh sách trang bị
            DrawLeftPanel();

            // Đường kẻ phân cách
            EditorGUILayout.BeginVertical(GUILayout.Width(2));
            GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(2));
            EditorGUILayout.EndVertical();

            // Cột phải - Chi tiết / Tạo mới
            DrawRightPanel();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));

            // Header
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("📦 DANH SÁCH TRANG BỊ", headerStyle);
            EditorGUILayout.Space(5);

            // Nút tạo mới
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("➕ TẠO TRANG BỊ MỚI", GUILayout.Height(30)))
            {
                isCreatingNew = true;
                selectedIndex = -1;
                ResetNewItemFields();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(5);

            // Nút tạo 4 mẫu
            if (database.AllEquipment.Count == 0)
            {
                GUI.backgroundColor = new Color(0.3f, 0.6f, 1.0f);
                if (GUILayout.Button("🎁 TẠO 4 TRANG BỊ MẪU", GUILayout.Height(25)))
                {
                    CreateSampleEquipment();
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space(5);
            }

            // Danh sách
            listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos);

            for (int i = 0; i < database.AllEquipment.Count; i++)
            {
                var item = database.AllEquipment[i];
                if (item == null) continue;

                // Highlight selected
                GUI.backgroundColor = (selectedIndex == i) ? new Color(0.4f, 0.7f, 1.0f) : Color.white;

                EditorGUILayout.BeginHorizontal("box");

                // Icon
                if (item.Icon != null)
                {
                    GUILayout.Label(AssetPreview.GetAssetPreview(item.Icon), GUILayout.Width(35), GUILayout.Height(35));
                }
                else
                {
                    GUILayout.Box("?", GUILayout.Width(35), GUILayout.Height(35));
                }

                // Info
                EditorGUILayout.BeginVertical();
                GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    normal = { textColor = RARITY_COLORS[(int)item.Rarity] }
                };
                EditorGUILayout.LabelField(item.ItemName, nameStyle);
                EditorGUILayout.LabelField($"{EQUIPMENT_TYPE_NAMES[(int)item.EquipmentType]} | {RARITY_NAMES[(int)item.Rarity]}", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();

                // Click để chọn
                Rect lastRect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
                {
                    selectedIndex = i;
                    isCreatingNew = false;
                    Repaint();
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical();
            detailScrollPos = EditorGUILayout.BeginScrollView(detailScrollPos);

            if (isCreatingNew)
            {
                DrawCreateNewPanel();
            }
            else if (selectedIndex >= 0 && selectedIndex < database.AllEquipment.Count)
            {
                DrawDetailPanel(database.AllEquipment[selectedIndex]);
            }
            else
            {
                EditorGUILayout.HelpBox("Chọn một trang bị từ danh sách bên trái hoặc tạo mới.", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawCreateNewPanel()
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("✨ TẠO TRANG BỊ MỚI", titleStyle);
            EditorGUILayout.Space(10);

            // Thông tin cơ bản
            EditorGUILayout.LabelField("📋 Thông Tin Cơ Bản", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            newItemName = EditorGUILayout.TextField("Tên trang bị", newItemName);
            newItemID = EditorGUILayout.TextField("ID (tự động)", string.IsNullOrEmpty(newItemID) ? GenerateID(newItemName) : newItemID);
            newItemIcon = (Sprite)EditorGUILayout.ObjectField("Hình icon", newItemIcon, typeof(Sprite), false);
            newItemType = (EquipmentType)EditorGUILayout.Popup("Loại trang bị", (int)newItemType, EQUIPMENT_TYPE_NAMES);
            newItemRarity = (EquipmentRarity)EditorGUILayout.Popup("Độ hiếm", (int)newItemRarity, RARITY_NAMES);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Chỉ số
            EditorGUILayout.LabelField("📊 Chỉ Số Cơ Bản", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            DrawBonusStatsFields(ref newBaseStats);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("📈 Chỉ Số Cộng Thêm Mỗi Cấp", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            DrawBonusStatsFields(ref newStatsPerLevel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Nâng cấp & Bán
            EditorGUILayout.LabelField("⬆️ Nâng Cấp & Bán", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            newMaxLevel = EditorGUILayout.IntField("Cấp tối đa", newMaxLevel);
            newSellPrice = EditorGUILayout.IntField("Giá bán (coins)", newSellPrice);

            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("Chi phí nâng cấp theo cấp:");

            // Resize array nếu cần
            if (newUpgradeCosts == null || newUpgradeCosts.Length != newMaxLevel)
            {
                int[] temp = new int[newMaxLevel];
                for (int i = 0; i < newMaxLevel; i++)
                {
                    temp[i] = (newUpgradeCosts != null && i < newUpgradeCosts.Length)
                        ? newUpgradeCosts[i]
                        : (i + 1) * 200;
                }
                newUpgradeCosts = temp;
            }

            EditorGUI.indentLevel++;
            for (int i = 0; i < newUpgradeCosts.Length; i++)
            {
                newUpgradeCosts[i] = EditorGUILayout.IntField($"Cấp {i} → {i + 1}", newUpgradeCosts[i]);
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15);

            // Nút tạo
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("✅ TẠO TRANG BỊ", GUILayout.Height(35)))
            {
                if (ValidateNewItem())
                {
                    CreateNewEquipment();
                }
            }
            GUI.backgroundColor = Color.white;

            GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f);
            if (GUILayout.Button("❌ Hủy", GUILayout.Height(35), GUILayout.Width(80)))
            {
                isCreatingNew = false;
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawDetailPanel(EquipmentData item)
        {
            if (item == null) return;

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = RARITY_COLORS[(int)item.Rarity] }
            };
            EditorGUILayout.LabelField(item.ItemName, titleStyle);
            EditorGUILayout.Space(5);

            // Icon preview
            if (item.Icon != null)
            {
                var preview = AssetPreview.GetAssetPreview(item.Icon);
                if (preview != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(preview, GUILayout.Width(80), GUILayout.Height(80));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space(5);

            // Info
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("ID", item.ItemID);
            EditorGUILayout.LabelField("Loại", EQUIPMENT_TYPE_NAMES[(int)item.EquipmentType]);
            EditorGUILayout.LabelField("Độ hiếm", RARITY_NAMES[(int)item.Rarity]);
            EditorGUILayout.LabelField("Cấp tối đa", item.MaxLevel.ToString());
            EditorGUILayout.LabelField("Giá bán", $"{item.SellPrice} coins");
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Stats
            EditorGUILayout.LabelField("📊 Chỉ Số Cơ Bản", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            DrawBonusStatsReadOnly(item.BaseStats);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("📈 Chỉ Số Tăng Mỗi Cấp", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            DrawBonusStatsReadOnly(item.StatsPerLevel);
            EditorGUILayout.EndVertical();

            // Preview chỉ số theo cấp
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("🔍 Xem Trước Chỉ Số Theo Cấp", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            for (int lv = 0; lv <= item.MaxLevel; lv++)
            {
                var stats = item.GetStatsAtLevel(lv);
                string preview = $"Cấp {lv}: ";
                List<string> parts = new List<string>();
                if (stats.bonusHP != 0) parts.Add($"HP+{stats.bonusHP}");
                if (stats.bonusDamagePercent != 0) parts.Add($"DMG+{stats.bonusDamagePercent}%");
                if (stats.bonusArmor != 0) parts.Add($"Giáp {stats.bonusArmor}%");
                if (stats.bonusMoveSpeed != 0) parts.Add($"Tốc độ+{stats.bonusMoveSpeed}%");
                preview += string.Join(" | ", parts);

                if (lv < item.MaxLevel && item.UpgradeCosts != null && lv < item.UpgradeCosts.Length)
                    preview += $"  [Nâng cấp: {item.UpgradeCosts[lv]} coins]";

                EditorGUILayout.LabelField(preview, EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Nút chỉnh sửa trực tiếp
            GUI.backgroundColor = new Color(1.0f, 0.9f, 0.4f);
            if (GUILayout.Button("📝 Mở chỉnh sửa trong Inspector", GUILayout.Height(25)))
            {
                Selection.activeObject = item;
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(3);

            // Nút xóa
            GUI.backgroundColor = new Color(1.0f, 0.3f, 0.3f);
            if (GUILayout.Button("🗑️ XÓA TRANG BỊ", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Xác nhận xóa",
                    $"Bạn có chắc muốn xóa trang bị \"{item.ItemName}\"?",
                    "Xóa", "Hủy"))
                {
                    DeleteEquipment(item);
                }
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawBonusStatsFields(ref EquipmentBonusStats stats)
        {
            stats.bonusHP = EditorGUILayout.FloatField("❤️ HP (cộng thêm)", stats.bonusHP);
            stats.bonusDamagePercent = EditorGUILayout.FloatField("⚔️ Sát thương (%)", stats.bonusDamagePercent);
            stats.bonusArmor = EditorGUILayout.FloatField("🛡️ Giáp (% giảm damage)", stats.bonusArmor);
            stats.bonusMoveSpeed = EditorGUILayout.FloatField("🏃 Tốc độ (% tăng)", stats.bonusMoveSpeed);
        }

        private void DrawBonusStatsReadOnly(EquipmentBonusStats stats)
        {
            if (stats.bonusHP != 0) EditorGUILayout.LabelField("❤️ HP", $"+{stats.bonusHP}");
            if (stats.bonusDamagePercent != 0) EditorGUILayout.LabelField("⚔️ Sát thương", $"+{stats.bonusDamagePercent}%");
            if (stats.bonusArmor != 0) EditorGUILayout.LabelField("🛡️ Giáp", $"{stats.bonusArmor}% giảm damage");
            if (stats.bonusMoveSpeed != 0) EditorGUILayout.LabelField("🏃 Tốc độ", $"+{stats.bonusMoveSpeed}%");

            if (stats.bonusHP == 0 && stats.bonusDamagePercent == 0 && stats.bonusArmor == 0 && stats.bonusMoveSpeed == 0)
                EditorGUILayout.LabelField("(Không có bonus)", EditorStyles.miniLabel);
        }

        private string GenerateID(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            return name.ToLower().Replace(" ", "_").Replace("đ", "d").Replace("ă", "a")
                .Replace("â", "a").Replace("ê", "e").Replace("ô", "o").Replace("ơ", "o")
                .Replace("ư", "u").Replace("á", "a").Replace("à", "a").Replace("ả", "a")
                .Replace("ã", "a").Replace("ạ", "a");
        }

        private bool ValidateNewItem()
        {
            if (string.IsNullOrEmpty(newItemName))
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng nhập tên trang bị!", "OK");
                return false;
            }

            if (string.IsNullOrEmpty(newItemID))
            {
                newItemID = GenerateID(newItemName);
            }

            // Kiểm tra trùng ID
            if (database.GetEquipmentByID(newItemID) != null)
            {
                EditorUtility.DisplayDialog("Lỗi", $"ID \"{newItemID}\" đã tồn tại!", "OK");
                return false;
            }

            return true;
        }

        private void CreateNewEquipment()
        {
            string itemsPath = Path.Combine(DATABASE_PATH, ITEMS_FOLDER);
            if (!AssetDatabase.IsValidFolder(itemsPath))
            {
                AssetDatabase.CreateFolder(DATABASE_PATH, ITEMS_FOLDER);
            }

            EquipmentData newItem = CreateInstance<EquipmentData>();
            newItem.SetupInEditor(newItemID, newItemName, newItemIcon, newItemType,
                newItemRarity, newBaseStats, newStatsPerLevel, newMaxLevel, newUpgradeCosts, newSellPrice);

            string assetPath = Path.Combine(itemsPath, $"{newItemID}.asset");
            AssetDatabase.CreateAsset(newItem, assetPath);

            database.AddEquipment(newItem);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            isCreatingNew = false;
            selectedIndex = database.AllEquipment.Count - 1;

            Debug.Log($"[Equipment Editor] Đã tạo trang bị: {newItemName} ({newItemRarity})");
        }

        private void DeleteEquipment(EquipmentData item)
        {
            database.RemoveEquipment(item);
            EditorUtility.SetDirty(database);

            string path = AssetDatabase.GetAssetPath(item);
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            AssetDatabase.SaveAssets();
            selectedIndex = -1;

            Debug.Log($"[Equipment Editor] Đã xóa trang bị: {item.ItemName}");
        }

        private void ResetNewItemFields()
        {
            newItemName = "";
            newItemID = "";
            newItemIcon = null;
            newItemType = EquipmentType.Hat;
            newItemRarity = EquipmentRarity.Common;
            newBaseStats = EquipmentBonusStats.Zero;
            newStatsPerLevel = EquipmentBonusStats.Zero;
            newMaxLevel = 5;
            newSellPrice = 10;
            newUpgradeCosts = new int[] { 100, 200, 400, 800, 1600 };
        }

        private void CreateSampleEquipment()
        {
            string itemsPath = Path.Combine(DATABASE_PATH, ITEMS_FOLDER);
            if (!AssetDatabase.IsValidFolder(itemsPath))
            {
                AssetDatabase.CreateFolder(DATABASE_PATH, ITEMS_FOLDER);
            }

            // 1. Mũ Chiến Binh - Thường - +HP
            CreateSampleItem("mu_chien_binh", "Mũ Chiến Binh", EquipmentType.Hat,
                EquipmentRarity.Common,
                new EquipmentBonusStats(20, 0, 0, 0),
                new EquipmentBonusStats(5, 0, 0, 0),
                5, new int[] { 50, 100, 200, 400, 800 }, 15);

            // 2. Áo Giáp Sắt - Hiếm - +Giáp
            CreateSampleItem("ao_giap_sat", "Áo Giáp Sắt", EquipmentType.Armor,
                EquipmentRarity.Rare,
                new EquipmentBonusStats(0, 0, 5, 0),
                new EquipmentBonusStats(0, 0, 2, 0),
                5, new int[] { 100, 200, 400, 800, 1600 }, 30);

            // 3. Quần Chiến Đấu - Thường - +Tốc độ
            CreateSampleItem("quan_chien_dau", "Quần Chiến Đấu", EquipmentType.Pants,
                EquipmentRarity.Common,
                new EquipmentBonusStats(0, 0, 0, 3),
                new EquipmentBonusStats(0, 0, 0, 1),
                5, new int[] { 50, 100, 200, 400, 800 }, 15);

            // 4. Giày Tốc Hành - Sử Thi - +DMG +Speed
            CreateSampleItem("giay_toc_hanh", "Giày Tốc Hành", EquipmentType.Shoes,
                EquipmentRarity.Epic,
                new EquipmentBonusStats(0, 5, 0, 5),
                new EquipmentBonusStats(0, 2, 0, 1),
                5, new int[] { 200, 400, 800, 1600, 3200 }, 50);

            // 5. Mũ Siêu Nhân - Sử Thi - +HP +DMG
            CreateSampleItem("mu_sieu_nhan", "Mũ Siêu Nhân", EquipmentType.Hat,
                EquipmentRarity.Epic,
                new EquipmentBonusStats(50, 8, 0, 0),
                new EquipmentBonusStats(15, 3, 0, 0),
                5, new int[] { 200, 400, 800, 1600, 3200 }, 55);

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Equipment Editor] Đã tạo 5 trang bị mẫu!");
        }

        private void CreateSampleItem(string id, string name, EquipmentType type,
            EquipmentRarity rarity, EquipmentBonusStats baseStats, EquipmentBonusStats perLevel,
            int maxLvl, int[] costs, int sellPrice)
        {
            string itemsPath = Path.Combine(DATABASE_PATH, ITEMS_FOLDER);
            string assetPath = Path.Combine(itemsPath, $"{id}.asset").Replace("\\", "/");

            EquipmentData item = AssetDatabase.LoadAssetAtPath<EquipmentData>(assetPath);
            if (item == null)
            {
                item = CreateInstance<EquipmentData>();
                item.SetupInEditor(id, name, null, type, rarity, baseStats, perLevel, maxLvl, costs, sellPrice);
                AssetDatabase.CreateAsset(item, assetPath);
            }

            database.AddEquipment(item);
        }
    }
}
#endif
