using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace LSCore
{
    [AttributeUsage(AttributeTargets.All)]
    [Conditional("UNITY_EDITOR")]
    public class IdAttribute : ValueDropdownAttribute
    {
        public string[] groupNames;
        public Type groupType;

        public IdAttribute(params string[] groupNames) : base("")
        {
            this.groupNames = groupNames;
            IsUniqueList = true;
        }
        
        public IdAttribute(Type groupType) : base("")
        {
            this.groupType = groupType;
            IsUniqueList = true;
        }
    }
}