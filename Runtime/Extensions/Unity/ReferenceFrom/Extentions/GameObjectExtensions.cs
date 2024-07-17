using System.Text;
using UnityEngine;

namespace LSCore.ReferenceFrom.Extensions
{
    public static partial class ReferenceFromComponentExtensions
    {
        public static string GetPath(this GameObject gameObject)
        {
            StringBuilder path = new StringBuilder(gameObject.name);
            Transform current = gameObject.transform.parent;
            while (current != null)
            {
                path.Insert(0, current.name + "/");
                current = current.parent;
            }
            return path.ToString();
        }
        
        private static GameObject GetGameObject(this GameObject gameObject, string path)
        {
            return gameObject.transform.GetGameObject(path);
        }

        private static string GetPathFrom(this GameObject gameObject, GameObject path)
        {
            return gameObject.transform.GetPathFrom(path);
        }

        public static T Get<T>(this GameObject gameObject, T path) where T : Component
        {
            return gameObject.GetGameObject(path.GetPathFrom(gameObject)).GetComponent<T>();
        }
        
        public static GameObject Get(this GameObject gameObject, GameObject path)
        {
            return gameObject.GetGameObject(path.GetPathFrom(gameObject));
        }
    }
}