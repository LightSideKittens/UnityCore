using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace LSCore
{
    [ScriptedImporter(2, "id")]
    //[InitializeOnLoad]
    public class IdImporter : ScriptedImporter
    {
        private static HashSet<string> allIdNames = new();
        private static bool isInited;
        
        static IdImporter()
        {
            EditorApplication.update += RunOnceAfterProjectIsLoaded;
        }

        private static void RunOnceAfterProjectIsLoaded()
        {
            UpdateNames();
            isInited = true;
            EditorApplication.update -= RunOnceAfterProjectIsLoaded;
        }

        private static void UpdateNames()
        {
            allIdNames.Clear();
            foreach (var id in AssetDatabaseUtils.LoadAllAssets<Id>())
            {
                allIdNames.Add(id);
            }
        }
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (isInited) UpdateNames();

            var fileName = Path.GetFileNameWithoutExtension(ctx.assetPath);

            if (allIdNames.Contains(fileName))
            {
                Debug.LogError("[IdImporter] Id with the same name already exists");
                return;
            }

            var idAsset = ScriptableObject.CreateInstance<Id>();
            ctx.AddObjectToAsset("main", idAsset, LSIcons.Get("id-icon"));
            ctx.SetMainObject(idAsset);
        }
    }
}