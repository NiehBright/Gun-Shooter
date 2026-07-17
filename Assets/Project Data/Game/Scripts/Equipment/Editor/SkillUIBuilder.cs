#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;

namespace Watermelon.SquadShooter
{
    public class SkillUIBuilder : EditorWindow
    {
        private const string PREFAB_PATH = "Assets/Project Data/Game/Prefabs/UI/Pages/UI Game.prefab";

        [MenuItem("Tools/Squad Shooter/Skill UI Builder")]
        public static void ShowWindow()
        {
            GetWindow<SkillUIBuilder>("Skill UI Builder").Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("ACTIVE SKILL - UI BUILDER", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH) != null;

            if (prefabExists)
            {
                EditorGUILayout.HelpBox(
                    "Cong cu nay se tu dong load prefab UI Game, sao chep nut Dash Button de tao thanh nut Skill Button,\n" +
                    "dat no ben canh nut Dash va tu dong lien ket references vao script UIGame.",
                    MessageType.Info);

                EditorGUILayout.Space(10);

                GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
                if (GUILayout.Button("BUILD SKILL BUTTON IN PREFAB", GUILayout.Height(50)))
                {
                    BuildSkillButton();
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                EditorGUILayout.HelpBox($"Khong tim thay prefab tai path: {PREFAB_PATH}", MessageType.Error);
            }
        }

        private void BuildSkillButton()
        {
            // 1. Load prefab contents
            GameObject gamePrefabObj = PrefabUtility.LoadPrefabContents(PREFAB_PATH);
            if (gamePrefabObj == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not load prefab contents!", "OK");
                return;
            }

            UIGame uiGame = gamePrefabObj.GetComponent<UIGame>();
            if (uiGame == null)
            {
                EditorUtility.DisplayDialog("Error", "Prefab is missing UIGame script!", "OK");
                PrefabUtility.UnloadPrefabContents(gamePrefabObj);
                return;
            }

            // Clean up existing Skill Button if already built previously
            Transform oldSkillBtn = FindChildRecursive(gamePrefabObj.transform, "Skill Button");
            if (oldSkillBtn != null) DestroyImmediate(oldSkillBtn.gameObject);

            // Find Dash Button to clone it
            Transform dashBtnTrans = FindChildRecursive(gamePrefabObj.transform, "Dash Button");
            if (dashBtnTrans == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not find Dash Button in UI Game to duplicate!", "OK");
                PrefabUtility.UnloadPrefabContents(gamePrefabObj);
                return;
            }

            // 2. Duplicate Dash Button as Skill Button
            GameObject skillBtnObj = Instantiate(dashBtnTrans.gameObject, dashBtnTrans.parent);
            skillBtnObj.name = "Skill Button";

            // Position it to the left of the Dash Button
            RectTransform skillRect = skillBtnObj.GetComponent<RectTransform>();
            RectTransform dashRect = dashBtnTrans.GetComponent<RectTransform>();
            
            // Copy rect transform values
            skillRect.anchorMin = dashRect.anchorMin;
            skillRect.anchorMax = dashRect.anchorMax;
            skillRect.pivot = dashRect.pivot;
            skillRect.sizeDelta = dashRect.sizeDelta;
            
            // Offset to the left (e.g. 170 pixels)
            skillRect.anchoredPosition = dashRect.anchoredPosition + new Vector2(-170f, 0f);

            // Clear any button click listeners carried over from prefab
            Button skillBtn = skillBtnObj.GetComponent<Button>();
            if (skillBtn != null)
            {
                // Serialized click events are cleared by resetting the persistent list
                SerializedObject serializedButton = new SerializedObject(skillBtn);
                SerializedProperty onClickProperty = serializedButton.FindProperty("m_OnClick");
                if (onClickProperty != null)
                {
                    SerializedProperty callsProperty = onClickProperty.FindPropertyRelative("m_PersistentCalls.m_Calls");
                    if (callsProperty != null)
                    {
                        callsProperty.ClearArray();
                    }
                }
                serializedButton.ApplyModifiedProperties();
            }

            // Find child images
            Image skillIconImg = skillBtnObj.GetComponent<Image>();
            
            Transform overlayTrans = FindChildRecursive(skillBtnObj.transform, "Cooldown Overlay");
            Image skillCooldownOverlayImg = overlayTrans != null ? overlayTrans.GetComponent<Image>() : null;

            // 3. Bind references to UIGame script on the prefab
            SetFieldValue(uiGame, "skillButton", skillBtn);
            SetFieldValue(uiGame, "skillCooldownOverlay", skillCooldownOverlayImg);
            SetFieldValue(uiGame, "skillIconImage", skillIconImg);

            // 4. Save prefab contents and unload
            PrefabUtility.SaveAsPrefabAsset(gamePrefabObj, PREFAB_PATH);
            PrefabUtility.UnloadPrefabContents(gamePrefabObj);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", "Build Skill Button successfully! Check the UI Game prefab now.", "OK");
        }

        private static void SetFieldValue(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                Debug.LogError($"[SkillUIBuilder] Field '{fieldName}' not found on type '{obj.GetType().Name}'");
            }
        }

        private Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Transform result = FindChildRecursive(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
#endif
