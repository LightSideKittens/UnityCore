//-----------------------------------------------------------------------
// <copyright file="OdinPrefabSerializationEditorUtility.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
//#define PREFAB_DEBUG
#if UNITY_EDITOR
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    public static class OdinPrefabSerializationEditorUtility
    {
        private static bool? hasNewPrefabWorkflow;

        public static bool HasNewPrefabWorkflow
        {
            get
            {
                if (hasNewPrefabWorkflow == null)
                {
                    hasNewPrefabWorkflow = DetectNewPrefabWorkflow();
                }

                return hasNewPrefabWorkflow.Value;
            }
        }

        private static bool DetectNewPrefabWorkflow()
        {
            try
            {
                var method = typeof(PrefabUtility).GetMethod("GetPrefabType", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(UnityEngine.Object) }, null);

                if (method == null) return true;

                if (method.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool ObjectIsPrefabInstance(UnityEngine.Object unityObject)
        {
            var prefabAssetType = PrefabUtility.GetPrefabAssetType(unityObject);
            return prefabAssetType is PrefabAssetType.Regular or PrefabAssetType.Variant;
        }

        public static bool ObjectHasNestedOdinPrefabData(UnityEngine.Object unityObject)
        {
            if (!HasNewPrefabWorkflow) return false;
            if (!(unityObject is ISupportsPrefabSerialization)) return false;
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(unityObject);
            return IsOdinSerializedPrefabInstance(prefab);
        }

        private static bool IsOdinSerializedPrefabInstance(UnityEngine.Object unityObject)
        {
            if (!(unityObject is ISupportsPrefabSerialization)) return false;
            return PrefabUtility.GetCorrespondingObjectFromSource(unityObject) != null;
        }
    }
}
#endif