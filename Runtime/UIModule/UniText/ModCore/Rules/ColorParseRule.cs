using System;

/// <summary>
/// Правило для тега &lt;color=#RRGGBB&gt;...&lt;/color&gt;
/// </summary>
[Serializable]
public sealed class ColorParseRule : TagParseRule
{
    protected override string TagName => "color";
    protected override bool HasParameter => true;
}