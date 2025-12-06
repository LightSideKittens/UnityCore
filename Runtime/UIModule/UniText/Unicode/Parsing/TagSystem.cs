using System;
using System.Collections.Generic;

namespace Tekst
{
    /// <summary>
    /// Контекст парсинга тэга
    /// </summary>
    public ref struct TagContext
    {
        /// <summary>
        /// Значение тэга (после =)
        /// </summary>
        public ReadOnlySpan<char> Value;
        
        /// <summary>
        /// Текущая позиция в plain text
        /// </summary>
        public int Position;
        
        /// <summary>
        /// Добавить codepoint в plain text
        /// </summary>
        public Action<int> AddCodepoint;
        
        /// <summary>
        /// Открыть scope атрибута
        /// </summary>
        public Action<string, object> OpenScope;
        
        /// <summary>
        /// Получить начальную позицию scope
        /// </summary>
        public Func<string, int> GetScopeStart;
        
        /// <summary>
        /// Получить значение scope
        /// </summary>
        public Func<string, object> GetScopeValue;
        
        /// <summary>
        /// Закрыть текущий scope
        /// </summary>
        public Action<string> CloseCurrentScope;
        
        /// <summary>
        /// Добавить атрибут
        /// </summary>
        public Action<ITextAttribute> AddAttribute;
    }
    
    /// <summary>
    /// Обработчик открывающего тэга
    /// </summary>
    /// <returns>true если тэг обработан</returns>
    public delegate bool TagOpenHandler(ref TagContext context);
    
    /// <summary>
    /// Обработчик закрывающего тэга
    /// </summary>
    public delegate void TagCloseHandler(ref TagContext context);
    
    /// <summary>
    /// Определение тэга
    /// </summary>
    public readonly struct TagDefinition
    {
        /// <summary>
        /// Имя тэга
        /// </summary>
        public readonly string Name;
        
        /// <summary>
        /// Обработчик открытия
        /// </summary>
        public readonly TagOpenHandler OnOpen;
        
        /// <summary>
        /// Обработчик закрытия (null для самозакрывающихся)
        /// </summary>
        public readonly TagCloseHandler OnClose;
        
        /// <summary>
        /// Требуется ли значение
        /// </summary>
        public readonly bool RequiresValue;
        
        public TagDefinition(string name, TagOpenHandler onOpen, TagCloseHandler onClose = null, bool requiresValue = false)
        {
            Name = name;
            OnOpen = onOpen;
            OnClose = onClose;
            RequiresValue = requiresValue;
        }
        
        /// <summary>
        /// Самозакрывающийся тэг
        /// </summary>
        public bool IsSelfClosing => OnClose == null;
    }
    
    /// <summary>
    /// Реестр тэгов
    /// </summary>
    public sealed class TagRegistry
    {
        private readonly Dictionary<string, TagDefinition> tags = new(StringComparer.OrdinalIgnoreCase);
        
        /// <summary>
        /// Зарегистрировать тэг
        /// </summary>
        public void Register(TagDefinition definition)
        {
            tags[definition.Name] = definition;
        }
        
        /// <summary>
        /// Зарегистрировать тэг с алиасами
        /// </summary>
        public void Register(TagDefinition definition, params string[] aliases)
        {
            tags[definition.Name] = definition;
            foreach (var alias in aliases)
            {
                tags[alias] = definition;
            }
        }
        
        /// <summary>
        /// Получить определение тэга
        /// </summary>
        public bool TryGet(ReadOnlySpan<char> name, out TagDefinition definition)
        {
            // TODO: оптимизировать без аллокации строки
            return tags.TryGetValue(name.ToString(), out definition);
        }
        
        /// <summary>
        /// Получить обработчик открытия по имени
        /// </summary>
        public TagOpenHandler GetOpenHandler(string name)
        {
            return tags.TryGetValue(name, out var def) ? def.OnOpen : null;
        }
        
        /// <summary>
        /// Получить обработчик закрытия по имени
        /// </summary>
        public TagCloseHandler GetCloseHandler(string name)
        {
            return tags.TryGetValue(name, out var def) ? def.OnClose : null;
        }
        
