using UnityEngine;

public static class LSPaths
{
    public const string Root = "LightSideCore";
    public const string Python = Root + "/" + nameof(Python);
    public const string Runtime = Root + "/" + nameof(Runtime);
    public const string Editor = Root + "/" + nameof(Editor);
    public const string Firebase = Runtime + "/Firebase";
    public const string Icons = Editor + "/LightSideIcons";
    public const string Backuper = Editor + "/BackupSystem/GitIgnored";
    
    public static class MenuItem
    {
        public const string Root = "LSCore";
        public const string Tools = Root + "/Tools";
    }

    public static class Windows
    {
        public const string Root = MenuItem.Root + "/Windows";
        public const string ModulesManager = Root + "/Modules Manager";
        public const string YamlEditor = Root + "/Yaml Editor";
        public const string ThemeEditor = Root + "/Theme Editor";
        public const string Backuper = Root + "/Backuper";
    }
    
        
    public static string ToFull(this string path) => $"{Application.dataPath}/{path}";
    public static string AssetsPathToFull(this string path) => $"{ApplicationUtils.ProjectPath}/{path}";
}
