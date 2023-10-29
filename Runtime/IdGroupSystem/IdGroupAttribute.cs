using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace LSCore
{
    [AttributeUsage(AttributeTargets.All)]
    [Conditional("UNITY_EDITOR")]
    public class IdGroupAttribute : ValueDropdownAttribute
    {
        public IdGroupAttribute() : base("")
        {
            IsUniqueList = true;
        }
    }
}