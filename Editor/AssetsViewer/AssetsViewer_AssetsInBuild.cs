using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Sirenix.OdinInspector;

internal partial class AssetsViewer
{
    [TabGroup("Tabs", "Assets in Build")]
    [TableList(IsReadOnly = true, AlwaysExpanded = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<AssetInfo> buildAssets = new();
    
    private List<AssetInfo> fullBuildAssets = new();
    
    [TabGroup("Tabs", "Assets in Build")]
    [Button("Refresh List")]
    private void RefreshList()
    {
        fullBuildAssets.Clear();
        string[] buildScenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        var dependencies = new HashSet<string>();

        for (int i = 0; i < buildScenes.Length; i++)
        {
            dependencies.Clear();
            AssetDatabaseUtils.GetUsesIndirect(buildScenes[i], dependencies, true);

            foreach (var assetPath in dependencies)
            {
                var assets = AssetDatabaseUtils.LoadAllSubAsset(assetPath, true);
                for (int j = 0; j < assets.Count; j++)
                {
                    fullBuildAssets.Add(BuildInfo(assets[j], j-1));
                }
            }
        }
        
        FilterBuildAssets();
    }
    
    private void FilterBuildAssets() => FilterAssets(fullBuildAssets, buildAssets);
}
