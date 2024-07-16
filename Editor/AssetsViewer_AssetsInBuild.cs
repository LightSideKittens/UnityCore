using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Sirenix.OdinInspector;

internal partial class AssetsViewer
{
    [TabGroup("Tabs", "Assets in Build")]
    [TableList(HideToolbar = true, IsReadOnly = true, AlwaysExpanded = true)]
    public List<AssetInfo> buildAssets = new();
    
    private List<AssetInfo> fullBuildAssets = new();
    
    [TabGroup("Tabs", "Assets in Build")]
    [Button("Refresh List")]
    private void RefreshList()
    {
        fullBuildAssets.Clear();
        string[] buildScenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        var dependencies = AssetDatabase.GetDependencies(buildScenes);

        foreach (var assetPath in dependencies)
        {
            fullBuildAssets.Add(BuildInfo(assetPath));
        }
        
        FilterBuildAssets();
    }

    private void FilterBuildAssets() => FilterAssets(fullBuildAssets, buildAssets);
}
