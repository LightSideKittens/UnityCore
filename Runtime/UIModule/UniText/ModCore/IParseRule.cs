using System.Collections.Generic;


public interface IParseRule
{
    /// <param name="text">Исходный текст</param>
    /// <param name="index">Текущая позиция в тексте</param>
    /// <param name="results">Список для записи завершённых диапазонов</param>
    /// <returns>
    /// Новый индекс после обработанного признака, или тот же index если ничего не найдено
    /// </returns>
    int TryMatch(string text, int index, IList<ParsedRange> results);


    /// <param name="textLength">Длина исходного текста</param>
    /// <param name="results">Список для записи диапазонов</param>
    void Finalize(int textLength, IList<ParsedRange> results);


    void Reset();
}