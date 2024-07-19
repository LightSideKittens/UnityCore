using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static partial class HierarchyExtensions
    {
        private static GameObject GetGameObject(this Transform transform, string path)
        {
            var gameObjectNames = path.Split('/');

            for (int i = 0; i < gameObjectNames.Length; i++)
            {
                for (int j = 0; j < transform.childCount; j++)
                {
                    var child = transform.GetChild(j);
                
                    if (child.name.Equals(gameObjectNames[i]))
                    {
                        transform = child;
                        break;
                    }
                }
            }

            return transform.gameObject;
        }
        
        private static GameObject GetGameObject<T>(this T component, string path) where T : Component => component.transform.GetGameObject(path);
        private static GameObject GetGameObject(this GameObject gameObject, string path) => gameObject.transform.GetGameObject(path);
    }
}