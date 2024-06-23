using System;
using System.Diagnostics;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

namespace LSCore.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class GetInspectorPropertyAttribute : ShowInInspectorAttribute { }
    
#if UNITY_EDITOR
    public class GetInspectorPropertyAttributeDrawer : OdinAttributeDrawer<GetInspectorPropertyAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Property.CallMethod(Property);
        }
    }
#endif
}