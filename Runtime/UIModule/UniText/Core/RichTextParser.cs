using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// Парсер rich text разметки.
/// Извлекает plain text (codepoints) и создаёт атрибуты из тэгов.
/// </summary>
public sealed class RichTextParser
{
    private readonly TagRegistry tagRegistry;

    // Стеки открытых тэгов (имя → стек состояний)
    private readonly Dictionary<string, Stack<TagScope>> openScopes = new(StringComparer.OrdinalIgnoreCase);

    private struct TagScope
    {
        public int startPosition;
        public object value;
    }

    // Callbacks for direct mode
    private Action<int> directAddCodepoint;
    private List<TextAttributeBase> directAttributes;
    private int directPosition;

    public RichTextParser(TagRegistry tagRegistry)
    {
        this.tagRegistry = tagRegistry ?? TagRegistry.CreateDefault();
    }

    public RichTextParser() : this(TagRegistry.CreateDefault())
    {
    }

    /// <summary>
    /// Обработать тэг напрямую, без внутренних буферов.
    /// Используется из TextProcessor для интеграции с unified buffers.
    /// </summary>
    public bool ProcessTagDirect(
        ReadOnlySpan<char> tagContent,
        int currentPosition,
        List<TextAttributeBase> attributes,
        Action<int> addCodepoint,
        out bool isNoparseOpen)
    {
        isNoparseOpen = false;

        if (tagContent.IsEmpty)
            return false;

        // Store callbacks for direct mode
        directAddCodepoint = addCodepoint;
        directAttributes = attributes;
        directPosition = currentPosition;

        // Closing tag?
        bool isClosing = tagContent[0] == '/';
        var content = isClosing ? tagContent.Slice(1) : tagContent;

        // Find tag name
        int nameEnd = 0;
        while (nameEnd < content.Length && content[nameEnd] != '=' && content[nameEnd] != ' ')
            nameEnd++;

        var tagName = content.Slice(0, nameEnd);

        if (tagName.IsEmpty)
            return false;

        // Lookup in registry
        if (!tagRegistry.TryGet(tagName, out var definition))
            return false;

        if (isClosing)
        {
            if (definition.OnClose == null)
                return false;

            var context = CreateContextDirect(ReadOnlySpan<char>.Empty);
            definition.OnClose(ref context);
            return true;
        }
        else
        {
            // Extract value
            ReadOnlySpan<char> value = default;
            if (nameEnd < content.Length && content[nameEnd] == '=')
            {
                value = content.Slice(nameEnd + 1);
                value = TrimQuotes(value);
            }

            if (definition.RequiresValue && value.IsEmpty)
                return false;

            var context = CreateContextDirect(value);
            bool result = definition.OnOpen(ref context);

            // Check for noparse
            if (result && tagName.Equals("noparse".AsSpan(), StringComparison.OrdinalIgnoreCase))
                isNoparseOpen = true;

            return result;
        }
    }

    private TagContext CreateContextDirect(ReadOnlySpan<char> value)
    {
        return new TagContext
        {
            Value = value,
            Position = directPosition,
            AddCodepoint = cp => directAddCodepoint?.Invoke(cp),
            OpenScope = OpenScope,
            GetScopeStart = GetScopeStart,
            GetScopeValue = GetScopeValue,
            CloseCurrentScope = CloseCurrentScope,
            AddAttribute = AddAttributeDirect
        };
    }

    private void AddAttributeDirect(TextAttributeBase attribute)
    {
        if (attribute != null)
        {
            directAttributes?.Add(attribute);
        }
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
            startPosition = directPosition,
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

    /// <summary>
    /// Сбросить состояние парсера.
    /// </summary>
    public void Reset()
    {
        foreach (var stack in openScopes.Values)
            stack.Clear();

        directAddCodepoint = null;
        directAttributes = null;
        directPosition = 0;
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
}