        /// <summary>
        /// Удалить тэг
        /// </summary>
        public bool Remove(string name) => tags.Remove(name);
        
        /// <summary>
        /// Очистить все тэги
        /// </summary>
        public void Clear() => tags.Clear();
        
        /// <summary>
        /// Создать реестр с базовыми тэгами
        /// </summary>
        public static TagRegistry CreateDefault()
        {
            var registry = new TagRegistry();
            BuiltInTags.RegisterAll(registry);
            return registry;
        }
    }
    
    /// <summary>
    /// Встроенные тэги
    /// </summary>
    public static class BuiltInTags
    {
        public static void RegisterAll(TagRegistry registry)
        {
            // Render attributes
            RegisterColorTag(registry);
            RegisterAlphaTag(registry);
            RegisterUnderlineTag(registry);
            RegisterStrikethroughTag(registry);
            RegisterMarkTag(registry);
            RegisterLinkTag(registry);
            
            // Shaping attributes
            RegisterBoldTag(registry);
            RegisterItalicTag(registry);
            RegisterSizeTag(registry);
            RegisterFontTag(registry);
            RegisterCharacterSpacingTag(registry);
            RegisterSuperscriptTag(registry);
            RegisterSubscriptTag(registry);
            
            // Layout attributes
            RegisterAlignTag(registry);
            RegisterIndentTag(registry);
            RegisterLineSpacingTag(registry);
            RegisterNoBreakTag(registry);
            
            // Special / self-closing
            RegisterBreakTags(registry);
        }
        
        // ==========================================
        // Render Attributes
        // ==========================================
        
        private static void RegisterColorTag(TagRegistry registry)
        {
            registry.Register(new TagDefinition(
                name: "color",
                onOpen: (ref TagContext ctx) =>
                {
                    if (ctx.Value.IsEmpty) return false;
                    var color = ParseColor(ctx.Value);
                    ctx.OpenScope("color", color);
                    return true;
                },
                onClose: (ref TagContext ctx) =>
                {
                    int start = ctx.GetScopeStart("color");
                    if (start >= 0 && ctx.Position > start)
                    {
                        var color = (uint)ctx.GetScopeValue("color");
                        ctx.AddAttribute(new ColorAttribute(start, ctx.Position, color));
                    }
                    ctx.CloseCurrentScope("color");
                },
                requiresValue: true
            ));
        }
        
        private static void RegisterAlphaTag(TagRegistry registry)
        {
            registry.Register(new TagDefinition(
                name: "alpha",
                onOpen: (ref TagContext ctx) =>
                {
                    if (ctx.Value.IsEmpty) return false;
                    var alpha = ParseAlpha(ctx.Value);
                    ctx.OpenScope("alpha", alpha);
                    return true;
                },
                onClose: (ref TagContext ctx) =>
                {
                    int start = ctx.GetScopeStart("alpha");
                    if (start >= 0 && ctx.Position > start)
                    {
                        var alpha = (byte)ctx.GetScopeValue("alpha");
                        ctx.AddAttribute(new AlphaAttribute(start, ctx.Position, alpha));
                    }
                    ctx.CloseCurrentScope("alpha");
                }
            ));
        }
        
        private static void RegisterUnderlineTag(TagRegistry registry)
        {
            registry.Register(new TagDefinition(
                name: "u",
                onOpen: (ref TagContext ctx) =>
                {
                    uint? color = ctx.Value.IsEmpty ? null : ParseColor(ctx.Value);
                    ctx.OpenScope("u", color);
                    return true;
                },
                onClose: (ref TagContext ctx) =>
                {
                    int start = ctx.GetScopeStart("u");
                    if (start >= 0 && ctx.Position > start)
                    {
                        var colorValue = ctx.GetScopeValue("u");
                        UnityEngine.Color32? color = colorValue != null 
                            ? UintToColor32((uint)colorValue) 
                            : null;
                        ctx.AddAttribute(new UnderlineAttribute(start, ctx.Position, color));
                    }
                    ctx.CloseCurrentScope("u");
                }
            ));
        }
        
