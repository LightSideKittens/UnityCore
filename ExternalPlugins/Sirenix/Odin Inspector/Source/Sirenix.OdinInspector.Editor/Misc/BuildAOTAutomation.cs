//-----------------------------------------------------------------------
// <copyright file="BuildAOTAutomation.cs" company="Sirenix ApS">
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

namespace Sirenix.Serialization.Internal
{
#pragma warning disable

    using Sirenix.Serialization;
    using UnityEditor;
    using UnityEditor.Build;
    using System.IO;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    public class PreBuildAOTAutomation : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return -1000; } }

        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            if (AOTGenerationConfig.Instance.ShouldAutomationGeneration(target))
            {
                AOTGenerationConfig.Instance.ScanProject();
                AOTGenerationConfig.Instance.GenerateDLL();
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            this.OnPreprocessBuild(report.summary.platform, report.summary.outputPath);
        }
    }

    public class PostBuildAOTAutomation : IPostprocessBuildWithReport
    {
        public int callbackOrder { get { return -1000; } }

        public void OnPostprocessBuild(BuildTarget target, string path)
        {
            if (AOTGenerationConfig.Instance.DeleteDllAfterBuilds && AOTGenerationConfig.Instance.ShouldAutomationGeneration(target))
            {
                if (!AssetDatabase.IsValidFolder(AOTGenerationConfig.Instance.AOTFolderPath))
                {
                    Debug.LogError($"Attempted to delete the Odin AOT DLL using an invalid folder path: {AOTGenerationConfig.Instance.AOTFolderPath}. Please report this issue and include the full error message along with the invalid folder path.");
                    return;
                }

                Directory.Delete(AOTGenerationConfig.Instance.AOTFolderPath, true);
                File.Delete(AOTGenerationConfig.Instance.AOTFolderPath.TrimEnd('/', '\\') + ".meta");
                AssetDatabase.Refresh();
            }
        }


        public void OnPostprocessBuild(BuildReport report)
        {
            this.OnPostprocessBuild(report.summary.platform, report.summary.outputPath);
        }
    }
}

#endif // UNITY_EDITOR 
#endif