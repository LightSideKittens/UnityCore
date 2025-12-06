using System;

namespace Tekst
{
    /// <summary>
    /// Анализатор скриптов (UAX #24).
    /// Определяет Unicode Script для каждого codepoint.
    /// </summary>
    public sealed class ScriptAnalyzer : IScriptAnalyzer
    {
        private readonly IUnicodeDataProvider dataProvider;
        
        // Буфер результатов
        private UnicodeScript[] scripts = new UnicodeScript[256];
        private int length;
        
        public ScriptAnalyzer(IUnicodeDataProvider dataProvider)
        {
            this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        }
        
        public void Analyze(ReadOnlySpan<int> codepoints, IScriptResult result)
        {
            EnsureCapacity(codepoints.Length);
            length = codepoints.Length;
            
            // Первый проход: определяем скрипт каждого codepoint
            for (int i = 0; i < codepoints.Length; i++)
            {
                scripts[i] = dataProvider.GetScript(codepoints[i]);
            }
            
            // Второй проход: разрешаем Common и Inherited
            // UAX #24: Common/Inherited наследуют скрипт от соседей
            ResolveInheritedScripts(codepoints);
            
            // Записываем результат
            if (result is ScriptResultBuffer buffer)
            {
                buffer.Set(scripts.AsSpan(0, length));
            }
        }
        
        /// <summary>
        /// Разрешение Common и Inherited скриптов.
        /// По UAX #24, эти скрипты должны наследовать от окружающего контекста.
        /// </summary>
        private void ResolveInheritedScripts(ReadOnlySpan<int> codepoints)
        {
            UnicodeScript lastRealScript = UnicodeScript.Unknown;
            
            // Forward pass: наследуем от предыдущего
            for (int i = 0; i < length; i++)
            {
                var script = scripts[i];
                
                if (script == UnicodeScript.Common || script == UnicodeScript.Inherited)
                {
                    if (lastRealScript != UnicodeScript.Unknown)
                    {
                        scripts[i] = lastRealScript;
                    }
                }
                else
                {
                    lastRealScript = script;
                }
            }
            
            // Backward pass: для оставшихся Common/Inherited в начале
            lastRealScript = UnicodeScript.Unknown;
            for (int i = length - 1; i >= 0; i--)
            {
                var script = scripts[i];
                
                if (script == UnicodeScript.Common || script == UnicodeScript.Inherited)
                {
                    if (lastRealScript != UnicodeScript.Unknown)
                    {
                        scripts[i] = lastRealScript;
                    }
                }
                else
                {
                    lastRealScript = script;
                }
            }
        }
        
        private void EnsureCapacity(int required)
        {
            if (scripts.Length >= required) return;
            int newSize = Math.Max(required, scripts.Length * 2);
            scripts = new UnicodeScript[newSize];
        }
    }
    
    /// <summary>
    /// Буфер результатов Script Analyzer
    /// </summary>
    public sealed class ScriptResultBuffer : IScriptResult
    {
        private UnicodeScript[] scripts = new UnicodeScript[256];
        private int length;
        
        public ReadOnlySpan<UnicodeScript> Scripts => scripts.AsSpan(0, length);
        
        internal void Set(ReadOnlySpan<UnicodeScript> source)
        {
            if (scripts.Length < source.Length)
                scripts = new UnicodeScript[source.Length];
            
            source.CopyTo(scripts);
            length = source.Length;
        }
        
        public void Clear() => length = 0;
    }
}