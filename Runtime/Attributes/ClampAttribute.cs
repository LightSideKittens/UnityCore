using System;
using System.Diagnostics;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
[Conditional("UNITY_EDITOR")]
public class ClampAttribute : Attribute
{
    public float min;
    public float max;

    public ClampAttribute(float min = float.MinValue, float max = float.MaxValue)
    {
        this.min = min;
        this.max = max;
    }
}
