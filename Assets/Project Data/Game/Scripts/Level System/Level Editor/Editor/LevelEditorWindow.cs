#pragma warning disable 649

using UnityEngine;
using UnityEditor;
using System;
using Watermelon.LevelSystem;
using UnityEditorInternal;
using System.Collections.Generic;
using Unity.AI;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEditor.SceneManagement;

namespace Watermelon.SquadShooter
{
    public class LevelEditorWindow : LevelEditorBase
    {

        //Path variables need to be changed ----------------------------------------
        private const string GAME_SCENE_PATH = "Assets/Project Data/Game/Scenes/Game.unity";
        private const string EDITOR_SCENE_PATH = "Assets/Project Data/Game/Scenes/LevelEditor.unity";
        private static string EDITOR_SCENE_NAME = "LevelEditor";

        //Window configuration
        private const string TITLE = "Level Editor";
        private const float WINDOW_MIN_WIDTH = 600;
        private const float WINDOW_MIN_HEIGHT = 560;
        private const float WINDOW_MAX_WIDTH = 800;
        private const float WINDOW_MAX_HEIGHT = 1200;

        //Level database fields
        private const string WORLDS_PROPERTY_NAME = "worlds";
        private SerializedProperty worldsSerializedProperty;


        //TabHandler
        private TabHandler tabHandler;

        //sidebar
        private LevelRepresentation selectedLevelRepresentation;
        private const int SIDEBAR_WIDTH = 140;
        private const string OPEN_GAME_SCENE_LABEL = "Mở Scene \"Game\"";

        private const string REMOVE_SELECTION = "Bỏ chọn đối tượng";

        //rest of levels tab
        private const string OBJECT_MANAGEMENT = "Quản lý đối tượng:";
        private const string CLEAR_SCENE = "Xóa sạch Scene";
        private const string SAVE = "Lưu";
        private const string LOAD = "Tải";

        private const string ITEM_ASSIGNED = "Nút này sẽ sinh ra vật phẩm.";
        private const string TEST_LEVEL = "Chơi thử màn này";

        private const float ITEMS_BUTTON_MAX_WIDTH = 150;
        private const float ITEMS_BUTTON_SPACE = 10;
        private const float ITEMS_BUTTON_WIDTH = 110;
        private const float ITEMS_BUTTON_HEIGHT = 110;
        private GameObject tempPrefab;
        private int tempType;
        private GUIContent itemContent;
        private Vector2 levelItemsScrollVector;
        private float itemPosX;
        private float itemPosY;
        private Rect itemsRect;
        private Rect itemRect;
        private int itemsPerRow;
        private int rowCount;

        // NEW STUFF

        bool isDatabaseLoaded;
        int selectedWorldIndex;
        private int lastSelectedLevelIndex;
        SerializedProperty selectedWorldSerializedProperty;
        ReorderableList levelsList;
        SerializedObject worldSerializedObject;
        private bool isWorldLoaded;
        private GUIContent worldNumber;
        private GUIContent presetType;
        private GUIContent previewSprite;
        private const string LEVELS_PROPERTY_PATH = "levels";
        private const string PREVIEW_SPRITE_PROPERTY_PATH = "previewSprite";
        private const string MUSIC_PROPERTY_PATH = "uniqueWorldMusicClip";
        private const string WORLD_TYPE_PROPERTY_PATH = "worldType";
        private const string ITEMS_PROPERTY_PATH = "items";
        private const string ROOM_PRESETS_PROPERTY_PATH = "roomEnvPresets";
        private const string WORLD_CUSTOM_OBJECTS_PROPERTY_PATH = "worldCustomObjects";
        private const string PREFAB_PROPERTY_PATH = "prefab";
        private const string TYPE_PROPERTY_PATH = "type";
        private const string HASH_PROPERTY_PATH = "hash";
        SerializedProperty levelsProperty;
        SerializedProperty previewSpriteProperty;
        SerializedProperty musicProperty;
        SerializedProperty worldTypeProperty;
        SerializedProperty itemsProperty;
        SerializedProperty roomPresetsProperty;
        SerializedProperty worldCustomObjectsProperty;
        SerializedProperty exitPointPrefabProperty;
        CatchedEnemyRefs[] enemies;
        CatchedPrefabRefs[] chests;
        string[] toolbarTab = { "Vật cản", "Kẻ địch", "Môi trường" };
        int selectedToolbarTab = 0;
        private Rect itemsListWidthRect;
        int tempRoomTabIndex;
        GameSettings gameSettings;
        EnemiesDatabase enemiesDatabase;
        EnemyType[] enemyEnumValues;
        private Rect elementTypeRect;
        private Rect elementObjectRefRect;
        private Rect elementButtonRect;
        private List<int> invalidIndexesList;
        private SerializedProperty tempEnumProperty;
        private SerializedProperty tempPrefabRefProperty;
        private Color backupColor;
        private SerializedObject levelSettingsObject;
        private bool listElementDragged;
        private ReorderableList itemsReordableList;
        private float currentItemListWidth;

        // ── NÂNG CẤP: Styles (khởi tạo 1 lần) ──────────────────────────────
        private GUIStyle _boxStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _labelBoldStyle;
        private GUIStyle _warningStyle;
        private GUIStyle _errorStyle;
        private GUIStyle _okStyle;
        private bool _stylesReady;
        private bool _showRoomToolsGroup = false;

