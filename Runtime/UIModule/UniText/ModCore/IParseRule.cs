using System.Collections.Generic;

/// <summary>
/// Контракт для правила поиска диапазонов в тексте.
/// Правило хранит внутреннее состояние (Stack) для отслеживания вложенности.
/// </summary>
public interface IParseRule
{
    /// <summary>
    /// Проверяет позицию на открывающий или закрывающий признак.
    /// </summary>
    /// <param name="text">Исходный текст</param>
    /// <param name="index">Текущая позиция в тексте</param>
    /// <param name="results">Список для записи завершённых диапазонов</param>
    /// <returns>
    /// Новый индекс после обработанного признака, или тот же index если ничего не найдено
    /// </returns>
    int TryMatch(string text, int index, List<ParsedRange> results);

    /// <summary>
    /// Сброс внутреннего состояния перед новым парсингом
    /// </summary>
    void Reset();
}