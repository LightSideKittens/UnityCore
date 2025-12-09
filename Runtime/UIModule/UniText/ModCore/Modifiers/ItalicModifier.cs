using System;

/// <summary>
/// Модификатор Italic текста.
/// Помечает глифы как italic. Реальный угол наклона берётся из FontAsset
/// при рендеринге в MeshGenerator (каждый шрифт имеет свой italicStyle).
/// </summary>
[Serializable]
public class ItalicModifier : IRenderModifier
{
    /// <summary>
    /// Значение-флаг для italic.
    /// Любое ненулевое значение означает "использовать italicStyle из FontAsset".
    /// MeshGenerator проверяет italicAngle != 0 и применяет shear.
    /// </summary>
    private const float ItalicFlag = 1f;

    void IModifier.Apply(int start, int end, string parameter)
    {
        ModifierHelper.ForEachGlyphInRange(start, end, (ref PositionedGlyph g) => g.italicAngle = ItalicFlag);
    }
}
