#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public static class UICharactersPanelEditorTool
    {
        private const string PREFAB_PATH = "Assets/Project Data/Game/Prefabs/UI/Pages/UI Characters Panel.prefab";

        [MenuItem("Tools/GunShooter/Setup UI Characters Panel", false, 100)]
        public static void SetupUICharactersPanel()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogError($"[UICharactersPanelEditorTool] Cannot find prefab at {PREFAB_PATH}");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError("[UICharactersPanelEditorTool] Failed to instantiate prefab.");
                return;
            }

            UICharactersPanel panel = instance.GetComponent<UICharactersPanel>();
            if (panel != null)
            {
                UICharactersPanelBuilder.BuildLayout(panel);
                PrefabUtility.SaveAsPrefabAsset(instance, PREFAB_PATH);
                Debug.Log("<color=green>[UICharactersPanelEditorTool] Successfully configured and saved UI Characters Panel prefab!</color>");
            }

            Object.DestroyImmediate(instance);
        }
    }
}
#endif
