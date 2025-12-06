using System;
using UnityEngine;

namespace Tekst
{
    /// <summary>
    /// Настройки обработки текста
    /// </summary>
    public struct TextProcessSettings
    {
        public float maxWidth;
        public float maxHeight;
        public TextDirection baseDirection;
        public bool enableRichText;
        public bool enableWordWrap;
        
        public static TextProcessSettings Default => new()
        {
            maxWidth = float.MaxValue,
            maxHeight = float.MaxValue,
            baseDirection = TextDirection.LeftToRight,
            enableRichText = true,
            enableWordWrap = true
        };
    }
    
    /// <summary>
    /// Результат обработки текста
    /// </summary>
    public interface ITextProcessResult
    {
        /// <summary>
        /// Plain text (codepoints)
        /// </summary>
        ReadOnlySpan<int> Codepoints { get; }
        
        /// <summary>
        /// Позиционированные глифы
        /// </summary>
        ReadOnlySpan<PositionedGlyph> Glyphs { get; }
        
        /// <summary>
        /// Строки
        /// </summary>
        ReadOnlySpan<TextLine> Lines { get; }
        
        /// <summary>
        /// Общий размер
        /// </summary>
        Vector2 Size { get; }
    }
    
    /// <summary>
    /// Главный координатор text processing pipeline.
    /// Orchestrates все этапы обработки текста.
    /// </summary>
    public sealed class TextProcessor
    {
        // Pipeline components (injected)
        private readonly ITextParser parser;
        private readonly IBidiAnalyzer bidiAnalyzer;
        private readonly IScriptAnalyzer scriptAnalyzer;
        private readonly IItemizer itemizer;
        private readonly ITextShaper shaper;
        private readonly ILineBreaker lineBreaker;
        private readonly ITextLayout layout;
        
        // Результаты промежуточных этапов (переиспользуются)
        private readonly ParseResult parseResult = new();
        private readonly BidiResult bidiResult = new();
        private readonly ScriptResult scriptResult = new();
        private readonly ItemizeResult itemizeResult = new();
        private readonly ShapeResult shapeResult = new();
        private readonly LineBreakResult lineBreakResult = new();
        private readonly LayoutResult layoutResult = new();
        
        public TextProcessor(
            ITextParser parser,
            IBidiAnalyzer bidiAnalyzer,
            IScriptAnalyzer scriptAnalyzer,
            IItemizer itemizer,
            ITextShaper shaper,
            ILineBreaker lineBreaker,
            ITextLayout layout)
        {
            this.parser = parser;
            this.bidiAnalyzer = bidiAnalyzer;
            this.scriptAnalyzer = scriptAnalyzer;
            this.itemizer = itemizer;
            this.shaper = shaper;
            this.lineBreaker = lineBreaker;
            this.layout = layout;
        }
        
        /// <summary>
        /// Обработать текст полностью
        /// </summary>
        public ITextProcessResult Process(
            ReadOnlySpan<char> text, 
            IFontProvider fontProvider,
            TextProcessSettings settings)
        {
            // 1. Parse
            if (settings.enableRichText)
                parser.Parse(text, parseResult);
            else
                parser.ParsePlain(text, parseResult);
            
            var codepoints = parseResult.Codepoints;
            
            if (codepoints.Length == 0)
            {
                layoutResult.Clear();
                return layoutResult;
            }
            
            // 2. Analyze BiDi
            bidiAnalyzer.Analyze(codepoints, settings.baseDirection, bidiResult);
            
            // 3. Detect scripts
            scriptAnalyzer.Analyze(codepoints, scriptResult);
            
            // 4. Itemize
            itemizer.Itemize(codepoints, bidiResult.Levels, scriptResult.Scripts, itemizeResult);
            
            // 5. Shape
            shaper.Shape(codepoints, itemizeResult.Runs, fontProvider, shapeResult);
            
            // 6. Line breaking
            float maxWidth = settings.enableWordWrap ? settings.maxWidth : float.MaxValue;
            lineBreaker.BreakLines(codepoints, shapeResult.Runs, shapeResult.Glyphs, maxWidth, lineBreakResult);
            
            // 7. Layout
            layout.Layout(lineBreakResult.Lines, lineBreakResult.OrderedRuns, shapeResult.Glyphs, layoutResult);
            
            return layoutResult;
        }
        
        // === Concrete result implementations ===
        
        private sealed class ParseResult : IParseResult
        {
            private int[] codepoints = new int[256];
            private int length;
            private ITextAttribute[] attributes = new ITextAttribute[32];
            private int attributeCount;
            
            public ReadOnlySpan<int> Codepoints => new(codepoints, 0, length);
            public int Length => length;
            
            public void Clear()
            {
                length = 0;
                attributeCount = 0;
            }
            
            public void SetCodepoints(ReadOnlySpan<int> source)
            {
                EnsureCapacity(ref codepoints, source.Length);
                source.CopyTo(codepoints);
                length = source.Length;
            }
            
            public void AddAttribute(ITextAttribute attr)
            {
                EnsureCapacity(ref attributes, attributeCount + 1);
                attributes[attributeCount++] = attr;
            }
            
            public AttributeEnumerator GetAttributes() => new(attributes, attributeCount);
            
            private static void EnsureCapacity<T>(ref T[] array, int required)
            {
                if (array.Length >= required) return;
                int newSize = Math.Max(required, array.Length * 2);
                Array.Resize(ref array, newSize);
            }
        }
        
        private sealed class BidiResult : IBidiResult
        {
            private byte[] levels = new byte[256];
            private int length;
            private TextDirection baseDirection;
            
            public ReadOnlySpan<byte> Levels => new(levels, 0, length);
            public TextDirection BaseDirection => baseDirection;
            
