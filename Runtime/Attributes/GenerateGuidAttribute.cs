using System;

[AttributeUsage(AttributeTargets.Field)]
public class GenerateGuidAttribute : Attribute
{
    public bool Hide;
}

