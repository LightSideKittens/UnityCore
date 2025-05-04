#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace LSCore.Extensions.Unity
{
    public static class SerializedPropertyExtensions
    {
        private static readonly Dictionary<Object, PropertyTree> treeByObject = new();

        static readonly Func<SerializedObject, string, SerializedProperty> findRefPath =
            (Func<SerializedObject, string, SerializedProperty>)
            typeof(SerializedObject).GetMethod(
                "FindFirstPropertyFromManagedReferencePath",
                BindingFlags.Instance | BindingFlags.NonPublic)
                .CreateDelegate(typeof(Func<SerializedObject, string, SerializedProperty>));
        
        public static SerializedProperty FindAnyProperty(this SerializedObject so, string path)
        {
            var sp = so.FindProperty(path);
            if (sp != null || !path.StartsWith("managedReferences[")) 
                return sp;

            sp = findRefPath(so, path);
            if (sp == null)
            {
                var it = so.GetIterator();
                while (it.Next(true))
                {
                    if (it.propertyPath == path)
                        return it.Copy();
                }
            }
            return sp;
        }
        
        public static Type GetFieldType(this SerializedProperty property)
        {
            var target = property.serializedObject.targetObject;
            if(target == null) return null;

            if (!treeByObject.TryGetValue(target, out var tree))
            { 
                tree = PropertyTree.Create(target);
                Add();
            }
            
            InspectorProperty op = tree.GetPropertyAtUnityPath(property.propertyPath);
            var type = op?.ValueEntry.TypeOfValue;
            
            if(type != null) return type;
            
            /*if (SerializedPropertyUtilities.GetUnityTypeWithName(target.GetType().Name) != null)
            {
                string properTypeName = property.GetProperTypeName();
                if (properTypeName == "Prefab") return typeof(GameObject); 
                return SerializedPropertyUtilities.GetUnityTypeWithName(properTypeName);
            }*/

            return null;

            async void Add()
            {
                treeByObject.Add(target, tree);
                await Task.Delay(TimeSpan.FromMinutes(1));
                tree.Dispose();
                treeByObject.Remove(target);
            }
        }
    }
}

#endif