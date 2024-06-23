using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static partial class ComponentExtensions
    {
        [MenuItem("GameObject/Duplicate Without Children", false, 0)]
        static void DuplicateWithoutChildren(MenuCommand command)
        {
            GameObject selected = command.context as GameObject;
            if (selected == null)
            {
                Debug.LogWarning("No GameObject selected!");
                return;
            }

            GameObject copiedObject = InstantiateWithoutChildren(selected);
            copiedObject.name = selected.name + " Copy";
        }

        static GameObject InstantiateWithoutChildren(GameObject original)
        {
            GameObject copy = new GameObject(original.name);
            copy.transform.SetParent(original.transform.parent);
            foreach (Component component in original.GetComponents<Component>())
            {
                ComponentUtility.CopyComponent(component);
                ComponentUtility.PasteComponentAsNew(copy);
            }

            return copy;
        }

        [MenuItem("GameObject/Duplicate Without Children", true)]
        private static bool ValidateDuplicateWithoutChildren()
        {
            return Selection.activeGameObject != null;
        }
    }
}