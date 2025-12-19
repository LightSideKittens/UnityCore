using System;


[Serializable]
public sealed class UnderlineParseRule : TagParseRule
{
    protected override string TagName => "u";
    protected override bool HasParameter => false;
}