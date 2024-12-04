using System.IO;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace LSCore.Editor
{
    public class SourcePrefabChanger : OdinEditorWindow
    {
        [SerializeField] private GameObject target;
        [SerializeField] private GameObject newSourcePrefab;

        [Button]
        public void Change()
        {
            var path = AssetDatabase.GetAssetPath(target);
            var targetText = File.ReadAllText(path);
            
            var newSourcePrefabGuid = AssetDatabaseUtils.GetGUID(newSourcePrefab, out _);
            var oldSourcePrefabGuid = Regex.Match(targetText, "m_SourcePrefab.+guid: (.+),").Groups[1].Value;
            targetText = targetText.Replace(oldSourcePrefabGuid, newSourcePrefabGuid);
            File.WriteAllText(path, targetText);
            
            AssetDatabase.Refresh();
        }
        
        [MenuItem(LSPaths.Windows.SourcePrefabChanger)]
        private static void OpenWindow()
        {
            GetWindow<SourcePrefabChanger>().Show();
        }
    }
}