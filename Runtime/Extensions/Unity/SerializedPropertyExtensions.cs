#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class SerializedPropertyExtensions
    {
        public static Type GetFieldType(this SerializedProperty property)
        {
            if (SerializedPropertyUtilities.GetUnityTypeWithName(property.serializedObject.targetObject.GetType().Name) != null)
            {
                string properTypeName = property.GetProperTypeName();
                if (properTypeName == "Prefab") return typeof(GameObject); 
                return SerializedPropertyUtilities.GetUnityTypeWithName(properTypeName);
            }
            
            var target = property.serializedObject.targetObject;
            var tree = PropertyTree.Create(target);
            InspectorProperty op = tree.GetPropertyAtUnityPath(property.propertyPath);
            var type = op?.ValueEntry.TypeOfValue;
            tree.Dispose();
            return type;
        }
    }
}

#endif