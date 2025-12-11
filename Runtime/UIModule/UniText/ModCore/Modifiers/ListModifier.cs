using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Информация об элементе списка.
/// </summary>
public struct ListItemInfo
{
    public int start;           // Начало контента в clean text
    public int end;             // Конец контента (не включительно)
    public int nestingLevel;    // Уровень вложенности (0, 1, 2...)
    public int displayNumber;   // Для ordered: отображаемый номер (1, 2, 3...), для bullet: -1
}

/// <summary>
/// Стиль нумерации для ordered lists.
/// </summary>
public enum OrderedMarkerStyle
{
    Decimal,      // 1. 2. 3.
    LowerAlpha,   // a. b. c.
    UpperAlpha,   // A. B. C.
    LowerRoman,   // i. ii. iii.
    UpperRoman    // I. II. III.
}

/// <summary>
/// Модификатор для списков (bullet и ordered).
/// Применяет hanging indent и рендерит маркеры.
/// </summary>
[Serializable]
public class ListModifier : IModifier
{
    // ═══════════════════════════════════════════════════════════════
    // Статические буферы
    // ═══════════════════════════════════════════════════════════════

    private static readonly List<ListItemInfo> items = new(32);
    private static bool markersDrawnThisFrame;
    private static UniTextFontProvider fontProviderRef;

    // DEBUG
    public static bool DebugLogging = false;

    // ═══════════════════════════════════════════════════════════════
    // Настройки (настраиваются в Inspector)
    // ═══════════════════════════════════════════════════════════════

    public float indentPerLevel = 20f;       // Отступ на уровень вложенности
    public float markerToTextGap = 8f;       // Расстояние от маркера до текста
    public float bulletMarkerWidth = 24f;    // Ширина зоны для bullet маркера

    // Маркеры bullet list по уровням
    // Используем символы которые есть в большинстве шрифтов
    public string[] bulletMarkers = { "•", "-", "·" };

    // Стили ordered list по уровням
    public OrderedMarkerStyle[] orderedStyles =
    {
        OrderedMarkerStyle.Decimal,
        OrderedMarkerStyle.LowerAlpha,
        OrderedMarkerStyle.LowerRoman
    };

    // ═══════════════════════════════════════════════════════════════
    // IModifier
    // ═══════════════════════════════════════════════════════════════

    void IModifier.Apply(int start, int end, string parameter)
    {
        var item = ParseParameter(start, end, parameter);
        items.Add(item);

        if (DebugLogging)
            UnityEngine.Debug.Log($"[ListModifier.Apply] start={start}, end={end}, param={parameter}, level={item.nestingLevel}, displayNum={item.displayNumber}");

        // Установить margins в SharedTextBuffers для hanging indent
        ApplyMargins(item);
    }

    void IModifier.Initialize(UniText uniText)
    {
        fontProviderRef = uniText.FontProvider;
        var gen = uniText.MeshGenerator;
        gen.OnRebuildStart += OnRebuildStart;
        gen.OnAfterGlyphs += OnAfterGlyphs;
    }

    void IModifier.Deinitialize(UniText uniText)
    {
        var gen = uniText.MeshGenerator;
        if (gen == null) return;
        gen.OnRebuildStart -= OnRebuildStart;
        gen.OnAfterGlyphs -= OnAfterGlyphs;
    }

    private static void OnRebuildStart()
    {
        markersDrawnThisFrame = false;
    }

    void IModifier.Reset()
    {
        items.Clear();
    }

    public static void ResetStatic()
    {
        items.Clear();
    }

    // ═══════════════════════════════════════════════════════════════
    // Парсинг параметра
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Парсит parameter строку.
    /// Bullet: "0", "1", "2" (только level)
    /// Ordered: "0:1", "1:5" (level:number)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ListItemInfo ParseParameter(int start, int end, string parameter)
    {
        var item = new ListItemInfo
        {
            start = start,
            end = end,
            displayNumber = -1
        };

        if (string.IsNullOrEmpty(parameter))
            return item;

        int colonIndex = parameter.IndexOf(':');
        if (colonIndex < 0)
        {
            // Bullet: "0", "1", "2"
            if (int.TryParse(parameter, out int level))
                item.nestingLevel = level;
        }
        else
        {
            // Ordered: "0:1", "1:5"
            if (int.TryParse(parameter.AsSpan(0, colonIndex), out int level))
                item.nestingLevel = level;
            if (int.TryParse(parameter.AsSpan(colonIndex + 1), out int number))
                item.displayNumber = number;
        }

        return item;
    }

