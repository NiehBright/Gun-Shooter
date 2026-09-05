using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class InitializeAddressables
{
    [MenuItem("Tools/Khởi tạo Addressables")]
    public static void Initialize()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            Debug.Log("[Addressables] Đã tạo mới Addressables Settings.");
        }

        // Tạo các Group cơ bản
        CreateOrGetGroup(settings, "Weapons");
        CreateOrGetGroup(settings, "Drones");
        CreateOrGetGroup(settings, "Characters");

        Debug.Log("[Addressables] Hoàn tất thiết lập Group!");
    }

    private static void CreateOrGetGroup(AddressableAssetSettings settings, string groupName)
    {
        AddressableAssetGroup group = settings.FindGroup(groupName);
        if (group == null)
        {
            group = settings.CreateGroup(groupName, false, false, true, settings.DefaultGroup.Schemas);
            Debug.Log($"[Addressables] Đã tạo Group: {groupName}");
        }
    }
}
