using System;

/// <summary>
/// Правило для тега &lt;b&gt;...&lt;/b&gt;
/// </summary>
[Serializable]
public sealed class BoldParseRule : TagParseRule
{
    protected override string TagName => "b";
    protected override bool HasParameter => false;
}
