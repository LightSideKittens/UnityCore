using UnityEngine;

namespace LSCore.ReferenceFrom.Extensions
{
    public static partial class ReferenceFromComponentExtensions
    {
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