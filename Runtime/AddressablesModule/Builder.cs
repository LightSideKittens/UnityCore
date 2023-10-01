#if UNITY_EDITOR
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine.Build.Pipeline;

namespace LSCore.AddressablesModule
{
    public class Builder : BuildScriptPackedMode
    {
        public override string Name => $"Build for {LSDebugData.Environment}";

        protected override string ConstructAssetBundleName(AddressableAssetGroup assetGroup, BundledAssetGroupSchema schema, BundleDetails info, string assetBundleName)
        {
            if (assetGroup != null)
            {
                var groupName = assetGroup.Name
                    .Replace('-', '/')
                    .Replace(" ", "")
                    .Replace('\\', '/')
                    .Replace("//", "/");
                
                assetBundleName = groupName + "/" + assetBundleName.Replace("assets_", string.Empty);
            }
            
            var bundleNameWithHashing = BuildUtility.GetNameWithHashNaming(schema.BundleNaming, info.Hash.ToString(), assetBundleName);
            //For no hash, we need the hash temporarily for content update purposes.  This will be stripped later on.
            if (schema.BundleNaming == BundledAssetGroupSchema.BundleNamingStyle.NoHash)
            {
                bundleNameWithHashing = bundleNameWithHashing.Replace(".bundle", "_" + info.Hash + ".bundle");
            }

            return bundleNameWithHashing;
        }
    }
}
#endif