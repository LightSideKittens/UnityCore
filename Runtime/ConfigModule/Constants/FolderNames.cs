using UnityEngine;

namespace LSCore.ConfigModule
{
    public static class FolderNames
    {
        public const string Configs = nameof(Configs);
        public const string StaticData = nameof(StaticData) + "/" + nameof(Resources);
        public const string DynamicData = nameof(DynamicData);
        public const string EditorData = nameof(EditorData);
        public const string DebugData = nameof(DebugData);
    }
}