        private static void RegisterStrikethroughTag(TagRegistry registry)
        {
            var handler = new TagDefinition(
                name: "s",
                onOpen: (ref TagContext ctx) =>
                {
                    uint? color = ctx.Value.IsEmpty ? null : ParseColor(ctx.Value);
                    ctx.OpenScope("s", color);
                    return true;
                },
                onClose: (ref TagContext ctx) =>
                {
                    int start = ctx.GetScopeStart("s");
                    if (start >= 0 && ctx.Position > start)
                    {
                        var colorValue = ctx.GetScopeValue("s");
                        UnityEngine.Color32? color = colorValue != null 
                            ? UintToColor32((uint)colorValue) 
                            : null;
                        ctx.AddAttribute(new StrikethroughAttribute(start, ctx.Position, color));
                    }
                    ctx.CloseCurrentScope("s");
                }
            );
            
            registry.Register(handler);
            registry.Register(handler, "strikethrough");
        }
        
        private static void RegisterMarkTag(TagRegistry registry)
        {
            registry.Register(new TagDefinition(
                name: "mark",
                onOpen: (ref TagContext ctx) =>
                {
                    uint color = ctx.Value.IsEmpty ? 0xFFFF0080 : ParseColor(ctx.Value); // Default yellow
                    ctx.OpenScope("mark", color);
                    return true;
                },
                onClose: (ref TagContext ctx) =>
                {
                    int start = ctx.GetScopeStart("mark");
                    if (start >= 0 && ctx.Position > start)
                    {
                        var color = UintToColor32((uint)ctx.GetScopeValue("mark"));
                        ctx.AddAttribute(new MarkAttribute(start, ctx.Position, color));
                    }
                    ctx.CloseCurrentScope("mark");
                }
            ));
        }
        
        private static void RegisterLinkTag(TagRegistry registry)
        {
            registry.Register(new TagDefinition(
                name: "link",
                onOpen: (ref TagContext ctx) =>
                {
                    string linkId = ctx.Value.IsEmpty ? "" : ctx.Value.ToString();
                    ctx.OpenScope("link", linkId);
                    return true;
                },
                onClose: (ref TagContext ctx) =>
                {
                    int start = ctx.GetScopeStart("link");
                    if (start >= 0 && ctx.Position > start)
                    {
                        var linkId = (string)ctx.GetScopeValue("link");
                        ctx.AddAttribute(new LinkAttribute(start, ctx.Position, linkId));
                    }
                    ctx.CloseCurrentScope("link");
                }
            ));
        }
        
        // ==========================================
        // Shaping Attributes
        // ==========================================
        
        private static void RegisterBoldTag(TagRegistry registry)
        {
            registry.Register(new TagDefinition(
                name: "b",
                onOpen: (ref TagContext ctx) =>
                {
                    ctx.OpenScope("b", FontStyle.Bold);
                    return true;
                },
                onClose: (ref TagContext ctx) =>
                {
                    int start = ctx.GetScopeStart("b");
                    if (start >= 0 && ctx.Position > start)
                    {
                        ctx.AddAttribute(new FontStyleAttribute(start, ctx.Position, FontStyle.Bold));
                    }
                    ctx.CloseCurrentScope("b");
                }
            ));
        }
        
        private static void RegisterItalicTag(TagRegistry registry)
        {
            registry.Register(new TagDefinition(
                name: "i",
                onOpen: (ref TagContext ctx) =>
                {
                    ctx.OpenScope("i", FontStyle.Italic);
                    return true;
                },
                onClose: (ref TagContext ctx) =>
                {
                    int start = ctx.GetScopeStart("i");
                    if (start >= 0 && ctx.Position > start)
                    {
                        ctx.AddAttribute(new FontStyleAttribute(start, ctx.Position, FontStyle.Italic));
                    }
                    ctx.CloseCurrentScope("i");
                }
            ));
        }
        
