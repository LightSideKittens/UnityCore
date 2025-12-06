using System;

namespace Tekst
{
    /// <summary>
    /// Адаптер для BidiEngine.
    /// Реализует IBidiAnalyzer, делегируя работу существующему BidiEngine.
    /// </summary>
    public sealed class BidiAnalyzerAdapter : IBidiAnalyzer
    {
        private readonly BidiEngine engine;
        
        public BidiAnalyzerAdapter(BidiEngine engine)
        {
            this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }
        
        public BidiAnalyzerAdapter(IUnicodeDataProvider dataProvider)
        {
            if (dataProvider == null)
                throw new ArgumentNullException(nameof(dataProvider));
            
            engine = new BidiEngine(dataProvider);
        }
        
        public void Analyze(ReadOnlySpan<int> codepoints, IBidiResult result)
        {
            var bidiResult = engine.Process(codepoints, BidiParagraphDirection.Auto);
            
            if (result is BidiResultAdapter adapter)
            {
                adapter.Set(bidiResult);
            }
        }
        
        public void Analyze(ReadOnlySpan<int> codepoints, TextDirection baseDirection, IBidiResult result)
        {
            var direction = baseDirection == TextDirection.RightToLeft
                ? BidiParagraphDirection.RightToLeft
                : BidiParagraphDirection.LeftToRight;
            
            var bidiResult = engine.Process(codepoints, direction);
            
            if (result is BidiResultAdapter adapter)
            {
                adapter.Set(bidiResult);
            }
        }
        
        /// <summary>
        /// Доступ к underlying engine для расширенных операций
        /// </summary>
        public BidiEngine Engine => engine;
    }
    
    /// <summary>
    /// Адаптер результата BiDi анализа.
    /// Оборачивает BidiResult для совместимости с IBidiResult.
    /// </summary>
    public sealed class BidiResultAdapter : IBidiResult
    {
        private BidiResult result;
        
        public ReadOnlySpan<byte> Levels => result.levels ?? ReadOnlySpan<byte>.Empty;
        
        public TextDirection BaseDirection => 
            result.Direction == BidiDirection.RightToLeft 
                ? TextDirection.RightToLeft 
                : TextDirection.LeftToRight;
        
        /// <summary>
        /// Доступ к underlying result для расширенных операций
        /// </summary>
        public BidiResult UnderlyingResult => result;
        
        /// <summary>
        /// Параграфы
        /// </summary>
        public ReadOnlySpan<BidiParagraph> Paragraphs => result.paragraphs ?? ReadOnlySpan<BidiParagraph>.Empty;
        
        /// <summary>
        /// Есть ли RTL контент
        /// </summary>
        public bool HasRtlContent => result.HasRtlContent;
        
        internal void Set(BidiResult bidiResult)
        {
            result = bidiResult;
        }
        
        public void Clear()
        {
            result = default;
        }
    }
}