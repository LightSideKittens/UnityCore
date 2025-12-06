using System;
using UnityEngine;

namespace Tekst
{
    /// <summary>
    /// Базовый класс атрибута с диапазоном
    /// </summary>
    public abstract class TextAttributeBase : ITextAttribute
    {
        public TextRange Range { get; }
        public virtual int Priority => 0;
        
        protected TextAttributeBase(int start, int end)
        {
            Range = new TextRange(start, end - start);
        }
        
        protected TextAttributeBase(TextRange range)
        {
            Range = range;
        }
    }
    
    // ==========================================
    // Render Attributes (влияют только на рендеринг)
    // ==========================================
    
    /// <summary>
    /// Цвет текста
    /// </summary>
    public sealed class ColorAttribute : TextAttributeBase, IRenderAttribute
    {
        public Color32 Color { get; }
        
        public ColorAttribute(int start, int end, Color32 color) : base(start, end)
        {
            Color = color;
        }
        
        public ColorAttribute(int start, int end, uint rgba) : base(start, end)
        {
            Color = new Color32(
                (byte)((rgba >> 24) & 0xFF),
                (byte)((rgba >> 16) & 0xFF),
                (byte)((rgba >> 8) & 0xFF),
                (byte)(rgba & 0xFF));
        }
    }
    
    /// <summary>
    /// Прозрачность
    /// </summary>
    public sealed class AlphaAttribute : TextAttributeBase, IRenderAttribute
    {
        public byte Alpha { get; }
        
        public AlphaAttribute(int start, int end, byte alpha) : base(start, end)
        {
            Alpha = alpha;
        }
    }
    
    /// <summary>
    /// Подчёркивание
    /// </summary>
    public sealed class UnderlineAttribute : TextAttributeBase, IRenderAttribute
    {
        public Color32? Color { get; }
        
        public UnderlineAttribute(int start, int end, Color32? color = null) : base(start, end)
        {
            Color = color;
        }
    }
    
    /// <summary>
    /// Зачёркивание
    /// </summary>
    public sealed class StrikethroughAttribute : TextAttributeBase, IRenderAttribute
    {
        public Color32? Color { get; }
        
        public StrikethroughAttribute(int start, int end, Color32? color = null) : base(start, end)
        {
            Color = color;
        }
    }
    
    /// <summary>
    /// Выделение (маркер)
    /// </summary>
    public sealed class MarkAttribute : TextAttributeBase, IRenderAttribute
    {
        public Color32 Color { get; }
        
        public MarkAttribute(int start, int end, Color32 color) : base(start, end)
        {
            Color = color;
        }
    }
    
    // ==========================================
    // Shaping Attributes (требуют пере-shaping)
    // ==========================================
    
    /// <summary>
    /// Размер шрифта
    /// </summary>
    public sealed class SizeAttribute : TextAttributeBase, IShapingAttribute
    {
        public float Size { get; }
        public SizeUnit Unit { get; }
        
        public SizeAttribute(int start, int end, float size, SizeUnit unit = SizeUnit.Points) : base(start, end)
        {
            Size = size;
            Unit = unit;
        }
    }
    
    /// <summary>
    /// Единицы измерения размера
    /// </summary>
    public enum SizeUnit : byte
    {
        Points,     // Абсолютные точки
        Pixels,     // Пиксели
        Em,         // Относительно текущего размера
        Percent     // Процент от базового
    }
    
    /// <summary>
    /// Шрифт
    /// </summary>
    public sealed class FontAttribute : TextAttributeBase, IShapingAttribute
    {
        public string FontName { get; }
        public int FontId { get; }
        
        public FontAttribute(int start, int end, string fontName) : base(start, end)
        {
            FontName = fontName;
            FontId = -1; // Resolve later
        }
        
        public FontAttribute(int start, int end, int fontId) : base(start, end)
        {
            FontName = null;
            FontId = fontId;
        }
    }
    
    /// <summary>
    /// Стиль шрифта (bold, italic)
    /// </summary>
    [Flags]
    public enum FontStyle : byte
    {
        Normal = 0,
        Bold = 1,
        Italic = 2,
        BoldItalic = Bold | Italic
    }
    
    /// <summary>
    /// Атрибут стиля шрифта
    /// </summary>
    public sealed class FontStyleAttribute : TextAttributeBase, IShapingAttribute
    {
        public FontStyle Style { get; }
        
        public FontStyleAttribute(int start, int end, FontStyle style) : base(start, end)
        {
            Style = style;
        }
    }
    
    /// <summary>
    /// Межсимвольный интервал
    /// </summary>
    public sealed class CharacterSpacingAttribute : TextAttributeBase, IShapingAttribute
    {
        public float Spacing { get; }
        
        public CharacterSpacingAttribute(int start, int end, float spacing) : base(start, end)
        {
            Spacing = spacing;
        }
    }
    
    // ==========================================
    // Layout Attributes (требуют пере-layout)
    // ==========================================
    
    /// <summary>
    /// Выравнивание
    /// </summary>
    public enum TextAlignment : byte
    {
        Left,
        Center,
        Right,
        Justified
    }
    
    /// <summary>
    /// Атрибут выравнивания
    /// </summary>
    public sealed class AlignmentAttribute : TextAttributeBase, ILayoutAttribute
    {
        public TextAlignment Alignment { get; }
        
        public AlignmentAttribute(int start, int end, TextAlignment alignment) : base(start, end)
        {
            Alignment = alignment;
        }
    }
    
    /// <summary>
    /// Межстрочный интервал
    /// </summary>
    public sealed class LineSpacingAttribute : TextAttributeBase, ILayoutAttribute
    {
        public float Spacing { get; }
        
        public LineSpacingAttribute(int start, int end, float spacing) : base(start, end)
        {
            Spacing = spacing;
        }
    }
    
    /// <summary>
    /// Отступ
    /// </summary>
    public sealed class IndentAttribute : TextAttributeBase, ILayoutAttribute
    {
        public float Indent { get; }
        
        public IndentAttribute(int start, int end, float indent) : base(start, end)
        {
            Indent = indent;
        }
    }
    
    /// <summary>
    /// Запрет переноса
    /// </summary>
    public sealed class NoBreakAttribute : TextAttributeBase, ILayoutAttribute
    {
        public NoBreakAttribute(int start, int end) : base(start, end)
        {
        }
    }
    
    // ==========================================
    // Special Attributes
    // ==========================================
    
    /// <summary>
    /// Ссылка
    /// </summary>
    public sealed class LinkAttribute : TextAttributeBase, IRenderAttribute
    {
        public string LinkId { get; }
        
        public LinkAttribute(int start, int end, string linkId) : base(start, end)
        {
            LinkId = linkId;
        }
    }
    
    /// <summary>
    /// Верхний индекс
    /// </summary>
    public sealed class SuperscriptAttribute : TextAttributeBase, IShapingAttribute, ILayoutAttribute
    {
        public SuperscriptAttribute(int start, int end) : base(start, end)
        {
        }
    }
    
    /// <summary>
    /// Нижний индекс
    /// </summary>
    public sealed class SubscriptAttribute : TextAttributeBase, IShapingAttribute, ILayoutAttribute
    {
        public SubscriptAttribute(int start, int end) : base(start, end)
        {
        }
    }
}
