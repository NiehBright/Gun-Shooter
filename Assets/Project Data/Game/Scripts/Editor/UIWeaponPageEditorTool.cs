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

            // Instantiate in scene so BuildLayout can manipulate hierarchy
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError("[UIWeaponPageEditorTool] Failed to instantiate prefab.");
                return;
            }

            UIWeaponPage page = instance.GetComponent<UIWeaponPage>();
            if (page != null)
            {
                // Run the builder to create/configure all child objects
                UIWeaponPageBuilder.BuildLayout(page);

                // Mark dirty so Unity knows to serialize the new state
                EditorUtility.SetDirty(instance);

                // Save back into the prefab asset
                bool success = false;
                PrefabUtility.SaveAsPrefabAsset(instance, PREFAB_PATH, out success);

                if (success)
                {
                    Debug.Log("<color=green>[UIWeaponPageEditorTool] ✅ Successfully baked UI Weapon Panel prefab with full layout!</color>");

                    // Log child count for verification
                    int childCount = instance.transform.childCount;
                    Debug.Log($"<color=cyan>[UIWeaponPageEditorTool] Prefab has {childCount} root children.</color>");

                    // Verify details panel was created
                    var detailsPanel = instance.GetComponentInChildren<UIWeaponDetailsPanel>(true);
                    if (detailsPanel != null)
                        Debug.Log("<color=cyan>[UIWeaponPageEditorTool] ✅ UIWeaponDetailsPanel found and serialized.</color>");
                    else
                        Debug.LogWarning("[UIWeaponPageEditorTool] ⚠ UIWeaponDetailsPanel NOT found in baked prefab!");
                }
                else
                {
                    Debug.LogError("[UIWeaponPageEditorTool] ❌ Failed to save prefab!");
                }
            }
            else
            {
                Debug.LogError("[UIWeaponPageEditorTool] UIWeaponPage component not found on prefab.");
            }

            Object.DestroyImmediate(instance);
            AssetDatabase.Refresh();
        }
    }
}
#endif
