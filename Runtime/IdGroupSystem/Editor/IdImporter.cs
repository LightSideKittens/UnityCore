using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace LSCore
{
    [ScriptedImporter(2, "id")]
    [InitializeOnLoad]
    public class IdImporter : ScriptedImporter
    {
        private static HashSet<string> allIdNames = new();
        
        static IdImporter()
        {
            EditorApplication.update += RunOnceAfterProjectIsLoaded;
        }

        private static void RunOnceAfterProjectIsLoaded()
        {
            foreach (var id in AssetDatabaseUtils.LoadAllAssets<Id>())
            {
                allIdNames.Add(id);
            }
            
            EditorApplication.update -= RunOnceAfterProjectIsLoaded;
        }
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var fileName = Path.GetFileNameWithoutExtension(ctx.assetPath);

            if (allIdNames.Contains(fileName))
            {
                Debug.LogError("[IdImporter] Id with the same name already exists");
                return;
            }

            var idAsset = ScriptableObject.CreateInstance<Id>();
            ctx.AddObjectToAsset("main", idAsset, LSIcons.Get("fire-icon"));
            ctx.SetMainObject(idAsset);
        }
    }
}