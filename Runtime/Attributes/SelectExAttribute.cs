using System;

[AttributeUsage(AttributeTargets.Field)]
public class SelectExAttribute : Attribute
{
    public string valueGetter;

    public SelectExAttribute(string valueGetter = null)
    {
        this.valueGetter = valueGetter;
    }
}