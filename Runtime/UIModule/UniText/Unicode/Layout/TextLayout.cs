using System;

namespace Tekst
{
    /// <summary>
    /// Text Layout — позиционирование глифов.
    /// Применяет alignment, line spacing, и т.д.
    /// </summary>
    public sealed class TextLayout : ITextLayout
    {
        // Буферы
        private PositionedGlyph[] positionedGlyphs = new PositionedGlyph[256];
        private int glyphCount;
        
        // Settings
        private LayoutSettings settings;
        
        public TextLayout()
        {
            settings = LayoutSettings.Default;
        }
        
        public TextLayout(LayoutSettings settings)
        {
            this.settings = settings;
        }
        
        public void Layout(
            ReadOnlySpan<TextLine> lines,
            ReadOnlySpan<ShapedRun> runs,
            ReadOnlySpan<ShapedGlyph> glyphs,
            ILayoutResult result)
        {
            glyphCount = 0;
            
            if (lines.IsEmpty)
            {
                WriteResult(result, 0, 0);
                return;
            }
            
            float y = 0;
            float maxWidth = 0;
            
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                float x = ComputeLineStartX(line, settings);
                
                // Позиционируем каждый run в строке
                for (int r = 0; r < line.runCount; r++)
                {
                    var run = runs[line.runStart + r];
                    var runGlyphs = glyphs.Slice(run.glyphStart, run.glyphCount);
                    
                    // Направление run
                    if (run.direction == TextDirection.RightToLeft)
                    {
                        // RTL: начинаем справа
                        x += run.width;
                        
                        for (int g = 0; g < runGlyphs.Length; g++)
                        {
                            var glyph = runGlyphs[g];
                            x -= glyph.advanceX;
                            
                            AddPositionedGlyph(new PositionedGlyph
                            {
                                glyphId = glyph.glyphId,
                                x = x + glyph.offsetX,
                                y = y + glyph.offsetY,
                                fontId = run.fontId,
                                attributeSnapshot = run.attributeSnapshot
                            });
                        }
                    }
                    else
                    {
                        // LTR
                        for (int g = 0; g < runGlyphs.Length; g++)
                        {
                            var glyph = runGlyphs[g];
                            
                            AddPositionedGlyph(new PositionedGlyph
                            {
                                glyphId = glyph.glyphId,
                                x = x + glyph.offsetX,
                                y = y + glyph.offsetY,
                                fontId = run.fontId,
                                attributeSnapshot = run.attributeSnapshot
                            });
                            
                            x += glyph.advanceX;
                        }
                    }
                }
                
                if (line.width > maxWidth)
                    maxWidth = line.width;
                
                // Следующая строка
                y += line.height > 0 ? line.height : settings.defaultLineHeight;
                y += settings.lineSpacing;
            }
            
            WriteResult(result, maxWidth, y);
        }
        
        /// <summary>
        /// Вычислить начальную X позицию строки (alignment)
        /// </summary>
        private float ComputeLineStartX(TextLine line, LayoutSettings settings)
        {
            float availableWidth = settings.maxWidth;
            
            switch (settings.alignment)
            {
                case TextAlignment.Center:
                    return (availableWidth - line.width) / 2;
                    
                case TextAlignment.Right:
                    return availableWidth - line.width;
                    
                case TextAlignment.Justified:
                    // TODO: justify spacing
                    return 0;
                    
                case TextAlignment.Left:
                default:
                    return 0;
            }
        }
        
        private void AddPositionedGlyph(PositionedGlyph glyph)
        {
            EnsureCapacity(glyphCount + 1);
            positionedGlyphs[glyphCount++] = glyph;
        }
        
        private void WriteResult(ILayoutResult result, float width, float height)
        {
            if (result is LayoutResultBuffer buffer)
            {
                buffer.Set(positionedGlyphs.AsSpan(0, glyphCount), width, height);
            }
        }
        
        private void EnsureCapacity(int required)
        {
            if (positionedGlyphs.Length >= required) return;
            Array.Resize(ref positionedGlyphs, Math.Max(required, positionedGlyphs.Length * 2));
        }
    }
    
    /// <summary>
    /// Настройки layout
    /// </summary>
    public struct LayoutSettings
    {
        public float maxWidth;
        public float maxHeight;
        public float lineSpacing;
        public float defaultLineHeight;
        public TextAlignment alignment;
        
        public static LayoutSettings Default => new()
        {
            maxWidth = float.MaxValue,
            maxHeight = float.MaxValue,
            lineSpacing = 0,
            defaultLineHeight = 20,
            alignment = TextAlignment.Left
        };
    }
    
    /// <summary>
    /// Буфер результатов Layout
    /// </summary>
    public sealed class LayoutResultBuffer : ILayoutResult
    {
        private PositionedGlyph[] glyphs = new PositionedGlyph[256];
        private int count;
        private float width;
        private float height;
        
        public ReadOnlySpan<PositionedGlyph> Glyphs => glyphs.AsSpan(0, count);
        public float Width => width;
        public float Height => height;
        
        internal void Set(ReadOnlySpan<PositionedGlyph> source, float w, float h)
        {
            if (glyphs.Length < source.Length)
                glyphs = new PositionedGlyph[source.Length];
            
            source.CopyTo(glyphs);
            count = source.Length;
            width = w;
            height = h;
        }
        
        public void Clear()
        {
            count = 0;
            width = 0;
            height = 0;
        }
    }
}
