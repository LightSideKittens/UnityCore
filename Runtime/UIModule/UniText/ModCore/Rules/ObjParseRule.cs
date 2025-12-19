using System;


[Serializable]
public sealed class ObjParseRule : TagParseRule
{
    protected override string TagName => "obj";
    protected override bool HasParameter => true;
    protected override bool IsSelfClosing => true;
    protected override string InsertString => "\uFFFC";
}