using UnityEngine;
using UnityEditor;
using Watermelon.SquadShooter;
using Watermelon.Upgrades;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace Watermelon.SquadShooter
{
    public class AddressablesMigrationTool : EditorWindow
    {
        [MenuItem("GunShooter/Addressables Migration Tool")]
        public static void ShowWindow()
        {
            GetWindow<AddressablesMigrationTool>("Addressables Migration");
        }

        private void OnGUI()
        {
            GUILayout.Label("Migration Tool", EditorStyles.boldLabel);

            if (GUILayout.Button("Migrate Characters, Weapons, Drones"))
            {
                MigrateData();
            }
        }

        private void MigrateData()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Addressable settings not found! Please create it first in Window > Asset Management > Addressables > Groups.");
                return;
            }

            AddressableAssetGroup defaultGroup = settings.DefaultGroup;

            // 1. Migrate Characters
            string[] characterDbGuids = AssetDatabase.FindAssets("t:CharactersDatabase");
            foreach (string guid in characterDbGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CharactersDatabase db = AssetDatabase.LoadAssetAtPath<CharactersDatabase>(path);
                if (db != null)
                {
                    SerializedObject so = new SerializedObject(db);
                    SerializedProperty charactersProp = so.FindProperty("characters");
                    for (int i = 0; i < charactersProp.arraySize; i++)
                    {
                        SerializedProperty stagesProp = charactersProp.GetArrayElementAtIndex(i).FindPropertyRelative("stages");
                        for (int j = 0; j < stagesProp.arraySize; j++)
                        {
                            SerializedProperty stageProp = stagesProp.GetArrayElementAtIndex(j);
                            SerializedProperty prefabProp = stageProp.FindPropertyRelative("prefab");
                            SerializedProperty prefabRefProp = stageProp.FindPropertyRelative("prefabRef");

                            if (prefabProp.objectReferenceValue != null)
                            {
                                GameObject prefab = prefabProp.objectReferenceValue as GameObject;
                                string assetPath = AssetDatabase.GetAssetPath(prefab);
                                string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                                
                                var entry = settings.CreateOrMoveEntry(assetGuid, defaultGroup, readOnly: false, postEvent: false);
                                entry.SetAddress(prefab.name);

                                prefabRefProp.FindPropertyRelative("m_AssetGUID").stringValue = assetGuid;
                            }
                        }
                    }
                    so.ApplyModifiedProperties();
                }
            }

            // 2. Migrate Weapons
            string[] weaponDbGuids = AssetDatabase.FindAssets("t:BaseWeaponUpgrade");
            foreach (string guid in weaponDbGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                BaseWeaponUpgrade upgrade = AssetDatabase.LoadAssetAtPath<BaseWeaponUpgrade>(path);
                if (upgrade != null)
                {
                    SerializedObject so = new SerializedObject(upgrade);
                    SerializedProperty upgradesProp = so.FindProperty("upgrades");
                    if (upgradesProp != null)
                    {
                        for (int i = 0; i < upgradesProp.arraySize; i++)
                        {
                            SerializedProperty stageProp = upgradesProp.GetArrayElementAtIndex(i);
                            
                            SerializedProperty wPrefabProp = stageProp.FindPropertyRelative("weaponPrefab");
                            SerializedProperty wPrefabRefProp = stageProp.FindPropertyRelative("weaponPrefabRef");

                            if (wPrefabProp.objectReferenceValue != null)
                            {
                                string assetPath = AssetDatabase.GetAssetPath(wPrefabProp.objectReferenceValue);
                                string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                                var entry = settings.CreateOrMoveEntry(assetGuid, defaultGroup, readOnly: false, postEvent: false);
                                entry.SetAddress(wPrefabProp.objectReferenceValue.name);
                                wPrefabRefProp.FindPropertyRelative("m_AssetGUID").stringValue = assetGuid;
                            }

                            SerializedProperty bPrefabProp = stageProp.FindPropertyRelative("bulletPrefab");
                            SerializedProperty bPrefabRefProp = stageProp.FindPropertyRelative("bulletPrefabRef");

                            if (bPrefabProp.objectReferenceValue != null)
                            {
                                string assetPath = AssetDatabase.GetAssetPath(bPrefabProp.objectReferenceValue);
                                string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                                var entry = settings.CreateOrMoveEntry(assetGuid, defaultGroup, readOnly: false, postEvent: false);
                                entry.SetAddress(bPrefabProp.objectReferenceValue.name);
                                bPrefabRefProp.FindPropertyRelative("m_AssetGUID").stringValue = assetGuid;
                            }
                        }
                    }
                    so.ApplyModifiedProperties();
                }
            }

            // 3. Migrate Drones
            string[] droneGuids = AssetDatabase.FindAssets("t:BaseDroneUpgrade");
            foreach (string guid in droneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject upgrade = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (upgrade != null)
                {
                    SerializedObject so = new SerializedObject(upgrade);
                    SerializedProperty upgradesProp = so.FindProperty("upgrades");
                    if (upgradesProp != null)
                    {
                        for (int i = 0; i < upgradesProp.arraySize; i++)
                        {
                            SerializedProperty stageProp = upgradesProp.GetArrayElementAtIndex(i);
                            
                            SerializedProperty dPrefabProp = stageProp.FindPropertyRelative("dronePrefab");
                            SerializedProperty dPrefabRefProp = stageProp.FindPropertyRelative("dronePrefabRef");

                            if (dPrefabProp.objectReferenceValue != null)
                            {
                                string assetPath = AssetDatabase.GetAssetPath(dPrefabProp.objectReferenceValue);
                                string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                                var entry = settings.CreateOrMoveEntry(assetGuid, defaultGroup, readOnly: false, postEvent: false);
                                entry.SetAddress(dPrefabProp.objectReferenceValue.name);
                                dPrefabRefProp.FindPropertyRelative("m_AssetGUID").stringValue = assetGuid;
                            }

                            SerializedProperty bPrefabProp = stageProp.FindPropertyRelative("bulletPrefab");
                            SerializedProperty bPrefabRefProp = stageProp.FindPropertyRelative("bulletPrefabRef");

                            if (bPrefabProp.objectReferenceValue != null)
                            {
                                string assetPath = AssetDatabase.GetAssetPath(bPrefabProp.objectReferenceValue);
                                string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                                var entry = settings.CreateOrMoveEntry(assetGuid, defaultGroup, readOnly: false, postEvent: false);
                                entry.SetAddress(bPrefabProp.objectReferenceValue.name);
                                bPrefabRefProp.FindPropertyRelative("m_AssetGUID").stringValue = assetGuid;
                            }
                        }
                    }
                    so.ApplyModifiedProperties();
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Migration finished! All characters, weapons, and drones are now Addressable.");
        }
    }
}
#endif
