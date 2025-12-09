using System;

/// <summary>
/// Модификатор Strikethrough текста.
/// Помечает глифы для отрисовки линии зачёркивания.
/// </summary>
[Serializable]
public class StrikethroughModifier : IRenderModifier
{
    void IModifier.Apply(int start, int end, string parameter)
    {
        ModifierHelper.ForEachGlyphInRange(start, end, (ref PositionedGlyph g) => g.hasStrikethrough = true);
    }
}