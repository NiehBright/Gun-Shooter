#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public static class UIWeaponPageEditorTool
    {
        private const string PREFAB_PATH = "Assets/Project Data/Game/Prefabs/UI/Pages/UI Weapon Panel.prefab";

        [MenuItem("Tools/GunShooter/Setup UI Weapon Panel", false, 101)]
        public static void SetupUIWeaponPanel()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogError($"[UIWeaponPageEditorTool] Cannot find prefab at {PREFAB_PATH}");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError("[UIWeaponPageEditorTool] Failed to instantiate prefab.");
                return;
            }

            UIWeaponPage page = instance.GetComponent<UIWeaponPage>();
            if (page != null)
            {
                UIWeaponPageBuilder.BuildLayout(page);
                PrefabUtility.SaveAsPrefabAsset(instance, PREFAB_PATH);
                Debug.Log("<color=green>[UIWeaponPageEditorTool] Successfully configured and saved UI Weapon Panel prefab (Full-Height Docked Layout)!</color>");
            }

            Object.DestroyImmediate(instance);
        }
    }
}
#endif
