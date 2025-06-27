﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using LSCore.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
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
            EditorUserBuildSettings.buildAppBundle = GUILayout.Toggle(EditorUserBuildSettings.buildAppBundle, "Build App Bundle (Google Play)");
            GUILayout.Space(5);
            
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
            var stacktraceInfo = Il2CppStacktraceInformation.MethodOnly;

            if (buildTarget == BuildTarget.Android)
            {
                if (mode == BuildMode.Debug)
                {
                    Defines.Enable("DEBUG");
                    buildOptions |= BuildOptions.CompressWithLz4HC;
                    stacktraceInfo = Il2CppStacktraceInformation.MethodFileLineNumber;
                }
                else
                {
                    Defines.Disable("DEBUG");
                    buildOptions |= BuildOptions.CompressWithLz4HC;
                }
            }
            else if (buildTarget == BuildTarget.iOS)
            {
                buildOptions |= BuildOptions.SymlinkSources;
            }

            PlayerSettings.SetIl2CppStacktraceInformation(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup), stacktraceInfo);
            var date = DateTime.UtcNow;
            var buildPath = $"{LSPaths.ProjectPath}/Builds/{buildTarget}";

            var productName = PlayerSettings.productName.Replace(" ", string.Empty);
            string buildName = $"{productName}_{date.Day:00}-{date.Month:00}_({mode}_{PlayerSettings.bundleVersion})";

            string extension = string.Empty;
            if (buildTarget == BuildTarget.Android)
            {
                extension = EditorUserBuildSettings.buildAppBundle ? ".aab" : ".apk";
                if (EditorUserBuildSettings.buildAppBundle)
                {
                    PlayerSettings.Android.bundleVersionCode++;
                }
            }

            string buildFilePath = $"{buildPath}/{buildName}{extension}";
            
            Defines.Apply();
            buildOptions |= BuildOptions.DetailedBuildReport;

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetEnabledScenePaths(),
                locationPathName = buildFilePath,
                target = buildTarget,
                options = buildOptions
            };
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            Debug.Log("Build result: " + summary.result);
            
            if (summary.result == BuildResult.Succeeded)
            {
                Process.Start(buildPath);
            }
            
            var packedAssets = report.packedAssets;
            foreach (var pack in packedAssets)
            {
                foreach (var content in pack.contents)
                {
                }
            }
        }
        
        private static string[] GetEnabledScenePaths()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            List<string> enabledScenePaths = new List<string>();
            
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled)
                {
                    enabledScenePaths.Add(scenes[i].path);
                }
            }
            
            return enabledScenePaths.ToArray();
        }

    }
}
