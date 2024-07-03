#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System.Linq;

public static class SingleAssetsManager
{
    private const string GroupName = "SingleAssets";

    public static void Add(Object asset)
    {
        AddressableAssetSettings addressableSettings = AddressableAssetSettingsDefaultObject.GetSettings(false);
        if (addressableSettings == null)
        {
            Debug.LogError("Addressable Asset Settings not found. Please create Addressable settings first.");
            return;
        }

        AddressableAssetGroup group = FindOrCreateGroup(addressableSettings);

        string assetPath = AssetDatabase.GetAssetPath(asset);
        string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrEmpty(assetGUID))
        {
            Debug.LogError("Invalid asset path: " + assetPath);
            return;
        }

        AddressableAssetEntry entry = addressableSettings.FindAssetEntry(assetGUID);
        if (entry == null)
        {
            entry = addressableSettings.CreateOrMoveEntry(assetGUID, group);
            Debug.Log("Asset added to Addressable group: " + GroupName);
        }   

        entry.address = asset.name;
        EditorUtility.SetDirty(addressableSettings);
        AssetDatabase.SaveAssets();
        
    }

    public static bool Exist(Object asset)
    {
        AddressableAssetSettings addressableSettings = AddressableAssetSettingsDefaultObject.GetSettings(false);
        if (addressableSettings == null)
        {
            Debug.LogError("Addressable Asset Settings not found. Please create Addressable settings first.");
            return false;
        }

        AddressableAssetGroup group = FindOrCreateGroup(addressableSettings);

        string assetPath = AssetDatabase.GetAssetPath(asset);
        string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrEmpty(assetGUID))
        {
            Debug.LogError("Invalid asset path: " + assetPath);
            return false;
        }
        
        
        AddressableAssetEntry entry = group.entries.FirstOrDefault(e => e.guid == assetGUID);
        return entry != null;
    }

    private static AddressableAssetGroup FindOrCreateGroup(AddressableAssetSettings addressableSettings)
    {
        AddressableAssetGroup group = addressableSettings.FindGroup(GroupName);
        if (group == null)
        {
            group = addressableSettings.CreateGroup(GroupName, false, false, false, null,
                typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
            Debug.Log("Addressable group created: " + GroupName);
        }

        return group;
    }

    public static bool TryAdd(Object asset)
    {
        if (!Exist(asset))
        {
            Add(asset);
            return true;
        }

        return false;
    }
}

#endif