using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using Path = LSPaths;

namespace LSCore.Editor
{
    public static partial class Defines
    {
        private const string Cs = ".cs";
        private const string Plug = ".cs.plug";
        
        private static readonly HashSet<string> defines;

        private static readonly Dictionary<string, string> pathByDefine = new()
        {
            {Names.FIREBASE_REMOTE_CONFIG, $"{Path.Firebase}/RemoteConfig"}
        };
        
        private static readonly Dictionary<string, Action> onApplyActions = new();

        static Defines()
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            defines =  PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup)).Split(';').ToHashSet();
        }

        public static bool Enable(string define)
        {
            OnAdd(define);
            return defines.Add(define);
        }

        public static bool Disable(string define)
        {
            OnRemove(define);
            return defines.Remove(define);
        }

        public static bool Exist(string define) => defines.Contains(define);

        private static void OnAdd(string define)
        {
            if (pathByDefine.TryGetValue(define, out var path))
            {
                var plugPath = $"{Application.dataPath}/{path}/Plug";
                var plugPaths = Directory.GetFiles(plugPath, $"*{Cs}");
                onApplyActions[define] = () => ReplaceExtenstion(plugPaths, Cs, Plug);
            }
        }
        
        private static void OnRemove(string define)
        {
            if (pathByDefine.TryGetValue(define, out var path))
            {
                var plugPath = $"{Application.dataPath}/{path}/Plug";
                var plugPaths = Directory.GetFiles(plugPath, $"*{Plug}");
                onApplyActions[define] = () => ReplaceExtenstion(plugPaths, Plug, Cs);
            }
        }

        private static void ReplaceExtenstion(string[] files, string srcExt, string dstExt)
        {
            for (int i = 0; i < files.Length; i++)
            {
                var plugFile = files[i];
                var newFile = plugFile.Replace(@"Plug\", string.Empty);
                
                var newPlugFile = plugFile.Replace(srcExt, dstExt);
                var oldFile = newFile.Replace(srcExt, dstExt);
                
                File.Move(plugFile, newPlugFile);
                File.Move(oldFile, newFile);
            }
        }
        
        public static void Apply()
        {
            foreach (var action in onApplyActions.Values)
            {
                action();
            }

            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup), GetDefines());
            AssetDatabase.Refresh();
        }

        private static string GetDefines()
        {
            var builder = new StringBuilder();
            foreach (var define in defines)
            {
                builder.Append($"{define};");
            }

            return builder.ToString();
        }
    }
}