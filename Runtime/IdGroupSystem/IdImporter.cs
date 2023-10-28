using LSCore.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(2, "id")]
public class IdImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var idAsset = ScriptableObject.CreateInstance<Id>();
        ctx.AddObjectToAsset("main", idAsset, LSIcons.Get("fire-icon"));
        ctx.SetMainObject(idAsset);
    }
}