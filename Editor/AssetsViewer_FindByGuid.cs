using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

internal partial class AssetsViewer
{
    [TabGroup("Tabs", "Find By GUID")]
    [OnValueChanged("OnGuidChanged")]
    public string guid;

    [TabGroup("Tabs", "Find By GUID")]
    public Object foundObject;
    
    private void OnGuidChanged()
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        foundObject = AssetDatabase.LoadMainAssetAtPath(path);
    }
}