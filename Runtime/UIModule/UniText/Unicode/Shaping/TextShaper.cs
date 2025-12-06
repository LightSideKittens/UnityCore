using System;

namespace Tekst
{
    /// <summary>
    /// Text Shaper — преобразует codepoints в глифы.
    /// Абстракция над shaping engine (HarfBuzz, platform-native, etc.)
    /// </summary>
    public sealed class TextShaper : ITextShaper
    {
        private readonly IShapingEngine engine;
        
        // Буферы результатов
        private ShapedRun[] shapedRuns = new ShapedRun[32];
        private int shapedRunCount;
        private ShapedGlyph[] glyphs = new ShapedGlyph[256];
        private int glyphCount;
        
        public TextShaper(IShapingEngine engine)
        {
            this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }
        
        public void Shape(
            ReadOnlySpan<int> codepoints,
            ReadOnlySpan<TextRun> runs,
            IFontProvider fontProvider,
            IShapeResult result)
        {
            shapedRunCount = 0;
            glyphCount = 0;
            
            if (runs.IsEmpty)
            {
                WriteResult(result);
                return;
            }
            
            for (int i = 0; i < runs.Length; i++)
            {
                var run = runs[i];
                var font = fontProvider.GetFont(run.fontId);
                
                // Получаем codepoints для этого run
                var runCodepoints = codepoints.Slice(run.range.start, run.range.length);
                
                // Shape через engine
                var shapingResult = engine.Shape(
                    runCodepoints,
                    font,
                    run.script,
                    run.Direction);
                
                // Добавляем глифы в общий буфер
                int glyphStart = glyphCount;
                AddGlyphs(shapingResult.Glyphs);
                
                // Создаём shaped run
                AddShapedRun(new ShapedRun
                {
                    range = run.range,
                    glyphStart = glyphStart,
                    glyphCount = shapingResult.Glyphs.Length,
                    width = shapingResult.TotalAdvance,
                    direction = run.Direction,
                    fontId = run.fontId,
                    attributeSnapshot = run.attributeSnapshot
                });
            }
            
            WriteResult(result);
        }
        
        private void AddGlyphs(ReadOnlySpan<ShapedGlyph> source)
        {
            EnsureGlyphCapacity(glyphCount + source.Length);
            source.CopyTo(glyphs.AsSpan(glyphCount));
            glyphCount += source.Length;
        }
        
        private void AddShapedRun(ShapedRun run)
        {
            EnsureRunCapacity(shapedRunCount + 1);
            shapedRuns[shapedRunCount++] = run;
        }
        
        private void WriteResult(IShapeResult result)
        {
            if (result is ShapeResultBuffer buffer)
            {
                buffer.Set(shapedRuns.AsSpan(0, shapedRunCount), glyphs.AsSpan(0, glyphCount));
            }
        }
        
        private void EnsureGlyphCapacity(int required)
        {
            if (glyphs.Length >= required) return;
            int newSize = Math.Max(required, glyphs.Length * 2);
            Array.Resize(ref glyphs, newSize);
        }
        
        private void EnsureRunCapacity(int required)
        {
            if (shapedRuns.Length >= required) return;
            int newSize = Math.Max(required, shapedRuns.Length * 2);
            Array.Resize(ref shapedRuns, newSize);
        }
    }
    
    /// <summary>
    /// Абстракция shaping engine (HarfBuzz, etc.)
    /// </summary>
    public interface IShapingEngine
    {
        /// <summary>
        /// Shape codepoints в глифы
        /// </summary>
        IShapingEngineResult Shape(
            ReadOnlySpan<int> codepoints,
            IFontAsset font,
            UnicodeScript script,
            TextDirection direction);
    }
    
    /// <summary>
    /// Результат shaping от engine
    /// </summary>
    public interface IShapingEngineResult
    {
        ReadOnlySpan<ShapedGlyph> Glyphs { get; }
        float TotalAdvance { get; }
    }
    
    /// <summary>
    /// Буфер результатов Text Shaper
    /// </summary>
    public sealed class ShapeResultBuffer : IShapeResult
    {
        private ShapedRun[] runs = new ShapedRun[32];
        private int runCount;
        private ShapedGlyph[] glyphs = new ShapedGlyph[256];
        private int glyphCount;
        
        public ReadOnlySpan<ShapedRun> Runs => runs.AsSpan(0, runCount);
        public ReadOnlySpan<ShapedGlyph> Glyphs => glyphs.AsSpan(0, glyphCount);
        
        internal void Set(ReadOnlySpan<ShapedRun> sourceRuns, ReadOnlySpan<ShapedGlyph> sourceGlyphs)
        {
            if (runs.Length < sourceRuns.Length)
                runs = new ShapedRun[sourceRuns.Length];
            sourceRuns.CopyTo(runs);
            runCount = sourceRuns.Length;
            
            if (glyphs.Length < sourceGlyphs.Length)
                glyphs = new ShapedGlyph[sourceGlyphs.Length];
            sourceGlyphs.CopyTo(glyphs);
            glyphCount = sourceGlyphs.Length;
        }
        
        public void Clear()
        {
            runCount = 0;
            glyphCount = 0;
        }
    }
}
