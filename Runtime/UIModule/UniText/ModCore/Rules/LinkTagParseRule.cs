using System;

[Serializable]
public sealed class LinkTagParseRule : TagParseRule
{
    protected override string TagName => "link";
    protected override bool HasParameter => true;
}
