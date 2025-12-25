using System;
using System.Runtime.CompilerServices;

[Serializable]
public class EllipsisModifier : BaseModifier
{
    public struct Range
    {
        public int start;
        public int end;
        public TruncationMode mode;
        public int truncateFromCluster;
        public int truncateToCluster;
        public int ellipsisCluster;
        public bool needsEllipsis;
    }
    
    private const string EllipsisText = "...";
    private const int EllipsisCodepoint = 0x2026;
    private const int MaxIterations = 10;

    private PooledList<Range> ranges;
    private PooledBuffer<float> originalAdvances;
    private bool needsRestore;
    private bool isProcessingRelayout;
    private int iterationCount;

    private PooledBuffer<int> glyphToGlobalCluster;
    private PooledBuffer<(int firstGlyph, int lastGlyph)> rangeGlyphBoundsCache;

    private PooledBuffer<float> currentCpWidths;

    protected override void CreateBuffers()
    {
        ranges = new PooledList<Range>(8);
        originalAdvances.Rent(256);
        glyphToGlobalCluster.Rent(256);
        rangeGlyphBoundsCache.Rent(8);
        currentCpWidths.Rent(256);
        needsRestore = false;
        isProcessingRelayout = false;
    }

    protected override void Subscribe()
    {
        uniText.RectHeightChanged += OnRectHeightChanged;
        uniText.TextProcessor.Shaped += OnShaped;
        uniText.TextProcessor.LayoutComplete += OnLayoutComplete;
        uniText.MeshGenerator.OnGlyph += OnGlyph;
        uniText.MeshGenerator.OnAfterGlyphsPerFont += OnAfterGlyphsPerFont;
        uniText.MeshGenerator.OnRebuildEnd += OnRebuildEnd;
    }

    private void OnRectHeightChanged()
    {
        uniText.SetDirty(UniText.DirtyFlags.Layout);
    }

    protected override void Unsubscribe()
    {
        uniText.RectHeightChanged -= OnRectHeightChanged;
        uniText.TextProcessor.Shaped -= OnShaped;
        uniText.TextProcessor.LayoutComplete -= OnLayoutComplete;
        uniText.MeshGenerator.OnGlyph -= OnGlyph;
        uniText.MeshGenerator.OnAfterGlyphsPerFont -= OnAfterGlyphsPerFont;
        uniText.MeshGenerator.OnRebuildEnd -= OnRebuildEnd;
    }

    protected override void ReleaseBuffers()
    {
        ranges?.Return();
        ranges = null;
        originalAdvances.Return();
        glyphToGlobalCluster.Return();
        rangeGlyphBoundsCache.Return();
        currentCpWidths.Return();
        needsRestore = false;
        isProcessingRelayout = false;
    }

    protected override void ClearBuffers()
    {
        ranges?.FakeClear();
        originalAdvances.FakeClear();
        glyphToGlobalCluster.FakeClear();
        rangeGlyphBoundsCache.FakeClear();
        currentCpWidths.FakeClear();
        needsRestore = false;
        isProcessingRelayout = false;
    }

    protected override void OnApply(int start, int end, string parameter)
    {
        var mode = ParseMode(parameter);
        ranges.Add(new Range
        {
            start = start,
            end = end,
            mode = mode,
            truncateFromCluster = -1,
            truncateToCluster = -1,
            ellipsisCluster = -1,
            needsEllipsis = false
        });
    }

