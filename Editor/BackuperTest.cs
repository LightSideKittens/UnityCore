/*using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LightSideCore.Editor
{
    [InitializeOnLoad]
    public static class BackuperTest
    {
        static BackuperTest()
        {
            Patchers._EditorUtility.SetDirty.Called += OnChanged;
            
            Patchers._SerializedObject.ApplyModifiedProperties.Called += (obj, res) =>
            {
                if (res)
                {
                    OnChanged(obj.targetObject);
                }
            };
        }

        private static void OnChanged(Object obj)
        {
            if (obj is Component comp)
            {
                obj = comp.gameObject;
            }
            
            if (obj is GameObject go)
            {
                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                
                if (prefabStage != null && prefabStage.IsPartOfPrefabContents(go))
                {
                    obj = AssetDatabase.LoadAssetAtPath<Object>(prefabStage.assetPath);
                }
                else if (PrefabUtility.IsPartOfPrefabAsset(obj))
                {
                    if (!AssetDatabase.Contains(obj))
                    {
                        obj = PrefabUtility.GetCorrespondingObjectFromSource(go);
                    }
                }
                else if(go.scene.IsValid())
                {
                    obj = AssetDatabase.LoadAssetAtPath<SceneAsset>(go.scene.path);
                }
            }
            else if (!AssetDatabase.Contains(obj))
            {
                return;
            }

            string path = AssetDatabase.GetAssetPath(obj);
            string data = File.ReadAllText(path);
            
            AssetDatabase.SaveAssetIfDirty(obj);
            
            path = AssetDatabase.GetAssetPath(obj);
            string backupData = File.ReadAllText(path);
            
            File.WriteAllText(path, data);
            Patchers._EditorUtility.SetDirty.Called -= OnChanged;
            EditorUtility.SetDirty(obj);
            AssetDatabase.ImportAsset(path);
            Patchers._EditorUtility.SetDirty.Called += OnChanged;
        }
    }
}*/