using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

[AttributeUsage(AttributeTargets.All)]
[Conditional("UNITY_EDITOR")]
public class IdAttribute : ValueDropdownAttribute
{
    public string GroupName;

    public IdAttribute(string name = "") : base(name)
    {
        GroupName = name;
        IsUniqueList = true;
    }
}