        private static void RegisterSizeTag(TagRegistry registry)
        {
            registry.Register(new TagDefinition(
                name: "size",
                onOpen: (ref TagContext ctx) =>
                {
                    if (ctx.Value.IsEmpty) return false;
                    var (size, unit) = ParseSizeWithUnit(ctx.Value);
                    ctx.OpenScope("size", (size, unit));
                    return true;
                },
                onClose: (ref TagContext ctx) =>
                {
                    int start = ctx.GetScopeStart("size");
                    if (start >= 0 && ctx.Position > start)
                    {
                        var (size, unit) = ((float, SizeUnit))ctx.GetScopeValue("size");
                        ctx.AddAttribute(new SizeAttribute(start, ctx.Position, size, unit));
                    }
                    ctx.CloseCurrentScope("size");
                },
                requiresValue: true
            ));
        }
        
        private static void RegisterFontTag(TagRegistry registry)
        {
            registry.Register(new TagDefinition(
                name: "font",
                onOpen: (ref TagContext ctx) =>
                {
                    if (ctx.Value.IsEmpty) return false;
                    ctx.OpenScope("font", ctx.Value.ToString());
                    return true;
                },
                onClose: (ref TagContext ctx) =>
                {
                    int start = ctx.GetScopeStart("font");
                    if (start >= 0 && ctx.Position > start)
                    {
                        var fontName = (string)ctx.GetScopeValue("font");
                        ctx.AddAttribute(new FontAttribute(start, ctx.Position, fontName));
                    }
                    ctx.CloseCurrentScope("font");
                },
                requiresValue: true
            ));
        }
        
        private static void RegisterCharacterSpacingTag(TagRegistry registry)
        {
            registry.Register(new TagDefinition(
                name: "cspace",
                onOpen: (ref TagContext ctx) =>
                {
                    if (ctx.Value.IsEmpty) return false;
                    var spacing = ParseFloat(ctx.Value);
                    ctx.OpenScope("cspace", spacing);
                    return true;
                },
                onClose: (ref TagContext ctx) =>
                {
                    int start = ctx.GetScopeStart("cspace");
                    if (start >= 0 && ctx.Position > start)
                    {
                        var spacing = (float)ctx.GetScopeValue("cspace");
                        ctx.AddAttribute(new CharacterSpacingAttribute(start, ctx.Position, spacing));
                    }
                    ctx.CloseCurrentScope("cspace");
                },
                requiresValue: true
            ));
        }
        
        private static void RegisterSuperscriptTag(TagRegistry registry)
        {
            registry.Register(new TagDefinition(
                name: "sup",
                onOpen: (ref TagContext ctx) =>
                {
                    ctx.OpenScope("sup", null);
                    return true;
                },
                onClose: (ref TagContext ctx) =>
                {
                    int start = ctx.GetScopeStart("sup");
                    if (start >= 0 && ctx.Position > start)
                    {
                        ctx.AddAttribute(new SuperscriptAttribute(start, ctx.Position));
                    }
                    ctx.CloseCurrentScope("sup");
                }
            ));
        }
        
        private static void RegisterSubscriptTag(TagRegistry registry)
        {
            registry.Register(new TagDefinition(
                name: "sub",
                onOpen: (ref TagContext ctx) =>
                {
                    ctx.OpenScope("sub", null);
                    return true;
                },
                onClose: (ref TagContext ctx) =>
                {
                    int start = ctx.GetScopeStart("sub");
                    if (start >= 0 && ctx.Position > start)
                    {
                        ctx.AddAttribute(new SubscriptAttribute(start, ctx.Position));
                    }
                    ctx.CloseCurrentScope("sub");
                }
            ));
        }
        
        // ==========================================
        // Layout Attributes
        // ==========================================
        
        private static void RegisterAlignTag(TagRegistry registry)
        {
            registry.Register(new TagDefinition(
                name: "align",
                onOpen: (ref TagContext ctx) =>
                {
                    if (ctx.Value.IsEmpty) return false;
                    var align = ParseAlignment(ctx.Value);
                    ctx.OpenScope("align", align);
                    return true;
                },
                onClose: (ref TagContext ctx) =>
                {
                    int start = ctx.GetScopeStart("align");
                    if (start >= 0 && ctx.Position > start)
                    {
                        var align = (TextAlignment)ctx.GetScopeValue("align");
                        ctx.AddAttribute(new AlignmentAttribute(start, ctx.Position, align));
                    }
                    ctx.CloseCurrentScope("align");
                },
                requiresValue: true
            ));
        }
        
