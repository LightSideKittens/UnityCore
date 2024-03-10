using UnityEngine;

namespace LSCore.ConfigModule
{
    public static class FolderNames
    {
        public const string Configs = nameof(Configs);
        public const string ProjectSettings = Configs + "/" + nameof(ProjectSettings);
        public const string DefaultSaveData = nameof(DefaultSaveData) + "/" + nameof(Resources);
        public const string SaveData = nameof(SaveData);
        public const string EditorConfigs = nameof(EditorConfigs);
        public const string Debug = nameof(Debug);
    }
}
