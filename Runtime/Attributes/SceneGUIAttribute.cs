using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace LSCore.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public class SceneGUIAttribute : ShowInInspectorAttribute
    {
        
    }
}