        private static void RegisterIndentTag(TagRegistry registry)
        {
            registry.Register(new TagDefinition(
                name: "indent",
                onOpen: (ref TagContext ctx) =>
                {
                    if (ctx.Value.IsEmpty) return false;
                    var indent = ParseFloat(ctx.Value);
                    ctx.OpenScope("indent", indent);
                    return true;
                },
                onClose: (ref TagContext ctx) =>
                {
                    int start = ctx.GetScopeStart("indent");
                    if (start >= 0 && ctx.Position > start)
                    {
                        var indent = (float)ctx.GetScopeValue("indent");
                        ctx.AddAttribute(new IndentAttribute(start, ctx.Position, indent));
                    }
                    ctx.CloseCurrentScope("indent");
                },
                requiresValue: true
            ));
        }
        
        private static void RegisterLineSpacingTag(TagRegistry registry)
        {
            registry.Register(new TagDefinition(
                name: "line-height",
                onOpen: (ref TagContext ctx) =>
                {
                    if (ctx.Value.IsEmpty) return false;
                    var spacing = ParseFloat(ctx.Value);
                    ctx.OpenScope("line-height", spacing);
                    return true;
                },
                onClose: (ref TagContext ctx) =>
                {
                    int start = ctx.GetScopeStart("line-height");
                    if (start >= 0 && ctx.Position > start)
                    {
                        var spacing = (float)ctx.GetScopeValue("line-height");
                        ctx.AddAttribute(new LineSpacingAttribute(start, ctx.Position, spacing));
                    }
                    ctx.CloseCurrentScope("line-height");
                },
                requiresValue: true
            ));
        }
        
        private static void RegisterNoBreakTag(TagRegistry registry)
        {
            registry.Register(new TagDefinition(
                name: "nobr",
                onOpen: (ref TagContext ctx) =>
                {
                    ctx.OpenScope("nobr", null);
                    return true;
                },
                onClose: (ref TagContext ctx) =>
                {
                    int start = ctx.GetScopeStart("nobr");
                    if (start >= 0 && ctx.Position > start)
                    {
                        ctx.AddAttribute(new NoBreakAttribute(start, ctx.Position));
                    }
                    ctx.CloseCurrentScope("nobr");
                }
            ));
        }
        
        // ==========================================
        // Self-closing Tags
        // ==========================================
        
        private static void RegisterBreakTags(TagRegistry registry)
        {
            registry.Register(new TagDefinition("br", (ref TagContext ctx) =>
            {
                ctx.AddCodepoint('\n');
                return true;
            }));
            
            registry.Register(new TagDefinition("nbsp", (ref TagContext ctx) =>
            {
                ctx.AddCodepoint(0x00A0); // Non-breaking space
                return true;
            }));
            
            registry.Register(new TagDefinition("zwsp", (ref TagContext ctx) =>
            {
                ctx.AddCodepoint(0x200B); // Zero-width space
                return true;
            }));
            
            registry.Register(new TagDefinition("shy", (ref TagContext ctx) =>
            {
                ctx.AddCodepoint(0x00AD); // Soft hyphen
                return true;
            }));
            
            registry.Register(new TagDefinition("page", (ref TagContext ctx) =>
            {
                ctx.AddCodepoint('\f'); // Form feed / page break
                return true;
            }));
        }
        
        // ==========================================
        // Value Parsers
        // ==========================================
        
        private static uint ParseColor(ReadOnlySpan<char> value)
        {
            // Named colors
            if (TryParseNamedColor(value, out uint named))
                return named;
            
            // Hex
            if (value.Length > 0 && value[0] == '#')
                value = value.Slice(1);
            
            if (value.Length == 6 && TryParseHex(value, out uint rgb))
                return (rgb << 8) | 0xFF; // RGB → RGBA
            
            if (value.Length == 8 && TryParseHex(value, out uint rgba))
                return rgba;
            
            return 0xFFFFFFFF; // white
        }
        
