using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class AssetIconSetter : OdinEditorWindow
{
    [SerializeField] private Object asset;
    [SerializeField] private Texture2D icon;

    [Button]
    private void Set()
    {
        EditorGUIUtility.SetIconForObject(asset, icon);
    }
    
    [MenuItem(LSPaths.Windows.AssetIconSetter)]
    private static void OpenWindow()
    {
        GetWindow<AssetIconSetter>().Show();
    }
}
