using System;

/// <summary>
/// Модификатор Bold текста.
/// Устанавливает stylePadding для расширения глифа в SDF рендеринге.
/// Работает аналогично TMP Bold - расширяет UV и vertex позиции.
/// </summary>
[Serializable]
public class BoldModifier : IRenderModifier
{
    /// <summary>
    /// Значение stylePadding для Bold эффекта.
    /// Типичное значение ~1.5-2.0 для atlasPadding=9.
    /// </summary>
    private const float BoldStylePadding = 2f;

    void IModifier.Apply(int start, int end, string parameter)
    {
        ModifierHelper.ForEachGlyphInRange(start, end, (ref PositionedGlyph g) => g.stylePadding = BoldStylePadding);
    }
}
