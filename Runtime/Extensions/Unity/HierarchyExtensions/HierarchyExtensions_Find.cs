using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static partial class HierarchyExtensions
    {
        public static Transform FindTransform(this Transform baseObject, string path)
        {
            if (string.IsNullOrEmpty(path)) return baseObject;
            string[] pathParts = path.Split('/');
            Transform currentTransform = baseObject.transform;
            
            foreach (string part in pathParts)
            {
                if (part == "..")
                {
                    currentTransform = currentTransform.parent;
                }
                else if (part != ".")
                {
                    currentTransform = currentTransform.Find(part);
                    if (currentTransform == null)
                    {
                        Debug.LogError($"Path part '{part}' not found in '{currentTransform.name}'");
                        return baseObject;
                    }
                }
            }

            return currentTransform;
        }

        public static T FindComponent<T>(this Transform baseObject, string path)
        {
            return baseObject.FindTransform(path).GetComponent<T>();
        }
    }
}