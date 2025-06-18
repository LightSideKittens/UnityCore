using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static partial class ComponentExtensions
    {
        public static bool IsLayerInMask(this GameObject target, LayerMask mask)
        {
            return ((1 << target.layer) & mask) != 0;
        }
        
        public static void GetAllChild(this GameObject gameObject, List<Transform> childs)
        {
            gameObject.transform.GetAllChild(childs);
        }
        
        public static void GetAllChildWithCurrent(this GameObject gameObject, List<Transform> childs)
        {
            gameObject.transform.GetAllChildWithCurrent(childs);
        }

        
        
        public static void GetAllChild(this GameObject gameObject, Action<Transform> onChild, Action<Transform> onParent)
        {
            gameObject.transform.GetAllChild(onChild, onParent);
        }
        
        public static void GetAllChildWithCurrent(this GameObject gameObject, Action<Transform> onChild, Action<Transform> onParent)
        {
            gameObject.transform.GetAllChildWithCurrent(onChild, onParent);
        }
        
        
        public static IEnumerable<Transform> GetAllChild(this GameObject gameObject)
        {
            return gameObject.transform.GetAllChild();
        }
        
        public static IEnumerable<Transform> GetAllChildWithCurrent(this GameObject gameObject)
        {
            return gameObject.transform.GetAllChildWithCurrent();
        }
        
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent(out T component))
            {
                return component;
            }

            return gameObject.AddComponent<T>();
        }
    }
}