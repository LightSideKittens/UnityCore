using System;


[Serializable]
public sealed class StrikethroughParseRule : TagParseRule
{
    protected override string TagName => "s";
    protected override bool HasParameter => false;
}