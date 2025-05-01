//-----------------------------------------------------------------------
// <copyright file="AddressablesUtility.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    public static class AddressablesUtility
    {
        public static readonly bool AddressablesAvailable;
        private static Type AddressableAssetSettingsDefaultObject_Type;
        private static PropertyInfo AddressableAssetSettingsDefaultObject_Settings;
        private static Type AddressableAssetSettings_Type;
        private static PropertyInfo AddressableAssetSettings_groups;
        private static Type AddressableAssetEntry_Type;
        private static Type List_AddressableAssetEntry_Type;
        private static Type Func_AddressableAssetEntry_bool_Type;
        private static PropertyInfo AddressableAssetEntry_AssetPath;
        private static Type AddressableAssetGroup_Type;
        private static PropertyInfo AddressableAssetGroup_Name;
        private static MethodInfo AddressableAssetGroup_HasSchema;
        private static MethodInfo AddressableAssetGroup_GatherAllAssets;

        static AddressablesUtility()
        {
            try
            {
                if (!UnityPackageUtility.HasPackageInstalled("com.unity.addressables", new Version(1, 2, 2)))
                {
                    AddressablesAvailable = false;
                    return;
                }

                AddressableAssetSettingsDefaultObject_Type = TwoWaySerializationBinder.Default.BindToType("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject");
                if (AddressableAssetSettingsDefaultObject_Type == null) throw new NotSupportedException("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject type not found");

                AddressableAssetSettingsDefaultObject_Settings = AddressableAssetSettingsDefaultObject_Type.GetProperty("Settings");
                if (AddressableAssetSettingsDefaultObject_Settings == null) throw new NotSupportedException("AddressableAssetSettingsDefaultObject.Settings property not found");

                AddressableAssetSettings_Type = TwoWaySerializationBinder.Default.BindToType("UnityEditor.AddressableAssets.Settings.AddressableAssetSettings");
                if (AddressableAssetSettings_Type == null) throw new NotSupportedException("UnityEditor.AddressableAssets.Settings.AddressableAssetSettings type not found");

                AddressableAssetSettings_groups = AddressableAssetSettings_Type.GetProperty("groups");
                if (AddressableAssetSettings_groups == null) throw new NotSupportedException("AddressableAssetSettings.groups property not found");

                AddressableAssetEntry_Type = TwoWaySerializationBinder.Default.BindToType("UnityEditor.AddressableAssets.Settings.AddressableAssetEntry");
                if (AddressableAssetEntry_Type == null) throw new NotSupportedException("AddressableAssetEntry type not found");
                List_AddressableAssetEntry_Type = typeof(List<>).MakeGenericType(AddressableAssetEntry_Type);
                Func_AddressableAssetEntry_bool_Type = typeof(Func<,>).MakeGenericType(AddressableAssetEntry_Type, typeof(bool));
                AddressableAssetEntry_AssetPath = AddressableAssetEntry_Type.GetProperty("AssetPath");
                if (AddressableAssetEntry_AssetPath == null) throw new NotSupportedException("AddressableAssetEntry.AssetPath property not found");

                AddressableAssetGroup_Type = TwoWaySerializationBinder.Default.BindToType("UnityEditor.AddressableAssets.Settings.AddressableAssetGroup");
                if (AddressableAssetGroup_Type == null) throw new NotSupportedException("UnityEditor.AddressableAssets.Settings.AddressableAssetGroup type not found");

                AddressableAssetGroup_Name = AddressableAssetGroup_Type.GetProperty("Name");
                if (AddressableAssetGroup_Type == null) throw new NotSupportedException("UnityEditor.AddressableAssets.Settings.AddressableAssetGroup.Name property not found");

                AddressableAssetGroup_HasSchema = AddressableAssetGroup_Type.GetMethod("HasSchema", Flags.InstancePublic, null, new Type[] { typeof(Type) }, null);
                if (AddressableAssetGroup_HasSchema == null) throw new NotSupportedException("AddressableAssetGroup.HasSchema(Type type) method not found");
                AddressableAssetGroup_GatherAllAssets = AddressableAssetGroup_Type.GetMethod("GatherAllAssets", Flags.InstancePublic, null, new Type[] { List_AddressableAssetEntry_Type, typeof(bool), typeof(bool), typeof(bool), Func_AddressableAssetEntry_bool_Type }, null);
                if (AddressableAssetGroup_GatherAllAssets == null) throw new NotSupportedException("AddressableAssetGroup.GatherAllAssets(List<AddressableAssetEntry> results, bool includeSelf, bool recurseAll, bool includeSubObjects, Func<AddressableAssetEntry, bool> entryFilter) method not found");

                AddressablesAvailable = true;
            }
            catch (NotSupportedException ex)
            {
                Debug.LogWarning("Addressables API has changed in this version of Unity, this API was missing: " + ex.Message);
                AddressablesAvailable = false;
            }
        }

        public static List<string> GetAddressableGroupNames()
        {
            if (!AddressablesAvailable) return new List<string>();

            List<string> results = new List<string>();
            ScriptableObject settings = (ScriptableObject)AddressableAssetSettingsDefaultObject_Settings.GetValue(null, null);
            if (settings != null)
            {
                IList groups = (IList)AddressableAssetSettings_groups.GetValue(settings, null);
                if (groups != null)
                {
                    foreach (object groupObj in groups)
                    {
                        ScriptableObject group = (ScriptableObject)groupObj;
                        if (group == null) continue;
                        string name = (string)AddressableAssetGroup_Name.GetValue(group, null);
                        results.Add(name);
                    }
                }
            }
            return results;
        }

        public static List<string> GetAssetPathsInGroup(string name)
        {
            if (!AddressablesAvailable) return new List<string>();

            ScriptableObject settings = (ScriptableObject)AddressableAssetSettingsDefaultObject_Settings.GetValue(null, null);
            if (settings != null)
            {
                IList groups = (IList)AddressableAssetSettings_groups.GetValue(settings, null);
                if (groups != null)
                {
                    foreach (object groupObj in groups)
                    {
                        ScriptableObject group = (ScriptableObject)groupObj;
                        if (group == null) continue;
                        string groupName = (string)AddressableAssetGroup_Name.GetValue(group, null);

                        if (groupName == name)
                        {
                            IList entries = (IList)Activator.CreateInstance(List_AddressableAssetEntry_Type);
                            AddressableAssetGroup_GatherAllAssets.Invoke(group, new object[] { entries, true, true, true, null });

                            List<string> results = new List<string>(entries.Count);

                            for (int i = 0; i < entries.Count; i++)
                            {
                                object entry = (object)entries[i];
                                if (entry == null) continue;
                                string assetPath = (string)AddressableAssetEntry_AssetPath.GetValue(entry, null);
                                results.Add(assetPath);
                            }

                            return results;
                        }
                    }
                }
            }
            return new List<string>();
        }
    }
}
#endif