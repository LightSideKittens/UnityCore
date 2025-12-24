using System;

[Serializable]
public sealed class EllipsisTagRule : TagParseRule
{
    protected override string TagName => "ellipsis";
    protected override bool HasParameter => true;
}