    // ═══════════════════════════════════════════════════════════════
    // Margins для hanging indent
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Измеряет ширину маркера через fontAsset.
    /// Для bullets возвращает фиксированную ширину.
    /// Для ordered измеряет реальную ширину через font metrics.
    /// </summary>
    private float MeasureMarkerWidthForLayout(ListItemInfo item)
    {
        if (item.displayNumber < 0)
            return bulletMarkerWidth;

        // Ordered: измерить реальную ширину через font
        var fontAsset = fontProviderRef?.GetFontAsset(0);
        if (fontAsset == null)
            return bulletMarkerWidth; // fallback

        float fontSize = fontProviderRef.FontSize;
        float scale = fontSize / fontAsset.FaceInfo.pointSize;

        string markerText = GetMarkerTextForMeasure(item);
        return MeasureStringWithScale(markerText, fontAsset, scale) + markerToTextGap;
    }

    /// <summary>
    /// Получить текст маркера для измерения (без RTL, т.к. ширина одинаковая).
    /// </summary>
    private string GetMarkerTextForMeasure(ListItemInfo item)
    {
        int level = Math.Min(item.nestingLevel, orderedStyles.Length - 1);
        if (level < 0) level = 0;
        string number = FormatOrderedNumber(item.displayNumber, orderedStyles[level]);
        return $"{number}.";
    }

    /// <summary>
    /// Измеряет ширину строки с заданным fontAsset и scale.
    /// </summary>
    private static float MeasureStringWithScale(string text, UniTextFontAsset fontAsset, float scale)
    {
        if (string.IsNullOrEmpty(text))
            return 0f;

        float totalWidth = 0f;
        var charTable = fontAsset.CharacterLookupTable;
        if (charTable == null)
            return 0f;

        for (int i = 0; i < text.Length; i++)
        {
            uint codepoint = text[i];
            if (charTable.TryGetValue(codepoint, out var character) && character?.glyph != null)
            {
                totalWidth += character.glyph.metrics.horizontalAdvance * scale;
            }
        }
        return totalWidth;
    }

