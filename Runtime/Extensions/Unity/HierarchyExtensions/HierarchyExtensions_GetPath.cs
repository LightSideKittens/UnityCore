using System.Text;
using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static partial class HierarchyExtensions
    {
        public static string GetPath(this Transform transform)
        {
            var path = new StringBuilder(transform.name);
            var current = transform.parent;
            while (current != null)
            {
                path.Insert(0, current.name + "/");
                current = current.parent;
            }
            return path.ToString();
        }
        
        public static string GetPath<T>(this T component) where T : Component => component.transform.GetPath();
        public static string GetPath(this GameObject gameObject) => gameObject.transform.GetPath();
    }
}