using System;

namespace Tekst
{
    /// <summary>
    /// Line Breaker — разбивает текст на строки.
    /// Использует LineBreakAlgorithm (UAX #14) для определения break opportunities
    /// и выполняет word wrapping.
    /// </summary>
    public sealed class LineBreaker : ILineBreaker
    {
        private readonly LineBreakAlgorithm lineBreakAlgorithm;
        
        // Mandatory line break characters (stable Unicode codepoints per UAX #14)
        // These are control characters that will never change in Unicode.
        // See: https://www.unicode.org/reports/tr14/#BK
        private const int LineFeed = 0x000A;           // LF - Line Feed
        private const int VerticalTab = 0x000B;        // VT - Vertical Tab (BK class)
        private const int FormFeed = 0x000C;           // FF - Form Feed (BK class)
        private const int CarriageReturn = 0x000D;     // CR - Carriage Return
        private const int NextLine = 0x0085;           // NEL - Next Line
        private const int LineSeparator = 0x2028;      // LS - Line Separator (BK class)
        private const int ParagraphSeparator = 0x2029; // PS - Paragraph Separator (BK class)
        
        // Буферы
        private TextLine[] lines = new TextLine[16];
        private int lineCount;
        private ShapedRun[] orderedRuns = new ShapedRun[32];
        private int orderedRunCount;
        
        // Break opportunities (от LineBreakAlgorithm)
        private bool[] breakOpportunities = new bool[257];
        
        public LineBreaker(LineBreakAlgorithm lineBreakAlgorithm)
        {
            this.lineBreakAlgorithm = lineBreakAlgorithm ?? throw new ArgumentNullException(nameof(lineBreakAlgorithm));
        }
        
        public LineBreaker(IUnicodeDataProvider dataProvider)
        {
            if (dataProvider == null)
                throw new ArgumentNullException(nameof(dataProvider));
            
            lineBreakAlgorithm = new LineBreakAlgorithm(dataProvider);
        }
        
        public void BreakLines(
            ReadOnlySpan<int> codepoints,
            ReadOnlySpan<ShapedRun> runs,
            ReadOnlySpan<ShapedGlyph> glyphs,
            float maxWidth,
            ILineBreakResult result)
        {
            lineCount = 0;
            orderedRunCount = 0;
            
            if (runs.IsEmpty)
            {
                WriteResult(result);
                return;
            }
            
            // Шаг 1: Получаем break opportunities через LineBreakAlgorithm
            GetBreakOpportunities(codepoints);
            
            // Шаг 2: Разбиваем на строки по ширине
            WrapLines(codepoints, runs, glyphs, maxWidth);
            
            // Шаг 3: BiDi reorder runs внутри каждой строки
            ReorderRunsPerLine(runs);
            
            WriteResult(result);
        }
        
        /// <summary>
        /// Получить break opportunities через LineBreakAlgorithm
        /// </summary>
        private void GetBreakOpportunities(ReadOnlySpan<int> codepoints)
        {
            int requiredLength = codepoints.Length + 1;
            if (breakOpportunities.Length < requiredLength)
            {
                breakOpportunities = new bool[Math.Max(requiredLength, breakOpportunities.Length * 2)];
            }
            
            lineBreakAlgorithm.GetBreakOpportunities(codepoints, breakOpportunities);
        }
        
        /// <summary>
        /// Можно ли разорвать ПОСЛЕ позиции index?
        /// </summary>
        private bool CanBreakAfter(int index)
        {
            // breakOpportunities[i+1] = можно ли разорвать между codepoints[i] и codepoints[i+1]
            int breakIndex = index + 1;
            if (breakIndex < 0 || breakIndex >= breakOpportunities.Length)
                return false;
            return breakOpportunities[breakIndex];
        }
        
        /// <summary>
        /// Является ли позиция обязательным разрывом?
        /// Per UAX #14, mandatory breaks occur at: BK, CR, LF, NL (with specific rules for CR×LF).
        /// </summary>
        private bool IsMandatoryBreak(ReadOnlySpan<int> codepoints, int index)
        {
            if (index < 0 || index >= codepoints.Length)
                return false;
            
            int cp = codepoints[index];
            // Mandatory breaks: LF, VT, FF, CR (if not followed by LF), NEL, LS, PS
            return cp == LineFeed ||
                   cp == VerticalTab ||
                   cp == FormFeed ||
                   cp == CarriageReturn ||
                   cp == NextLine ||
                   cp == LineSeparator ||
                   cp == ParagraphSeparator;
        }
        
        /// <summary>
        /// Word wrapping
        /// </summary>
        private void WrapLines(
            ReadOnlySpan<int> codepoints,
            ReadOnlySpan<ShapedRun> runs,
            ReadOnlySpan<ShapedGlyph> glyphs,
            float maxWidth)
        {
            int runStartIndex = 0;
            float currentWidth = 0;
            int lastBreakRun = -1;
            float widthAtLastBreak = 0;
            int runCountAtLastBreak = 0;
            
            for (int i = 0; i < runs.Length; i++)
            {
                var run = runs[i];
                
                // Накапливаем ширину
                currentWidth += run.width;
                
                // Проверяем break opportunity в конце run
                int runEnd = run.range.End - 1;
                if (runEnd >= 0 && CanBreakAfter(runEnd))
                {
                    lastBreakRun = i;
                    widthAtLastBreak = currentWidth;
                    runCountAtLastBreak = i - runStartIndex + 1;
                }
                
                // Превысили ширину?
                if (currentWidth > maxWidth && lastBreakRun >= 0)
                {
                    // Создаём строку до последней break opportunity
                    CreateLine(runs, runStartIndex, runCountAtLastBreak, codepoints, glyphs);
                    
                    // Начинаем новую строку
                    runStartIndex = lastBreakRun + 1;
                    currentWidth = currentWidth - widthAtLastBreak;
                    lastBreakRun = -1;
                }
                
                // Mandatory break?
                if (runEnd >= 0 && IsMandatoryBreak(codepoints, runEnd))
                {
                    CreateLine(runs, runStartIndex, i - runStartIndex + 1, codepoints, glyphs);
                    runStartIndex = i + 1;
                    currentWidth = 0;
                    lastBreakRun = -1;
                }
            }
            
            // Последняя строка
            if (runStartIndex < runs.Length)
            {
                CreateLine(runs, runStartIndex, runs.Length - runStartIndex, codepoints, glyphs);
            }
        }
        