    private void ApplyMargins(ListItemInfo item)
    {
        float baseIndent = item.nestingLevel * indentPerLevel;
        float markerWidth = MeasureMarkerWidthForLayout(item);
        float contentIndent = baseIndent + markerWidth;

        int cpCount = SharedTextBuffers.codepointCount;
        var margins = SharedTextBuffers.startMargins;

        // Убедимся что буфер достаточно большой
        if (item.end > margins.Length)
            SharedTextBuffers.EnsureCodepointCapacity(item.end);

        margins = SharedTextBuffers.startMargins; // Перезагрузить после возможного resize

        int safeEnd = Math.Min(item.end, cpCount);

        // Для list item: весь диапазон получает contentIndent (hanging indent)
        // Первая строка будет иметь margin = contentIndent, но маркер рисуется отдельно
        // в зоне [baseIndent, baseIndent + fixedMarkerWidth]
        // ВАЖНО: не перезаписываем если уже есть больший margin (вложенные items)
        for (int i = item.start; i < safeEnd; i++)
        {
            if (contentIndent > margins[i])
                margins[i] = contentIndent;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Рендеринг маркеров
    // ═══════════════════════════════════════════════════════════════

    private void OnAfterGlyphs()
    {
        // Рисуем маркеры только один раз за rebuild (OnAfterGlyphs вызывается для каждого шрифта)
        if (markersDrawnThisFrame)
            return;
        markersDrawnThisFrame = true;

        if (DebugLogging)
            UnityEngine.Debug.Log($"[ListModifier.OnAfterGlyphs] items.Count={items.Count}");

        if (items.Count == 0)
            return;

        foreach (var item in items)
        {
            RenderMarker(item);
        }
    }

    private void RenderMarker(ListItemInfo item)
    {
        // Получить baseline Y позицию первого символа item
        float baselineY = GetItemBaselineY(item.start);
        if (float.IsNaN(baselineY))
        {
            if (DebugLogging)
                UnityEngine.Debug.LogWarning($"[ListModifier.RenderMarker] baselineY is NaN for item.start={item.start}, level={item.nestingLevel}");
            return;
        }

        // Направление каждого item определяется по его контенту (как в Google Docs)
        bool isRtl = IsItemRtl(item.start);

        // Определить X позицию маркера
        float baseIndent = item.nestingLevel * indentPerLevel;
        float offsetX = UniTextMeshGenerator.offsetX;

        string markerText = GetMarkerText(item, isRtl);
        float measuredMarkerWidth = MeasureMarkerWidth(markerText);
        float markerZoneWidth = MeasureMarkerWidthForLayout(item); // Та же ширина что использовалась для margins

        float markerX;
        if (isRtl)
        {
            // RTL: маркер справа от текста
            float availableWidth = GetAvailableWidth();
            markerX = offsetX + availableWidth - baseIndent - markerZoneWidth + markerToTextGap;
        }
        else
        {
            // LTR: маркер слева, выравнен по правому краю зоны маркера
            markerX = offsetX + baseIndent + markerZoneWidth - measuredMarkerWidth - markerToTextGap;
        }

        if (DebugLogging)
            UnityEngine.Debug.Log($"[ListModifier.RenderMarker] level={item.nestingLevel}, marker='{markerText}', x={markerX:F1}, y={baselineY:F1}, zoneWidth={markerZoneWidth:F1}");

        // Рендерить маркер как глифы
        RenderMarkerGlyphs(markerText, markerX, baselineY);
    }

    /// <summary>
    /// Определить RTL по bidiLevel первого символа item.
    /// RTL если (bidiLevel & 1) == 1
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsItemRtl(int startCluster)
    {
        var bidiLevels = SharedTextBuffers.bidiLevels;
        if ((uint)startCluster >= (uint)bidiLevels.Length)
            return false;

        return (bidiLevels[startCluster] & 1) == 1;
    }

    /// <summary>
    /// Получить доступную ширину для layout.
    /// Используется для RTL позиционирования.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float GetAvailableWidth()
    {
        return UniTextMeshGenerator.rectWidth;
    }

    private float GetItemBaselineY(int startCluster)
    {
        // Найти baseline Y позицию первого глифа с cluster >= startCluster
        var glyphs = SharedTextBuffers.positionedGlyphs;
        int count = SharedTextBuffers.positionedGlyphCount;
        float offsetY = UniTextMeshGenerator.offsetY;

        for (int i = 0; i < count; i++)
        {
            if (glyphs[i].cluster >= startCluster)
            {
                // baselineY = offsetY - glyph.y (как в UniTextMeshGenerator)
                return offsetY - glyphs[i].y;
            }
        }

        return float.NaN;
    }

    private string GetMarkerText(ListItemInfo item, bool isRtl)
    {
        if (item.displayNumber < 0)
        {
            // Bullet
            int level = Math.Min(item.nestingLevel, bulletMarkers.Length - 1);
            if (level < 0) level = 0;
            return bulletMarkers[level];
        }
        else
        {
            // Ordered
            int level = Math.Min(item.nestingLevel, orderedStyles.Length - 1);
            if (level < 0) level = 0;
            string number = FormatOrderedNumber(item.displayNumber, orderedStyles[level]);

            // В RTL точка идёт перед числом
            return isRtl ? $".{number}" : $"{number}.";
        }
    }

    private static string FormatOrderedNumber(int number, OrderedMarkerStyle style)
    {
        return style switch
        {
            OrderedMarkerStyle.Decimal => number.ToString(),
            OrderedMarkerStyle.LowerAlpha => ToLowerAlpha(number),
            OrderedMarkerStyle.UpperAlpha => ToUpperAlpha(number),
            OrderedMarkerStyle.LowerRoman => ToRoman(number).ToLowerInvariant(),
            OrderedMarkerStyle.UpperRoman => ToRoman(number),
            _ => number.ToString()
        };
    }

    private static string ToLowerAlpha(int n)
    {
        if (n <= 0) return "?";
        return ((char)('a' + (n - 1) % 26)).ToString();
    }

    private static string ToUpperAlpha(int n)
    {
        if (n <= 0) return "?";
        return ((char)('A' + (n - 1) % 26)).ToString();
    }

    private static string ToRoman(int number)
    {
        if (number <= 0 || number > 3999)
            return number.ToString();

        // Стандартная реализация римских цифр
        ReadOnlySpan<int> values = stackalloc int[] { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        ReadOnlySpan<string> numerals = new[] { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

        var result = new System.Text.StringBuilder(15);
        for (int i = 0; i < values.Length; i++)
        {
            while (number >= values[i])
            {
                number -= values[i];
                result.Append(numerals[i]);
            }
        }
        return result.ToString();
    }

    private float MeasureMarkerWidth(string markerText)
    {
        return GlyphRenderHelper.MeasureString(markerText);
    }

    private void RenderMarkerGlyphs(string markerText, float x, float baselineY)
    {
        Color32 color = UniTextMeshGenerator.currentDefaultColor;
        GlyphRenderHelper.DrawString(markerText, x, baselineY, color);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        items.Clear();
        markersDrawnThisFrame = false;
        fontProviderRef = null;
    }
}