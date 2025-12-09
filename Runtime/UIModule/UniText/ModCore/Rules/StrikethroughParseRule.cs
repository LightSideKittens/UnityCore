using System;

/// <summary>
/// Правило для тега &lt;s&gt;...&lt;/s&gt;
/// </summary>
[Serializable]
public sealed class StrikethroughParseRule : TagParseRule
{
    protected override string TagName => "s";
    protected override bool HasParameter => false;
}
