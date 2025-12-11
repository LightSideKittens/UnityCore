using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Информация об элементе списка.
/// </summary>
public struct ListItemInfo
{
    public int start;
    public int end;
    public int nestingLevel;
    public int displayNumber;
}

/// <summary>
/// Стиль нумерации для ordered lists.
/// </summary>
public enum OrderedMarkerStyle
{
    Decimal,
    LowerAlpha,
    UpperAlpha,
    LowerRoman,
    UpperRoman
}

/// <summary>
/// Модификатор для списков (bullet и ordered).
/// Применяет hanging indent и рендерит маркеры.
/// </summary>
[Serializable]
public class ListModifier : IModifier
{
    // Instance буферы
    private List<ListItemInfo> instanceItems;
    private bool instanceMarkersDrawnThisFrame;
    private UniTextFontProvider instanceFontProvider;

    // Статические указатели на текущие буферы
    private static List<ListItemInfo> items;
    private static bool markersDrawnThisFrame;
    private static UniTextFontProvider fontProviderRef;

    public static bool DebugLogging = false;

    public float indentPerLevel = 20f;
    public float markerToTextGap = 8f;
    public float bulletMarkerWidth = 24f;

    public string[] bulletMarkers = { "•", "-", "·" };

    public OrderedMarkerStyle[] orderedStyles =
    {
        OrderedMarkerStyle.Decimal,
        OrderedMarkerStyle.LowerAlpha,
        OrderedMarkerStyle.LowerRoman
    };

    void IModifier.Apply(int start, int end, string parameter)
    {
        var item = ParseParameter(start, end, parameter);
        items.Add(item);

        if (DebugLogging)
            UnityEngine.Debug.Log($"[ListModifier.Apply] start={start}, end={end}, param={parameter}, level={item.nestingLevel}, displayNum={item.displayNumber}");

        ApplyMargins(item);
    }

    void IModifier.Initialize(UniText uniText)
    {
        instanceItems = new List<ListItemInfo>(32);
        instanceFontProvider = uniText.FontProvider;

        uniText.Rebuilding += OnRebuilding;
        var gen = uniText.MeshGenerator;
        gen.OnRebuildStart += OnRebuildStart;
        gen.OnAfterGlyphs += OnAfterGlyphs;
    }

    void IModifier.Deinitialize(UniText uniText)
    {
        uniText.Rebuilding -= OnRebuilding;
        var gen = uniText.MeshGenerator;
        if (gen != null)
        {
            gen.OnRebuildStart -= OnRebuildStart;
            gen.OnAfterGlyphs -= OnAfterGlyphs;
        }
        instanceItems = null;
        instanceFontProvider = null;
    }

    private void OnRebuilding()
    {
        items = instanceItems;
        fontProviderRef = instanceFontProvider;
        markersDrawnThisFrame = instanceMarkersDrawnThisFrame;
    }

    private void OnRebuildStart()
    {
        markersDrawnThisFrame = false;
        instanceMarkersDrawnThisFrame = false;
    }