    private void ClearEllipsisState()
    {
        if (ranges == null) return;

        for (var r = 0; r < ranges.Count; r++)
        {
            var range = ranges[r];
            range.needsEllipsis = false;
            range.truncateFromCluster = -1;
            range.truncateToCluster = -1;
            range.ellipsisCluster = -1;
            ranges[r] = range;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TruncationMode ParseMode(string parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            return TruncationMode.End;

        return parameter.ToLowerInvariant() switch
        {
            "start" => TruncationMode.Start,
            "mid" => TruncationMode.Middle,
            _ => TruncationMode.End
        };
    }

    private void OnShaped()
    {
        BuildGlyphToClusterMap();
        BuildRangeGlyphBounds();
    }

    private void BuildGlyphToClusterMap()
    {
        var buf = buffers;
        var glyphCount = buf.shapedGlyphs.count;

        glyphToGlobalCluster.FakeClear();
        if (glyphCount == 0)
            return;

        glyphToGlobalCluster.EnsureCapacity(glyphCount);

        var runs = buf.shapedRuns.data;
        var runCount = buf.shapedRuns.count;
        var glyphs = buf.shapedGlyphs.data;
        var clusterData = glyphToGlobalCluster.data;

        for (var r = 0; r < runCount; r++)
        {
            ref readonly var run = ref runs[r];
            var rangeStart = run.range.start;
            var end = run.glyphStart + run.glyphCount;
            for (var g = run.glyphStart; g < end; g++)
                clusterData[g] = rangeStart + glyphs[g].cluster;
        }

        glyphToGlobalCluster.count = glyphCount;
    }

    private void BuildRangeGlyphBounds()
    {
        rangeGlyphBoundsCache.FakeClear();
        if (ranges == null || ranges.Count == 0)
            return;

        var rangeCount = ranges.Count;
        rangeGlyphBoundsCache.EnsureCapacity(rangeCount);

        var glyphCount = glyphToGlobalCluster.count;
        var clusterData = glyphToGlobalCluster.data;
        var boundsData = rangeGlyphBoundsCache.data;

        for (var r = 0; r < rangeCount; r++)
        {
            var range = ranges[r];
            var first = -1;
            var last = -1;

            for (var g = 0; g < glyphCount; g++)
            {
                var cluster = clusterData[g];
                if (cluster >= range.start && cluster < range.end)
                {
                    if (first < 0) first = g;
                    last = g;
                }
            }

            boundsData[r] = (first, last);
        }

        rangeGlyphBoundsCache.count = rangeCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetGlobalCluster(int glyphIndex) => glyphToGlobalCluster.data[glyphIndex];

    private void OnLayoutComplete()
    {
        if (ranges == null || ranges.Count == 0)
            return;

        if (isProcessingRelayout)
            return;

        ClearEllipsisState();
        iterationCount = 0;

        var maxHeight = uniText.rectTransform.rect.height;
        var resultHeight = uniText.TextProcessor.ResultHeight;

        if (maxHeight <= 0 || float.IsInfinity(maxHeight))
            return;

        if (resultHeight <= maxHeight)
            return;

        ProcessOverflowIterative(maxHeight);
    }

    private void ProcessOverflowIterative(float maxHeight)
    {
        var buf = buffers;
        var glyphs = buf.shapedGlyphs.data;
        var glyphCount = buf.shapedGlyphs.count;
        var runs = buf.shapedRuns.data;
        var runCount = buf.shapedRuns.count;
        var cpCount = buf.codepoints.count;

        if (glyphCount == 0)
            return;

        originalAdvances.EnsureCapacity(glyphCount);
        SaveOriginalAdvances(glyphs, glyphCount);

        currentCpWidths.EnsureCapacity(cpCount);

        var ellipsisWidth = MeasureEllipsisWidth();

        isProcessingRelayout = true;
        needsRestore = true;

        var low = 0f;
        var high = 1f;
        const float epsilon = 0.01f;

        while (iterationCount < MaxIterations && (high - low) > epsilon)
        {
            var mid = (low + high) / 2f;

            RestoreAdvancesForBinarySearch(glyphs, glyphCount);

            CopyBaseCpWidths(cpCount);
            ApplyTruncationRatio(glyphs, mid, ellipsisWidth);
            RecalculateRunWidths(glyphs, runs, runCount);

            uniText.TextProcessor.ForceRelayout(currentCpWidths.Span);
            var resultHeight = uniText.TextProcessor.ResultHeight;

            if (Math.Abs(resultHeight - maxHeight) < 2f)
                break;

            if (resultHeight > maxHeight)
            {
                low = mid;
            }
            else
            {
                high = mid;
            }

            iterationCount++;
        }

        RestoreAdvancesForBinarySearch(glyphs, glyphCount);
        CopyBaseCpWidths(cpCount);
        ApplyTruncationRatio(glyphs, high, ellipsisWidth);
        RecalculateRunWidths(glyphs, runs, runCount);
        uniText.TextProcessor.ForceRelayout(currentCpWidths.Span);

        isProcessingRelayout = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RestoreAdvancesForBinarySearch(ShapedGlyph[] glyphs, int glyphCount)
    {
        var advances = originalAdvances.data;
        var count = Math.Min(originalAdvances.count, glyphCount);
        for (var i = 0; i < count; i++)
            glyphs[i].advanceX = advances[i];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyBaseCpWidths(int cpCount)
    {
        var src = buffers.cpWidths.data;
        var dst = currentCpWidths.data;
        var count = Math.Min(buffers.cpWidths.count, cpCount);
        Array.Copy(src, dst, count);
        currentCpWidths.count = count;
    }

    private void ApplyTruncationRatio(ShapedGlyph[] glyphs, float ratio, float ellipsisWidth)
    {
        if (ratio <= 0)
            return;

        var rangeCount = ranges.Count;
        var boundsData = rangeGlyphBoundsCache.data;
        var origAdvances = originalAdvances.data;
        var cpWidths = currentCpWidths.data;
        var clusterData = glyphToGlobalCluster.data;
        var cpCount = currentCpWidths.count;

        for (var r = 0; r < rangeCount; r++)
        {
            var range = ranges[r];
            var (firstGlyph, lastGlyph) = boundsData[r];

            if (firstGlyph < 0)
                continue;

            var rangeGlyphCount = lastGlyph - firstGlyph + 1;
            var glyphsToTruncate = (int)Math.Ceiling(rangeGlyphCount * ratio);

            if (glyphsToTruncate <= 0)
                continue;

            int truncStart, truncEnd, ellipsisAt;
            switch (range.mode)
            {
                case TruncationMode.Start:
                    truncStart = firstGlyph;
                    truncEnd = Math.Min(firstGlyph + glyphsToTruncate - 1, lastGlyph);
                    ellipsisAt = truncEnd;
                    break;
                case TruncationMode.Middle:
                    var midPoint = (firstGlyph + lastGlyph) / 2;
                    var halfTrunc = glyphsToTruncate / 2;
                    truncStart = Math.Max(midPoint - halfTrunc, firstGlyph);
                    truncEnd = Math.Min(midPoint + halfTrunc, lastGlyph);
                    ellipsisAt = truncStart;
                    break;
                default:
                    truncEnd = lastGlyph;
                    truncStart = Math.Max(lastGlyph - glyphsToTruncate + 1, firstGlyph);
                    ellipsisAt = truncStart;
                    break;
            }

            for (var g = truncStart; g <= truncEnd; g++)
            {
                var cluster = clusterData[g];
                var oldAdvance = origAdvances[g];
                var newAdvance = (g == ellipsisAt) ? ellipsisWidth : 0f;

                glyphs[g].advanceX = newAdvance;

                if ((uint)cluster < (uint)cpCount)
                    cpWidths[cluster] += newAdvance - oldAdvance;
            }

            range.truncateFromCluster = GetGlobalCluster(truncStart);
            range.truncateToCluster = GetGlobalCluster(truncEnd);
            range.ellipsisCluster = GetGlobalCluster(ellipsisAt);
            range.needsEllipsis = true;
            ranges[r] = range;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SaveOriginalAdvances(ShapedGlyph[] glyphs, int count)
    {
        var advances = originalAdvances.data;
        for (var i = 0; i < count; i++)
            advances[i] = glyphs[i].advanceX;

        originalAdvances.count = count;
    }

    private float MeasureEllipsisWidth()
    {
        var fontProvider = uniText.FontProvider;
        if (fontProvider == null)
            return 0f;

        var fontAsset = fontProvider.GetFontAsset(0);
        if (fontAsset == null)
            return 0f;

        var fontSize = uniText.CurrentFontSize;
        var scale = fontSize / fontAsset.FaceInfo.pointSize;

        var charTable = fontAsset.CharacterLookupTable;
        if (charTable != null && charTable.TryGetValue(EllipsisCodepoint, out var ch) && ch?.glyph != null)
            return ch.glyph.metrics.horizontalAdvance * scale;

        if (charTable != null && charTable.TryGetValue('.', out var dotCh) && dotCh?.glyph != null)
            return dotCh.glyph.metrics.horizontalAdvance * scale * 3;

        return 0f;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RecalculateRunWidths(ShapedGlyph[] glyphs, ShapedRun[] runs, int runCount)
    {
        for (var r = 0; r < runCount; r++)
        {
            ref var run = ref runs[r];
            var width = 0f;
            var end = run.glyphStart + run.glyphCount;

            for (var g = run.glyphStart; g < end; g++)
                width += glyphs[g].advanceX;

            run.width = width;
        }
    }

    private void OnGlyph()
    {
        if (ranges == null || ranges.Count == 0)
            return;

        var cluster = UniTextMeshGenerator.currentCluster;

        for (var r = 0; r < ranges.Count; r++)
        {
            var range = ranges[r];
            if (!range.needsEllipsis)
                continue;

            if (cluster >= range.truncateFromCluster && cluster <= range.truncateToCluster)
            {
                UniTextMeshGenerator.vertexCount -= 4;
                UniTextMeshGenerator.triangleCount -= 6;
                return;
            }
        }
    }

    private void OnAfterGlyphsPerFont()
    {
        if (ranges == null || ranges.Count == 0)
            return;

        for (var r = 0; r < ranges.Count; r++)
        {
            var range = ranges[r];
            if (!range.needsEllipsis)
                continue;

            DrawEllipsisForRange(range);
        }
    }

    private void DrawEllipsisForRange(Range range)
    {
        var positionedGlyphs = buffers.positionedGlyphs.data;
        var positionedCount = buffers.positionedGlyphs.count;

        if (positionedCount == 0)
            return;

        for (var i = 0; i < positionedCount; i++)
        {
            if (positionedGlyphs[i].cluster == range.ellipsisCluster)
            {
                ref readonly var pg = ref positionedGlyphs[i];
                var x = UniTextMeshGenerator.offsetX + pg.x;
                var y = UniTextMeshGenerator.offsetY - pg.y;
                GlyphRenderHelper.DrawString(EllipsisText, x, y, UniTextMeshGenerator.currentDefaultColor);
                return;
            }
        }
    }

    private void OnRebuildEnd()
    {
        if (!needsRestore)
            return;

        RestoreOriginalAdvances();
        needsRestore = false;
    }

    private void RestoreOriginalAdvances()
    {
        if (originalAdvances.count == 0)
            return;

        var glyphs = buffers.shapedGlyphs.data;
        var advances = originalAdvances.data;
        var count = Math.Min(originalAdvances.count, buffers.shapedGlyphs.count);

        for (var i = 0; i < count; i++)
            glyphs[i].advanceX = advances[i];

        var runs = buffers.shapedRuns.data;
        var runCount = buffers.shapedRuns.count;
        RecalculateRunWidths(glyphs, runs, runCount);

        originalAdvances.FakeClear();
    }
}