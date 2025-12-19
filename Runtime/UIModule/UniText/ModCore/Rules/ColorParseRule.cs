using System;


[Serializable]
public sealed class ColorParseRule : TagParseRule
{
    protected override string TagName => "color";
    protected override bool HasParameter => true;
}