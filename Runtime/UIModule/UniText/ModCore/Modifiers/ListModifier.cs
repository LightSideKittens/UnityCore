using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public struct ListItemInfo
{
    public int start;
    public int end;
    public int nestingLevel;
    public int displayNumber;
}

public enum OrderedMarkerStyle
{
    Decimal,
    LowerAlpha,
    UpperAlpha,
    LowerRoman,
    UpperRoman
}

[Serializable]
public class ListModifier : BaseModifier
{
    private List<ListItemInfo> instanceItems;
    private bool instanceMarkersDrawnThisFrame;
    private UniTextFontProvider instanceFontProvider;

    private static List<ListItemInfo> items;
    private static bool markersDrawnThisFrame;
    private static UniTextFontProvider fontProviderRef;

    public static bool DebugLogging = false;

    public float indentPerLevel = 20f;
    public float markerToTextGap = 8f;
    public float bulletMarkerWidth = 24f;
    public string[] bulletMarkers = { "•", "-", "·" };
    public OrderedMarkerStyle[] orderedStyles = { OrderedMarkerStyle.Decimal, OrderedMarkerStyle.LowerAlpha, OrderedMarkerStyle.LowerRoman };

    protected override void CreateBuffers()
    {
        instanceItems = new List<ListItemInfo>(32);
        instanceFontProvider = cachedUniText.FontProvider;
        items = instanceItems;
        fontProviderRef = instanceFontProvider;
    }

    protected override void Subscribe()
    {
        cachedUniText.Rebuilding += OnRebuilding;
        cachedUniText.MeshGenerator.OnRebuildStart += OnRebuildStart;
        cachedUniText.MeshGenerator.OnAfterGlyphs += OnAfterGlyphs;
    }

    protected override void Unsubscribe()
    {
        cachedUniText.Rebuilding -= OnRebuilding;
        cachedUniText.MeshGenerator.OnRebuildStart -= OnRebuildStart;
        cachedUniText.MeshGenerator.OnAfterGlyphs -= OnAfterGlyphs;
    }

    protected override void ReleaseBuffers()
    {
        instanceItems = null;
        instanceFontProvider = null;
    }

    protected override void ClearBuffers() => instanceItems.Clear();

