#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore
{
    public class AssetRefDrawer<TAssetRef, TAsset> : OdinValueDrawer<TAssetRef> where TAssetRef : AssetRef<TAsset> where TAsset : Object
    {
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
                
            if (newAsset != null && newAsset != currentAsset)
            {
                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newAsset));
                var assetRef = (TAssetRef)Activator.CreateInstance(typeof(TAssetRef));
                assetRef.SetGuid(guid);
                entry.SmartValue = assetRef;
            }
        }
    }
}
#endif