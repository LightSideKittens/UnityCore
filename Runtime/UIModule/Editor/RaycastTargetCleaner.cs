using LSCore.Extensions.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace LSCore.UIModule.Editor
{
    public static class RaycastTargetCleaner
    {
        [MenuItem(LSPaths.MenuItem.Tools + "/Disable Raycast Target")]
        public static void Clean()
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (prefabStage == null)
            {
                var gameObjects = SceneManager.GetActiveScene().GetRootGameObjects();

                for (int i = 0; i < gameObjects.Length; i++)
                {
                    Clean(gameObjects[i]);
                }
            }
            else
            {
                Clean(prefabStage.prefabContentsRoot);
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
            }
        }

        private static void Clean(GameObject gameObject)
        {
            foreach (var target in gameObject.GetAllChildWithCurrent())
            {
                Clean(target);
            }
        }

        private static void Clean(Component component)
        {
            var hasSelectable = component.TryGetComponent<Selectable>(out var selectable);
            var hasLSButton = component.TryGetComponent<LSButton>(out _);
            var hasGraphic = component.TryGetComponent<Graphic>(out var graphic);

            if (hasSelectable || hasLSButton)
            {
                if (hasGraphic)
                {
                    graphic.raycastTarget = true;
                }
                else
                {
                    Debug.LogWarning($"GameObject {selectable.name} with {nameof(Selectable)} component has no {nameof(Graphic)} component", selectable);
                }
            }
            else if (hasGraphic)
            {
                graphic.raycastTarget = false;
            }
        }
    }
}
