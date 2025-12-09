using System;

/// <summary>
/// Модификатор Underline текста.
/// Помечает глифы для отрисовки линии подчёркивания.
/// </summary>
[Serializable]
public class UnderlineModifier : IRenderModifier
{
    void IModifier.Apply(int start, int end, string parameter)
    {
        ModifierHelper.ForEachGlyphInRange(start, end, (ref PositionedGlyph g) => g.hasUnderline = true);
    }
}