using System;
using System.Runtime.CompilerServices;
using System.Text;
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
    private PooledList<ListItemInfo> instanceItems;
    private UniTextFontProvider instanceFontProvider;

    [ThreadStatic] private static PooledList<ListItemInfo> items;
    [ThreadStatic] private static UniTextFontProvider fontProviderRef;
    [ThreadStatic] private static StringBuilder sharedBuilder;

    public float indentPerLevel = 20f;
    public float markerToTextGap = 8f;
    public float bulletMarkerWidth = 24f;
    public string[] bulletMarkers = { "•", "-", "·" };

    public OrderedMarkerStyle[] orderedStyles =
        { OrderedMarkerStyle.Decimal, OrderedMarkerStyle.LowerAlpha, OrderedMarkerStyle.LowerRoman };

    protected override void CreateBuffers()
    {
        instanceItems = new PooledList<ListItemInfo>(32);
        instanceFontProvider = uniText.FontProvider;
        items = instanceItems;
        fontProviderRef = instanceFontProvider;
        sharedBuilder ??= new StringBuilder(32);
    }

    protected override void Subscribe()
    {
        uniText.Rebuilding += OnRebuilding;
        uniText.MeshGenerator.OnAfterGlyphsPerFont += OnAfterGlyphs;
    }

    protected override void Unsubscribe()
    {
        uniText.Rebuilding -= OnRebuilding;
        uniText.MeshGenerator.OnAfterGlyphsPerFont -= OnAfterGlyphs;
    }

    protected override void ReleaseBuffers()
    {
        instanceItems?.Return();
        instanceItems = null;
        instanceFontProvider = null;
    }

    protected override void ClearBuffers()
    {
        instanceItems.FakeClear();
    }

    protected override void OnApply(int start, int end, string parameter)
    {
        var item = ParseParameter(start, end, parameter);
        items.Add(item);
        ApplyMargins(item);
        CollectVirtualCodepoints(item);
    }

    private void CollectVirtualCodepoints(ListItemInfo item)
    {
        if (item.displayNumber < 0)
        {
            var marker = bulletMarkers[Math.Max(0, Math.Min(item.nestingLevel, bulletMarkers.Length - 1))];
            for (var i = 0; i < marker.Length; i++)
                buffers.virtualCodepoints.Add(marker[i]);
        }
    }

    private void OnRebuilding()
    {
        items = instanceItems;
        fontProviderRef = instanceFontProvider;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ListItemInfo ParseParameter(int start, int end, string parameter)
    {
        var item = new ListItemInfo { start = start, end = end, displayNumber = -1 };
        if (string.IsNullOrEmpty(parameter)) return item;

        var colonIndex = parameter.IndexOf(':');
        if (colonIndex < 0)
        {
            if (int.TryParse(parameter, out var level)) item.nestingLevel = level;
        }
        else
        {
            if (int.TryParse(parameter.AsSpan(0, colonIndex), out var level)) item.nestingLevel = level;
            if (int.TryParse(parameter.AsSpan(colonIndex + 1), out var number)) item.displayNumber = number;
        }

        return item;
    }

    private float MeasureMarkerWidthForLayout(ListItemInfo item)
    {
        if (item.displayNumber < 0) return bulletMarkerWidth + markerToTextGap;
        var fontAsset = fontProviderRef?.GetFontAsset(0);
        if (fontAsset == null) return bulletMarkerWidth;

        var buf = buffers;
        var fontSize = buf.shapingFontSize > 0 ? buf.shapingFontSize : fontProviderRef.FontSize;

        sharedBuilder ??= new StringBuilder(32);
        sharedBuilder.Clear();
        var level = Math.Max(0, Math.Min(item.nestingLevel, orderedStyles.Length - 1));
        AppendOrderedNumber(sharedBuilder, item.displayNumber, orderedStyles[level]);
        sharedBuilder.Append('.');

        return MeasureStringWithHarfBuzz(sharedBuilder, fontAsset, fontSize, buf) + markerToTextGap;
    }

    private static float MeasureStringWithHarfBuzz(StringBuilder sb, UniTextFont font, float fontSize, UniTextBuffers buf)
    {
        if (sb == null || sb.Length == 0) return 0f;
        if (!font.HasFontData) return 0f;

        var totalWidth = 0f;
        var len = sb.Length;
        for (var i = 0; i < len; i++)
        {
            uint codepoint = sb[i];
            if (HarfBuzzFontValidator.TryGetGlyphInfo(font, codepoint, fontSize, out _, out var advance))
            {
                totalWidth += advance;
                buf.virtualCodepoints.Add(codepoint);
            }
        }

        return totalWidth;
    }

    private void ApplyMargins(ListItemInfo item)
    {
        var buf = buffers;

        var contentIndent = item.nestingLevel * indentPerLevel + MeasureMarkerWidthForLayout(item);

        if (item.end > buf.startMargins.Length) buf.EnsureCodepointCapacity(item.end);
        var margins = buf.startMargins;
        var safeEnd = Math.Min(item.end, buf.codepoints.count);
        for (var i = item.start; i < safeEnd; i++)
            if (contentIndent > margins[i])
                margins[i] = contentIndent;
    }

    private void OnAfterGlyphs()
    {
        if (items == null || items.Count == 0) return;
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            RenderMarker(item);
        }
    }

    private void RenderMarker(ListItemInfo item)
    {
        var isRtl = IsItemRtl(item.start);
        var baselineY = GetItemBaselineY(item.start, out var firstGlyphX);
        if (float.IsNaN(baselineY)) return;

        sharedBuilder ??= new StringBuilder(32);
        GetMarkerText(item, isRtl, sharedBuilder);

        var buf = buffers;
        var glyphScale = buf.GetGlyphScale(uniText.CurrentFontSize);
        var scaledGap = markerToTextGap * glyphScale;

        var markerX = isRtl
            ? firstGlyphX + GetLineWidth(item.start) * glyphScale + scaledGap
            : firstGlyphX - GlyphRenderHelper.MeasureString(fontProviderRef, sharedBuilder) - scaledGap;

        GlyphRenderHelper.DrawString(fontProviderRef, sharedBuilder, markerX, baselineY, UniTextMeshGenerator.Current.defaultColor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsItemRtl(int cluster)
    {
        var dir = uniText.BaseDirection;
        if (dir == TextDirection.LeftToRight) return false;
        if (dir == TextDirection.RightToLeft) return true;
        var levels = buffers.bidiLevels;
        return (uint)cluster < (uint)levels.Length && (levels[cluster] & 1) == 1;
    }

    private float GetItemBaselineY(int cluster, out float firstGlyphX)
    {
        var buf = buffers;
        var gen = UniTextMeshGenerator.Current;
        for (var i = 0; i < buf.positionedGlyphs.count; i++)
            if (buf.positionedGlyphs.data[i].cluster >= cluster)
            {
                firstGlyphX = gen.offsetX + buf.positionedGlyphs.data[i].x;
                return gen.offsetY - buf.positionedGlyphs.data[i].y;
            }

        firstGlyphX = 0;
        return float.NaN;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float GetLineWidth(int cluster)
    {
        var buf = buffers;
        for (var i = 0; i < buf.lines.count; i++)
        {
            ref readonly var line = ref buf.lines.data[i];
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

        var level = Math.Max(0, Math.Min(item.nestingLevel, orderedStyles.Length - 1));
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
        if (n == 0)
        {
            sb.Append('0');
            return;
        }

        if (n < 0)
        {
            sb.Append('-');
            n = -n;
        }

        var start = sb.Length;
        while (n > 0)
        {
            sb.Append((char)('0' + n % 10));
            n /= 10;
        }

        var end = sb.Length - 1;
        while (start < end)
        {
            (sb[start], sb[end]) = (sb[end], sb[start]);
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

        ReadOnlySpan<int> values = stackalloc int[] { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        if (lower)
        {
            while (n >= 1000)
            {
                sb.Append('m');
                n -= 1000;
            }

            if (n >= 900)
            {
                sb.Append("cm");
                n -= 900;
            }

            if (n >= 500)
            {
                sb.Append('d');
                n -= 500;
            }

            if (n >= 400)
            {
                sb.Append("cd");
                n -= 400;
            }

            while (n >= 100)
            {
                sb.Append('c');
                n -= 100;
            }

            if (n >= 90)
            {
                sb.Append("xc");
                n -= 90;
            }

            if (n >= 50)
            {
                sb.Append('l');
                n -= 50;
            }

            if (n >= 40)
            {
                sb.Append("xl");
                n -= 40;
            }

            while (n >= 10)
            {
                sb.Append('x');
                n -= 10;
            }

            if (n >= 9)
            {
                sb.Append("ix");
                n -= 9;
            }

            if (n >= 5)
            {
                sb.Append('v');
                n -= 5;
            }

            if (n >= 4)
            {
                sb.Append("iv");
                n -= 4;
            }

            while (n >= 1)
            {
                sb.Append('i');
                n -= 1;
            }
        }
        else
        {
            while (n >= 1000)
            {
                sb.Append('M');
                n -= 1000;
            }

            if (n >= 900)
            {
                sb.Append("CM");
                n -= 900;
            }

            if (n >= 500)
            {
                sb.Append('D');
                n -= 500;
            }

            if (n >= 400)
            {
                sb.Append("CD");
                n -= 400;
            }

            while (n >= 100)
            {
                sb.Append('C');
                n -= 100;
            }

            if (n >= 90)
            {
                sb.Append("XC");
                n -= 90;
            }

            if (n >= 50)
            {
                sb.Append('L');
                n -= 50;
            }

            if (n >= 40)
            {
                sb.Append("XL");
                n -= 40;
            }

            while (n >= 10)
            {
                sb.Append('X');
                n -= 10;
            }

            if (n >= 9)
            {
                sb.Append("IX");
                n -= 9;
            }

            if (n >= 5)
            {
                sb.Append('V');
                n -= 5;
            }

            if (n >= 4)
            {
                sb.Append("IV");
                n -= 4;
            }

            while (n >= 1)
            {
                sb.Append('I');
                n -= 1;
            }
        }
    }
}