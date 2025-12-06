using System;

namespace Tekst
{
    /// <summary>
    /// Результат парсинга текста
    /// </summary>
    public interface IParseResult
    {
        /// <summary>
        /// Plain text (codepoints)
        /// </summary>
        ReadOnlySpan<int> Codepoints { get; }
        
        /// <summary>
        /// Количество codepoints
        /// </summary>
        int Length { get; }
        
        /// <summary>
        /// Итератор по атрибутам
        /// </summary>
        AttributeEnumerator GetAttributes();
    }
    
    /// <summary>
    /// Парсер текстовой разметки
    /// </summary>
    public interface ITextParser
    {
        /// <summary>
        /// Парсить текст с разметкой
        /// </summary>
        void Parse(ReadOnlySpan<char> text, IParseResult result);
        
        /// <summary>
        /// Парсить plain text без разметки
        /// </summary>
        void ParsePlain(ReadOnlySpan<char> text, IParseResult result);
    }
    
    /// <summary>
    /// Результат BiDi анализа
    /// </summary>
    public interface IBidiResult
    {
        /// <summary>
        /// BiDi level для каждого codepoint
        /// </summary>
        ReadOnlySpan<byte> Levels { get; }
        
        /// <summary>
        /// Базовое направление параграфа
        /// </summary>
        TextDirection BaseDirection { get; }
    }
    
    /// <summary>
    /// BiDi анализатор (UAX #9)
    /// </summary>
    public interface IBidiAnalyzer
    {
        /// <summary>
        /// Анализировать текст
        /// </summary>
        void Analyze(ReadOnlySpan<int> codepoints, IBidiResult result);
        
        /// <summary>
        /// Анализировать с явным базовым направлением
        /// </summary>
        void Analyze(ReadOnlySpan<int> codepoints, TextDirection baseDirection, IBidiResult result);
    }
    
    /// <summary>
    /// Результат определения скриптов
    /// </summary>
    public interface IScriptResult
    {
        /// <summary>
        /// Script для каждого codepoint
        /// </summary>
        ReadOnlySpan<UnicodeScript> Scripts { get; }
    }
    
    /// <summary>
    /// Анализатор скриптов (UAX #24)
    /// </summary>
    public interface IScriptAnalyzer
    {
        /// <summary>
        /// Определить скрипты
        /// </summary>
        void Analyze(ReadOnlySpan<int> codepoints, IScriptResult result);
    }
    
    /// <summary>
    /// Результат itemization
    /// </summary>
    public interface IItemizeResult
    {
        /// <summary>
        /// Список runs
        /// </summary>
        ReadOnlySpan<TextRun> Runs { get; }
        
        /// <summary>
        /// Количество runs
        /// </summary>
        int RunCount { get; }
    }
    
    /// <summary>
    /// Itemizer - разбивает текст на runs
    /// </summary>
    public interface IItemizer
    {
        /// <summary>
        /// Создать runs из анализированного текста
        /// </summary>
        void Itemize(
            ReadOnlySpan<int> codepoints,
            ReadOnlySpan<byte> bidiLevels,
            ReadOnlySpan<UnicodeScript> scripts,
            IItemizeResult result);
    }
    
    /// <summary>
    /// Результат shaping
    /// </summary>
    public interface IShapeResult
    {
        /// <summary>
        /// Shaped runs
        /// </summary>
        ReadOnlySpan<ShapedRun> Runs { get; }
        
        /// <summary>
        /// Все глифы (runs ссылаются на диапазоны в этом массиве)
        /// </summary>
        ReadOnlySpan<ShapedGlyph> Glyphs { get; }
    }
    
    /// <summary>
    /// Text shaper (обёртка над HarfBuzz)
    /// </summary>
    public interface ITextShaper
    {
        /// <summary>
        /// Shape runs
        /// </summary>
        void Shape(
            ReadOnlySpan<int> codepoints,
            ReadOnlySpan<TextRun> runs,
            IFontProvider fontProvider,
            IShapeResult result);
    }
    
    /// <summary>
    /// Результат line breaking
    /// </summary>
    public interface ILineBreakResult
    {
        /// <summary>
        /// Строки
        /// </summary>
        ReadOnlySpan<TextLine> Lines { get; }
        
        /// <summary>
        /// Runs после BiDi reordering (per line)
        /// </summary>
        ReadOnlySpan<ShapedRun> OrderedRuns { get; }
    }
    
    /// <summary>
    /// Line breaker (UAX #14 + wrapping)
    /// </summary>
    public interface ILineBreaker
    {
        /// <summary>
        /// Разбить на строки
        /// </summary>
        void BreakLines(
            ReadOnlySpan<int> codepoints,
            ReadOnlySpan<ShapedRun> runs,
            ReadOnlySpan<ShapedGlyph> glyphs,
            float maxWidth,
            ILineBreakResult result);
    }
    
    /// <summary>
    /// Результат layout
    /// </summary>
    public interface ILayoutResult
    {
        /// <summary>
        /// Позиционированные глифы
        /// </summary>
        ReadOnlySpan<PositionedGlyph> Glyphs { get; }
        
        /// <summary>
        /// Общая ширина текста
        /// </summary>
        float Width { get; }
        
        /// <summary>
        /// Общая высота текста
        /// </summary>
        float Height { get; }
    }
    
    /// <summary>
    /// Layout engine
    /// </summary>
    public interface ITextLayout
    {
        /// <summary>
        /// Позиционировать глифы
        /// </summary>
        void Layout(
            ReadOnlySpan<TextLine> lines,
            ReadOnlySpan<ShapedRun> runs,
            ReadOnlySpan<ShapedGlyph> glyphs,
            ILayoutResult result);
    }
    
    /// <summary>
    /// Провайдер шрифтов
    /// </summary>
    public interface IFontProvider
    {
        /// <summary>
        /// Получить шрифт по ID
        /// </summary>
        IFontAsset GetFont(int fontId);
        
        /// <summary>
        /// Найти шрифт для codepoint (font fallback)
        /// </summary>
        int FindFontForCodepoint(int codepoint, int preferredFontId);
    }
    
    /// <summary>
    /// Абстракция шрифта
    /// </summary>
    public interface IFontAsset
    {
        /// <summary>
        /// ID шрифта
        /// </summary>
        int Id { get; }
        
        /// <summary>
        /// Имя шрифта
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Есть ли глиф для codepoint
        /// </summary>
        bool HasGlyph(int codepoint);
        
        /// <summary>
        /// Получить метрики глифа
        /// </summary>
        bool TryGetGlyphMetrics(int glyphId, out GlyphMetrics metrics);
        
        /// <summary>
        /// Данные шрифта для HarfBuzz (TTF/OTF bytes)
        /// </summary>
        ReadOnlySpan<byte> GetFontData();
    }
    
    /// <summary>
    /// Метрики глифа
    /// </summary>
    public struct GlyphMetrics
    {
        public float width;
        public float height;
        public float bearingX;
        public float bearingY;
        public float advance;
    }
    
    /// <summary>
    /// Enumerator для атрибутов (избегаем аллокаций)
    /// </summary>
    public ref struct AttributeEnumerator
    {
        // Реализация будет зависеть от хранилища атрибутов
        private readonly ITextAttribute[] attributes;
        private readonly int count;
        private int index;
        
        public AttributeEnumerator(ITextAttribute[] attributes, int count)
        {
            this.attributes = attributes;
            this.count = count;
            this.index = -1;
        }
        
        public ITextAttribute Current => attributes[index];
        
        public bool MoveNext()
        {
            index++;
            return index < count;
        }
    }
}
