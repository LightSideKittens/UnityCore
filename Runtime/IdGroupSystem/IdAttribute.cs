using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace LSCore
{
    [AttributeUsage(AttributeTargets.All)]
    [Conditional("UNITY_EDITOR")]
    public class IdAttribute : ValueDropdownAttribute
    {
        public string GroupName;

        public IdAttribute(string groupName = "") : base(groupName)
        {
            GroupName = groupName;
            IsUniqueList = true;
        }
    }
}