            public void SetLevels(ReadOnlySpan<byte> source, TextDirection direction)
            {
                EnsureCapacity(ref levels, source.Length);
                source.CopyTo(levels);
                length = source.Length;
                baseDirection = direction;
            }
            
            private static void EnsureCapacity(ref byte[] array, int required)
            {
                if (array.Length >= required) return;
                Array.Resize(ref array, Math.Max(required, array.Length * 2));
            }
        }
        
        private sealed class ScriptResult : IScriptResult
        {
            private UnicodeScript[] scripts = new UnicodeScript[256];
            private int length;
            
            public ReadOnlySpan<UnicodeScript> Scripts => new(scripts, 0, length);
            
            public void SetScripts(ReadOnlySpan<UnicodeScript> source)
            {
                EnsureCapacity(ref scripts, source.Length);
                source.CopyTo(scripts);
                length = source.Length;
            }
            
            private static void EnsureCapacity(ref UnicodeScript[] array, int required)
            {
                if (array.Length >= required) return;
                Array.Resize(ref array, Math.Max(required, array.Length * 2));
            }
        }
        
        private sealed class ItemizeResult : IItemizeResult
        {
            private TextRun[] runs = new TextRun[32];
            private int count;
            
            public ReadOnlySpan<TextRun> Runs => new(runs, 0, count);
            public int RunCount => count;
            
            public void Clear() => count = 0;
            
            public void AddRun(TextRun run)
            {
                EnsureCapacity(ref runs, count + 1);
                runs[count++] = run;
            }
            
            private static void EnsureCapacity(ref TextRun[] array, int required)
            {
                if (array.Length >= required) return;
                Array.Resize(ref array, Math.Max(required, array.Length * 2));
            }
        }
        
        private sealed class ShapeResult : IShapeResult
        {
            private ShapedRun[] runs = new ShapedRun[32];
            private int runCount;
            private ShapedGlyph[] glyphs = new ShapedGlyph[256];
            private int glyphCount;
            
            public ReadOnlySpan<ShapedRun> Runs => new(runs, 0, runCount);
            public ReadOnlySpan<ShapedGlyph> Glyphs => new(glyphs, 0, glyphCount);
            
            public void Clear()
            {
                runCount = 0;
                glyphCount = 0;
            }
            
            public void AddRun(ShapedRun run)
            {
                EnsureCapacity(ref runs, runCount + 1);
                runs[runCount++] = run;
            }
            
            public int AddGlyphs(ReadOnlySpan<ShapedGlyph> source)
            {
                int start = glyphCount;
                EnsureCapacity(ref glyphs, glyphCount + source.Length);
                source.CopyTo(glyphs.AsSpan(glyphCount));
                glyphCount += source.Length;
                return start;
            }
            
            private static void EnsureCapacity<T>(ref T[] array, int required)
            {
                if (array.Length >= required) return;
                Array.Resize(ref array, Math.Max(required, array.Length * 2));
            }
        }
        
        private sealed class LineBreakResult : ILineBreakResult
        {
            private TextLine[] lines = new TextLine[16];
            private int lineCount;
            private ShapedRun[] orderedRuns = new ShapedRun[32];
            private int runCount;
            
            public ReadOnlySpan<TextLine> Lines => new(lines, 0, lineCount);
            public ReadOnlySpan<ShapedRun> OrderedRuns => new(orderedRuns, 0, runCount);
            
            public void Clear()
            {
                lineCount = 0;
                runCount = 0;
            }
            
            public void AddLine(TextLine line)
            {
                EnsureCapacity(ref lines, lineCount + 1);
                lines[lineCount++] = line;
            }
            
            public void AddRun(ShapedRun run)
            {
                EnsureCapacity(ref orderedRuns, runCount + 1);
                orderedRuns[runCount++] = run;
            }
            
            private static void EnsureCapacity<T>(ref T[] array, int required)
            {
                if (array.Length >= required) return;
                Array.Resize(ref array, Math.Max(required, array.Length * 2));
            }
        }
        
        private sealed class LayoutResult : ILayoutResult, ITextProcessResult
        {
            private int[] codepoints = Array.Empty<int>();
            private PositionedGlyph[] glyphs = new PositionedGlyph[256];
            private int glyphCount;
            private TextLine[] lines = new TextLine[16];
            private int lineCount;
            private Vector2 size;
            
            // ILayoutResult
            public ReadOnlySpan<PositionedGlyph> Glyphs => new(glyphs, 0, glyphCount);
            public float Width => size.x;
            public float Height => size.y;
            
            // ITextProcessResult
            ReadOnlySpan<int> ITextProcessResult.Codepoints => codepoints;
            ReadOnlySpan<TextLine> ITextProcessResult.Lines => new(lines, 0, lineCount);
            public Vector2 Size => size;
            
            public void Clear()
            {
                glyphCount = 0;
                lineCount = 0;
                size = Vector2.zero;
            }
            
            public void SetCodepoints(int[] source) => codepoints = source;
            
            public void AddGlyph(PositionedGlyph glyph)
            {
                EnsureCapacity(ref glyphs, glyphCount + 1);
                glyphs[glyphCount++] = glyph;
            }
            
            public void SetLines(ReadOnlySpan<TextLine> source)
            {
                EnsureCapacity(ref lines, source.Length);
                source.CopyTo(lines);
                lineCount = source.Length;
            }
            
            public void SetSize(Vector2 s) => size = s;
            
            private static void EnsureCapacity<T>(ref T[] array, int required)
            {
                if (array.Length >= required) return;
                Array.Resize(ref array, Math.Max(required, array.Length * 2));
            }
        }
    }
}