    protected override void ApplyModifier(int start, int end, string parameter)
    {
        var item = ParseParameter(start, end, parameter);
        items.Add(item);

        if (DebugLogging)
            UnityEngine.Debug.Log($"[ListModifier.Apply] start={start}, end={end}, param={parameter}, level={item.nestingLevel}, displayNum={item.displayNumber}");

        ApplyMargins(item);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ListItemInfo ParseParameter(int start, int end, string parameter)
    {
        var item = new ListItemInfo { start = start, end = end, displayNumber = -1 };
        if (string.IsNullOrEmpty(parameter)) return item;

        int colonIndex = parameter.IndexOf(':');
        if (colonIndex < 0)
        {
            if (int.TryParse(parameter, out int level)) item.nestingLevel = level;
        }
        else
        {
            if (int.TryParse(parameter.AsSpan(0, colonIndex), out int level)) item.nestingLevel = level;
            if (int.TryParse(parameter.AsSpan(colonIndex + 1), out int number)) item.displayNumber = number;
        }
        return item;
    }

    private float MeasureMarkerWidthForLayout(ListItemInfo item)
    {
        if (item.displayNumber < 0) return bulletMarkerWidth + markerToTextGap;
        var fontAsset = fontProviderRef?.GetFontAsset(0);
        if (fontAsset == null) return bulletMarkerWidth;
        float scale = fontProviderRef.FontSize / fontAsset.FaceInfo.pointSize;
        return MeasureStringWithScale(GetMarkerTextForMeasure(item), fontAsset, scale) + markerToTextGap;
    }

    private string GetMarkerTextForMeasure(ListItemInfo item)
    {
        int level = Math.Max(0, Math.Min(item.nestingLevel, orderedStyles.Length - 1));
        return $"{FormatOrderedNumber(item.displayNumber, orderedStyles[level])}.";
    }

    private static float MeasureStringWithScale(string text, UniTextFontAsset fontAsset, float scale)
    {
        if (string.IsNullOrEmpty(text)) return 0f;
        var charTable = fontAsset.CharacterLookupTable;
        if (charTable == null) return 0f;
        float totalWidth = 0f;
        for (int i = 0; i < text.Length; i++)
            if (charTable.TryGetValue(text[i], out var ch) && ch?.glyph != null)
                totalWidth += ch.glyph.metrics.horizontalAdvance * scale;
        return totalWidth;
    }

    private void ApplyMargins(ListItemInfo item)
    {
        float contentIndent = item.nestingLevel * indentPerLevel + MeasureMarkerWidthForLayout(item);
        var buf = CommonData.Current;
        if (item.end > buf.startMargins.Length) buf.EnsureCodepointCapacity(item.end);
        var margins = buf.startMargins;
        int safeEnd = Math.Min(item.end, buf.codepointCount);
        for (int i = item.start; i < safeEnd; i++)
            if (contentIndent > margins[i]) margins[i] = contentIndent;
    }

    private void OnAfterGlyphs()
    {
        if (items == null || items.Count == 0 || markersDrawnThisFrame) return;
        markersDrawnThisFrame = true;
        instanceMarkersDrawnThisFrame = true;
        foreach (var item in items) RenderMarker(item);
    }

    private void RenderMarker(ListItemInfo item)
    {
        bool isRtl = IsItemRtl(item.start);
        float baselineY = GetItemBaselineY(item.start, out float firstGlyphX);
        if (float.IsNaN(baselineY)) return;

        string markerText = GetMarkerText(item, isRtl);
        float markerX = isRtl
            ? firstGlyphX + GetLineWidth(item.start) + markerToTextGap
            : firstGlyphX - GlyphRenderHelper.MeasureString(markerText) - markerToTextGap;

        GlyphRenderHelper.DrawString(markerText, markerX, baselineY, UniTextMeshGenerator.currentDefaultColor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsItemRtl(int cluster)
    {
        var levels = CommonData.Current.bidiLevels;
        return (uint)cluster < (uint)levels.Length && (levels[cluster] & 1) == 1;
    }

    private static float GetItemBaselineY(int cluster, out float firstGlyphX)
    {
        var buf = CommonData.Current;
        for (int i = 0; i < buf.positionedGlyphCount; i++)
            if (buf.positionedGlyphs[i].cluster >= cluster)
            {
                firstGlyphX = UniTextMeshGenerator.offsetX + buf.positionedGlyphs[i].x;
                return UniTextMeshGenerator.offsetY - buf.positionedGlyphs[i].y;
            }
        firstGlyphX = 0;
        return float.NaN;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float GetLineWidth(int cluster)
    {
        var buf = CommonData.Current;
        for (int i = 0; i < buf.lineCount; i++)
        {
            ref readonly var line = ref buf.lines[i];
            if (cluster >= line.range.start && cluster < line.range.start + line.range.length)
                return line.width;
        }
        return 0f;
    }

    private string GetMarkerText(ListItemInfo item, bool isRtl)
    {
        if (item.displayNumber < 0)
            return bulletMarkers[Math.Max(0, Math.Min(item.nestingLevel, bulletMarkers.Length - 1))];
        int level = Math.Max(0, Math.Min(item.nestingLevel, orderedStyles.Length - 1));
        string num = FormatOrderedNumber(item.displayNumber, orderedStyles[level]);
        return isRtl ? $".{num}" : $"{num}.";
    }

    private static string FormatOrderedNumber(int n, OrderedMarkerStyle style) => style switch
    {
        OrderedMarkerStyle.Decimal => n.ToString(),
        OrderedMarkerStyle.LowerAlpha => n > 0 ? ((char)('a' + (n - 1) % 26)).ToString() : "?",
        OrderedMarkerStyle.UpperAlpha => n > 0 ? ((char)('A' + (n - 1) % 26)).ToString() : "?",
        OrderedMarkerStyle.LowerRoman => ToRoman(n).ToLowerInvariant(),
        OrderedMarkerStyle.UpperRoman => ToRoman(n),
        _ => n.ToString()
    };

    private static string ToRoman(int n)
    {
        if (n <= 0 || n > 3999) return n.ToString();
        ReadOnlySpan<int> v = stackalloc int[] { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        ReadOnlySpan<string> s = new[] { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
        var r = new System.Text.StringBuilder(15);
        for (int i = 0; i < v.Length; i++) while (n >= v[i]) { n -= v[i]; r.Append(s[i]); }
        return r.ToString();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload() { items = null; markersDrawnThisFrame = false; fontProviderRef = null; }
}
