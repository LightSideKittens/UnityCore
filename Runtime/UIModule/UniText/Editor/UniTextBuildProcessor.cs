using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;


public class UniTextBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        var guids = AssetDatabase.FindAssets("t:UniTextFontAsset");
        int clearedCount = 0;

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var fontAsset = AssetDatabase.LoadAssetAtPath<UniTextFont>(path);

            if (fontAsset == null || !fontAsset.ClearDynamicDataOnBuild)
                continue;

            var serializedObject = new SerializedObject(fontAsset);
            var glyphTableProp = serializedObject.FindProperty("glyphTable");
            var characterTableProp = serializedObject.FindProperty("characterTable");

            int glyphCount = glyphTableProp?.arraySize ?? 0;
            int charCount = characterTableProp?.arraySize ?? 0;

            if (glyphCount > 0 || charCount > 0)
            {
                fontAsset.ClearDynamicData();
                EditorUtility.SetDirty(fontAsset);
                clearedCount++;

                Debug.Log($"[UniText Build] Cleared dynamic data from '{fontAsset.name}': " +
                          $"{glyphCount} glyphs, {charCount} characters");
            }
        }

        if (clearedCount > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"[UniText Build] Cleared dynamic data from {clearedCount} font asset(s)");
        }
    }
}
