using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

[AttributeUsage(AttributeTargets.All)]
[Conditional("UNITY_EDITOR")]
public class IdGroupAttribute : ValueDropdownAttribute
{
    public IdGroupAttribute(string name = "") : base(name)
    {
        IsUniqueList = true;
    }
}