        private void CreateLine(
            ReadOnlySpan<ShapedRun> runs,
            int startRun,
            int runCount,
            ReadOnlySpan<int> codepoints,
            ReadOnlySpan<ShapedGlyph> glyphs)
        {
            if (runCount <= 0) return;
            
            var firstRun = runs[startRun];
            var lastRun = runs[startRun + runCount - 1];
            
            float width = 0;
            for (int i = 0; i < runCount; i++)
            {
                width += runs[startRun + i].width;
            }
            
            EnsureLineCapacity(lineCount + 1);
            lines[lineCount++] = new TextLine
            {
                range = new TextRange(firstRun.range.start, lastRun.range.End - firstRun.range.start),
                runStart = orderedRunCount,
                runCount = runCount,
                width = width,
                height = 0, // TODO: вычислить из метрик шрифта
                baseline = 0
            };
            
            // Копируем runs (пока без reorder)
            for (int i = 0; i < runCount; i++)
            {
                EnsureOrderedRunCapacity(orderedRunCount + 1);
                orderedRuns[orderedRunCount++] = runs[startRun + i];
            }
        }
        
        /// <summary>
        /// BiDi reorder runs внутри строки
        /// </summary>
        private void ReorderRunsPerLine(ReadOnlySpan<ShapedRun> originalRuns)
        {
            for (int i = 0; i < lineCount; i++)
            {
                var line = lines[i];
                ReorderRunsInLine(line.runStart, line.runCount);
            }
        }
        
        /// <summary>
        /// Reorder runs по BiDi levels (правило L2)
        /// </summary>
        private void ReorderRunsInLine(int start, int count)
        {
            if (count <= 1) return;
            
            // Находим max level
            byte maxLevel = 0;
            for (int i = 0; i < count; i++)
            {
                var level = GetRunLevel(orderedRuns[start + i]);
                if (level > maxLevel) maxLevel = level;
            }
            
            // Reverse subsequences с level >= currentLevel, от maxLevel до 1
            for (byte level = maxLevel; level >= 1; level--)
            {
                int runStart = -1;
                
                for (int i = 0; i <= count; i++)
                {
                    bool inSequence = i < count && GetRunLevel(orderedRuns[start + i]) >= level;
                    
                    if (inSequence && runStart < 0)
                    {
                        runStart = i;
                    }
                    else if (!inSequence && runStart >= 0)
                    {
                        // Reverse [runStart, i)
                        ReverseRuns(start + runStart, i - runStart);
                        runStart = -1;
                    }
                }
            }
        }
        
        private byte GetRunLevel(ShapedRun run)
        {
            return run.direction == TextDirection.RightToLeft ? (byte)1 : (byte)0;
        }
        
        private void ReverseRuns(int start, int count)
        {
            int end = start + count - 1;
            while (start < end)
            {
                var temp = orderedRuns[start];
                orderedRuns[start] = orderedRuns[end];
                orderedRuns[end] = temp;
                start++;
                end--;
            }
        }
        
        private void WriteResult(ILineBreakResult result)
        {
            if (result is LineBreakResultBuffer buffer)
            {
                buffer.Set(lines.AsSpan(0, lineCount), orderedRuns.AsSpan(0, orderedRunCount));
            }
        }
        
        private void EnsureLineCapacity(int required)
        {
            if (lines.Length >= required) return;
            Array.Resize(ref lines, Math.Max(required, lines.Length * 2));
        }
        
        private void EnsureOrderedRunCapacity(int required)
        {
            if (orderedRuns.Length >= required) return;
            Array.Resize(ref orderedRuns, Math.Max(required, orderedRuns.Length * 2));
        }
    }
    
    /// <summary>
    /// Буфер результатов Line Breaker
    /// </summary>
    public sealed class LineBreakResultBuffer : ILineBreakResult
    {
        private TextLine[] lines = new TextLine[16];
        private int lineCount;
        private ShapedRun[] orderedRuns = new ShapedRun[32];
        private int runCount;
        
        public ReadOnlySpan<TextLine> Lines => lines.AsSpan(0, lineCount);
        public ReadOnlySpan<ShapedRun> OrderedRuns => orderedRuns.AsSpan(0, runCount);
        
        internal void Set(ReadOnlySpan<TextLine> sourceLines, ReadOnlySpan<ShapedRun> sourceRuns)
        {
            if (lines.Length < sourceLines.Length)
                lines = new TextLine[sourceLines.Length];
            sourceLines.CopyTo(lines);
            lineCount = sourceLines.Length;
            
            if (orderedRuns.Length < sourceRuns.Length)
                orderedRuns = new ShapedRun[sourceRuns.Length];
            sourceRuns.CopyTo(orderedRuns);
            runCount = sourceRuns.Length;
        }
        
        public void Clear()
        {
            lineCount = 0;
            runCount = 0;
        }
    }
}