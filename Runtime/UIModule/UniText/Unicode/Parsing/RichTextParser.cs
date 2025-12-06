using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tekst
{
    /// <summary>
    /// Парсер rich text разметки.
    /// Извлекает plain text (codepoints) и создаёт атрибуты из тэгов.
    /// </summary>
    public sealed class RichTextParser : ITextParser
    {
        private readonly TagRegistry tagRegistry;
        
        // Буферы для парсинга
        private int[] codepoints = new int[256];
        private int codepointCount;
        
        private ITextAttribute[] attributes = new ITextAttribute[32];
        private int attributeCount;
        
        // Стеки открытых тэгов (имя → стек состояний)
        private readonly Dictionary<string, Stack<TagScope>> openScopes = new(StringComparer.OrdinalIgnoreCase);
        
        // Буфер для имени тэга (избегаем аллокаций)
        private char[] tagNameBuffer = new char[32];
        
        private struct TagScope
        {
            public int startPosition;
            public object value;
        }
        
        public RichTextParser(TagRegistry tagRegistry)
        {
            this.tagRegistry = tagRegistry ?? TagRegistry.CreateDefault();
        }
        
        public RichTextParser() : this(TagRegistry.CreateDefault())
        {
        }
        
        /// <summary>
        /// Парсить текст с разметкой
        /// </summary>
        public void Parse(ReadOnlySpan<char> text, IParseResult result)
        {
            Reset();
            
            if (text.IsEmpty)
            {
                WriteResult(result);
                return;
            }
            
            EnsureCodepointCapacity(text.Length);
            
            int i = 0;
            bool inNoparse = false;
            
            while (i < text.Length)
            {
                char c = text[i];
                
                if (c == '<')
                {
                    // В режиме noparse ищем только </noparse>
                    if (inNoparse)
                    {
                        if (MatchesClosingTag(text, i, "noparse"))
                        {
                            CloseScope("noparse");
                            inNoparse = false;
                            i = SkipToTagEnd(text, i) + 1;
                            continue;
                        }
                    }
                    else
                    {
                        int tagEnd = FindTagEnd(text, i);
                        if (tagEnd > i)
                        {
                            var tagContent = text.Slice(i + 1, tagEnd - i - 1);
                            if (ProcessTag(tagContent, out bool isNoparseOpen))
                            {
                                if (isNoparseOpen)
                                    inNoparse = true;
                                i = tagEnd + 1;
                                continue;
                            }
                        }
                    }
                }
                
                // Обычный символ
                AddCharacter(text, ref i);
            }
            
            // Закрываем незакрытые тэги
            CloseAllScopes();
            
            WriteResult(result);
        }
        
        /// <summary>
        /// Парсить plain text без обработки тэгов
        /// </summary>
        public void ParsePlain(ReadOnlySpan<char> text, IParseResult result)
        {
            Reset();
            
            if (text.IsEmpty)
            {
                WriteResult(result);
                return;
            }
            
            EnsureCodepointCapacity(text.Length);
            
            int i = 0;
            while (i < text.Length)
            {
                AddCharacter(text, ref i);
            }
            
            WriteResult(result);
        }
        
        #region Tag Processing
        
        private bool ProcessTag(ReadOnlySpan<char> content, out bool isNoparseOpen)
        {
            isNoparseOpen = false;
            
            if (content.IsEmpty)
                return false;
            
            // Закрывающий тэг?
            bool isClosing = content[0] == '/';
            if (isClosing)
                content = content.Slice(1);
            
            // Находим имя тэга (до '=' или пробела)
            int nameEnd = 0;
            while (nameEnd < content.Length && content[nameEnd] != '=' && content[nameEnd] != ' ')
                nameEnd++;
            
            var tagName = content.Slice(0, nameEnd);
            
            if (tagName.IsEmpty)
                return false;
            
            // Ищем в реестре
            if (!tagRegistry.TryGet(tagName, out var definition))
                return false;
            
            if (isClosing)
            {
                if (definition.OnClose == null)
                    return false;
                
                var context = CreateContext(ReadOnlySpan<char>.Empty);
                definition.OnClose(ref context);
                return true;
            }
            else
            {
                // Извлекаем значение
                ReadOnlySpan<char> value = default;
                if (nameEnd < content.Length && content[nameEnd] == '=')
                {
                    value = content.Slice(nameEnd + 1);
                    value = TrimQuotes(value);
                }
                
                if (definition.RequiresValue && value.IsEmpty)
                    return false;
                
                var context = CreateContext(value);
                bool result = definition.OnOpen(ref context);
                
                // Проверяем на noparse
                if (result && tagName.Equals("noparse".AsSpan(), StringComparison.OrdinalIgnoreCase))
                    isNoparseOpen = true;
                
                return result;
            }
        }
        
        private TagContext CreateContext(ReadOnlySpan<char> value)
        {
            return new TagContext
            {
                Value = value,
                Position = codepointCount,
                AddCodepoint = AddCodepoint,
                OpenScope = OpenScope,
                GetScopeStart = GetScopeStart,
                GetScopeValue = GetScopeValue,
                CloseCurrentScope = CloseCurrentScope,
                AddAttribute = AddAttribute
            };
        }
        
        private void OpenScope(string tagName, object value)
        {
            if (!openScopes.TryGetValue(tagName, out var stack))
            {
                stack = new Stack<TagScope>(4);
                openScopes[tagName] = stack;
            }
            
            stack.Push(new TagScope
            {
                startPosition = codepointCount,
                value = value
            });
        }
        
        private int GetScopeStart(string tagName)
        {
            if (openScopes.TryGetValue(tagName, out var stack) && stack.Count > 0)
            {
                return stack.Peek().startPosition;
            }
            return -1;
        }
        
        private object GetScopeValue(string tagName)
        {
            if (openScopes.TryGetValue(tagName, out var stack) && stack.Count > 0)
            {
                return stack.Peek().value;
            }
            return null;
        }
        
        private void CloseCurrentScope(string tagName)
        {
            if (openScopes.TryGetValue(tagName, out var stack) && stack.Count > 0)
            {
                stack.Pop();
            }
        }
        
        private void CloseScope(string tagName)
        {
            if (openScopes.TryGetValue(tagName, out var stack) && stack.Count > 0)
            {
                stack.Pop();
            }
        }
        
        private ITextAttribute CloseScopeAndCreateAttribute(string tagName, int endPosition, object additionalData)
        {
            if (!openScopes.TryGetValue(tagName, out var stack) || stack.Count == 0)
                return null;
            
            var scope = stack.Pop();
            
            if (endPosition <= scope.startPosition)
                return null;
            
            // Создание атрибута зависит от типа тэга
            // Это будет делать сам обработчик тэга через additionalData или factory
            return null; // Placeholder - реальная логика в обработчике тэга
        }
        
        private void CloseAllScopes()
        {
            foreach (var kvp in openScopes)
            {
                var stack = kvp.Value;
                while (stack.Count > 0)
                {
                    if (tagRegistry.TryGet(kvp.Key.AsSpan(), out var definition) && definition.OnClose != null)
                    {
                        var context = CreateContext(ReadOnlySpan<char>.Empty);
                        definition.OnClose(ref context);
                    }
                    else
                    {
                        stack.Pop();
                    }
                }
            }
        }
        
        #endregion
        
        #region Character Handling
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddCharacter(ReadOnlySpan<char> text, ref int i)
        {
            char c = text[i];
            
            // Суррогатная пара?
            if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                int codepoint = char.ConvertToUtf32(c, text[i + 1]);
                AddCodepoint(codepoint);
                i += 2;
            }
            else
            {
                AddCodepoint(c);
                i++;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddCodepoint(int codepoint)
        {
            EnsureCodepointCapacity(codepointCount + 1);
            codepoints[codepointCount++] = codepoint;
        }
        
        private void AddAttribute(ITextAttribute attribute)
        {
            if (attribute == null) return;
            
            EnsureAttributeCapacity(attributeCount + 1);
            attributes[attributeCount++] = attribute;
        }
        
        #endregion
        
        #region Utilities
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FindTagEnd(ReadOnlySpan<char> text, int start)
        {
            for (int i = start + 1; i < text.Length && i < start + 128; i++)
            {
                if (text[i] == '>')
                    return i;
            }
            return -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int SkipToTagEnd(ReadOnlySpan<char> text, int start)
        {
            for (int i = start; i < text.Length; i++)
            {
                if (text[i] == '>')
                    return i;
            }
            return text.Length - 1;
        }
        
        private static bool MatchesClosingTag(ReadOnlySpan<char> text, int start, string tagName)
        {
            // Ожидаем </tagName>
            int required = 3 + tagName.Length; // < / name >
            if (start + required > text.Length)
                return false;
            
            if (text[start] != '<' || text[start + 1] != '/')
                return false;
            
            for (int i = 0; i < tagName.Length; i++)
            {
                char c = text[start + 2 + i];
                char t = tagName[i];
                
                // Case-insensitive
                if (char.ToLowerInvariant(c) != char.ToLowerInvariant(t))
                    return false;
            }
            
            return text[start + 2 + tagName.Length] == '>';
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<char> TrimQuotes(ReadOnlySpan<char> value)
        {
            if (value.Length >= 2)
            {
                char first = value[0];
                char last = value[value.Length - 1];
                if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
                {
                    return value.Slice(1, value.Length - 2);
                }
            }
            return value;
        }
        
        private void Reset()
        {
            codepointCount = 0;
            attributeCount = 0;
            
            foreach (var stack in openScopes.Values)
                stack.Clear();
        }
        
        private void EnsureCodepointCapacity(int required)
        {
            if (codepoints.Length >= required) return;
            int newSize = Math.Max(required, codepoints.Length * 2);
            Array.Resize(ref codepoints, newSize);
        }
        
        private void EnsureAttributeCapacity(int required)
        {
            if (attributes.Length >= required) return;
            int newSize = Math.Max(required, attributes.Length * 2);
            Array.Resize(ref attributes, newSize);
        }
        
        private void WriteResult(IParseResult result)
        {
            if (result is ParseResultBuffer buffer)
            {
                buffer.Set(codepoints.AsSpan(0, codepointCount), attributes, attributeCount);
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Конкретная реализация IParseResult с внутренними буферами
    /// </summary>
    public sealed class ParseResultBuffer : IParseResult
    {
        private int[] codepoints = new int[256];
        private int codepointCount;
        private ITextAttribute[] attributes = new ITextAttribute[32];
        private int attributeCount;
        
        public ReadOnlySpan<int> Codepoints => codepoints.AsSpan(0, codepointCount);
        public int Length => codepointCount;
        
        public AttributeEnumerator GetAttributes() => new(attributes, attributeCount);
        
        internal void Set(ReadOnlySpan<int> newCodepoints, ITextAttribute[] newAttributes, int newAttributeCount)
        {
            // Codepoints
            if (codepoints.Length < newCodepoints.Length)
                codepoints = new int[newCodepoints.Length];
            newCodepoints.CopyTo(codepoints);
            codepointCount = newCodepoints.Length;
            
            // Attributes
            if (attributes.Length < newAttributeCount)
                attributes = new ITextAttribute[newAttributeCount];
            Array.Copy(newAttributes, attributes, newAttributeCount);
            attributeCount = newAttributeCount;
        }
        
        public void Clear()
        {
            codepointCount = 0;
            attributeCount = 0;
        }
    }
}
