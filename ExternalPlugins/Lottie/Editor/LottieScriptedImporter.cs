#if UNITY_EDITOR
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "lottie")]
public class LottieScriptedImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var text = File.ReadAllText(ctx.assetPath);

        var asset = ScriptableObject.CreateInstance<LottieAsset>();
        asset.name = Path.GetFileNameWithoutExtension(ctx.assetPath);
        asset.json = text;
        ctx.AddObjectToAsset("LottieAsset", asset);
        ctx.SetMainObject(asset);
    }
}
#endif