        private static bool TryParseNamedColor(ReadOnlySpan<char> name, out uint color)
        {
            color = 0xFFFFFFFF;
            
            // Simple case-insensitive comparison
            if (name.Equals("red".AsSpan(), StringComparison.OrdinalIgnoreCase))
                color = 0xFF0000FF;
            else if (name.Equals("green".AsSpan(), StringComparison.OrdinalIgnoreCase))
                color = 0x008000FF;
            else if (name.Equals("blue".AsSpan(), StringComparison.OrdinalIgnoreCase))
                color = 0x0000FFFF;
            else if (name.Equals("white".AsSpan(), StringComparison.OrdinalIgnoreCase))
                color = 0xFFFFFFFF;
            else if (name.Equals("black".AsSpan(), StringComparison.OrdinalIgnoreCase))
                color = 0x000000FF;
            else if (name.Equals("yellow".AsSpan(), StringComparison.OrdinalIgnoreCase))
                color = 0xFFFF00FF;
            else if (name.Equals("cyan".AsSpan(), StringComparison.OrdinalIgnoreCase))
                color = 0x00FFFFFF;
            else if (name.Equals("magenta".AsSpan(), StringComparison.OrdinalIgnoreCase))
                color = 0xFF00FFFF;
            else if (name.Equals("gray".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                     name.Equals("grey".AsSpan(), StringComparison.OrdinalIgnoreCase))
                color = 0x808080FF;
            else if (name.Equals("orange".AsSpan(), StringComparison.OrdinalIgnoreCase))
                color = 0xFFA500FF;
            else if (name.Equals("purple".AsSpan(), StringComparison.OrdinalIgnoreCase))
                color = 0x800080FF;
            else
                return false;
            
            return true;
        }
        
        private static byte ParseAlpha(ReadOnlySpan<char> value)
        {
            if (value.Length > 0 && value[0] == '#')
                value = value.Slice(1);
            
            if (value.Length <= 2 && TryParseHex(value, out uint alpha))
                return (byte)alpha;
            
            return 255;
        }
        
        private static (float size, SizeUnit unit) ParseSizeWithUnit(ReadOnlySpan<char> value)
        {
            SizeUnit unit = SizeUnit.Points;
            
            if (value.EndsWith("%".AsSpan(), StringComparison.Ordinal))
            {
                unit = SizeUnit.Percent;
                value = value.Slice(0, value.Length - 1);
            }
            else if (value.EndsWith("em".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                unit = SizeUnit.Em;
                value = value.Slice(0, value.Length - 2);
            }
            else if (value.EndsWith("px".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                unit = SizeUnit.Pixels;
                value = value.Slice(0, value.Length - 2);
            }
            
            float size = ParseFloat(value);
            return (size, unit);
        }
        
        private static float ParseFloat(ReadOnlySpan<char> value)
        {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            if (float.TryParse(value, System.Globalization.NumberStyles.Float, 
                System.Globalization.CultureInfo.InvariantCulture, out float result))
                return result;
#else
            if (float.TryParse(value.ToString(), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float result))
                return result;
#endif
            return 0;
        }
        
        private static TextAlignment ParseAlignment(ReadOnlySpan<char> value)
        {
            if (value.Equals("left".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return TextAlignment.Left;
            if (value.Equals("center".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return TextAlignment.Center;
            if (value.Equals("right".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return TextAlignment.Right;
            if (value.Equals("justified".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                value.Equals("justify".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return TextAlignment.Justified;
            
            return TextAlignment.Left;
        }
        
        private static bool TryParseHex(ReadOnlySpan<char> hex, out uint value)
        {
            value = 0;
            foreach (char c in hex)
            {
                value <<= 4;
                if (c >= '0' && c <= '9')
                    value |= (uint)(c - '0');
                else if (c >= 'a' && c <= 'f')
                    value |= (uint)(c - 'a' + 10);
                else if (c >= 'A' && c <= 'F')
                    value |= (uint)(c - 'A' + 10);
                else
                    return false;
            }
            return true;
        }
        
        private static UnityEngine.Color32 UintToColor32(uint rgba)
        {
            return new UnityEngine.Color32(
                (byte)((rgba >> 24) & 0xFF),
                (byte)((rgba >> 16) & 0xFF),
                (byte)((rgba >> 8) & 0xFF),
                (byte)(rgba & 0xFF));
        }
    }
}
