using System;

/// <summary>
/// Правило для self-closing тега &lt;obj="name"/&gt; или &lt;obj="name"&gt;
/// Вставляет U+FFFC (Object Replacement Character) в текст.
/// </summary>
[Serializable]
public sealed class ObjParseRule : TagParseRule
{
    protected override string TagName => "obj";
    protected override bool HasParameter => true;
    protected override bool IsSelfClosing => true;
    protected override string InsertString => "\uFFFC";
}
