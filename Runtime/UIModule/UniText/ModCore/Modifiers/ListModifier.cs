using System;
using System.Runtime.CompilerServices;
using System.Text;
using LSCore;
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
    private LSList<ListItemInfo> instanceItems;
    private bool instanceMarkersDrawnThisFrame;
    private UniTextFontProvider instanceFontProvider;

    private static LSList<ListItemInfo> items;
    private static bool markersDrawnThisFrame;
    private static UniTextFontProvider fontProviderRef;

    // Shared StringBuilder for zero-allocation string formatting
    private static readonly StringBuilder sharedBuilder = new(32);

    public static bool DebugLogging = false;

    public float indentPerLevel = 20f;
    public float markerToTextGap = 8f;
    public float bulletMarkerWidth = 24f;
    public string[] bulletMarkers = { "•", "-", "·" };
    public OrderedMarkerStyle[] orderedStyles = { OrderedMarkerStyle.Decimal, OrderedMarkerStyle.LowerAlpha, OrderedMarkerStyle.LowerRoman };

    protected override void CreateBuffers()
    {
        instanceItems = new LSList<ListItemInfo>(32);
        instanceFontProvider = uniText.FontProvider;
        items = instanceItems;
        fontProviderRef = instanceFontProvider;
    }

    protected override void Subscribe()
    {
        uniText.Rebuilding += OnRebuilding;
        uniText.MeshGenerator.OnRebuildStart += OnRebuildStart;
        uniText.MeshGenerator.OnAfterGlyphsPerFont += OnAfterGlyphs;
    }

    protected override void Unsubscribe()
    {
        uniText.Rebuilding -= OnRebuilding;
        uniText.MeshGenerator.OnRebuildStart -= OnRebuildStart;
        uniText.MeshGenerator.OnAfterGlyphsPerFont -= OnAfterGlyphs;
    }

    protected override void ReleaseBuffers()
    {
        instanceItems = null;
        instanceFontProvider = null;
    }

    protected override void ClearBuffers() => instanceItems.FakeClear();

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

        // Use shapingFontSize for consistent margin calculation during auto size
        // Margins are applied during shaping phase, so they must use shapingFontSize
        var buf = CommonData.Current;
        float fontSize = buf.shapingFontSize > 0 ? buf.shapingFontSize : fontProviderRef.FontSize;
        float scale = fontSize / fontAsset.FaceInfo.pointSize;

        // Use shared builder for measurement
        sharedBuilder.Clear();
        int level = Math.Max(0, Math.Min(item.nestingLevel, orderedStyles.Length - 1));
        AppendOrderedNumber(sharedBuilder, item.displayNumber, orderedStyles[level]);
        sharedBuilder.Append('.');

        return MeasureStringWithScale(sharedBuilder, fontAsset, scale) + markerToTextGap;
    }

    private static float MeasureStringWithScale(StringBuilder sb, UniTextFontAsset fontAsset, float scale)
    {
        if (sb == null || sb.Length == 0) return 0f;
        var charTable = fontAsset.CharacterLookupTable;
        if (charTable == null) return 0f;
        float totalWidth = 0f;
        int len = sb.Length;
        for (int i = 0; i < len; i++)
        {
            uint codepoint = sb[i];
            if (charTable.TryGetValue(codepoint, out var ch) && ch?.glyph != null)
                totalWidth += ch.glyph.metrics.horizontalAdvance * scale;
            else if (fontAsset.TryAddCharacter(codepoint, out var addedCh) && addedCh?.glyph != null)
                totalWidth += addedCh.glyph.metrics.horizontalAdvance * scale;
        }
        return totalWidth;
    }

    private void ApplyMargins(ListItemInfo item)
    {
        var buf = CommonData.Current;

        // Calculate margin in absolute pixels at shapingFontSize
        // Margins are stored in pixels (at shapingFontSize) and will be scaled by LineBreaker
        float contentIndent = item.nestingLevel * indentPerLevel + MeasureMarkerWidthForLayout(item);

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

        // Build marker text into shared builder (zero-allocation)
        GetMarkerText(item, isRtl, sharedBuilder);

        // Calculate glyphScale for proper gap scaling during auto size
        var buf = CommonData.Current;
        float glyphScale = buf.shapingFontSize > 0 ? uniText.CurrentFontSize / buf.shapingFontSize : 1f;
        float scaledGap = markerToTextGap * glyphScale;

        float markerX = isRtl
            ? firstGlyphX + GetLineWidth(item.start) * glyphScale + scaledGap
            : firstGlyphX - GlyphRenderHelper.MeasureString(sharedBuilder) - scaledGap;

        GlyphRenderHelper.DrawString(sharedBuilder, markerX, baselineY, UniTextMeshGenerator.currentDefaultColor);
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

    private void GetMarkerText(ListItemInfo item, bool isRtl, StringBuilder sb)
    {
        sb.Clear();
        if (item.displayNumber < 0)
        {
            sb.Append(bulletMarkers[Math.Max(0, Math.Min(item.nestingLevel, bulletMarkers.Length - 1))]);
            return;
        }
        int level = Math.Max(0, Math.Min(item.nestingLevel, orderedStyles.Length - 1));
        if (isRtl) sb.Append('.');
        AppendOrderedNumber(sb, item.displayNumber, orderedStyles[level]);
        if (!isRtl) sb.Append('.');
    }

    private static void AppendOrderedNumber(StringBuilder sb, int n, OrderedMarkerStyle style)
    {
        switch (style)
        {
            case OrderedMarkerStyle.Decimal:
                AppendInt(sb, n);
                break;
            case OrderedMarkerStyle.LowerAlpha:
                sb.Append(n > 0 ? (char)('a' + (n - 1) % 26) : '?');
                break;
            case OrderedMarkerStyle.UpperAlpha:
                sb.Append(n > 0 ? (char)('A' + (n - 1) % 26) : '?');
                break;
            case OrderedMarkerStyle.LowerRoman:
                AppendRoman(sb, n, true);
                break;
            case OrderedMarkerStyle.UpperRoman:
                AppendRoman(sb, n, false);
                break;
            default:
                AppendInt(sb, n);
                break;
        }
    }

    private static void AppendInt(StringBuilder sb, int n)
    {
        if (n == 0) { sb.Append('0'); return; }
        if (n < 0) { sb.Append('-'); n = -n; }
        int start = sb.Length;
        while (n > 0) { sb.Append((char)('0' + n % 10)); n /= 10; }
        // Reverse digits
        int end = sb.Length - 1;
        while (start < end)
        {
            char tmp = sb[start];
            sb[start] = sb[end];
            sb[end] = tmp;
            start++;
            end--;
        }
    }

    private static void AppendRoman(StringBuilder sb, int n, bool lower)
    {
        if (n <= 0 || n > 3999)
        {
            AppendInt(sb, n);
            return;
        }
        // Roman numeral values and symbols
        ReadOnlySpan<int> values = stackalloc int[] { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        if (lower)
        {
            while (n >= 1000) { sb.Append('m'); n -= 1000; }
            if (n >= 900) { sb.Append("cm"); n -= 900; }
            if (n >= 500) { sb.Append('d'); n -= 500; }
            if (n >= 400) { sb.Append("cd"); n -= 400; }
            while (n >= 100) { sb.Append('c'); n -= 100; }
            if (n >= 90) { sb.Append("xc"); n -= 90; }
            if (n >= 50) { sb.Append('l'); n -= 50; }
            if (n >= 40) { sb.Append("xl"); n -= 40; }
            while (n >= 10) { sb.Append('x'); n -= 10; }
            if (n >= 9) { sb.Append("ix"); n -= 9; }
            if (n >= 5) { sb.Append('v'); n -= 5; }
            if (n >= 4) { sb.Append("iv"); n -= 4; }
            while (n >= 1) { sb.Append('i'); n -= 1; }
        }
        else
        {
            while (n >= 1000) { sb.Append('M'); n -= 1000; }
            if (n >= 900) { sb.Append("CM"); n -= 900; }
            if (n >= 500) { sb.Append('D'); n -= 500; }
            if (n >= 400) { sb.Append("CD"); n -= 400; }
            while (n >= 100) { sb.Append('C'); n -= 100; }
            if (n >= 90) { sb.Append("XC"); n -= 90; }
            if (n >= 50) { sb.Append('L'); n -= 50; }
            if (n >= 40) { sb.Append("XL"); n -= 40; }
            while (n >= 10) { sb.Append('X'); n -= 10; }
            if (n >= 9) { sb.Append("IX"); n -= 9; }
            if (n >= 5) { sb.Append('V'); n -= 5; }
            if (n >= 4) { sb.Append("IV"); n -= 4; }
            while (n >= 1) { sb.Append('I'); n -= 1; }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload() { items = null; markersDrawnThisFrame = false; fontProviderRef = null; sharedBuilder.Clear(); }
}
