#if UNITY_EDITOR
using System;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore
{
    public class AssetRefDrawer<TAssetRef, TAsset> : OdinValueDrawer<TAssetRef> where TAssetRef : AssetRef<TAsset> where TAsset : Object
    {
        private ConstructorInfo constructor;

        protected override void Initialize()
        {
            base.Initialize();
            constructor = ValueEntry.TypeOfValue.GetConstructor(new[] { typeof(string) });
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = ValueEntry;
            var rect = EditorGUILayout.GetControlRect();

            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }
                
            var currentAsset = entry.SmartValue?.editorAsset;
            var newAsset = EditorGUI.ObjectField(rect, currentAsset, typeof(TAsset), false);
                
            if (newAsset != currentAsset)
            {
                string guid = newAsset ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newAsset)) : null;
                entry.SmartValue = (TAssetRef)constructor?.Invoke(new object[] {guid});
            }
        }
    }
}
#endif