using System;
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
    public struct AppVersion
    {
        public int major;
        public int minor;
        public int patch;

        public static implicit operator AppVersion(string version)
        {
            AppVersion v = new AppVersion();
            string[] split = version.Split('.');
            v.major = int.Parse(split[0]);
            v.minor = int.Parse(split[1]);
            v.patch = int.Parse(split[2]);
            return v;
        }

        public static implicit operator string(AppVersion version)
        {
            return version.major + "." + version.minor + "." + version.patch;
        }

        public void Upgrade(VersionImportance importance)
        {
            switch (importance)
            {
                case VersionImportance.Patch:
                    patch++;
                    break;
                case VersionImportance.Minor:
                    patch = 0;
                    minor++; 
                    break;
                case VersionImportance.Major:
                    patch = 0;
                    minor = 0;
                    major++;
                    break;
            }
        }
    }
    
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
    
    public enum VersionImportance
    {
        None,
        Patch,
        Minor,
        Major
    }
    
    public enum BuildMode
    {
        Release,
        Debug,
    }
    
    private class NavigationPopup : PopupWindowContent
    {
        private VersionImportance importance;

        private string fileName
        {
            get => SessionState.GetString("build_file_name", string.Empty);
            set => SessionState.SetString("build_file_name", value);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(350, 220);
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.LabelField("Build Settings");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Override File Name");
            fileName = EditorGUILayout.TextField(fileName);
            EditorGUILayout.Space();
            EditorUserBuildSettings.buildAppBundle = GUILayout.Toggle(EditorUserBuildSettings.buildAppBundle, "Build App Bundle (Google Play)");
            GUILayout.Space(5);
            DrawButton(BuildMode.Release);
            DrawButton(BuildMode.Debug);
        }

        private void DrawButton(BuildMode mode)
        {
            if (GUILayout.Button(mode.ToString(), GUILayout.MinWidth(1000), GUILayout.MinHeight(40)))
            {
                Popup.Draw(() =>
                {
                    var values = Enum.GetValues(typeof(VersionImportance));
                    foreach (var v in values)
                    {
                        if (GUILayout.Button(v.ToString(), GUILayout.MinWidth(1000), GUILayout.MinHeight(40)))
                        {
                            importance = (VersionImportance)v;
                            PerformBuild(mode);
                        }
                    }
                });
            }
        }
        
        private void PerformBuild(BuildMode mode)
        {
            Building?.Invoke(mode);
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            BuildOptions buildOptions = BuildOptions.None;
            var stacktraceInfo = Il2CppStacktraceInformation.MethodOnly;

            var lastVersion = PlayerSettings.bundleVersion;
            
            AppVersion version = PlayerSettings.bundleVersion;
            version.Upgrade(importance);
            PlayerSettings.bundleVersion = version;
            
            if (buildTarget == BuildTarget.Android)
            {
                if (mode == BuildMode.Debug)
                {
                    Defines.Enable("DEBUG");
                    PlayerSettings.bundleVersion = string.Concat(PlayerSettings.bundleVersion, ".debug");
                    buildOptions |= BuildOptions.CompressWithLz4HC;
                    stacktraceInfo = Il2CppStacktraceInformation.MethodOnly;
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

            var lastVersionCode = 0;
            if (buildTarget == BuildTarget.Android)
            {
                extension = EditorUserBuildSettings.buildAppBundle ? ".aab" : ".apk";
                if (EditorUserBuildSettings.buildAppBundle)
                {
                    lastVersionCode = PlayerSettings.Android.bundleVersionCode++;
                }
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                buildName = fileName;
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
            else
            {
                if (buildTarget == BuildTarget.Android)
                {
                    PlayerSettings.bundleVersion = lastVersion;
                    if (EditorUserBuildSettings.buildAppBundle)
                    {
                        PlayerSettings.Android.bundleVersionCode = lastVersionCode;
                    }
                }
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
