using UnityEditor;

namespace LSCore.Editor
{
    public static class DeveloperModeSwitcher
    {
        [MenuItem(LSPaths.MenuItem.Tools + "/Switch Developer Mode")]
        public static void Switch()
        {
            EditorPrefs.SetBool("DeveloperMode", !EditorPrefs.GetBool("DeveloperMode"));
        }
    }
}