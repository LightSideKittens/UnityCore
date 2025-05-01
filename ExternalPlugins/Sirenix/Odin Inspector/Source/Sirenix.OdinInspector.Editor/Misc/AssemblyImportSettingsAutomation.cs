//-----------------------------------------------------------------------
// <copyright file="AssemblyImportSettingsAutomation.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
#if UNITY_EDITOR

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System.IO;
    using System.Collections.Generic;
    using Sirenix.Serialization.Utilities.Editor;
    using Sirenix.Utilities;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;

    public class AssemblyImportSettingsAutomation : IPreprocessBuildWithReport
    {

        public int callbackOrder { get { return -1500; } }

        private static void ConfigureImportSettings()
        {
            if (EditorOnlyModeConfig.Instance.IsEditorOnlyModeEnabled() || 
                ImportSettingsConfig.Instance.AutomateBeforeBuild == false ||
                EditorOnlyModeConfig.Instance.IsInSourceCode())
            {
                return;
            }

            var assemblyDir = new DirectoryInfo(SirenixAssetPaths.SirenixAssembliesPath).FullName;
            var projectAssetsPath = Directory.GetCurrentDirectory().TrimEnd('\\', '/');

            var isPackage = PathUtilities.HasSubDirectory(new DirectoryInfo(projectAssetsPath), new DirectoryInfo(assemblyDir)) == false;

            var aotDirPath = assemblyDir + "NoEmitAndNoEditor/";
            var jitDirPath = assemblyDir + "NoEditor/";

            var aotDir = new DirectoryInfo(aotDirPath);
            var jitDir = new DirectoryInfo(jitDirPath);

            var aotAssemblies = new List<string>();
            var jitAssemblies = new List<string>();

            foreach (var file in aotDir.GetFiles("*.dll"))
            {
                string path = file.FullName;
                if (isPackage)
                {
                    path = SirenixAssetPaths.SirenixAssembliesPath.TrimEnd('\\', '/') + "/" + path.Substring(assemblyDir.Length);
                }
                else
                {
                    path = path.Substring(projectAssetsPath.Length + 1);
                }

                aotAssemblies.Add(path);
            }

            foreach (var file in jitDir.GetFiles("*.dll"))
            {
                string path = file.FullName;
                if (isPackage)
                {
                    path = SirenixAssetPaths.SirenixAssembliesPath.TrimEnd('\\', '/') + "/" + path.Substring(assemblyDir.Length);
                }
                else
                {
                    path = path.Substring(projectAssetsPath.Length + 1);
                }

                jitAssemblies.Add(path);
            }

            AssetDatabase.StartAssetEditing();
            try
            {
                var platform = EditorUserBuildSettings.activeBuildTarget;

                if (AssemblyImportSettingsUtilities.IsJITSupported(
                    platform,
                    AssemblyImportSettingsUtilities.GetCurrentScriptingBackend(),
                    AssemblyImportSettingsUtilities.GetCurrentApiCompatibilityLevel()))
                {
                    ApplyImportSettings(platform, aotAssemblies.ToArray(), OdinAssemblyImportSettings.ExcludeFromAll);
                    ApplyImportSettings(platform, jitAssemblies.ToArray(), OdinAssemblyImportSettings.IncludeInBuildOnly);
                }
                else
                {
                    ApplyImportSettings(platform, aotAssemblies.ToArray(), OdinAssemblyImportSettings.IncludeInBuildOnly);
                    ApplyImportSettings(platform, jitAssemblies.ToArray(), OdinAssemblyImportSettings.ExcludeFromAll);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private static void ApplyImportSettings(BuildTarget platform, string[] assemblyPaths, OdinAssemblyImportSettings importSettings)
        {
            for (int i = 0; i < assemblyPaths.Length; i++)
            {
                AssemblyImportSettingsUtilities.SetAssemblyImportSettings(platform, assemblyPaths[i], importSettings);
            }
        }

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            ConfigureImportSettings();
        }
    }
}

#endif // UNITY_EDITOR
#endif