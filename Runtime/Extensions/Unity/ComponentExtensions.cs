using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static partial class ComponentExtensions
    {
        public static bool IsLayerInMask<T>(this T target, LayerMask mask) where T : Component
        {
            return IsLayerInMask(target.gameObject, mask);
        }
        
        public static void GetAllChild(this Transform target, List<Transform> childs)
        {
            childs.Clear();
            target.GetAllChild(childs.Add, childs.Add);
        }
        
        public static void GetAllChildWithCurrent(this Transform target, List<Transform> childs)
        {
            childs.Clear();
            target.GetAllChildWithCurrent(childs.Add, childs.Add);
        }
        
        public static void GetAllChild(this Transform target, Action<Transform> onParent, Action<Transform> onChild)
        {
            for (int i = 0; i < target.childCount; i++)
            {
                GetAllChildWithCurrent(target.GetChild(i), onParent, onChild);
            }
        }
        
        public static void GetAllChildWithCurrent(this Transform target, Action<Transform> onParent, Action<Transform> onChild)
        {
            onParent(target);
            
            for (int i = 0; i < target.childCount; i++)
            {
                var child = target.GetChild(i);
                onChild(child);
                GetAllChild(child, onParent, onChild);
            }
        }
        
        public static IEnumerable<Transform> GetAllChildWithCurrent(this Transform parentTransform)
        {
            yield return parentTransform;
            
            for (int i = 0; i < parentTransform.childCount; i++)
            {
                foreach (Transform child in GetAllChildWithCurrent(parentTransform.GetChild(i)))
                {
                    yield return child;
                }
            }
        }
        
        public static IEnumerable<Transform> GetAllChild(this Transform parentTransform)
        {
            for (int i = 0; i < parentTransform.childCount; i++)
            {
                foreach (Transform child in GetAllChildWithCurrent(parentTransform.GetChild(i)))
                {
                    yield return child;
                }
            }
        }
    }
}
