using System;


[Serializable]
public sealed class BoldParseRule : TagParseRule
{
    protected override string TagName => "b";
    protected override bool HasParameter => false;
}