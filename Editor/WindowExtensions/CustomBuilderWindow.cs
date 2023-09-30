using System;
using System.Diagnostics;
using LSCore.Editor;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public static class CustomBuilder
{
    public static event Action<BuildMode> Building;

    static CustomBuilder()
    {
        ToolbarExtender.RightToolbarGUI.Add(OnGUI);
    }

    private static void OnGUI()
    {
        if (GUILayout.Button("Build", GUILayout.MaxWidth(100)))
        {
            PopupWindow.Show(GUILayoutUtility.GetLastRect(), new NavigationPopup());
        }
    }
    
    public enum BuildMode
    {
        Release,
        Debug,
    }
    
    private class NavigationPopup : PopupWindowContent
    {
        public override void OnGUI(Rect rect)
        {
            DrawButton(BuildMode.Release);
            DrawButton(BuildMode.Debug);
        }

        private static void DrawButton(BuildMode mode)
        {
            if (GUILayout.Button(mode.ToString(), GUILayout.MaxWidth(200)))
            {
                PerformBuild(mode);
            }
        }
        
        private static void PerformBuild(BuildMode mode)
        {
            Building?.Invoke(mode);
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            BuildOptions buildOptions = BuildOptions.None;
        
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                if (mode == BuildMode.Debug)
                {
                    Defines.Add("DEBUG");
                    buildOptions |= BuildOptions.CompressWithLz4;
                }
                else
                {
                    Defines.Remove("DEBUG");
                    buildOptions |= BuildOptions.CompressWithLz4HC;
                }
            }
            else if (buildTarget == BuildTarget.iOS)
            {
                buildOptions |= BuildOptions.SymlinkSources;
            }

            var date = DateTime.UtcNow;
            var buildPath = $"{ApplicationUtils.ProjectPath}/Builds/{buildTarget}";

            var productName = PlayerSettings.productName.Replace(" ", string.Empty);
            string buildName = $"{productName}_{date.Day:00}-{date.Month:00}_({mode}_{PlayerSettings.bundleVersion})";
            string extension = buildTarget == BuildTarget.Android ? ".apk" : "";
            string buildFilePath = $"{buildPath}/{buildName}{extension}";
            
            Defines.Apply();
            BuildPipeline.BuildPlayer(GetEnabledScenePaths(), buildFilePath, buildTarget, buildOptions);
            Debug.Log("Build completed!");
            Process.Start(buildPath);
        }
        
        private static string[] GetEnabledScenePaths()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            string[] scenePaths = new string[scenes.Length];

            for (int i = 0; i < scenes.Length; i++)
            {
                scenePaths[i] = scenes[i].path;
            }

            return scenePaths;
        }
    }
}
