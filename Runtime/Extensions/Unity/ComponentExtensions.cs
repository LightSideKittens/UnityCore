using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static partial class ComponentExtensions
    {
        public static void GetAllChild<TComponent>(this TComponent component, Action<Transform> onChild, Action<Transform> onParent) where TComponent : Component
        {
            component.Internal_GetAllChild(onChild, onParent);
        }
        
        public static void GetAllChild<TComponent>(this TComponent component, List<Transform> childs) where TComponent : Component
        {
            childs.Clear();
            component.GetAllChildWithoutClear(childs);
        }
        
        private static void Internal_GetAllChild<TComponent>(this TComponent component, Action<Transform> onChild, Action<Transform> onParent) where TComponent : Component
        {
            var transfrom = component.transform;
            var childCount = transfrom.childCount;

            if (childCount == 0)
            {
                return;
            }

            for (int i = 0; i < childCount; i++)
            {
                var child = transfrom.GetChild(i);
                onChild(child);
                child.Internal_GetAllChild2(onChild, onParent);
            }
        }
        
        private static void Internal_GetAllChild2<TComponent>(this TComponent component, Action<Transform> onChild, Action<Transform> onParent) where TComponent : Component
        {
            var transfrom = component.transform;
            var childCount = transfrom.childCount;

            if (childCount == 0)
            {
                return;
            }
        
            onParent(transfrom);
            
            for (int i = 0; i < childCount; i++)
            {
                var child = transfrom.GetChild(i);
                onChild(child);
                child.Internal_GetAllChild2(onChild, onParent);
            }
        }
        
        
        private static void GetAllChildWithoutClear<TComponent>(this TComponent component, List<Transform> childs) where TComponent : Component
        {
            component.Internal_GetAllChild(childs.Add, childs.Add);
        }
    }
}
