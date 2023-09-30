using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace LSCore.Editor
{
    internal partial class ModulesManager : OdinEditorWindow
    {
        [MenuItem(LSConsts.Path.Windows.ModulesManager)]
        private static void OpenWindow()
        {
            GetWindow<ModulesManager>().Show();
        }

        [PropertySpace(30)]
        [Button(ButtonSizes.Large)]
        private void Apply()
        {
            Defines.Apply();
        }
    }
}