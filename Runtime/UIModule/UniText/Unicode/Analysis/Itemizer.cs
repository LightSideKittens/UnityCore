using System;

namespace Tekst
{
    /// <summary>
    /// Itemizer — разбивает текст на runs.
    /// Run создаётся при изменении любого из:
    /// - BiDi level
    /// - Script
    /// - Font
    /// - Shaping-affecting attributes
    /// </summary>
    public sealed class Itemizer : IItemizer
    {
        // Буферы
        private TextRun[] runs = new TextRun[32];
        private int runCount;
        
        public void Itemize(
            ReadOnlySpan<int> codepoints,
            ReadOnlySpan<byte> bidiLevels,
            ReadOnlySpan<UnicodeScript> scripts,
            IItemizeResult result)
        {
            runCount = 0;
            
            if (codepoints.IsEmpty)
            {
                WriteResult(result);
                return;
            }
            
            // Начинаем первый run
            int runStart = 0;
            byte currentLevel = bidiLevels[0];
            var currentScript = scripts[0];
            
            for (int i = 1; i < codepoints.Length; i++)
            {
                bool needBreak = false;
                
                // Проверяем изменение BiDi level
                if (bidiLevels[i] != currentLevel)
                    needBreak = true;
                
                // Проверяем изменение Script
                if (scripts[i] != currentScript)
                    needBreak = true;
                
                // TODO: Проверка изменения font (font fallback)
                // TODO: Проверка изменения shaping attributes
                
                if (needBreak)
                {
                    // Завершаем текущий run
                    AddRun(runStart, i - runStart, currentLevel, currentScript);
                    
                    // Начинаем новый
                    runStart = i;
                    currentLevel = bidiLevels[i];
                    currentScript = scripts[i];
                }
            }
            
            // Последний run
            AddRun(runStart, codepoints.Length - runStart, currentLevel, currentScript);
            
            WriteResult(result);
        }
        
        /// <summary>
        /// Расширенная itemization с учётом атрибутов и font fallback
        /// </summary>
        public void Itemize(
            ReadOnlySpan<int> codepoints,
            ReadOnlySpan<byte> bidiLevels,
            ReadOnlySpan<UnicodeScript> scripts,
            IAttributeSource attributes,
            IFontProvider fontProvider,
            int baseFontId,
            IItemizeResult result)
        {
            runCount = 0;
            
            if (codepoints.IsEmpty)
            {
                WriteResult(result);
                return;
            }
            
            int runStart = 0;
            byte currentLevel = bidiLevels[0];
            var currentScript = scripts[0];
            int currentFontId = fontProvider?.FindFontForCodepoint(codepoints[0], baseFontId) ?? baseFontId;
            int currentAttrSnapshot = attributes?.GetSnapshotAt(0) ?? 0;
            
            for (int i = 1; i < codepoints.Length; i++)
            {
                bool needBreak = false;
                
                // BiDi level change
                if (bidiLevels[i] != currentLevel)
                    needBreak = true;
                
                // Script change
                if (scripts[i] != currentScript)
                    needBreak = true;
                
                // Font change (for fallback)
                int fontId = fontProvider?.FindFontForCodepoint(codepoints[i], baseFontId) ?? baseFontId;
                if (fontId != currentFontId)
                    needBreak = true;
                
                // Attribute change (only shaping-affecting)
                int attrSnapshot = attributes?.GetSnapshotAt(i) ?? 0;
                if (attrSnapshot != currentAttrSnapshot)
                    needBreak = true;
                
                if (needBreak)
                {
                    AddRun(runStart, i - runStart, currentLevel, currentScript, currentFontId, currentAttrSnapshot);
                    
                    runStart = i;
                    currentLevel = bidiLevels[i];
                    currentScript = scripts[i];
                    currentFontId = fontId;
                    currentAttrSnapshot = attrSnapshot;
                }
            }
            
            // Last run
            AddRun(runStart, codepoints.Length - runStart, currentLevel, currentScript, currentFontId, currentAttrSnapshot);
            
            WriteResult(result);
        }
        
        private void AddRun(int start, int length, byte bidiLevel, UnicodeScript script, int fontId = 0, int attrSnapshot = 0)
        {
            EnsureCapacity(runCount + 1);
            
            runs[runCount++] = new TextRun
            {
                range = new TextRange(start, length),
                bidiLevel = bidiLevel,
                script = script,
                fontId = fontId,
                attributeSnapshot = attrSnapshot
            };
        }
        
        private void WriteResult(IItemizeResult result)
        {
            if (result is ItemizeResultBuffer buffer)
            {
                buffer.Set(runs.AsSpan(0, runCount));
            }
        }
        
        private void EnsureCapacity(int required)
        {
            if (runs.Length >= required) return;
            int newSize = Math.Max(required, runs.Length * 2);
            Array.Resize(ref runs, newSize);
        }
    }
    
    /// <summary>
    /// Источник атрибутов для itemization
    /// </summary>
    public interface IAttributeSource
    {
        /// <summary>
        /// Получить snapshot ID атрибутов в позиции.
        /// Одинаковые ID означают одинаковые shaping-affecting атрибуты.
        /// </summary>
        int GetSnapshotAt(int position);
    }
    
    /// <summary>
    /// Буфер результатов Itemizer
    /// </summary>
    public sealed class ItemizeResultBuffer : IItemizeResult
    {
        private TextRun[] runs = new TextRun[32];
        private int count;
        
        public ReadOnlySpan<TextRun> Runs => runs.AsSpan(0, count);
        public int RunCount => count;
        
        internal void Set(ReadOnlySpan<TextRun> source)
        {
            if (runs.Length < source.Length)
                runs = new TextRun[source.Length];
            
            source.CopyTo(runs);
            count = source.Length;
        }
        
        public void Clear() => count = 0;
    }
}
