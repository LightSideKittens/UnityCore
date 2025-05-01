//-----------------------------------------------------------------------
// <copyright file="CustomEditorUtility.cs" company="Sirenix ApS">
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

    using Utilities;
    using UnityEngine;
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Collections;
    using UnityEditor;

    public static class CustomEditorUtility
    {
        private static class UniversalAPI
        {
            public static Type CustomEditorAttributesType;
            public static Type MonoEditorType;

            public static FieldInfo MonoEditorType_InspectorType;
            public static FieldInfo MonoEditorType_EditorForChildClasses;
            public static FieldInfo MonoEditorType_IsFallback;

            public static FieldInfo CustomEditor_EditorForChildClassesField;

            public static bool IsValid;

            static UniversalAPI()
            {
                try
                {
                    CustomEditorAttributesType = typeof(Editor).Assembly.GetType("UnityEditor.CustomEditorAttributes");

                    MonoEditorType = CustomEditorAttributesType.GetNestedType("MonoEditorType", Flags.AnyVisibility);
                    MonoEditorType_InspectorType = MonoEditorType.GetField("m_InspectorType", Flags.InstanceAnyVisibility) ?? MonoEditorType.GetField("inspectorType", Flags.InstanceAnyVisibility);
                    MonoEditorType_EditorForChildClasses = MonoEditorType.GetField("m_EditorForChildClasses", Flags.InstanceAnyVisibility) ?? MonoEditorType.GetField("editorForChildClasses", Flags.InstanceAnyVisibility);
                    MonoEditorType_IsFallback = MonoEditorType.GetField("m_IsFallback", Flags.InstanceAnyVisibility) ?? MonoEditorType.GetField("isFallback", Flags.InstanceAnyVisibility);

                    CustomEditor_EditorForChildClassesField = typeof(CustomEditor).GetField("m_EditorForChildClasses", Flags.InstanceAnyVisibility);
                    IsValid = true;
                }
                catch (NullReferenceException)
                {
                    IsValid = false;
                }

                if (IsValid)
                {
                    if (MonoEditorType_InspectorType == null ||
                        MonoEditorType_EditorForChildClasses == null ||
                        MonoEditorType_IsFallback == null ||
                        CustomEditor_EditorForChildClassesField == null)
                    {
                        IsValid = false;
                    }
                }
            }
        }

        private static class Unity_2023_1_API
        {
            public static readonly PropertyInfo CustomEditorAttributesType_Instance;
            public static readonly MethodInfo CustomEditorAttributesType_Rebuild;
            public static readonly FieldInfo CustomEditorAttributesType_Cache;

            public static readonly Type CustomEditorCache_Type;
            public static readonly FieldInfo CustomEditorCache_CustomEditorCacheDict;

            public static readonly Type MonoEditorTypeStorage_Type;
            public static readonly FieldInfo MonoEditorTypeStorage_CustomEditors;
            public static readonly FieldInfo MonoEditorTypeStorage_CustomEditorsMultiEdition;

            public static readonly Type Dictionary_Type_MonoEditorTypeStorage;
            public static readonly MethodInfo Dictionary_Type_MonoEditorTypeStorage_Add;
            public static readonly MethodInfo Dictionary_Type_MonoEditorTypeStorage_TryGetValue;

            public static bool IsValid;

            static Unity_2023_1_API()
            {
                if (!UniversalAPI.IsValid)
                {
                    IsValid = false;
                    return;
                }

                try
                {
                    CustomEditorAttributesType_Instance = UniversalAPI.CustomEditorAttributesType.GetProperty("instance", Flags.StaticAnyVisibility);
                    CustomEditorAttributesType_Rebuild = UniversalAPI.CustomEditorAttributesType.GetMethod("Rebuild", Flags.StaticInstanceAnyVisibility, null, Type.EmptyTypes, null);
                    CustomEditorAttributesType_Cache = UniversalAPI.CustomEditorAttributesType.GetField("m_Cache", Flags.InstanceAnyVisibility);

                    MonoEditorTypeStorage_Type = UniversalAPI.CustomEditorAttributesType.GetNestedType("MonoEditorTypeStorage", Flags.AnyVisibility);
                    MonoEditorTypeStorage_CustomEditors = MonoEditorTypeStorage_Type.GetField("customEditors", Flags.InstanceAnyVisibility);
                    MonoEditorTypeStorage_CustomEditorsMultiEdition = MonoEditorTypeStorage_Type.GetField("customEditorsMultiEdition", Flags.InstanceAnyVisibility);

                    CustomEditorCache_Type = UniversalAPI.CustomEditorAttributesType.GetNestedType("CustomEditorCache", Flags.AnyVisibility);
                    CustomEditorCache_CustomEditorCacheDict = CustomEditorCache_Type.GetField("m_CustomEditorCache", Flags.InstanceAnyVisibility);

                    Dictionary_Type_MonoEditorTypeStorage = typeof(Dictionary<,>).MakeGenericType(typeof(Type), MonoEditorTypeStorage_Type);
                    Dictionary_Type_MonoEditorTypeStorage_Add = Dictionary_Type_MonoEditorTypeStorage.GetMethod("Add", Flags.InstancePublic, null, new Type[] { typeof(Type), MonoEditorTypeStorage_Type }, null);
                    Dictionary_Type_MonoEditorTypeStorage_TryGetValue = Dictionary_Type_MonoEditorTypeStorage.GetMethod("TryGetValue", Flags.InstancePublic, null, new Type[] { typeof(Type), MonoEditorTypeStorage_Type.MakeByRefType() }, null);

                    if (CustomEditorCache_CustomEditorCacheDict.FieldType != Dictionary_Type_MonoEditorTypeStorage
                        || CustomEditorAttributesType_Rebuild == null
                        || CustomEditorAttributesType_Cache == null
                        || MonoEditorTypeStorage_CustomEditors == null
                        || MonoEditorTypeStorage_CustomEditorsMultiEdition == null
                        || CustomEditorCache_CustomEditorCacheDict == null
                        || Dictionary_Type_MonoEditorTypeStorage_Add == null
                        || Dictionary_Type_MonoEditorTypeStorage_TryGetValue == null)
                        return;

                    IsValid = true;
                }
                catch (NullReferenceException)
                {
                    IsValid = false;
                }
            }

            public static void ResetCustomEditors()
            {
                if (!IsValid) return;

                if (CustomEditorAttributesType_Rebuild.IsStatic)
                {
                    CustomEditorAttributesType_Rebuild.Invoke(null, null);
                }
                else
                {
                    // Pre 2023.1.0a19
                    object instance = CustomEditorAttributesType_Instance.GetValue(null);
                    CustomEditorAttributesType_Rebuild.Invoke(instance, null);
                }
            }

            public static void RegisterCustomMonoEditorEntry(object entry, Type inspectedType, Type editorType, bool isMultiEditor)
            {
                if (!IsValid) return;

                object instance = CustomEditorAttributesType_Instance.GetValue(null);
                object cache = CustomEditorAttributesType_Cache.GetValue(instance);
                object cacheDict = CustomEditorCache_CustomEditorCacheDict.GetValue(cache);

                object[] args = new object[] { inspectedType, null };

                bool exists = (bool)Dictionary_Type_MonoEditorTypeStorage_TryGetValue.Invoke(cacheDict, args);

                if (!exists)
                {
                    args[1] = Activator.CreateInstance(MonoEditorTypeStorage_Type);
                    MonoEditorTypeStorage_CustomEditors.SetValue(args[1], Activator.CreateInstance(MonoEditorTypeStorage_CustomEditors.FieldType));
                    MonoEditorTypeStorage_CustomEditorsMultiEdition.SetValue(args[1], Activator.CreateInstance(MonoEditorTypeStorage_CustomEditorsMultiEdition.FieldType));
                    Dictionary_Type_MonoEditorTypeStorage_Add.Invoke(cacheDict, args);
                }

                object monoEditorTypeStorage = args[1];

                // When Odin wants to go first, Odin goes first! None of this Unity first sorting, we insert at 0.
                IList editorsList = MonoEditorTypeStorage_CustomEditors.GetValue(monoEditorTypeStorage) as IList;
                editorsList.Insert(0, entry);

                if (isMultiEditor)
                {
                    IList multiEditorsList = MonoEditorTypeStorage_CustomEditorsMultiEdition.GetValue(monoEditorTypeStorage) as IList;
                    multiEditorsList.Insert(0, entry);
                }

            }
        }

        private static class Unity_Pre_2023_API
        {
            public static readonly FieldInfo CustomEditorAttributesType_CachedEditorForType;
            public static readonly FieldInfo CustomEditorAttributesType_CachedMultiEditorForType;
            public static readonly FieldInfo CustomEditorAttributesType_CustomEditors;
            public static readonly FieldInfo CustomEditorAttributesType_CustomMultiEditors;
            public static readonly FieldInfo CustomEditorAttributesType_Initialized;
            public static readonly MethodInfo CustomEditorAttributesType_Rebuild;

            public static FieldInfo MonoEditorType_InspectedType;

            public static readonly bool IsBackedByADictionary;
            public static bool IsValid;

            static Unity_Pre_2023_API()
            {
                if (!UniversalAPI.IsValid)
                {
                    IsValid = false;
                    return;
                }

                try
                {
                    CustomEditorAttributesType_Initialized = UniversalAPI.CustomEditorAttributesType.GetField("s_Initialized", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    CustomEditorAttributesType_CachedEditorForType = UniversalAPI.CustomEditorAttributesType.GetField("kCachedEditorForType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    CustomEditorAttributesType_CachedMultiEditorForType = UniversalAPI.CustomEditorAttributesType.GetField("kCachedMultiEditorForType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    CustomEditorAttributesType_CustomEditors = UniversalAPI.CustomEditorAttributesType.GetField("kSCustomEditors", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    CustomEditorAttributesType_CustomMultiEditors = UniversalAPI.CustomEditorAttributesType.GetField("kSCustomMultiEditors", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    CustomEditorAttributesType_Rebuild = UniversalAPI.CustomEditorAttributesType.GetMethod("Rebuild", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                    MonoEditorType_InspectedType = UniversalAPI.MonoEditorType.GetField("m_InspectedType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    // CustomEditorAttributesType_SingleTypeDictField and CustomEditorAttributesType_MultiTypeDictField can be null
                    if (CustomEditorAttributesType_Initialized == null
                        || CustomEditorAttributesType_CustomEditors == null
                        || CustomEditorAttributesType_CustomMultiEditors == null
                        || MonoEditorType_InspectedType == null)
                    {
                        throw new NullReferenceException();
                    }

                    // This is true for some newer versions of 2017.4 and 2018.x
                    IsBackedByADictionary = typeof(IDictionary).IsAssignableFrom(CustomEditorAttributesType_CustomEditors.FieldType);
                    IsValid = true;
                }
                catch (NullReferenceException)
                {
                    IsValid = false;
                }
            }

            public static void ResetCustomEditors()
            {
                if (!IsValid) return;

                if (IsBackedByADictionary)
                {
                    ((IDictionary)CustomEditorAttributesType_CustomEditors.GetValue(null)).Clear();
                    ((IDictionary)CustomEditorAttributesType_CustomMultiEditors.GetValue(null)).Clear();
                }
                else
                {
                    if (CustomEditorAttributesType_CachedEditorForType != null)
                    {
                        ((Dictionary<Type, Type>)CustomEditorAttributesType_CachedEditorForType.GetValue(null)).Clear();
                    }
                    if (CustomEditorAttributesType_CachedMultiEditorForType != null)
                    {
                        ((Dictionary<Type, Type>)CustomEditorAttributesType_CachedMultiEditorForType.GetValue(null)).Clear();
                    }
                    ((IList)CustomEditorAttributesType_CustomEditors.GetValue(null)).Clear();
                    ((IList)CustomEditorAttributesType_CustomMultiEditors.GetValue(null)).Clear();
                }

                if (UnityVersion.IsVersionOrGreater(2019, 1))
                {
                    // Manually trigger a Rebuild instead of setting Initialized to false.
                    CustomEditorAttributesType_Rebuild.Invoke(null, null);
                    CustomEditorAttributesType_Initialized.SetValue(null, true); // Ensure Unity doesn't do a second rebuild again.
                }
                else
                {
                    CustomEditorAttributesType_Initialized.SetValue(null, false);
                }
            }

            public static void RegisterCustomMonoEditorEntry(object entry, Type inspectedType, Type editorType, bool isMultiEditor)
            {
                if (!IsValid) return;

                MonoEditorType_InspectedType.SetValue(entry, inspectedType);

                if (IsBackedByADictionary)
                {
                    AddEntryToDictList((IDictionary)CustomEditorAttributesType_CustomEditors.GetValue(null), entry, inspectedType);

                    if (isMultiEditor)
                    {
                        AddEntryToDictList((IDictionary)CustomEditorAttributesType_CustomMultiEditors.GetValue(null), entry, inspectedType);
                    }
                }
                else
                {
                    if (CustomEditorAttributesType_CachedEditorForType != null && CustomEditorAttributesType_CachedMultiEditorForType != null)
                    {
                        // Just set the dictionary cache
                        ((IDictionary)CustomEditorAttributesType_CachedEditorForType.GetValue(null))[inspectedType] = editorType;

                        if (isMultiEditor)
                        {
                            ((IDictionary)CustomEditorAttributesType_CachedMultiEditorForType.GetValue(null))[inspectedType] = editorType;
                        }
                    }

                    // Insert a new type entry at the beginning of the relevant lists
                    {
                        ((IList)CustomEditorAttributesType_CustomEditors.GetValue(null)).Insert(0, entry);

                        if (isMultiEditor)
                        {
                            ((IList)CustomEditorAttributesType_CustomMultiEditors.GetValue(null)).Insert(0, entry);
                        }
                    }
                }
            }

            private static void AddEntryToDictList(IDictionary dict, object entry, Type inspectedType)
            {
                IList list;

                if (dict.Contains(inspectedType))
                {
                    list = (IList)dict[inspectedType];
                }
                else
                {
                    list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(UniversalAPI.MonoEditorType));
                    dict[inspectedType] = list;
                }

                list.Insert(0, entry);
            }
        }

        public static readonly bool IsValid;

        static CustomEditorUtility()
        {
            IsValid = UniversalAPI.IsValid &&
                (Unity_2023_1_API.IsValid || Unity_Pre_2023_API.IsValid);

            if (!IsValid)
            {
                Debug.LogError("Unity's internal custom editor management classes have changed in this version of Unity (" + Application.unityVersion + "). Odin will not be able to dynamically register any editors; only hardcoded Odin editors will work.");
            }
        }

        public static void ResetCustomEditors()
        {
            if (Unity_2023_1_API.IsValid)
            {
                Unity_2023_1_API.ResetCustomEditors();
            }
            else if (Unity_Pre_2023_API.IsValid)
            {
                Unity_Pre_2023_API.ResetCustomEditors();
            }
        }

        public static void SetCustomEditor(Type inspectedType, Type editorType)
        {
            if (!IsValid) return;

            var attr = editorType.GetCustomAttribute<UnityEditor.CustomEditor>();
            if (attr == null) throw new ArgumentException("Editor type to set '" + editorType.GetNiceName() + "' has no CustomEditor attribute applied! Use a SetCustomEditor overload that takes isFallbackEditor and isEditorForChildClasses parameters.");
            SetCustomEditor(inspectedType, editorType, attr.isFallback, (bool)UniversalAPI.CustomEditor_EditorForChildClassesField.GetValue(attr));
        }

        public static void SetCustomEditor(Type inspectedType, Type editorType, bool isFallbackEditor, bool isEditorForChildClasses)
        {
            if (!IsValid) return;

            SetCustomEditor(inspectedType, editorType, isFallbackEditor, isEditorForChildClasses, editorType.IsDefined<UnityEditor.CanEditMultipleObjects>());
        }

        public static void SetCustomEditor(Type inspectedType, Type editorType, bool isFallbackEditor, bool isEditorForChildClasses, bool isMultiEditor)
        {
            if (!IsValid) return;

            object entry = Activator.CreateInstance(UniversalAPI.MonoEditorType);

            UniversalAPI.MonoEditorType_InspectorType.SetValue(entry, editorType);
            UniversalAPI.MonoEditorType_IsFallback.SetValue(entry, isFallbackEditor);
            UniversalAPI.MonoEditorType_EditorForChildClasses.SetValue(entry, isEditorForChildClasses);

            if (Unity_2023_1_API.IsValid)
            {
                Unity_2023_1_API.RegisterCustomMonoEditorEntry(entry, inspectedType, editorType, isMultiEditor);
            }
            else if (Unity_Pre_2023_API.IsValid)
            {
                Unity_Pre_2023_API.RegisterCustomMonoEditorEntry(entry, inspectedType, editorType, isMultiEditor);
            }
        }
    }
}
#endif