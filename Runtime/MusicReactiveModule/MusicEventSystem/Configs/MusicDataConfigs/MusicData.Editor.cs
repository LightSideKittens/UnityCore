#if UNITY_EDITOR

using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("MusiverseCore.Editor")]

namespace MusicEventSystem.Configs
{
    public partial class MusicData
    {
        internal static void Editor_SetMusicName(string name)
        {
            configName = name;
        }
    }
}
#endif
