using System;


[Serializable]
public sealed class ItalicParseRule : TagParseRule
{
    protected override string TagName => "i";
    protected override bool HasParameter => false;
}