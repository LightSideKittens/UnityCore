using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace LSCore
{
    [AttributeUsage(AttributeTargets.All)]
    [Conditional("UNITY_EDITOR")]
    public class CurrencyIdAttribute : ValueDropdownAttribute
    {
        public string GroupName;

        public CurrencyIdAttribute(string groupName = "") : base(groupName)
        {
            GroupName = groupName;
            IsUniqueList = true;
        }
    }
}