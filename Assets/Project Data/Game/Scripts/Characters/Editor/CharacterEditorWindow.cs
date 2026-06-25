using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Watermelon.SquadShooter
{
    public class CharacterEditorWindow : EditorWindow
    {
        private const string DATABASE_PATH = "Assets/Project Data/Content/Data/Characters System/Characters Database.asset";
        private const string ENUM_FILE_PATH = "Assets/Project Data/Game/Scripts/Characters/CharacterType.cs";

        private CharactersDatabase database;
        private SerializedObject serializedDatabase;
        private SerializedProperty charactersProperty;

        private Vector2 sidebarScroll;
        private Vector2 detailScroll;
        private int selectedIndex = -1;

        private string newEnumName = "Character_04";

        [MenuItem("Tools/Character Editor")]
        public static void OpenWindow()
        {
            CharacterEditorWindow window = GetWindow<CharacterEditorWindow>();
            window.titleContent = new GUIContent("Character Editor");
            window.minSize = new Vector2(700, 500);
            window.Show();
        }

        private void OnEnable()
        {
            LoadDatabase();
        }

        private void LoadDatabase()
        {
            database = AssetDatabase.LoadAssetAtPath<CharactersDatabase>(DATABASE_PATH);
            if (database != null)
            {
                serializedDatabase = new SerializedObject(database);
                charactersProperty = serializedDatabase.FindProperty("characters");
                
                if (charactersProperty.arraySize > 0 && selectedIndex == -1)
                {
                    selectedIndex = 0;
                }
            }
        }

        private void OnGUI()
        {
            if (database == null)
            {
                EditorGUILayout.HelpBox("Không tìm thấy Characters Database tại đường dẫn:\n" + DATABASE_PATH, MessageType.Error);
                if (GUILayout.Button("Tìm lại Database"))
                {
                    LoadDatabase();
                }
                return;
            }

            serializedDatabase.Update();

            EditorGUILayout.BeginHorizontal();

            // 1. SIDEBAR - DANH SÁCH NHÂN VẬT
            DrawSidebar();

            // Khoảng cách giữa 2 khung
            GUILayout.Box("", GUILayout.Width(2), GUILayout.ExpandHeight(true));

            // 2. KHU VỰC CHI TIẾT
            DrawDetailArea();

            EditorGUILayout.EndHorizontal();

            serializedDatabase.ApplyModifiedProperties();
        }

        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(200), GUILayout.ExpandHeight(true));
            
            EditorGUILayout.LabelField("DANH SÁCH NHÂN VẬT", EditorStyles.boldLabel);
            
            sidebarScroll = EditorGUILayout.BeginScrollView(sidebarScroll, GUI.skin.box, GUILayout.ExpandHeight(true));
            
            for (int i = 0; i < charactersProperty.arraySize; i++)
            {
                SerializedProperty charProp = charactersProperty.GetArrayElementAtIndex(i);
                SerializedProperty nameProp = charProp.FindPropertyRelative("name");
                SerializedProperty typeProp = charProp.FindPropertyRelative("type");

                string charName = string.IsNullOrEmpty(nameProp.stringValue) ? "Chưa đặt tên" : nameProp.stringValue;
                string label = $"#{i + 1} - {charName} ({ (CharacterType)typeProp.enumValueIndex })";

                GUI.backgroundColor = (selectedIndex == i) ? new Color(0.3f, 0.5f, 0.8f, 1f) : Color.white;
                
                if (GUILayout.Button(label, GUILayout.Height(30)))
                {
                    selectedIndex = i;
                    GUI.FocusControl(null);
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndScrollView();

            // Nút Thêm / Xóa nhân vật ở Sidebar
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Thêm nhân vật", GUILayout.Height(25)))
            {
                AddNewCharacter();
            }
            if (GUILayout.Button("Xóa nhân vật", GUILayout.Height(25)))
            {
                DeleteSelectedCharacter();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawDetailArea()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (selectedIndex < 0 || selectedIndex >= charactersProperty.arraySize)
            {
                EditorGUILayout.HelpBox("Chọn một nhân vật bên danh sách để bắt đầu chỉnh sửa.", MessageType.Info);
                DrawEnumTool();
                EditorGUILayout.EndVertical();
                return;
            }

            SerializedProperty selectedCharacter = charactersProperty.GetArrayElementAtIndex(selectedIndex);
            SerializedProperty nameProp = selectedCharacter.FindPropertyRelative("name");
            SerializedProperty typeProp = selectedCharacter.FindPropertyRelative("type");
            SerializedProperty reqLevelProp = selectedCharacter.FindPropertyRelative("requiredLevel");
            SerializedProperty lockedSpriteProp = selectedCharacter.FindPropertyRelative("lockedSprite");
            SerializedProperty stagesProp = selectedCharacter.FindPropertyRelative("stages");
            SerializedProperty upgradesProp = selectedCharacter.FindPropertyRelative("upgrades");

            EditorGUILayout.LabelField($"CÀI ĐẶT CHI TIẾT: {nameProp.stringValue.ToUpper()}", EditorStyles.boldLabel);

            detailScroll = EditorGUILayout.BeginScrollView(detailScroll, GUI.skin.box);

            // A. THÔNG TIN CƠ BẢN
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("THÔNG TIN CƠ BẢN", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(nameProp, new GUIContent("Tên nhân vật (Name):"));
            EditorGUILayout.PropertyField(typeProp, new GUIContent("Loại nhân vật (Type):"));
            EditorGUILayout.PropertyField(reqLevelProp, new GUIContent("Cấp độ mở khóa (Req Level):"));
            EditorGUILayout.PropertyField(lockedSpriteProp, new GUIContent("Ảnh khi bị khóa (Locked Sprite):"));
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            // B. QUẢN LÝ STAGES
            DrawStagesSection(stagesProp);

            GUILayout.Space(10);

            // C. QUẢN LÝ UPGRADES
            DrawUpgradesSection(upgradesProp, stagesProp.arraySize);

            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);
            DrawEnumTool();

            EditorGUILayout.EndVertical();
        }

        private void DrawStagesSection(SerializedProperty stagesProp)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"DANH SÁCH STAGES ({stagesProp.arraySize})", EditorStyles.boldLabel);
            if (GUILayout.Button("+ Thêm Stage", GUILayout.Width(100)))
            {
                stagesProp.arraySize++;
            }
            EditorGUILayout.EndHorizontal();

            for (int j = 0; j < stagesProp.arraySize; j++)
            {
                SerializedProperty stage = stagesProp.GetArrayElementAtIndex(j);
                SerializedProperty prevSprite = stage.FindPropertyRelative("previewSprite");
                SerializedProperty lockSprite = stage.FindPropertyRelative("lockedSprite");
                SerializedProperty prefab = stage.FindPropertyRelative("prefab");
                SerializedProperty offset = stage.FindPropertyRelative("healthBarOffset");

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Stage #{j + 1}", EditorStyles.boldLabel);
                GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f, 1f);
                if (GUILayout.Button("Xóa Stage", GUILayout.Width(80)))
                {
                    stagesProp.DeleteArrayElementAtIndex(j);
                    break;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                if (stage != null)
                {
                    EditorGUILayout.PropertyField(prevSprite, new GUIContent("Ảnh xem trước (Preview):"));
                    EditorGUILayout.PropertyField(lockSprite, new GUIContent("Ảnh khóa (Locked Sprite):"));
                    EditorGUILayout.PropertyField(prefab, new GUIContent("Prefab 3D (Model):"));
                    EditorGUILayout.PropertyField(offset, new GUIContent("Health Bar Offset (Vị trí thanh máu):"));
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawUpgradesSection(SerializedProperty upgradesProp, int stagesCount)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"DANH SÁCH NÂNG CẤP ({upgradesProp.arraySize})", EditorStyles.boldLabel);
            if (GUILayout.Button("+ Thêm Cấp Nâng Cấp", GUILayout.Width(150)))
            {
                upgradesProp.arraySize++;
            }
            EditorGUILayout.EndHorizontal();

            for (int k = 0; k < upgradesProp.arraySize; k++)
            {
                SerializedProperty upgrade = upgradesProp.GetArrayElementAtIndex(k);
                SerializedProperty currencyType = upgrade.FindPropertyRelative("currencyType");
                SerializedProperty price = upgrade.FindPropertyRelative("price");
                SerializedProperty stats = upgrade.FindPropertyRelative("stats");
                SerializedProperty changeStage = upgrade.FindPropertyRelative("changeStage");
                SerializedProperty stageIndex = upgrade.FindPropertyRelative("stageIndex");

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Cấp độ Upgrade #{k + 1}", EditorStyles.boldLabel);
                GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f, 1f);
                if (GUILayout.Button("Xóa Cấp", GUILayout.Width(80)))
                {
                    upgradesProp.DeleteArrayElementAtIndex(k);
                    break;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(currencyType, new GUIContent("Loại tiền (Currency):"));
                EditorGUILayout.PropertyField(price, new GUIContent("Giá nâng cấp (Price):"));

                // Stats sub-fields
                if (stats != null)
                {
                    SerializedProperty health = stats.FindPropertyRelative("health");
                    SerializedProperty bulletDmg = stats.FindPropertyRelative("bulletDamageMultiplier");
                    SerializedProperty power = stats.FindPropertyRelative("power");
                    SerializedProperty keyUpgrade = stats.FindPropertyRelative("keyUpgradeNumber");

                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.LabelField("Chỉ Số (Stats):", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(health, new GUIContent("Máu (Health):"));
                    EditorGUILayout.PropertyField(bulletDmg, new GUIContent("Multiplier Sát Thương (Bullet Damage Multiplier):"));
                    EditorGUILayout.PropertyField(power, new GUIContent("Sức mạnh (Power):"));
                    EditorGUILayout.PropertyField(keyUpgrade, new GUIContent("Key Upgrade Number:"));
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.PropertyField(changeStage, new GUIContent("Thay đổi Stage ngoại hình?"));
                if (changeStage.boolValue)
                {
                    string[] stageLabels = new string[stagesCount];
                    for (int s = 0; s < stagesCount; s++) stageLabels[s] = $"Stage #{s + 1}";
                    stageIndex.intValue = EditorGUILayout.Popup("Stage chuyển đổi:", stageIndex.intValue, stageLabels);
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawEnumTool()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("TIỆN ÍCH: THÊM ĐỊNH DANH NHÂN VẬT VÀO CODE (ENUM)", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            newEnumName = EditorGUILayout.TextField("Tên Enum mới:", newEnumName);
            if (GUILayout.Button("Thêm vào Enum & Biên dịch", GUILayout.Width(200)))
            {
                AddNewCharacterTypeToEnum(newEnumName);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void AddNewCharacter()
        {
            charactersProperty.arraySize++;
            SerializedProperty newChar = charactersProperty.GetArrayElementAtIndex(charactersProperty.arraySize - 1);
            
            // Đặt các giá trị mặc định cho nhân vật mới tạo
            newChar.FindPropertyRelative("name").stringValue = "Nhân vật mới";
            newChar.FindPropertyRelative("type").enumValueIndex = 0;
            newChar.FindPropertyRelative("requiredLevel").intValue = 1;
            newChar.FindPropertyRelative("lockedSprite").objectReferenceValue = null;
            newChar.FindPropertyRelative("stages").arraySize = 0;
            newChar.FindPropertyRelative("upgrades").arraySize = 0;

            serializedDatabase.ApplyModifiedProperties();
            selectedIndex = charactersProperty.arraySize - 1;
        }

        private void DeleteSelectedCharacter()
        {
            if (selectedIndex >= 0 && selectedIndex < charactersProperty.arraySize)
            {
                if (EditorUtility.DisplayDialog("Xác nhận xóa", $"Bạn có muốn xóa nhân vật #{selectedIndex + 1}?", "Có", "Hủy"))
                {
                    charactersProperty.DeleteArrayElementAtIndex(selectedIndex);
                    serializedDatabase.ApplyModifiedProperties();
                    selectedIndex = Mathf.Clamp(selectedIndex - 1, 0, charactersProperty.arraySize - 1);
                }
            }
        }

        private void AddNewCharacterTypeToEnum(string enumName)
        {
            if (string.IsNullOrEmpty(enumName))
            {
                EditorUtility.DisplayDialog("Lỗi", "Tên định danh không được để trống!", "Ok");
                return;
            }

            // Chuẩn hóa tên enum
            enumName = Regex.Replace(enumName, @"[^a-zA-Z0-9_]", "");
            if (string.IsNullOrEmpty(enumName)) return;

            if (!File.Exists(ENUM_FILE_PATH))
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy file CharacterType.cs tại: " + ENUM_FILE_PATH, "Ok");
                return;
            }

            string content = File.ReadAllText(ENUM_FILE_PATH);

            if (content.Contains(enumName))
            {
                EditorUtility.DisplayDialog("Lỗi", $"Định danh '{enumName}' đã tồn tại trong enum rồi!", "Ok");
                return;
            }

            // Tìm vị trí đóng ngoặc nhọn cuối cùng của enum
            int lastBraceIndex = content.LastIndexOf('}');
            if (lastBraceIndex == -1) return;

            // Tìm dấu đóng ngoặc nhọn của enum
            int enumEndBraceIndex = content.LastIndexOf('}', lastBraceIndex - 1);
            if (enumEndBraceIndex == -1) return;

            // Đọc các dòng và tìm giá trị lớn nhất trong enum hiện tại để gán giá trị tiếp theo
            Regex regex = new Regex(@"(\w+)\s*=\s*(\d+)");
            var matches = regex.Matches(content);
            int maxVal = -1;
            foreach (Match match in matches)
            {
                if (int.TryParse(match.Groups[2].Value, out int val))
                {
                    if (val > maxVal) maxVal = val;
                }
            }

            int newVal = maxVal + 1;
            string insertString = $"\n        {enumName} = {newVal},";

            // Chèn giá trị mới trước dấu đóng ngoặc nhọn của enum
            string newContent = content.Insert(enumEndBraceIndex, insertString);
            
            File.WriteAllText(ENUM_FILE_PATH, newContent);
            AssetDatabase.ImportAsset(ENUM_FILE_PATH);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Thành công", $"Đã thêm '{enumName} = {newVal}' vào file CharacterType.cs thành công và đang biên dịch lại!", "Ok");
        }
    }
}
