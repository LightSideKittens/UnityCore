using System;

/// <summary>
/// Правило для тега &lt;u&gt;...&lt;/u&gt;
/// </summary>
[Serializable]
public sealed class UnderlineParseRule : TagParseRule
{
    protected override string TagName => "u";
    protected override bool HasParameter => false;
}