        private void EnsureUpgradeStyles()
        {
            if (_stylesReady) return;
            _boxStyle = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(8, 8, 6, 6) };
            _headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 11 };
            _labelBoldStyle = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
            _warningStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = new Color(0.9f, 0.65f, 0f) }, fontStyle = FontStyle.Bold };
            _errorStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = new Color(0.85f, 0.2f, 0.2f) }, fontStyle = FontStyle.Bold };
            _okStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = new Color(0.2f, 0.75f, 0.3f) }, fontStyle = FontStyle.Bold };
            _stylesReady = true;
        }

        // ── NÂNG CẤP 1: Room Stats ──────────────────────────────────────────
        private bool _showStats = false;
        private void DrawRoomStatsDashboard()
        {
            if (selectedLevelRepresentation == null) return;
            if (selectedLevelRepresentation.selectedRoomindex < 0) return;
            if (selectedLevelRepresentation.enemyEntitiesProperty == null) return;
            EnsureUpgradeStyles();

            int enemyCount = selectedLevelRepresentation.enemyEntitiesProperty.arraySize;
            int eliteCount = 0;
            int chestCount = selectedLevelRepresentation.chestEntitiesProperty != null
                ? selectedLevelRepresentation.chestEntitiesProperty.arraySize : 0;
            int itemCount = selectedLevelRepresentation.itemEntitiesProperty != null
                ? selectedLevelRepresentation.itemEntitiesProperty.arraySize : 0;
            for (int i = 0; i < enemyCount; i++)
            {
                var elem = selectedLevelRepresentation.enemyEntitiesProperty.GetArrayElementAtIndex(i);
                if (elem.FindPropertyRelative("IsElite") != null && elem.FindPropertyRelative("IsElite").boolValue)
                    eliteCount++;
            }
            int normalCount = enemyCount - eliteCount;

            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.BeginHorizontal();
            // Header luôn hiện kèm tóm tắt nhanh
            EditorGUILayout.LabelField($"📊  Thống Kê Phòng  |  Địch: {enemyCount}  Rương: {chestCount}", _headerStyle, GUILayout.ExpandWidth(true));
            if (GUILayout.Button(_showStats ? "▲" : "▼", GUILayout.Width(28)))
                _showStats = !_showStats;
            EditorGUILayout.EndHorizontal();

            if (_showStats)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"👾 Địch: {enemyCount}  (Thường: {normalCount}  Elite: {eliteCount})", GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField($"📦 Rương: {chestCount}  🧱 Vật cản: {itemCount}", GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();

                if (enemyCount == 0)
                    EditorGUILayout.LabelField("⚠️  Phòng không có kẻ địch!", _warningStyle);
                else if (enemyCount > 20)
                    EditorGUILayout.LabelField($"⚠️  Phòng có nhiều địch ({enemyCount} > 20) — có thể gây giật lag!", _warningStyle);
                else
                    EditorGUILayout.LabelField($"✅  Số lượng địch hợp lý.", _okStyle);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        // ── NÂNG CẤP 2: Global Validation ───────────────────────────────────
        private bool _showValidationPanel;
        private List<string> _validationErrors = new List<string>();
        private List<string> _validationWarnings = new List<string>();

        private void DrawGlobalValidationPanel()
        {
            EnsureUpgradeStyles();
            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("🔍  Kiểm Tra Toàn Bộ (Global Validation)", _headerStyle, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Quét", GUILayout.Width(60)))
                RunGlobalValidation();
            if (GUILayout.Button(_showValidationPanel ? "▲" : "▼", GUILayout.Width(28)))
                _showValidationPanel = !_showValidationPanel;
            EditorGUILayout.EndHorizontal();

            if (_showValidationPanel)
            {
                if (_validationErrors.Count == 0 && _validationWarnings.Count == 0)
                {
                    EditorGUILayout.LabelField("✅  Tất cả Level đều hợp lệ!", _okStyle);
                }
                else
                {
                    foreach (var err in _validationErrors)
                        EditorGUILayout.LabelField("❌  " + err, _errorStyle);
                    foreach (var warn in _validationWarnings)
                        EditorGUILayout.LabelField("⚠️  " + warn, _warningStyle);
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        private void RunGlobalValidation()
        {
            _validationErrors.Clear();
            _validationWarnings.Clear();
            if (levelsProperty == null) { _validationErrors.Add("Chưa load World!"); _showValidationPanel = true; return; }

            for (int lvl = 0; lvl < levelsProperty.arraySize; lvl++)
            {
                var levelProp = levelsProperty.GetArrayElementAtIndex(lvl);
                var roomsProp = levelProp.FindPropertyRelative("rooms");
                if (roomsProp == null) continue;

                if (roomsProp.arraySize == 0)
                    _validationWarnings.Add($"Level #{lvl + 1}: Không có phòng nào.");

                for (int r = 0; r < roomsProp.arraySize; r++)
                {
                    var room = roomsProp.GetArrayElementAtIndex(r);
                    var enemProp = room.FindPropertyRelative("enemyEntities");
                    if (enemProp != null && enemProp.arraySize == 0)
                        _validationWarnings.Add($"Level #{lvl + 1} – Phòng #{r + 1}: Không có kẻ địch.");
                    if (enemProp != null && enemProp.arraySize > 25)
                        _validationWarnings.Add($"Level #{lvl + 1} – Phòng #{r + 1}: Quá nhiều địch ({enemProp.arraySize})!");
                }
            }
            _showValidationPanel = true;
        }

        // ── NÂNG CẤP 4: Difficulty Meter ────────────────────────────────────
        private bool _showDifficulty = false;
        private void DrawDifficultyMeter()
        {
            if (selectedLevelRepresentation == null) return;
            if (selectedLevelRepresentation.enemyEntitiesProperty == null) return;
            EnsureUpgradeStyles();

            int enemyCount = selectedLevelRepresentation.enemyEntitiesProperty.arraySize;
            int eliteCount = 0;
            for (int i = 0; i < enemyCount; i++)
            {
                var e = selectedLevelRepresentation.enemyEntitiesProperty.GetArrayElementAtIndex(i);
                if (e.FindPropertyRelative("IsElite") != null && e.FindPropertyRelative("IsElite").boolValue)
                    eliteCount++;
            }
            float rawScore = (enemyCount - eliteCount) * 1f + eliteCount * 2.5f;
            float difficulty = Mathf.Clamp01(rawScore / 30f);

            string label;
            Color barColor;
            if (difficulty < 0.3f)       { label = "De";        barColor = new Color(0.2f, 0.8f, 0.3f); }
            else if (difficulty < 0.6f)  { label = "TB";        barColor = new Color(0.9f, 0.75f, 0.1f); }
            else if (difficulty < 0.85f) { label = "Kho";       barColor = new Color(0.9f, 0.45f, 0.1f); }
            else                         { label = "Cuc kho";   barColor = new Color(0.85f, 0.1f, 0.1f); }

            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Do Kho: {label} ({Mathf.RoundToInt(difficulty * 100)}%)", _headerStyle, GUILayout.ExpandWidth(true));
            if (GUILayout.Button(_showDifficulty ? "▲" : "▼", GUILayout.Width(28)))
                _showDifficulty = !_showDifficulty;
            EditorGUILayout.EndHorizontal();

            if (_showDifficulty)
            {
                Rect barBg = GUILayoutUtility.GetRect(0, 14, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(barBg, new Color(0.15f, 0.15f, 0.15f));
                Rect barFill = new Rect(barBg.x, barBg.y, barBg.width * difficulty, barBg.height);
                EditorGUI.DrawRect(barFill, barColor);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        // ── NÂNG CẤP 5: Bulk Actions ────────────────────────────────────────
        private bool _showBulkActions;
        private float _scalePercent = 20f;
        private int  _bulkEnemyLevel = 1;

        private void DrawBulkActionsPanel()
        {
            if (selectedLevelRepresentation == null) return;
            if (selectedLevelRepresentation.selectedRoomindex < 0) return;
            EnsureUpgradeStyles();

            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("⚡  Thao Tác Hàng Loạt (Bulk Actions)", _headerStyle, GUILayout.ExpandWidth(true));
            if (GUILayout.Button(_showBulkActions ? "▲" : "▼", GUILayout.Width(28)))
                _showBulkActions = !_showBulkActions;
            EditorGUILayout.EndHorizontal();

            if (_showBulkActions)
            {
                EditorGUILayout.Space(2);

                // --- Nhân bản phòng ---
                EditorGUILayout.LabelField("Nhân bản phòng:", _labelBoldStyle);
                if (GUILayout.Button("📋  Nhân bản phòng hiện tại thành phòng mới"))
                    BulkDuplicateRoom();

                EditorGUILayout.Space(4);

                // --- Scale độ khó ---
                EditorGUILayout.LabelField("Scale độ khó địch trong phòng:", _labelBoldStyle);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Tỉ lệ (%):", GUILayout.Width(70));
                _scalePercent = EditorGUILayout.Slider(_scalePercent, -80f, 200f);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("📈  Tăng Elite Count"))
                    BulkScaleElite(true);
                if (GUILayout.Button("📉  Giảm Elite Count"))
                    BulkScaleElite(false);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(4);

                // --- Set enemy level hàng loạt ---
                EditorGUILayout.LabelField("Đặt cấp địch (Level) cho toàn Level:", _labelBoldStyle);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Cấp:", GUILayout.Width(40));
                _bulkEnemyLevel = EditorGUILayout.IntSlider(_bulkEnemyLevel, 1, 30);
                if (GUILayout.Button("Áp dụng", GUILayout.Width(80)))
                    BulkSetEnemyLevel(_bulkEnemyLevel);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        private void BulkDuplicateRoom()
        {
            if (selectedLevelRepresentation == null || selectedLevelRepresentation.selectedRoomindex < 0) return;
            SaveRoom();
            int srcIdx = selectedLevelRepresentation.selectedRoomindex;
            selectedLevelRepresentation.roomsProperty.arraySize++;
            int newIdx = selectedLevelRepresentation.roomsProperty.arraySize - 1;
            selectedLevelRepresentation.roomTabs.Add("Room #" + (newIdx + 1));

            // Copy dữ liệu từ phòng nguồn
            SerializedProperty src = selectedLevelRepresentation.roomsProperty.GetArrayElementAtIndex(srcIdx);
            SerializedProperty dst = selectedLevelRepresentation.roomsProperty.GetArrayElementAtIndex(newIdx);

            // spawnPoint
            dst.FindPropertyRelative("spawnPoint").vector3Value = src.FindPropertyRelative("spawnPoint").vector3Value;

            // enemyEntities
            SerializedProperty srcEnemies = src.FindPropertyRelative("enemyEntities");
            SerializedProperty dstEnemies = dst.FindPropertyRelative("enemyEntities");
            dstEnemies.arraySize = srcEnemies.arraySize;
            for (int i = 0; i < srcEnemies.arraySize; i++)
            {
                var se = srcEnemies.GetArrayElementAtIndex(i);
                var de = dstEnemies.GetArrayElementAtIndex(i);
                de.FindPropertyRelative("EnemyType").enumValueIndex = se.FindPropertyRelative("EnemyType").enumValueIndex;
                de.FindPropertyRelative("Position").vector3Value    = se.FindPropertyRelative("Position").vector3Value;
                de.FindPropertyRelative("Rotation").quaternionValue = se.FindPropertyRelative("Rotation").quaternionValue;
                de.FindPropertyRelative("Scale").vector3Value       = se.FindPropertyRelative("Scale").vector3Value;
                de.FindPropertyRelative("IsElite").boolValue        = se.FindPropertyRelative("IsElite").boolValue;
                var sp = se.FindPropertyRelative("PathPoints");
                var dp = de.FindPropertyRelative("PathPoints");
                dp.arraySize = sp.arraySize;
                for (int j = 0; j < sp.arraySize; j++)
                    dp.GetArrayElementAtIndex(j).vector3Value = sp.GetArrayElementAtIndex(j).vector3Value;
            }

            // itemEntities
            SerializedProperty srcItems = src.FindPropertyRelative("itemEntities");
            SerializedProperty dstItems = dst.FindPropertyRelative("itemEntities");
            dstItems.arraySize = srcItems.arraySize;
            for (int i = 0; i < srcItems.arraySize; i++)
            {
                var si = srcItems.GetArrayElementAtIndex(i);
                var di = dstItems.GetArrayElementAtIndex(i);
                di.FindPropertyRelative("Hash").intValue            = si.FindPropertyRelative("Hash").intValue;
                di.FindPropertyRelative("Position").vector3Value    = si.FindPropertyRelative("Position").vector3Value;
                di.FindPropertyRelative("Rotation").quaternionValue = si.FindPropertyRelative("Rotation").quaternionValue;
                di.FindPropertyRelative("Scale").vector3Value       = si.FindPropertyRelative("Scale").vector3Value;
            }

            worldSerializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();

            selectedLevelRepresentation.selectedRoomindex = newIdx;
            LoadRoom();
            Debug.Log($"[Level Editor] Đã nhân bản phòng #{srcIdx + 1} → Phòng #{newIdx + 1}");
        }

        private void BulkScaleElite(bool increase)
        {
            if (selectedLevelRepresentation?.enemyEntitiesProperty == null) return;
            for (int i = 0; i < selectedLevelRepresentation.enemyEntitiesProperty.arraySize; i++)
            {
                var e = selectedLevelRepresentation.enemyEntitiesProperty.GetArrayElementAtIndex(i);
                var isEliteProp = e.FindPropertyRelative("IsElite");
                if (isEliteProp == null) continue;
                isEliteProp.boolValue = increase;
            }
            worldSerializedObject.ApplyModifiedProperties();
            LoadRoom();
            Debug.Log($"[Level Editor] Đã {(increase ? "bật" : "tắt")} Elite cho toàn bộ địch trong phòng.");
        }

        private void BulkSetEnemyLevel(int level)
        {
            if (selectedLevelRepresentation?.enemiesLevelProperty == null) return;
            selectedLevelRepresentation.enemiesLevelProperty.intValue = level;
            worldSerializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log($"[Level Editor] Đã đặt cấp địch = {level} cho Level này.");
        }

        // ── NÂNG CẤP 6: Room Snapshot ───────────────────────────────────────
        private struct RoomSnapshot
        {
            public int enemyCount;
            public int itemCount;
            public int chestCount;
            public string label;
        }
        private RoomSnapshot? _snapshot;

        private void DrawSnapshotPanel()
        {
            if (selectedLevelRepresentation == null) return;
            if (selectedLevelRepresentation.selectedRoomindex < 0) return;
            EnsureUpgradeStyles();

            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.LabelField("📸  Snapshot Phòng", _headerStyle);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Chụp snapshot"))
            {
                _snapshot = new RoomSnapshot
                {
                    enemyCount = selectedLevelRepresentation.enemyEntitiesProperty?.arraySize ?? 0,
                    itemCount  = selectedLevelRepresentation.itemEntitiesProperty?.arraySize ?? 0,
                    chestCount = selectedLevelRepresentation.chestEntitiesProperty?.arraySize ?? 0,
                    label      = $"Phòng #{selectedLevelRepresentation.selectedRoomindex + 1} – {System.DateTime.Now:HH:mm:ss}"
                };
                Debug.Log($"[Level Editor] Đã chụp snapshot: {_snapshot.Value.label}");
            }

            EditorGUI.BeginDisabledGroup(_snapshot == null);
            if (GUILayout.Button("Xem thông tin snapshot"))
            {
                var s = _snapshot.Value;
                EditorUtility.DisplayDialog("Snapshot: " + s.label,
                    $"Địch: {s.enemyCount}\nVật cản: {s.itemCount}\nRương: {s.chestCount}", "OK");
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            if (_snapshot != null)
                EditorGUILayout.LabelField($"Snapshot gần nhất: {_snapshot.Value.label}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        protected override string LEVELS_FOLDER_NAME => "Worlds";

        protected override string LEVELS_DATABASE_FOLDER_PATH => "Assets/Project Data/Content/Data/Level System";

        public static void CreateLevelEditorWindow(GameSettings gameSettings, EnemiesDatabase enemiesDatabase)
        {
            window = GetWindow(typeof(LevelEditorWindow));
            window.titleContent = new GUIContent(DEFAULT_LEVEL_EDITOR_TITLE);
            window.minSize = new Vector2(DEFAULT_WINDOW_MIN_SIZE, DEFAULT_WINDOW_MIN_SIZE);
            window.Show();
            ((LevelEditorWindow)window).SetUpDatabases(gameSettings, enemiesDatabase);
        }

        protected override WindowConfiguration SetUpWindowConfiguration(WindowConfiguration.Builder builder)
        {
            builder.KeepWindowOpenOnScriptReload(true);
            builder.SetWindowMinSize(new Vector2(WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT));
            return builder.Build();
        }

        protected override Type GetLevelsDatabaseType()
        {
            return typeof(LevelsDatabase);
        }

        public override Type GetLevelType()
        {
            return typeof(LevelData);
        }

        protected override void ReadLevelDatabaseFields()
        {
            worldsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(WORLDS_PROPERTY_NAME);
            isDatabaseLoaded = true;

        }

        protected override void InitialiseVariables()
        {
            gameSettings = AssetDatabase.LoadAssetAtPath<GameSettings>("Assets/Project Data/Content/Data/Game Settings.asset");
            enemiesDatabase = AssetDatabase.LoadAssetAtPath<EnemiesDatabase>("Assets/Project Data/Content/Data/Enemies/Enemies Database.asset");
            CollectDataFromLevelsSettings();
            CollectDataFromEnemiesDatabase();
            selectedWorldIndex = 0;
            
            OpenWorld();


            tabHandler = new TabHandler();
            tabHandler.AddTab(new TabHandler.Tab("Tạo màn chơi (Levels Creation)", DisplayLevelsCreationTab));
            tabHandler.AddTab(new TabHandler.Tab("Cài đặt thế giới (World Settings)", DisplayWorldSettingsTab, InitStuffForWorldSettingsTab));

            previewSprite = new GUIContent("Ảnh xem trước (Preview Sprite):");
            presetType = new GUIContent("Loại Preset (Preset Type):");

            PrepareStyles();
            RemoveUnnesesaryComponensFromPrefabs();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (EditorSceneManager.GetActiveScene().name != EDITOR_SCENE_NAME)
            {
                return;
            }

            if (change != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }

            if (levelsList.index == -1)
            {
                OpenScene(GAME_SCENE_PATH);
            }
            else
            {
                RewriteSave(selectedWorldIndex, levelsList.index);
            }
        }

        private void RemoveUnnesesaryComponensFromPrefabs()
        {
            GameObject temp;
            LevelEditorItem[] itemComponents;
            LevelEditorEnemy[] enemyComponents;

            for (int i = 0; i < itemsProperty.arraySize; i++)
            {
                temp = itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue as GameObject;
                itemComponents = temp.GetComponentsInChildren<LevelEditorItem>();

                if(itemComponents.Length > 0)
                {
                    string assetPath = AssetDatabase.GetAssetPath(temp);

                    // Load the contents of the Prefab Asset.
                    GameObject contentsRoot = PrefabUtility.LoadPrefabContents(assetPath);

                    // Modify Prefab contents.
                    itemComponents = contentsRoot.GetComponentsInChildren<LevelEditorItem>();

                    for (int j = itemComponents.Length - 1; j >= 0; j--)
                    {
                        GameObject.DestroyImmediate(itemComponents[j]);
                    }

                    // Save contents back to Prefab Asset and unload contents.
                    PrefabUtility.SaveAsPrefabAsset(contentsRoot, assetPath);
                    PrefabUtility.UnloadPrefabContents(contentsRoot);

                    Debug.LogWarning($"Some unnesesary componens of type \"LevelEditorItem\" were removed from prefab \"{AssetDatabase.GetAssetPath(temp)}\" to avoid causing bugs with duplication in level editor.");
                }
            }

            for (int i = 0; i < enemies.Length; i++)
            {
                temp = enemies[i].prefabRef as GameObject;
                enemyComponents = temp.GetComponentsInChildren<LevelEditorEnemy>();

                if(enemyComponents.Length > 0)
                {
                    string assetPath = AssetDatabase.GetAssetPath(temp);

                    // Load the contents of the Prefab Asset.
                    GameObject contentsRoot = PrefabUtility.LoadPrefabContents(assetPath);

                    // Modify Prefab contents.
                    enemyComponents = contentsRoot.GetComponentsInChildren<LevelEditorEnemy>();

                    for (int j = enemyComponents.Length - 1; j >= 0; j--)
                    {
                        GameObject.DestroyImmediate(enemyComponents[j]);
                    }

                    // Save contents back to Prefab Asset and unload contents.
                    PrefabUtility.SaveAsPrefabAsset(contentsRoot, assetPath);
                    PrefabUtility.UnloadPrefabContents(contentsRoot);

                    Debug.LogWarning($"Some unnesesary componens of type \"LevelEditorEnemy\" were removed from prefab \"{AssetDatabase.GetAssetPath(temp)}\" to avoid causing bugs with duplication in level editor.");
                }
            }
        }

        public void SetUpDatabases(GameSettings gameSettings, EnemiesDatabase enemiesDatabase)
        {
            this.gameSettings = gameSettings;
            this.enemiesDatabase = enemiesDatabase;
            CollectDataFromLevelsSettings();
            CollectDataFromEnemiesDatabase();
            selectedWorldIndex = 0;
            OpenWorld();
            levelsList.index = 0;
            selectedLevelRepresentation = new LevelRepresentation(levelsProperty.GetArrayElementAtIndex(0));
            LoadRoom();


        }

        private void OnDestroy()
        {
            SaveLevelIfPosssible();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void DisplayLevelFields()
        {
            EditorGUILayout.PropertyField(selectedLevelRepresentation.levelTypeProperty, new GUIContent("Loại màn chơi (Level Type)"));
            EditorGUILayout.PropertyField(selectedLevelRepresentation.xpAmountProperty, new GUIContent("Điểm XP nhận được (XP Amount)"));
            EditorGUILayout.PropertyField(selectedLevelRepresentation.requiredUpgProperty, new GUIContent("Cấp độ súng yêu cầu (Required Upgrade)"));
            EditorGUILayout.PropertyField(selectedLevelRepresentation.enemiesLevelProperty, new GUIContent("Cấp độ kẻ địch (Enemies Level)"));
            EditorGUILayout.PropertyField(selectedLevelRepresentation.hasCharacterSuggestionProperty, new GUIContent("Có gợi ý nhân vật"));
            EditorGUILayout.PropertyField(selectedLevelRepresentation.healSpawnPercentProperty, new GUIContent("Tỷ lệ rơi bình máu (%)"));
            EditorGUILayout.PropertyField(selectedLevelRepresentation.dropDataProperty, new GUIContent("Vật phẩm rơi ra (Drop Data)"));
            EditorGUILayout.PropertyField(selectedLevelRepresentation.specialBehavioursProperty, new GUIContent("Hành vi đặc biệt"));
        }

        private void CollectDataFromLevelsSettings()
        {
            if (gameSettings == null)
            {
                Debug.LogError("Game settings file is null");
            }
            levelSettingsObject = new SerializedObject(gameSettings);

            //SerializedProperty element;

            //levels database
            levelsDatabase = levelSettingsObject.FindProperty("levelsDatabase").objectReferenceValue;
            levelsDatabaseSerializedObject = new SerializedObject(levelsDatabase);
            ReadLevelDatabaseFields();

            //exit exitPoints
            exitPointPrefabProperty = levelSettingsObject.FindProperty("exitPointPrefab");


            //Chest
            var chestsDataProperty = levelSettingsObject.FindProperty("chestData");
            chests = new CatchedPrefabRefs[chestsDataProperty.arraySize];

            for (int i = 0; i < chestsDataProperty.arraySize; i++)
            {
                var chestProp = chestsDataProperty.GetArrayElementAtIndex(i);
                var chestRefs = new CatchedPrefabRefs();

                chestRefs.prefabRef = chestProp.FindPropertyRelative("prefab").objectReferenceValue;
                chestRefs.typeEnumValueIndex = chestProp.FindPropertyRelative("type").intValue;

                chests[i] = chestRefs;

            }
        }

        private void CollectDataFromEnemiesDatabase()
        {
            if (enemiesDatabase == null)
            {
                Debug.LogError("enemiesDatabase database is null");
            }

            SerializedObject enemiesDatabaseObject = new SerializedObject(enemiesDatabase);
            SerializedProperty element;

            SerializedProperty enemiesProperty = enemiesDatabaseObject.FindProperty("enemies");
            enemies = new CatchedEnemyRefs[enemiesProperty.arraySize];

            enemyEnumValues = (EnemyType[])Enum.GetValues(typeof(EnemyType));

            for (int i = 0; i < enemiesProperty.arraySize; i++)
            {
                element = enemiesProperty.GetArrayElementAtIndex(i);
                enemies[i] = new CatchedEnemyRefs();
                enemies[i].prefabRef = element.FindPropertyRelative("prefab").objectReferenceValue;
                enemies[i].typeEnumValueIndex = element.FindPropertyRelative("enemyType").enumValueIndex;
                enemies[i].enemyType = enemyEnumValues[enemies[i].typeEnumValueIndex];
                enemies[i].image = element.FindPropertyRelative("icon").objectReferenceValue as Texture2D;

            }
        }

        public int ConvertToEnumIndex(int enumValueIndex)
        {
            EnemyType[] values = (EnemyType[])Enum.GetValues(typeof(EnemyType));
            return (int)values[enumValueIndex];

        }

        private void OpenWorld()
        {
            SaveLevelIfPosssible();
            selectedLevelRepresentation = null;

            if(EditorSceneController.Instance != null)
            {
                EditorSceneController.Instance.Clear();
                EditorSceneController.Instance.ClearWorldCustomObjectsContainer();
                EditorSceneController.Instance.ClearRoomCustomObjectsContainer();
                EditorSceneController.Instance.UpdateContainerLabel(-1);
            }
            
            worldNumber = new GUIContent("World #" + (selectedWorldIndex + 1));
            selectedWorldSerializedProperty = worldsSerializedProperty.GetArrayElementAtIndex(selectedWorldIndex);
            isWorldLoaded = selectedWorldSerializedProperty.objectReferenceValue != null;

            if (!isWorldLoaded)
                return;

            worldSerializedObject = new SerializedObject(selectedWorldSerializedProperty.objectReferenceValue);

            lastSelectedLevelIndex = -1;
            previewSpriteProperty = worldSerializedObject.FindProperty(PREVIEW_SPRITE_PROPERTY_PATH);
            musicProperty = worldSerializedObject.FindProperty(MUSIC_PROPERTY_PATH);
            worldTypeProperty = worldSerializedObject.FindProperty(WORLD_TYPE_PROPERTY_PATH);
            levelsProperty = worldSerializedObject.FindProperty(LEVELS_PROPERTY_PATH);
            itemsProperty = worldSerializedObject.FindProperty(ITEMS_PROPERTY_PATH);
            roomPresetsProperty = worldSerializedObject.FindProperty(ROOM_PRESETS_PROPERTY_PATH);
            worldCustomObjectsProperty = worldSerializedObject.FindProperty(WORLD_CUSTOM_OBJECTS_PROPERTY_PATH);
            SpawnWorldCustomObjects();

            levelsList = new ReorderableList(worldSerializedObject, levelsProperty, true, true, true, true);
            levelsList.onRemoveCallback = RemoveCallback;
            levelsList.drawHeaderCallback = HeaderCallback;
            levelsList.drawElementCallback = ElementCallback;
            levelsList.onSelectCallback = LevelSelectedCallback;
            levelsList.onAddCallback = AddCallback;
            levelsList.onMouseDragCallback = DragCallback;
            levelsList.onReorderCallback = ReorderCallback;
            levelsList.onMouseUpCallback = MouseUpCallback;
        }

        private void MouseUpCallback(ReorderableList list)
        {
            listElementDragged = false;
        }

        private void ReorderCallback(ReorderableList list)
        {
            listElementDragged = true;
            lastSelectedLevelIndex = list.index;
            SaveLevelIfPosssible();
            selectedLevelRepresentation = new LevelRepresentation(levelsProperty.GetArrayElementAtIndex(list.index));
            LoadRoom();
        }

        private void DragCallback(ReorderableList list)
        {
            listElementDragged = true;
        }

        private void AddCallback(ReorderableList list)
        {
            levelsProperty.arraySize++;
            new LevelRepresentation(levelsProperty.GetArrayElementAtIndex(levelsProperty.arraySize - 1)).Clear();
            worldSerializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            levelsList.Select(levelsProperty.arraySize - 1);
            LevelSelectedCallback(list);
        }

        private void ElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (levelsProperty.GetArrayElementAtIndex(index).FindPropertyRelative("rooms").arraySize == 0)
            {
                GUI.Label(rect, $"Level #{index + 1} | [Empty]");
            }
            else
            {
                GUI.Label(rect, "Level #" + (index + 1));
            }

        }

        private void RemoveCallback(ReorderableList list)
        {
            if (EditorUtility.DisplayDialog("Cảnh báo", "Bạn có chắc chắn muốn xóa level #" + (list.index + 1) + " không?", "Có", "Hủy"))
            {
                levelsProperty.DeleteArrayElementAtIndex(levelsList.index);
                worldSerializedObject.ApplyModifiedProperties();
                selectedLevelRepresentation = null;
                AssetDatabase.SaveAssets();
            }
        }

        private void HeaderCallback(Rect rect)
        {
            GUI.Label(rect, "Số lượng level: " + levelsProperty.arraySize);
        }


        private void LevelSelectedCallback(ReorderableList list)
        {
            if(lastSelectedLevelIndex == list.index)
            {
                return;
            }

            if (listElementDragged)
            {
                return;
            }

            lastSelectedLevelIndex = list.index;
            SaveLevelIfPosssible();
            selectedLevelRepresentation = new LevelRepresentation(levelsProperty.GetArrayElementAtIndex(list.index));
            LoadRoom();
        }

        protected void PrepareStyles()
        {
            if (tabHandler != null)
            {
                tabHandler.SetDefaultToolbarStyle();
            }
        }

        #region unusedStuff
        public override void OpenLevel(UnityEngine.Object levelObject, int index)
        {
        }

        public override string GetLevelLabel(UnityEngine.Object levelObject, int index)
        {
            return string.Empty;
        }

        public override void ClearLevel(UnityEngine.Object levelObject)
        {
        }



        #endregion




        protected override void DrawContent()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != EDITOR_SCENE_NAME)
            {
                DrawOpenEditorScene();
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                RemoveSelection();
                EditorGUILayout.HelpBox("Level editor không hỗ trợ trong Play mode.", MessageType.Error, true);

                if (GUILayout.Button("Thoát play mode"))
                {
                    EditorApplication.ExitPlaymode();
                }
                
                return;
            }

            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(400));
            DisplayDatabaseRef();

            if (!isDatabaseLoaded)
                return;

            DisplayArea();

            EditorGUILayout.EndVertical();
            tabHandler.DisplayTab();
        }





        private void DrawOpenEditorScene()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox("Cần mở scene " + EDITOR_SCENE_NAME + " để sử dụng Level Editor.", MessageType.Error, true);

            if (GUILayout.Button("Mở scene \"" + EDITOR_SCENE_NAME + "\""))
            {
                OpenScene(EDITOR_SCENE_PATH);
            }

            EditorGUILayout.EndVertical();
        }

        private void DisplayDatabaseRef()
        {

            EditorGUI.BeginChangeCheck();
            gameSettings = EditorGUILayout.ObjectField("Game Settings: ", gameSettings, typeof(GameSettings), false) as GameSettings;

            if (EditorGUI.EndChangeCheck())
            {
                CollectDataFromLevelsSettings();
                OpenWorld();
            }

            EditorGUI.BeginChangeCheck();
            enemiesDatabase = EditorGUILayout.ObjectField("Enemies database: ", enemiesDatabase, typeof(EnemiesDatabase), false) as EnemiesDatabase;

            if (EditorGUI.EndChangeCheck())
            {
                CollectDataFromEnemiesDatabase();
                OpenWorld();
            }


        }

        private void DisplayArea()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(selectedWorldIndex == 0);

            if (GUILayout.Button("◀", GUILayout.MaxWidth(30)))
            {
                selectedWorldIndex--;
                OpenWorld();
            }

            EditorGUI.EndDisabledGroup();


            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(selectedWorldSerializedProperty, worldNumber);

            if (EditorGUI.EndChangeCheck())
            {
                levelsDatabaseSerializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                OpenWorld();
            }



            EditorGUI.BeginDisabledGroup(selectedWorldIndex == worldsSerializedProperty.arraySize - 1);

            if (GUILayout.Button("▶", GUILayout.MaxWidth(30)))
            {
                selectedWorldIndex++;
                OpenWorld();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void DisplayLevelsCreationTab()
        {
            if (!isWorldLoaded)
                return;

            // ── NÂNG CẤP 2: Global Validation luôn hiển thị trên cùng tab ──
            DrawGlobalValidationPanel();

            EditorGUILayout.BeginHorizontal();
            //sidebar 
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(SIDEBAR_WIDTH));
            levelsList.DoLayoutList();
            DisplaySidebarButtons();
            EditorGUILayout.EndVertical();

            GUILayout.Space(8);

            //level content
            EditorGUILayout.BeginVertical(GUI.skin.box);
            DisplaySelectedLevel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DisplaySidebarButtons()
        {
            if (GUILayout.Button(REMOVE_SELECTION, WatermelonEditor.Styles.button_01))
            {
                RemoveSelection();
            }

            if (GUILayout.Button(OPEN_GAME_SCENE_LABEL, WatermelonEditor.Styles.button_01))
            {
                RemoveSelection();
                OpenScene(GAME_SCENE_PATH);
            }
        }

        private void RemoveSelection()
        {
            SaveLevelIfPosssible();
            selectedLevelRepresentation = null;
            levelsList.index = -1;
            ClearScene();
        }

        private static void ClearScene()
        {
            EditorSceneController.Instance.Clear();
        }


        private void DisplaySelectedLevel()
        {
            if (selectedLevelRepresentation == null)
            {
                return;
            }

            if (GUILayout.Button(TEST_LEVEL, WatermelonEditor.Styles.button_01, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                RewriteSave(selectedWorldIndex, levelsList.index);
            }

            DisplayRoomSection();

            EditorGUILayout.Space();

            if (selectedLevelRepresentation.selectedRoomindex != -1)
            {
                // ── NÂNG CẤP: Group toàn bộ công cụ mới vào 1 Collapse lớn để tránh chật màn hình ──
                EnsureUpgradeStyles();
                EditorGUILayout.BeginVertical(_boxStyle);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("🔧  Phân Tích & Hỗ Trợ Phòng", _headerStyle, GUILayout.ExpandWidth(true));
                if (GUILayout.Button(_showRoomToolsGroup ? "▲ Thu gọn" : "▼ Mở rộng", GUILayout.Width(80)))
                    _showRoomToolsGroup = !_showRoomToolsGroup;
                EditorGUILayout.EndHorizontal();

                if (_showRoomToolsGroup)
                {
                    EditorGUILayout.Space(4);
                    DrawRoomStatsDashboard();
                    DrawDifficultyMeter();
                    DrawBulkActionsPanel();
                    DrawSnapshotPanel();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
                // ──────────────────────────────────────────────────────────

                DisplayToolbar();
                EditorGUILayout.Space();
            }

            DisplyLevelObjectMenagementSection();
            EditorGUILayout.Space();
        }

        private void RewriteSave(int worldIndex, int levelIndex)
        {
            GlobalSave tempSave = SaveController.GetGlobalSave();

            LevelSave levelSave = tempSave.GetSaveObject<LevelSave>("level");
            levelSave.LevelIndex = levelIndex;
            levelSave.WorldIndex = worldIndex;
            tempSave.Flush();

            SaveController.SaveCustom(tempSave);
            SaveLevelIfPosssible();
            OpenScene(GAME_SCENE_PATH);
            EditorApplication.isPlaying = true;
        }

        private void DisplayRoomSection()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Toggle(selectedLevelRepresentation.selectedRoomindex == -1, "Cài đặt", GUI.skin.button))
            {
                SaveRoom();
                selectedLevelRepresentation.selectedRoomindex = -1;
                LoadRoom();
            }

            tempRoomTabIndex = GUILayout.Toolbar(selectedLevelRepresentation.selectedRoomindex, selectedLevelRepresentation.roomTabs.ToArray());

            if (GUILayout.Button("+", GUILayout.MaxWidth(24)))
            {
                HandleAddRoomButton();
            }

            EditorGUILayout.EndHorizontal();

            if (tempRoomTabIndex != selectedLevelRepresentation.selectedRoomindex)
            {
                SaveRoom();
                selectedLevelRepresentation.selectedRoomindex = tempRoomTabIndex;
                LoadRoom();
            }

            if (selectedLevelRepresentation.selectedRoomindex != -1)
            {
                EditorGUILayout.PropertyField(selectedLevelRepresentation.spawnPointProperty, new GUIContent("Điểm xuất phát (vòng tròn lưới màu trắng)"));
                EditorSceneController.Instance.SpawnPoint = selectedLevelRepresentation.spawnPointProperty.vector3Value;

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Lưu thành Preset", WatermelonEditor.Styles.button_02))
                {
                    SaveLevelIfPosssible();
                    RoomPresetSaveWindow.CreateRoomPresetSaveWindow(CreateRoomPreset);
                }

                if (GUILayout.Button("Xóa phòng", WatermelonEditor.Styles.button_04))
                {
                    if (EditorUtility.DisplayDialog("Cảnh báo", "Bạn có chắc chắn muốn xóa phòng này không?", "Có", "Hủy"))
                    {
                        selectedLevelRepresentation.roomsProperty.DeleteArrayElementAtIndex(selectedLevelRepresentation.selectedRoomindex);
                        worldSerializedObject.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();
                        ReloadLevel();
                    }
                }

                EditorGUILayout.EndHorizontal();

            }
            else
            {
                if (selectedLevelRepresentation != null)
                {
                    DisplayLevelFields();
                }

            }

            EditorGUILayout.EndVertical();
        }

        private void CreateRoomPreset(string presetName)
        {
            roomPresetsProperty.arraySize++;
            SerializedProperty newPreset = roomPresetsProperty.GetArrayElementAtIndex(roomPresetsProperty.arraySize - 1);

            newPreset.FindPropertyRelative("name").stringValue = presetName;
            newPreset.FindPropertyRelative("spawnPos").vector3Value = selectedLevelRepresentation.spawnPointProperty.vector3Value;

            SerializedProperty newArray = newPreset.FindPropertyRelative("itemEntities");
            newArray.arraySize = selectedLevelRepresentation.itemEntitiesProperty.arraySize;

            for (int i = 0; i < selectedLevelRepresentation.itemEntitiesProperty.arraySize; i++)
            {
                newArray.GetArrayElementAtIndex(i).FindPropertyRelative("Hash").intValue = selectedLevelRepresentation.itemEntitiesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Hash").intValue;
                newArray.GetArrayElementAtIndex(i).FindPropertyRelative("Position").vector3Value = selectedLevelRepresentation.itemEntitiesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Position").vector3Value;
                newArray.GetArrayElementAtIndex(i).FindPropertyRelative("Rotation").quaternionValue = selectedLevelRepresentation.itemEntitiesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Rotation").quaternionValue;
                newArray.GetArrayElementAtIndex(i).FindPropertyRelative("Scale").vector3Value = selectedLevelRepresentation.itemEntitiesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Scale").vector3Value;
            }

            for (int i = newArray.arraySize - 1; i >= 0; i--)
            {
                if (!IsEnvironment(newArray.GetArrayElementAtIndex(i).FindPropertyRelative("Hash").intValue))
                {
                    newArray.DeleteArrayElementAtIndex(i);
                }
            }

            worldSerializedObject.ApplyModifiedProperties();
        }

        private void ReloadLevel()
        {
            //we reload everything
            selectedLevelRepresentation = new LevelRepresentation(levelsProperty.GetArrayElementAtIndex(levelsList.index));
            LoadRoom();
        }

        private void HandleAddRoomButton()
        {
            GenericMenu menu = new GenericMenu();

            for (int i = 0; i < roomPresetsProperty.arraySize; i++)
            {
                menu.AddItem(new GUIContent(roomPresetsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue), false, CreateRoomFromPreset, i);
            }

            menu.ShowAsContext();
        }

        private void CreateRoomFromPreset(object data)
        {
            int index = (int)data;

            SerializedProperty element;
            int hash;
            Vector3 position;
            Vector3 scale;
            Quaternion rotation;
            UnityEngine.Object prefab;

            EditorSceneController.Instance.Clear();
            selectedLevelRepresentation.AddRoom();
            EditorSceneController.Instance.UpdateContainerLabel(selectedLevelRepresentation.selectedRoomindex);

            SerializedProperty preset = roomPresetsProperty.GetArrayElementAtIndex(index);
            SerializedProperty itemsData = preset.FindPropertyRelative("itemEntities");

            for (int i = 0; i < itemsData.arraySize; i++)
            {
                element = itemsData.GetArrayElementAtIndex(i);
                hash = element.FindPropertyRelative("Hash").intValue;
                position = element.FindPropertyRelative("Position").vector3Value;
                rotation = element.FindPropertyRelative("Rotation").quaternionValue;
                scale = element.FindPropertyRelative("Scale").vector3Value;
                prefab = GetPrefabByHash(hash);
                EditorSceneController.Instance.SpawnItem(prefab as GameObject, position, rotation, scale, hash);
            }

            
            

            selectedLevelRepresentation.spawnPointProperty.vector3Value = preset.FindPropertyRelative("spawnPos").vector3Value;
            EditorSceneController.Instance.UpdateContainerLabel(selectedLevelRepresentation.selectedRoomindex);
            SaveRoom();
        }

        private void DisplayToolbar()
        {
            selectedToolbarTab = GUILayout.Toolbar(selectedToolbarTab, toolbarTab);
            itemsListWidthRect = GUILayoutUtility.GetRect(1, Screen.width, 0, 0, GUILayout.ExpandWidth(true));

            if(itemsListWidthRect.width > 1)
            {
                currentItemListWidth = itemsListWidthRect.width;
            }

            if (selectedToolbarTab == 0)
            {
                DisplayObstaclesListSection();
            }
            else if (selectedToolbarTab == 1)
            {
                DisplayEnemiesListSection();
            }
            else if (selectedToolbarTab == 2)
            {
                DisplayEnvironmentListSelection();
            }
        }

        private void DisplayObstaclesListSection()
        {
            EditorGUILayout.LabelField("Vật cản (Obstacles):");
            levelItemsScrollVector = EditorGUILayout.BeginScrollView(levelItemsScrollVector);

            itemsRect = EditorGUILayout.BeginVertical();
            itemPosX = itemsRect.x;
            itemPosY = itemsRect.y;
            int counter = 0;

            for (int i = 0; i < itemsProperty.arraySize; i++)
            {
                tempType = itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(TYPE_PROPERTY_PATH).intValue;

                if(tempType == (int)LevelItemType.Obstacle)
                {
                    counter++;
                }
            }

            //assigning space
            if (counter + chests.Length != 0)
            {
                itemsPerRow = Mathf.FloorToInt((currentItemListWidth - 16) / (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH)); // 16- space for vertical scroll
                rowCount = Mathf.CeilToInt((counter + chests.Length) * 1f / itemsPerRow);
                GUILayout.Space(rowCount * (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_HEIGHT));
            }

            for (int i = 0; i < itemsProperty.arraySize; i++)
            {
                tempType = itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(TYPE_PROPERTY_PATH).intValue;

                if (tempType != (int)LevelItemType.Obstacle)
                {
                    continue;
                }

                tempPrefab = itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue as GameObject;
                itemContent = new GUIContent(AssetPreview.GetAssetPreview(tempPrefab), ITEM_ASSIGNED);

                //check if need to start new row
                if (itemPosX + ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH > currentItemListWidth - 16)
                {
                    itemPosX = itemsRect.x;
                    itemPosY = itemPosY + ITEMS_BUTTON_HEIGHT + ITEMS_BUTTON_SPACE;
                }

                itemRect = new Rect(itemPosX, itemPosY, ITEMS_BUTTON_WIDTH, ITEMS_BUTTON_HEIGHT);

                if (GUI.Button(itemRect, itemContent, WatermelonEditor.Styles.button_01))
                {
                    EditorSceneController.Instance.SpawnItem(tempPrefab, Vector3.zero, Quaternion.identity, Vector3.one, itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(HASH_PROPERTY_PATH).intValue, true);
                }
                itemPosX += ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH;
            }

            //chest
            for (int i = 0; i < chests.Length; i++)
            {
                itemContent = new GUIContent(AssetPreview.GetAssetPreview(chests[i].prefabRef), ITEM_ASSIGNED);

                //check if need to start new row
                if (itemPosX + ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH > currentItemListWidth - 16)
                {
                    itemPosX = itemsRect.x;
                    itemPosY = itemPosY + ITEMS_BUTTON_HEIGHT + ITEMS_BUTTON_SPACE;
                }

                itemRect = new Rect(itemPosX, itemPosY, ITEMS_BUTTON_WIDTH, ITEMS_BUTTON_HEIGHT);

                if (GUI.Button(itemRect, itemContent, WatermelonEditor.Styles.button_01))
                {
                    selectedLevelRepresentation.chestEntitiesProperty.arraySize++;
                    var chestProp = new ChestProperty();
                    chestProp.Init(selectedLevelRepresentation.chestEntitiesProperty.GetArrayElementAtIndex(selectedLevelRepresentation.chestEntitiesProperty.arraySize - 1));

                    chestProp.chestTypeProperty.intValue = chests[i].typeEnumValueIndex;

                    var newChestProperties = new ChestProperty[selectedLevelRepresentation.chestEntitiesProperty.arraySize];
                    Array.Copy(selectedLevelRepresentation.chestProperties, newChestProperties, newChestProperties.Length - 1);
                    newChestProperties[^1] = chestProp;
                    selectedLevelRepresentation.chestProperties = newChestProperties;

                    EditorSceneController.Instance.SpawnChest(chests[i].prefabRef as GameObject,
                        chestProp.chestPositionProperty.vector3Value,
                        chestProp.chestRotationProperty.quaternionValue,
                        Vector3.one,
                        (LevelChestType)chestProp.chestTypeProperty.intValue);

                    chestProp.isChestInitedProperty.boolValue = true;
                    worldSerializedObject.ApplyModifiedProperties();
                }

                itemPosX += ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void DisplayEnemiesListSection()
        {
            EditorGUILayout.LabelField("Kẻ địch (Enemies):");
            levelItemsScrollVector = EditorGUILayout.BeginScrollView(levelItemsScrollVector);

            itemsRect = EditorGUILayout.BeginVertical();
            itemPosX = itemsRect.x;
            itemPosY = itemsRect.y;

            //assigning space
            if (enemies.Length != 0)
            {
                itemsPerRow = Mathf.FloorToInt((currentItemListWidth - 16) / (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH)); // 16- space for vertical scroll
                rowCount = Mathf.CeilToInt(enemies.Length * 1f / itemsPerRow);
                GUILayout.Space(rowCount * (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_HEIGHT));
            }

            for (int i = 0; i < enemies.Length; i++)
            {
                itemContent = new GUIContent(enemies[i].image, ITEM_ASSIGNED);

                //check if need to start new row
                if (itemPosX + ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH > currentItemListWidth - 16)
                {
                    itemPosX = itemsRect.x;
                    itemPosY = itemPosY + ITEMS_BUTTON_HEIGHT + ITEMS_BUTTON_SPACE;
                }

                itemRect = new Rect(itemPosX, itemPosY, ITEMS_BUTTON_WIDTH, ITEMS_BUTTON_HEIGHT);

                if (GUI.Button(itemRect, itemContent, WatermelonEditor.Styles.button_01))
                {
                    EditorSceneController.Instance.SpawnEnemy(enemies[i].prefabRef as GameObject, Vector3.zero, Quaternion.Euler(0, 180, 0), Vector3.one, enemies[i].enemyType, false, new Vector3[0]);
                }
                itemPosX += ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH;
            }


            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void DisplayEnvironmentListSelection()
        {
            EditorGUILayout.LabelField("Môi trường (Environments):");
            levelItemsScrollVector = EditorGUILayout.BeginScrollView(levelItemsScrollVector);

            itemsRect = EditorGUILayout.BeginVertical();
            itemPosX = itemsRect.x;
            itemPosY = itemsRect.y;

            int counter = 0;

            for (int i = 0; i < itemsProperty.arraySize; i++)
            {
                tempType = itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(TYPE_PROPERTY_PATH).intValue;

                if (tempType == (int)LevelItemType.Environment)
                {
                    counter++;
                }
            }

            //assigning space
            if (counter != 0)
            {
                itemsPerRow = Mathf.FloorToInt((currentItemListWidth - 16) / (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH)); // 16- space for vertical scroll
                rowCount = Mathf.CeilToInt(counter * 1f / itemsPerRow);
                GUILayout.Space(rowCount * (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_HEIGHT));
            }

            for (int i = 0; i < itemsProperty.arraySize; i++)
            {
                tempType = itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(TYPE_PROPERTY_PATH).intValue;

                if (tempType != (int)LevelItemType.Environment)
                {
                    continue;
                }

                tempPrefab = itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue as GameObject;
                itemContent = new GUIContent(AssetPreview.GetAssetPreview(tempPrefab), ITEM_ASSIGNED);

                //check if need to start new row
                if (itemPosX + ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH > currentItemListWidth - 16)
                {
                    itemPosX = itemsRect.x;
                    itemPosY = itemPosY + ITEMS_BUTTON_HEIGHT + ITEMS_BUTTON_SPACE;
                }

                itemRect = new Rect(itemPosX, itemPosY, ITEMS_BUTTON_WIDTH, ITEMS_BUTTON_HEIGHT);

                if (GUI.Button(itemRect, itemContent, WatermelonEditor.Styles.button_01))
                {
                    EditorSceneController.Instance.SpawnItem(tempPrefab, Vector3.zero, Quaternion.Euler(0, 180, 0), Vector3.one, itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(HASH_PROPERTY_PATH).intValue, true);
                }
                itemPosX += ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH;
            }


            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void DisplyLevelObjectMenagementSection()
        {
            EditorGUILayout.LabelField(OBJECT_MANAGEMENT);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(CLEAR_SCENE, WatermelonEditor.Styles.button_04, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                ClearScene();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button(LOAD, WatermelonEditor.Styles.button_03, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                LoadRoom();
            }

            if (GUILayout.Button(SAVE, WatermelonEditor.Styles.button_02, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                SaveRoom();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void LoadRoom()
        {
            EditorSceneController.Instance.Clear();
            EditorSceneController.Instance.ClearRoomCustomObjectsContainer();
            EditorSceneController.Instance.UpdateContainerLabel(selectedLevelRepresentation.selectedRoomindex);

            if (selectedLevelRepresentation.selectedRoomindex == -1)
            {
                return;
            }

            selectedLevelRepresentation.OpenRoom(selectedLevelRepresentation.selectedRoomindex);

            SpawnItems();
            SpawnEnemy();
            SpawnChest();
            SpawnRoomCustomObjects();
        }

        private void SpawnItems()
        {
            SerializedProperty element;
            int hash;
            Vector3 position;
            Vector3 scale;
            Quaternion rotation;
            UnityEngine.Object prefab;

            for (int i = 0; i < selectedLevelRepresentation.itemEntitiesProperty.arraySize; i++)
            {
                element = selectedLevelRepresentation.itemEntitiesProperty.GetArrayElementAtIndex(i);
                hash = element.FindPropertyRelative("Hash").intValue;
                position = element.FindPropertyRelative("Position").vector3Value;
                rotation = element.FindPropertyRelative("Rotation").quaternionValue;
                scale = element.FindPropertyRelative("Scale").vector3Value;
                prefab = GetPrefabByHash(hash);
                EditorSceneController.Instance.SpawnItem(prefab as GameObject, position, rotation, scale, hash);
            }
        }

        private void SpawnEnemy()
        {
            SerializedProperty element;
            SerializedProperty pointsArray;
            int typeIndex;
            Vector3 position;
            Quaternion rotation;
            Vector3 scale;
            UnityEngine.Object prefab = enemies[0].prefabRef;
            bool isElite;
            Vector3[] pathPoints;
            EnemyType type = EnemyType.BatMelee;

            for (int i = 0; i < selectedLevelRepresentation.enemyEntitiesProperty.arraySize; i++)
            {
                element = selectedLevelRepresentation.enemyEntitiesProperty.GetArrayElementAtIndex(i);
                typeIndex = element.FindPropertyRelative("EnemyType").enumValueIndex;
                position = element.FindPropertyRelative("Position").vector3Value;
                scale = element.FindPropertyRelative("Scale").vector3Value;
                rotation = element.FindPropertyRelative("Rotation").quaternionValue;
                isElite = element.FindPropertyRelative("IsElite").boolValue;
                pointsArray = element.FindPropertyRelative("PathPoints");
                pathPoints = new Vector3[pointsArray.arraySize];

                for (int j = 0; j < pointsArray.arraySize; j++)
                {
                    pathPoints[j] = pointsArray.GetArrayElementAtIndex(j).vector3Value;
                }


                for (int j = 0; j < enemies.Length; j++)
                {
                    if (enemies[j].typeEnumValueIndex == typeIndex)
                    {
                        prefab = enemies[j].prefabRef;
                        type = enemies[j].enemyType;
                        break;
                    }
                }

                EditorSceneController.Instance.SpawnEnemy(prefab as GameObject, position, rotation, scale, type, isElite, pathPoints);
            }
        }

        private void SpawnChest()
        {
            for (int i = 0; i < selectedLevelRepresentation.chestProperties.Length; i++)
            {
                if (selectedLevelRepresentation.chestProperties[i].isChestInitedProperty.boolValue)
                {
                    var chestProp = selectedLevelRepresentation.chestProperties[i];
                    var chestType = chestProp.chestTypeProperty.intValue;
                    UnityEngine.Object prefab = null;
                    for (int j = 0; j < chests.Length; j++)
                    {
                        if (chests[j].typeEnumValueIndex == chestType)
                        {
                            prefab = chests[j].prefabRef;
                            break;
                        }
                    }

                    EditorSceneController.Instance.SpawnChest(prefab as GameObject, chestProp.chestPositionProperty.vector3Value, chestProp.chestRotationProperty.quaternionValue, chestProp.chestScaleProperty.vector3Value, (LevelChestType)chestType);
                }
            }
        }

        private void SpawnRoomCustomObjects()
        {
            SerializedProperty element;
            Vector3 position;
            Vector3 scale;
            Quaternion rotation;
            UnityEngine.Object prefab;

            for (int i = 0; i < selectedLevelRepresentation.roomCustomObjectsProperty.arraySize; i++)
            {
                element = selectedLevelRepresentation.roomCustomObjectsProperty.GetArrayElementAtIndex(i);
                prefab = element.FindPropertyRelative("PrefabRef").objectReferenceValue;
                position = element.FindPropertyRelative("Position").vector3Value;
                rotation = element.FindPropertyRelative("Rotation").quaternionValue;
                scale = element.FindPropertyRelative("Scale").vector3Value;
                EditorSceneController.Instance.SpawnRoomCustomObject(prefab as GameObject, position, rotation, scale);
            }
        }

        private void SpawnWorldCustomObjects()
        {
            SerializedProperty element;
            Vector3 position;
            Vector3 scale;
            Quaternion rotation;
            UnityEngine.Object prefab;

            for (int i = 0; i < worldCustomObjectsProperty.arraySize; i++)
            {
                element = worldCustomObjectsProperty.GetArrayElementAtIndex(i);
                prefab = element.FindPropertyRelative("PrefabRef").objectReferenceValue;
                position = element.FindPropertyRelative("Position").vector3Value;
                rotation = element.FindPropertyRelative("Rotation").quaternionValue;
                scale = element.FindPropertyRelative("Scale").vector3Value;
                EditorSceneController.Instance.SpawnWorldCustomObject(prefab as GameObject, position, rotation, scale);
            }
        }

        private void SaveRoom()
        {
            if (selectedLevelRepresentation.selectedRoomindex != -1)
            {
                selectedLevelRepresentation.OpenRoom(selectedLevelRepresentation.selectedRoomindex);
                SaveItems();
                SaveEnemy();
                SaveChest();
                SaveRoomCustomObjects();
                RemoveDuplicates(selectedLevelRepresentation.itemEntitiesProperty);
                RemoveDuplicates(selectedLevelRepresentation.enemyEntitiesProperty);
                RemoveDuplicates(selectedLevelRepresentation.chestEntitiesProperty);
                RemoveDuplicates(selectedLevelRepresentation.roomCustomObjectsProperty);
            }

            worldSerializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        private void RemoveDuplicates(SerializedProperty targetProperty)
        {
            Vector3 position1;
            Vector3 position2;
            Quaternion quaternion1;
            Quaternion quaternion2;

            for (int i = 0; i < targetProperty.arraySize - 1; i++)
            {
                position1 = targetProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Position").vector3Value;
                quaternion1 = targetProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Rotation").quaternionValue;

                for (int j = targetProperty.arraySize - 1; j > i; j--)
                {
                    position2 = targetProperty.GetArrayElementAtIndex(j).FindPropertyRelative("Position").vector3Value;
                    quaternion2 = targetProperty.GetArrayElementAtIndex(j).FindPropertyRelative("Rotation").quaternionValue;

                    if(position1.Equals(position2) && (quaternion1.Equals(quaternion2)))
                    {
                        Debug.LogWarning($"Removed duplicate with position: {position1} and rotation: {quaternion2} from property {targetProperty.displayName}.");
                        targetProperty.DeleteArrayElementAtIndex(j);
                    }
                }
            }
        }

        private void SaveItems()
        {
            SerializedProperty element;
            ItemEntityData[] data = EditorSceneController.Instance.CollectItemsFromRoom();
            selectedLevelRepresentation.itemEntitiesProperty.arraySize = data.Length;

            for (int i = 0; i < selectedLevelRepresentation.itemEntitiesProperty.arraySize; i++)
            {
                element = selectedLevelRepresentation.itemEntitiesProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("Hash").intValue = data[i].Hash;
                element.FindPropertyRelative("Position").vector3Value = data[i].Position;
                element.FindPropertyRelative("Rotation").quaternionValue = data[i].Rotation;
                element.FindPropertyRelative("Scale").vector3Value = data[i].Scale;
            }

        }


        private void SaveEnemy()
        {
            SerializedProperty element;
            SerializedProperty pathPoints;
            EnemyEntityData[] data = EditorSceneController.Instance.CollectEnemiesFromRoom();
            selectedLevelRepresentation.enemyEntitiesProperty.arraySize = data.Length;

            for (int i = 0; i < selectedLevelRepresentation.enemyEntitiesProperty.arraySize; i++)
            {
                element = selectedLevelRepresentation.enemyEntitiesProperty.GetArrayElementAtIndex(i);

                for (int j = 0; j < enemies.Length; j++)
                {
                    if (enemies[j].enemyType == data[i].EnemyType)
                    {
                        element.FindPropertyRelative("EnemyType").enumValueIndex = enemies[j].typeEnumValueIndex;
                    }
                }

                element.FindPropertyRelative("Position").vector3Value = data[i].Position;
                element.FindPropertyRelative("Rotation").quaternionValue = data[i].Rotation;
                element.FindPropertyRelative("Scale").vector3Value = data[i].Scale;
                element.FindPropertyRelative("IsElite").boolValue = data[i].IsElite;

                pathPoints = element.FindPropertyRelative("PathPoints");
                pathPoints.arraySize = data[i].PathPoints.Length;

                for (int j = 0; j < pathPoints.arraySize; j++)
                {
                    pathPoints.GetArrayElementAtIndex(j).vector3Value = data[i].PathPoints[j];
                }
            }
        }

        private void SaveChest()
        {
            var chests = EditorSceneController.Instance.CollectChestFromRoom();

            selectedLevelRepresentation.chestEntitiesProperty.arraySize = chests.Count;

            for (int i = 0; i < chests.Count; i++)
            {
                var chestData = chests[i];

                var chestProp = new ChestProperty();
                chestProp.Init(selectedLevelRepresentation.chestEntitiesProperty.GetArrayElementAtIndex(i));

                chestProp.chestPositionProperty.vector3Value = chestData.transform.localPosition;
                chestProp.chestRotationProperty.quaternionValue = chestData.transform.localRotation;
                chestProp.chestScaleProperty.vector3Value = chestData.transform.localScale;
                chestProp.isChestInitedProperty.boolValue = true;
                chestProp.chestTypeProperty.intValue = (int)chestData.type;
            }
        }


        private void SaveRoomCustomObjects()
        {
            SerializedProperty element;
            List<CustomObjectData> data = EditorSceneController.Instance.CollectRoomCustomObjects();
            selectedLevelRepresentation.roomCustomObjectsProperty.arraySize = data.Count;

            for (int i = 0; i < selectedLevelRepresentation.roomCustomObjectsProperty.arraySize; i++)
            {
                element = selectedLevelRepresentation.roomCustomObjectsProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("PrefabRef").objectReferenceValue = data[i].PrefabRef;
                element.FindPropertyRelative("Position").vector3Value = data[i].Position;
                element.FindPropertyRelative("Rotation").quaternionValue = data[i].Rotation;
                element.FindPropertyRelative("Scale").vector3Value = data[i].Scale;
            }
        }

        private void SaveWorldCustomObjects()
        {
            SerializedProperty element;
            List<CustomObjectData> data = EditorSceneController.Instance.CollectWorldCustomObjects();
            worldCustomObjectsProperty.arraySize = data.Count;

            for (int i = 0; i < worldCustomObjectsProperty.arraySize; i++)
            {
                element = worldCustomObjectsProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("PrefabRef").objectReferenceValue = data[i].PrefabRef;
                element.FindPropertyRelative("Position").vector3Value = data[i].Position;
                element.FindPropertyRelative("Rotation").quaternionValue = data[i].Rotation;
                element.FindPropertyRelative("Scale").vector3Value = data[i].Scale;
            }

            worldSerializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        public bool IsEnvironment(int hash)
        {
            for (int i = 0; i < itemsProperty.arraySize; i++)
            {
                if (itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("hash").intValue == hash)
                {
                    return itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("type").intValue == (int)LevelItemType.Environment;
                }
            }

            return false;
        }

        public UnityEngine.Object GetPrefabByHash(int hash)
        {
            for (int i = 0; i < itemsProperty.arraySize; i++)
            {
                if (itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("hash").intValue == hash)
                {
                    return itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("prefab").objectReferenceValue;
                }
            }

            Debug.LogError($"objectReferenceValue not found for hash {hash}");
            return null;
        }

        private void InitStuffForWorldSettingsTab()
        {
            itemsReordableList = new ReorderableList(worldSerializedObject,itemsProperty,true,false,true,true);
            itemsReordableList.drawElementCallback = DrawItemCallback;
            itemsReordableList.onAddCallback = AddItemCallback;
            invalidIndexesList = new List<int>();
            elementTypeRect = new Rect();
            elementObjectRefRect = new Rect();
            elementButtonRect = new Rect();
        }

        private void DrawItemCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (itemsProperty == null || index < 0 || index >= itemsProperty.arraySize)
                return;

            backupColor = GUI.backgroundColor;

            if (invalidIndexesList.Contains(index))
            {
                GUI.backgroundColor = Color.red;
            }

            elementTypeRect.Set(rect.x, rect.y + 2, rect.width / 3f - 16, rect.height - 4);
            elementObjectRefRect.Set(rect.x + elementTypeRect.width + 8, elementTypeRect.y, elementTypeRect.width, elementTypeRect.height);
            elementButtonRect.Set(elementObjectRefRect.x + elementTypeRect.width + 8, elementTypeRect.y, elementTypeRect.width, elementTypeRect.height);

            tempEnumProperty = itemsProperty.GetArrayElementAtIndex(index).FindPropertyRelative(TYPE_PROPERTY_PATH);
            tempPrefabRefProperty = itemsProperty.GetArrayElementAtIndex(index).FindPropertyRelative(PREFAB_PROPERTY_PATH);

            tempEnumProperty.intValue = (int)((LevelItemType)EditorGUI.EnumPopup(elementTypeRect, GUIContent.none, (LevelItemType)tempEnumProperty.intValue));
            EditorGUI.ObjectField(elementObjectRefRect, tempPrefabRefProperty, GUIContent.none);

            if (invalidIndexesList.Contains(index))
            {
                if (GUI.Button(elementButtonRect, "Lỗi - Xem chi tiết"))
                {
                    EditorUtility.DisplayDialog("Lỗi xác minh", GetValidationMessage(index), "Ok");
                }
            }

            GUI.backgroundColor = backupColor;
        }

        private void AddItemCallback(ReorderableList list)
        {
            int hash = TimeUtils.GetCurrentUnixTimestamp().GetHashCode();
            bool unique = true;

            do
            {
                if (!unique)
                {
                    hash = (TimeUtils.GetCurrentUnixTimestamp() + UnityEngine.Random.Range(1, 1000)).GetHashCode();
                }

                for (int i = 0; unique && (i < itemsProperty.arraySize); i++)
                {
                    if(itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(HASH_PROPERTY_PATH).intValue == hash)
                    {
                        unique = false;
                    }
                }

            } while (!unique);

            itemsProperty.arraySize++;

            SerializedProperty newElement = itemsProperty.GetArrayElementAtIndex(itemsProperty.arraySize - 1);
            newElement.ClearProperty();
            newElement.FindPropertyRelative(HASH_PROPERTY_PATH).intValue = hash;
        }


        public void DisplayWorldSettingsTab()
        {
            worldSerializedObject.Update();
            EditorGUILayout.PropertyField(previewSpriteProperty, previewSprite);
            EditorGUILayout.PropertyField(musicProperty, new GUIContent("Nhạc nền (Music):"));
            EditorGUILayout.PropertyField(worldTypeProperty, new GUIContent("Loại thế giới (World Type):"));
            itemsReordableList.DoLayoutList();
            worldSerializedObject.ApplyModifiedProperties();
            ValidateItems();
        }

        private void ValidateItems()
        {
            SerializedProperty element;
            GameObject prefab;
            invalidIndexesList.Clear();

            for (int i = 0; i < itemsProperty.arraySize; i++)
            {
                element = itemsProperty.GetArrayElementAtIndex(i);
                prefab = element.FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue as GameObject;

                if (prefab == null)
                {
                    invalidIndexesList.Add(i);
                    continue;
                }

                if(prefab.GetComponent<Collider>() == null)
                {
                    invalidIndexesList.Add(i);
                    continue;
                }

                if(element.FindPropertyRelative(TYPE_PROPERTY_PATH).intValue == (int)LevelItemType.Obstacle)
                {
                    if (prefab.GetComponent<NavMeshObstacle>() == null)
                    {
                        invalidIndexesList.Add(i);
                        continue;
                    }

                    if (prefab.GetComponent<NavMeshModifier>() == null)
                    {
                        invalidIndexesList.Add(i);
                        continue;
                    }

                    if (prefab.layer != LayerMask.NameToLayer("Obstacle"))
                    {
                        invalidIndexesList.Add(i);
                        continue;
                    }

                }
                else
                {
                    if (!((prefab.layer == LayerMask.NameToLayer("Obstacle")) || (prefab.layer == LayerMask.NameToLayer("Ground"))))
                    {
                        invalidIndexesList.Add(i);
                        continue;
                    }
                }
            }
        }

        private string GetValidationMessage(int index)
        {
            SerializedProperty element;
            GameObject prefab;
            element = itemsProperty.GetArrayElementAtIndex(index);
            prefab = element.FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue as GameObject;

            if (prefab == null)
            {
                return "Prefab reference is null";
            }

            if (prefab.GetComponent<Collider>() == null)
            {
                return "Prefab doesn't have a Collider.";
            }

            if (element.FindPropertyRelative(TYPE_PROPERTY_PATH).intValue == (int)LevelItemType.Obstacle)
            {
                if (prefab.GetComponent<NavMeshObstacle>() == null)
                {
                    return "Prefab doesn't have a NavMeshObstacle.";
                }

                if (prefab.GetComponent<NavMeshModifier>() == null)
                {
                    return "Prefab doesn't have a NavMeshModifier.";
                }

                if (prefab.layer != LayerMask.NameToLayer("Obstacle"))
                {
                    return "Prefab assigned to incorrect layer. Obstacle is the only correct layer for Obstacle type items.";
                }

            }
            else
            {
                if (!((prefab.layer == LayerMask.NameToLayer("Obstacle")) || (prefab.layer == LayerMask.NameToLayer("Ground"))))
                {
                    return "Prefab assigned to incorrect layer. Obstacle or Ground can be assigned as correct layers for Environment type items.";
                }
            }

            return string.Empty; // shound newer be called
        }

        private void SaveLevelIfPosssible()
        {
            if (selectedLevelRepresentation == null)
            {
                return;
            }

            try
            {
                SaveRoom();
                SaveWorldCustomObjects();
                RemoveDuplicates(worldCustomObjectsProperty);
            }
            catch
            {

            }

        }

        // this 2 overriden methods prevent level editor from closing in play mode 

        public override void OnBeforeAssemblyReload()
        {
        }

        public override bool WindowClosedInPlaymode()
        {
            return false;
        }

        protected class LevelRepresentation
        {
            public SerializedProperty levelProperty;

            //level
            public SerializedProperty levelTypeProperty;
            public SerializedProperty roomsProperty;
            public SerializedProperty specialBehavioursProperty;
            public SerializedProperty xpAmountProperty;
            public SerializedProperty requiredUpgProperty;
            public SerializedProperty enemiesLevelProperty;
            public SerializedProperty hasCharacterSuggestionProperty;
            public SerializedProperty dropDataProperty;
            public SerializedProperty healSpawnPercentProperty;


            //rooms
            public int selectedRoomindex;
            public SerializedProperty selectedRoom;

            public SerializedProperty spawnPointProperty;
            public SerializedProperty enemyEntitiesProperty;
            public SerializedProperty itemEntitiesProperty;
            public SerializedProperty roomCustomObjectsProperty;

            public SerializedProperty chestEntitiesProperty;
            public ChestProperty[] chestProperties;

            //room tabs
            public List<string> roomTabs;


            public LevelRepresentation(SerializedProperty levelProperty)
            {
                this.levelProperty = levelProperty;
                levelTypeProperty = levelProperty.FindPropertyRelative("type");
                roomsProperty = levelProperty.FindPropertyRelative("rooms");
                specialBehavioursProperty = levelProperty.FindPropertyRelative("specialBehaviours");
                xpAmountProperty = levelProperty.FindPropertyRelative("xpAmount");
                requiredUpgProperty = levelProperty.FindPropertyRelative("requiredUpg");
                enemiesLevelProperty = levelProperty.FindPropertyRelative("enemiesLevel");
                hasCharacterSuggestionProperty = levelProperty.FindPropertyRelative("hasCharacterSuggestion");
                dropDataProperty = levelProperty.FindPropertyRelative("dropData");
                healSpawnPercentProperty = levelProperty.FindPropertyRelative("healSpawnPercent");

                selectedRoomindex = -1;
                roomTabs = new List<string>();

                for (int i = 0; i < roomsProperty.arraySize; i++)
                {
                    roomTabs.Add("Room #" + (i + 1));
                }
            }

            public void OpenRoom(int index)
            {
                selectedRoom = roomsProperty.GetArrayElementAtIndex(index);
                spawnPointProperty = selectedRoom.FindPropertyRelative("spawnPoint");
                enemyEntitiesProperty = selectedRoom.FindPropertyRelative("enemyEntities");
                itemEntitiesProperty = selectedRoom.FindPropertyRelative("itemEntities");
                chestEntitiesProperty = selectedRoom.FindPropertyRelative("chestEntities");
                roomCustomObjectsProperty = selectedRoom.FindPropertyRelative("roomCustomObjects");


                chestProperties = new ChestProperty[chestEntitiesProperty.arraySize];
                for (int i = 0; i < chestEntitiesProperty.arraySize; i++)
                {
                    var chestProperty = chestEntitiesProperty.GetArrayElementAtIndex(i);

                    chestProperties[i] = new ChestProperty();
                    chestProperties[i].Init(chestProperty);
                }
            }

            public void AddRoom()
            {
                roomsProperty.arraySize++;
                roomTabs.Add("Room #" + roomsProperty.arraySize);
                selectedRoomindex = roomsProperty.arraySize - 1;
                OpenRoom(selectedRoomindex);

                spawnPointProperty.vector3Value = new Vector3(0, 0, -90);
                enemyEntitiesProperty.arraySize = 0;
                chestEntitiesProperty.arraySize = 0;
            }



            public void Clear()
            {
                levelTypeProperty.enumValueIndex = 0;
                roomsProperty.arraySize = 0;
                specialBehavioursProperty.arraySize = 0;
                xpAmountProperty.intValue = 0;
                requiredUpgProperty.intValue = 0;
                enemiesLevelProperty.intValue = 0;
                hasCharacterSuggestionProperty.boolValue = false;
                dropDataProperty.arraySize = 0;
                healSpawnPercentProperty.floatValue = 0.5f;
            }
        }

        public class ChestProperty
        {
            public SerializedProperty chestProperty;
            public SerializedProperty isChestInitedProperty;
            public SerializedProperty chestTypeProperty;
            public SerializedProperty chestPositionProperty;
            public SerializedProperty chestRotationProperty;
            public SerializedProperty chestScaleProperty;

            public void Init(SerializedProperty chestProperty)
            {
                this.chestProperty = chestProperty;

                isChestInitedProperty = chestProperty.FindPropertyRelative("IsInited");
                chestTypeProperty = chestProperty.FindPropertyRelative("ChestType");
                chestPositionProperty = chestProperty.FindPropertyRelative("Position");
                chestRotationProperty = chestProperty.FindPropertyRelative("Rotation");
                chestScaleProperty = chestProperty.FindPropertyRelative("Scale");
            }
        }

        private class CatchedPrefabRefs
        {
            public UnityEngine.Object prefabRef;
            public int typeEnumValueIndex;
        }

        private class CatchedEnemyRefs
        {
            public UnityEngine.Object prefabRef;
            public int typeEnumValueIndex;
            public EnemyType enemyType;
            public Texture2D image;
        }
    }
}

// -----------------
// Scene interraction level editor V1.5
// -----------------

// Changelog
// v 1.4
// • Updated EnumObjectlist
// • Updated object preview
// v 1.4
// • Updated EnumObjectlist
// • Fixed bug with window size
// v 1.3
// • Updated EnumObjectlist
// • Added StartPointHandles script that can be added to gameobjects
// v 1.2
// • Reordered some methods
// v 1.1
// • Added spawner tool
// v 1 basic version works
