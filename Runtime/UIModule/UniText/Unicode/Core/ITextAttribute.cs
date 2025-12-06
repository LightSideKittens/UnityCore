using System;

namespace Tekst
{
    /// <summary>
    /// Базовый интерфейс текстового атрибута.
    /// Атрибут описывает форматирование участка текста.
    /// </summary>
    public interface ITextAttribute
    {
        /// <summary>
        /// Диапазон применения атрибута
        /// </summary>
        TextRange Range { get; }
        
        /// <summary>
        /// Приоритет атрибута (для разрешения конфликтов).
        /// Больший приоритет перекрывает меньший.
        /// </summary>
        int Priority { get; }
    }
    
    /// <summary>
    /// Атрибут, влияющий на shaping (требует пере-shaping при изменении)
    /// </summary>
    public interface IShapingAttribute : ITextAttribute
    {
        // Marker interface
        // Примеры: FontAttribute, SizeAttribute, FontFeatureAttribute
    }
    
    /// <summary>
    /// Атрибут, влияющий на layout (требует пере-layout при изменении)
    /// </summary>
    public interface ILayoutAttribute : ITextAttribute
    {
        // Marker interface
        // Примеры: LineSpacingAttribute, AlignmentAttribute
    }
    
    /// <summary>
    /// Атрибут, влияющий только на рендеринг (только перерисовка)
    /// </summary>
    public interface IRenderAttribute : ITextAttribute
    {
        // Marker interface
        // Примеры: ColorAttribute, UnderlineAttribute
    }
    
    /// <summary>
    /// Контекст для применения атрибутов при рендеринге
    /// </summary>
    public interface IAttributeContext
    {
        // Доступ к текущим значениям, модификация состояния рендеринга
    }
}
