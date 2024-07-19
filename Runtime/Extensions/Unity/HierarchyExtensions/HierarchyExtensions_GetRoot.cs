using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static partial class HierarchyExtensions
    {
        public static Transform GetRoot(this Transform transform)
        {
            var currentTransform = transform;

            while (currentTransform.parent != null)
            {
                currentTransform = currentTransform.parent;
            }

            return currentTransform;
        }
        
        public static Transform GetRoot<T>(this T component) where T : Component => component.transform.GetRoot();
        public static Transform GetRoot(this GameObject gameObject) => gameObject.transform.GetRoot();
    }
}