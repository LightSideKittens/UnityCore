using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static partial class ComponentExtensions
    {
        public static void GetAllChild(this GameObject gameObject, List<Transform> childs)
        {
            gameObject.transform.GetAllChild(childs);
        }
        
        public static void GetAllChild(this GameObject gameObject, Action<Transform> onChild, Action<Transform> onParent)
        {
            gameObject.transform.Internal_GetAllChild(onChild, onParent);
        }
    }
}