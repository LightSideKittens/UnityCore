/// <summary>
/// Базовый интерфейс для модификаторов текста.
/// Модификатор обращается к SharedTextBuffers для чтения/модификации данных.
/// </summary>
public interface IModifier
{
    /// <summary>
    /// Применить модификатор к диапазону
    /// </summary>
    /// <param name="start">Начало диапазона в clean text</param>
    /// <param name="end">Конец диапазона (не включительно)</param>
    /// <param name="parameter">Параметр из тега (например, "#FF0000")</param>
    void Apply(int start, int end, string parameter);

    /// <summary>
    /// Инициализация модификатора. Вызывается при регистрации в UniText.
    /// Модификатор может подписаться на события MeshGenerator.
    /// Default implementation: do nothing.
    /// </summary>
    void Initialize(UniText uniText) { }

    /// <summary>
    /// Деинициализация модификатора. Вызывается при удалении из списка или в OnValidate.
    /// Модификатор должен отписаться от событий MeshGenerator.
    /// Default implementation: do nothing.
    /// </summary>
    void Deinitialize(UniText uniText) { }

    /// <summary>
    /// Сброс буферов модификатора перед обработкой нового текста.
    /// Default implementation: do nothing.
    /// </summary>
    void Reset() { }
}

/// <summary>
/// Модификатор уровня Itemization — влияет на разбиение на runs (font, bold, italic, size).
/// Вызывается перед Itemize().
/// </summary>
public interface IItemizationModifier : IModifier
{
}

/// <summary>
/// Модификатор уровня Layout — влияет на позиции глифов (superscript, subscript, baseline).
/// Вызывается после BreakLines(), перед Layout().
/// </summary>
public interface ILayoutModifier : IModifier
{
}

/// <summary>
/// Модификатор уровня Render — визуальные эффекты (color, underline, shadow).
/// Вызывается после Layout(), перед MeshGenerator.
/// </summary>
public interface IRenderModifier : IModifier
{
}