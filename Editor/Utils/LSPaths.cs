using System.IO;
using UnityEngine;

public static class LSPaths
{
    public const string Core = "LightSideCore";
    public const string Python = Core + "/" + nameof(Python);
    public const string Runtime = Core + "/" + nameof(Runtime);
    public const string Editor = Core + "/" + nameof(Editor);
    public const string Firebase = Runtime + "/Firebase";
    public const string Icons = Editor + "/LightSideIcons";
    
    public static class Folders
    {
        public const string Art = nameof(Art);
        public const string Audio = nameof(Art) + "/" + nameof(Audio);
    }
    
    public static class Preferences
    {
        public const string Root = "Preferences/Light Side Core";
        public const string Backuper = Root + "/" + nameof(Backuper);
        public const string Profiles = Root + "/" + nameof(Profiles);
    }
    
    public static class MenuItem
    {
        public const string Root = "LSCore";
        public const string Tools = Root + "/Tools";
    }
    
    public static class AssetMenuItem
    {
        public const string Assets = nameof(Assets);
        public const string Root = Assets + "/LSCore";
    }

    public static class Windows
    {
        public const string Root = MenuItem.Root + "/Windows";
        public const string ModulesManager = Root + "/Modules Manager";
        public const string AnimationClipsEditor = Root + "/Animation Clips Editor";
        public const string YamlEditor = Root + "/Yaml Editor";
        public const string ThemeEditor = Root + "/Theme Editor";
        public const string AssetIconSetter = Root + "/Asset Icon Setter";
        public const string AssetsViewer = Root + "/Assets Viewer";
        public const string SourcePrefabChanger = Root + "/Source Prefab Changer";
        public const string BadassAnimation = Root + "/Badass Animation";
    }

    public static string ProjectPath { get; } = Application.dataPath[..^7];
    public static string ProjectSettingsPath { get; } = Path.Combine(ProjectPath, "ProjectSettings");
    public static string LibraryPath { get; } = Path.Combine(ProjectPath, "Library");
    
    public static string ToFull(this string path) => Path.Combine(Application.dataPath, path);
    public static string AssetsPathToFull(this string path) => Path.Combine(ProjectPath, path);
}
