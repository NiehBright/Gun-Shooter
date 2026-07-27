#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.SceneManagement;

namespace Watermelon.SquadShooter
{
    public class LoadingScreenBuilder : EditorWindow
    {
        private const string TEMPLATE_PREFAB_PATH = "Assets/Project Data/Watermelon Core/Modules/UI Manager/Overlay/Prefabs/UI Overlay Canvas (Fade).prefab";
        private const string TARGET_PREFAB_PATH = "Assets/Project Data/Game/Prefabs/UI/Pages/UI Loading Screen.prefab";
        private const string SCENE_PATH = "Assets/Project Data/Game/Scenes/Game.unity";

        [MenuItem("Tools/Squad Shooter/Loading Screen Builder")]
        public static void ShowWindow()
        {
            GetWindow<LoadingScreenBuilder>("Loading Screen Builder").Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("LOADING SCREEN PREFAB BUILDER", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Cong cu nay se:\n" +
                "1. Copy prefab Fade Overlay goc vao thu muc game thanh 'UI Loading Screen.prefab' de ban tu do sua doi.\n" +
                "2. Tu dong load scene Game.unity va dua prefab nay vao lam con cua UI Controller Canvas.\n" +
                "3. Luu lai scene de khi choi game, he thong se load man hinh loading thuc te tu prefab nay thay vi man hinh den dummy.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("BUILD LOADING SCREEN PREFAB & SCENE LINK", GUILayout.Height(50)))
            {
                BuildLoadingScreen();
            }
            GUI.backgroundColor = Color.white;
        }

        private void BuildLoadingScreen()
        {
            // 1. Copy template prefab to target
            if (!File.Exists(TEMPLATE_PREFAB_PATH))
            {
                EditorUtility.DisplayDialog("Error", $"Khong tim thay prefab template tai: {TEMPLATE_PREFAB_PATH}", "OK");
                return;
            }

            if (!File.Exists(TARGET_PREFAB_PATH))
            {
                string targetDir = Path.GetDirectoryName(TARGET_PREFAB_PATH);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                AssetDatabase.CopyAsset(TEMPLATE_PREFAB_PATH, TARGET_PREFAB_PATH);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[LoadingScreenBuilder] Copyed template prefab to {TARGET_PREFAB_PATH}");
            }

            // 2. Load the prefab to add some default visual indicators (like background and loading text)
            GameObject targetPrefabObj = PrefabUtility.LoadPrefabContents(TARGET_PREFAB_PATH);
            if (targetPrefabObj != null)
            {
                // Check if it already has a background image and text
                Transform bgTrans = targetPrefabObj.transform.Find("Background");
                if (bgTrans == null)
                {
                    // Create Background Image
                    GameObject bgObj = new GameObject("Background");
                    bgObj.transform.SetParent(targetPrefabObj.transform, false);
                    RectTransform bgRect = bgObj.AddComponent<RectTransform>();
                    bgRect.anchorMin = Vector2.zero;
                    bgRect.anchorMax = Vector2.one;
                    bgRect.sizeDelta = Vector2.zero;
                    var bgImg = bgObj.AddComponent<UnityEngine.UI.Image>();
                    bgImg.color = new Color(0.08f, 0.08f, 0.1f, 1.0f); // Dark background
                    
                    // Create Text
                    GameObject textObj = new GameObject("LoadingText");
                    textObj.transform.SetParent(bgObj.transform, false);
                    RectTransform textRect = textObj.AddComponent<RectTransform>();
                    textRect.anchorMin = new Vector2(0.5f, 0.5f);
                    textRect.anchorMax = new Vector2(0.5f, 0.5f);
                    textRect.anchoredPosition = new Vector2(0, -50f);
                    textRect.sizeDelta = new Vector2(400f, 60f);
                    
                    var tmpText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
                    tmpText.text = "LOADING...";
                    tmpText.fontSize = 32;
                    tmpText.alignment = TMPro.TextAlignmentOptions.Center;
                    tmpText.fontStyle = TMPro.FontStyles.Bold;
                    tmpText.color = Color.white;

                    PrefabUtility.SaveAsPrefabAsset(targetPrefabObj, TARGET_PREFAB_PATH);
                    Debug.Log("[LoadingScreenBuilder] Created default Background and Text in UI Loading Screen prefab.");
                }
                PrefabUtility.UnloadPrefabContents(targetPrefabObj);
            }

            // 3. Open Game.unity scene
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                UnityEngine.SceneManagement.Scene activeScene = EditorSceneManager.OpenScene(SCENE_PATH);
                
                // Find Canvas with UIController
                UIController uiController = FindObjectOfType<UIController>();
                if (uiController == null)
                {
                    EditorUtility.DisplayDialog("Error", "Khong tim thay UIController trong scene Game.unity!", "OK");
                    return;
                }

                // Check if loading screen prefab is already instantiated in the scene under Canvas
                Transform existingLoadingScreen = uiController.transform.Find("UI Loading Screen");
                if (existingLoadingScreen == null)
                {
                    GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(TARGET_PREFAB_PATH);
                    if (prefabAsset != null)
                    {
                        GameObject screenInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset, uiController.transform);
                        screenInstance.name = "UI Loading Screen";
                        
                        // Mark scene as dirty and save
                        EditorSceneManager.MarkSceneDirty(activeScene);
                        EditorSceneManager.SaveScene(activeScene);
                        Debug.Log("[LoadingScreenBuilder] Instantiated UI Loading Screen prefab into Game.unity scene.");
                    }
                }

                EditorUtility.DisplayDialog("Success", "Build Loading Screen Prefab & Scene link successfully!", "OK");
            }
        }
    }
}
#endif
