using System;
using Sirenix.Utilities;
using UnityEngine;
using static System.IO.Path;

namespace LSCore.ConfigModule
{
    public class ConfigPathAttribute : Attribute
    {
        public string path;

        public ConfigPathAttribute(string path)
        {
            this.path = path;
        }

        public static string Path<T>()
        {
            return typeof(T).GetAttribute<ConfigPathAttribute>().path;
        }
    }
    
    public static class ConfigPaths
    {
        public static string DataPath
        {
            get
            {
#if UNITY_EDITOR
                return Application.dataPath;
#else
                return Application.persistentDataPath;
#endif
            }
        }
        
        public static string Root { get; } = Combine(DataPath, "Configs");
        
        public static class Game
        {
            public static string DynamicRoot { get; } = Combine(Root, "DynamicData");
            public static string StaticRoot { get; } = Combine(Root, "StaticData");
            public static string DebugRoot { get; } = Combine(Root, "DebugData");
            
            public static string Dynamic(string path, params string[] paths)
            {
                return Combine(DynamicRoot, path, Combine(paths));
            }
            
            public static string Static(string path, params string[] paths)
            {
                return Combine(StaticRoot, path, Combine(paths));
            }
            
            public static string Resources(string path, params string[] paths)
            {
                return Combine(StaticRoot, "Resources", path, Combine(paths));
            }
            
            public static string Debug(string path, params string[] paths)
            {
                return Combine(DebugRoot, path, Combine(paths));
            }
        }
#if UNITY_EDITOR        
        public static class Editor
        {
            public static string Library(string path, params string[] paths)
            {
                return Combine(LSPaths.LibraryPath, path, Combine(paths));
            }
        }
#endif
    }
}