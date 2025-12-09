using System;

/// <summary>
/// Правило для тега &lt;i&gt;...&lt;/i&gt;
/// </summary>
[Serializable]
public sealed class ItalicParseRule : TagParseRule
{
    protected override string TagName => "i";
    protected override bool HasParameter => false;
}