    void IModifier.Reset()
    {
        items.Clear();
    }

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
            if (int.TryParse(parameter, out int level))
                item.nestingLevel = level;
        }
        else
        {
            if (int.TryParse(parameter.AsSpan(0, colonIndex), out int level))
                item.nestingLevel = level;
            if (int.TryParse(parameter.AsSpan(colonIndex + 1), out int number))
                item.displayNumber = number;
        }

        return item;
    }

    private float MeasureMarkerWidthForLayout(ListItemInfo item)
    {
        if (item.displayNumber < 0)
            return bulletMarkerWidth + markerToTextGap;

        var fontAsset = fontProviderRef?.GetFontAsset(0);
        if (fontAsset == null)
            return bulletMarkerWidth;

        float fontSize = fontProviderRef.FontSize;
        float scale = fontSize / fontAsset.FaceInfo.pointSize;

        string markerText = GetMarkerTextForMeasure(item);
        return MeasureStringWithScale(markerText, fontAsset, scale) + markerToTextGap;
    }

    private string GetMarkerTextForMeasure(ListItemInfo item)
    {
        int level = Math.Min(item.nestingLevel, orderedStyles.Length - 1);
        if (level < 0) level = 0;
        string number = FormatOrderedNumber(item.displayNumber, orderedStyles[level]);
        return $"{number}.";
    }

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

        var buf = SharedTextBuffers.Current;
        int cpCount = buf.codepointCount;
        var margins = buf.startMargins;

        if (item.end > margins.Length)
            buf.EnsureCodepointCapacity(item.end);

        margins = buf.startMargins;

        int safeEnd = Math.Min(item.end, cpCount);

        for (int i = item.start; i < safeEnd; i++)
        {
            if (contentIndent > margins[i])
                margins[i] = contentIndent;
        }
    }

    private void OnAfterGlyphs()
    {
        if (markersDrawnThisFrame)
            return;
        markersDrawnThisFrame = true;
        instanceMarkersDrawnThisFrame = true;

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
        bool isRtl = IsItemRtl(item.start);

        float baselineY = GetItemBaselineY(item.start, out float firstGlyphX);
        if (float.IsNaN(baselineY))
        {
            if (DebugLogging)
                UnityEngine.Debug.LogWarning($"[ListModifier.RenderMarker] baselineY is NaN for item.start={item.start}, level={item.nestingLevel}");
            return;
        }

        string markerText = GetMarkerText(item, isRtl);

        float markerX;
        if (isRtl)
        {
            float lineWidth = GetLineWidth(item.start);
            float textRightEdge = firstGlyphX + lineWidth;
            markerX = textRightEdge + markerToTextGap;
        }
        else
        {
            float measuredMarkerWidth = MeasureMarkerWidth(markerText);
            markerX = firstGlyphX - measuredMarkerWidth - markerToTextGap;
        }

        if (DebugLogging)
            UnityEngine.Debug.Log($"[ListModifier.RenderMarker] level={item.nestingLevel}, marker='{markerText}', x={markerX:F1}, y={baselineY:F1}, isRtl={isRtl}");

        RenderMarkerGlyphs(markerText, markerX, baselineY);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsItemRtl(int startCluster)
    {
        var bidiLevels = SharedTextBuffers.Current.bidiLevels;
        if ((uint)startCluster >= (uint)bidiLevels.Length)
            return false;

        return (bidiLevels[startCluster] & 1) == 1;
    }

    private static float GetItemBaselineY(int startCluster, out float firstGlyphX)
    {
        var buf = SharedTextBuffers.Current;
        var glyphs = buf.positionedGlyphs;
        int count = buf.positionedGlyphCount;
        float offsetX = UniTextMeshGenerator.offsetX;
        float offsetY = UniTextMeshGenerator.offsetY;

        for (int i = 0; i < count; i++)
        {
            if (glyphs[i].cluster >= startCluster)
            {
                firstGlyphX = offsetX + glyphs[i].x;
                return offsetY - glyphs[i].y;
            }
        }

        firstGlyphX = 0;
        return float.NaN;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float GetLineWidth(int cluster)
    {
        var buf = SharedTextBuffers.Current;
        var lines = buf.lines;
        int lineCount = buf.lineCount;

        for (int i = 0; i < lineCount; i++)
        {
            ref readonly var line = ref lines[i];
            if (cluster >= line.range.start && cluster < line.range.start + line.range.length)
                return line.width;
        }

        return 0f;
    }

    private string GetMarkerText(ListItemInfo item, bool isRtl)
    {
        if (item.displayNumber < 0)
        {
            int level = Math.Min(item.nestingLevel, bulletMarkers.Length - 1);
            if (level < 0) level = 0;
            return bulletMarkers[level];
        }
        else
        {
            int level = Math.Min(item.nestingLevel, orderedStyles.Length - 1);
            if (level < 0) level = 0;
            string number = FormatOrderedNumber(item.displayNumber, orderedStyles[level]);
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

    private static float MeasureMarkerWidth(string markerText)
    {
        return GlyphRenderHelper.MeasureString(markerText);
    }

    private static void RenderMarkerGlyphs(string markerText, float x, float baselineY)
    {
        Color32 color = UniTextMeshGenerator.currentDefaultColor;
        GlyphRenderHelper.DrawString(markerText, x, baselineY, color);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        items = null;
        markersDrawnThisFrame = false;
        fontProviderRef = null;
    }
}
