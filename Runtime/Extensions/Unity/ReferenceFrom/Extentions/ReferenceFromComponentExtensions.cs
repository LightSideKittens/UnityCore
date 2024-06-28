using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LSCore.ReferenceFrom.Extensions
{
    public static partial class ReferenceFromComponentExtensions
    {
        private static GameObject GetGameObject<TComponent>(this TComponent component, string path) where TComponent : Component
        {
            var gameObjectNames = path.Split('/');
            var transform = component.transform;

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
        
        private static string GetPathFrom(this Component component, GameObject from)
        {
            var names = new List<string>();

            var targetName = from.name;
            var currentName = component.name;

            while (targetName.StartsWith(currentName) == false)
            {
                names.Add(currentName);
                component = component.transform.parent;
                currentName = component.name;
            }

            var sb = new StringBuilder();
            const char separator = '/';
            
            for (int i = names.Count - 1; i > 0; i--)
            {
                sb.Append(names[i]);
                sb.Append(separator);
            }

            sb.Append(names[0]);
            return sb.ToString();
        }

        private static string GetPathFrom<TComponent, T>(this TComponent component, T from) where TComponent : Component where T : Component
        {
            return component.GetPathFrom(from.gameObject);
        }
        
        public static GameObject FindGameObject(this GameObject baseObject, string path)
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

            return currentTransform.gameObject;
        }

        public static T FindComponent<T>(this Transform baseObject, string path)
        {
            return baseObject.gameObject.FindGameObject(path).GetComponent<T>();
        }

        public static GameObject Get<TComponent>(this TComponent component, GameObject path) where TComponent : Component
        {
            return component.GetGameObject(path.GetPathFrom(component.gameObject));
        }
        
        public static T Get<TComponent, T>(this TComponent component, T path) where T : Component where TComponent : Component
        {
            return component.GetGameObject(path.GetPathFrom(component)).GetComponent<T>();
        }
        
    }
}
