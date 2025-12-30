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
        public int truncateMinCluster;
        public int truncateMaxCluster;
        public int ellipsisCluster;
        public bool needsEllipsis;
    }

    private struct LineTruncation
    {
        public int truncateMinCluster;
        public int truncateMaxCluster;
        public int ellipsisCluster;
    }
    
    private const string EllipsisText = "...";
    private const int MaxIterations = 10;

    private PooledList<Range> ranges;
    private PooledBuffer<float> originalAdvances;
    private bool needsRestore;
    private bool isProcessingRelayout;
    private int iterationCount;

    private PooledBuffer<int> glyphToGlobalCluster;
    private PooledBuffer<(int firstGlyph, int lastGlyph, int minCluster, int maxCluster)> rangeGlyphBoundsCache;

    private PooledBuffer<float> currentCpWidths;
    private PooledList<LineTruncation> lineTruncations;

    protected override void CreateBuffers()
    {
        ranges = new PooledList<Range>(8);
        originalAdvances.Rent(256);
        glyphToGlobalCluster.Rent(256);
        rangeGlyphBoundsCache.Rent(8);
        currentCpWidths.Rent(256);
        lineTruncations = new PooledList<LineTruncation>(8);
        needsRestore = false;
        isProcessingRelayout = false;
    }

    protected override void Subscribe()
    {
        uniText.RectHeightChanged += OnRectHeightChanged;
        uniText.DirtyFlagsChanged += OnDirtyFlagsChanged;
        uniText.TextProcessor.Shaped += OnShaped;
        uniText.TextProcessor.LayoutComplete += OnLayoutComplete;
        uniText.MeshGenerator.OnGlyph += OnGlyph;
        uniText.MeshGenerator.OnAfterGlyphsPerFont += OnAfterGlyphsPerFont;
        uniText.MeshGenerator.OnRebuildEnd += OnRebuildEnd;
    }

    private void OnRectHeightChanged()
    {
        if ((uniText.CurrentDirtyFlags & UniText.DirtyFlags.Layout) == 0)
            uniText.SetDirty(UniText.DirtyFlags.Layout);
    }

    private void OnDirtyFlagsChanged(UniText.DirtyFlags flags)
    {
        if ((flags & UniText.DirtyFlags.Alignment) != 0 &&
            (uniText.CurrentDirtyFlags & UniText.DirtyFlags.Layout) == 0)
        {
            uniText.SetDirty(UniText.DirtyFlags.Layout);
        }
    }

    protected override void Unsubscribe()
    {
        uniText.RectHeightChanged -= OnRectHeightChanged;
        uniText.DirtyFlagsChanged -= OnDirtyFlagsChanged;
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
        lineTruncations?.Return();
        lineTruncations = null;
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
        lineTruncations?.FakeClear();
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
            truncateMinCluster = -1,
            truncateMaxCluster = -1,
            ellipsisCluster = -1,
            needsEllipsis = false
        });

        for (var i = 0; i < EllipsisText.Length; i++)
            buffers.virtualCodepoints.Add(EllipsisText[i]);
    }

    private void ClearEllipsisState()
    {
        lineTruncations?.FakeClear();

        if (ranges == null) return;

        for (var r = 0; r < ranges.Count; r++)
        {
            var range = ranges[r];
            range.needsEllipsis = false;
            range.truncateMinCluster = -1;
            range.truncateMaxCluster = -1;
            range.ellipsisCluster = -1;
            ranges[r] = range;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TruncationMode ParseMode(string parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            return TruncationMode.End;

        if (parameter.Equals("start", StringComparison.OrdinalIgnoreCase))
            return TruncationMode.Start;
        if (parameter.Equals("mid", StringComparison.OrdinalIgnoreCase))
            return TruncationMode.Middle;

        return TruncationMode.End;
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
            var firstGlyph = -1;
            var lastGlyph = -1;
            var minCluster = int.MaxValue;
            var maxCluster = int.MinValue;

            for (var g = 0; g < glyphCount; g++)
            {
                var cluster = clusterData[g];
                if (cluster >= range.start && cluster < range.end)
                {
                    if (firstGlyph < 0) firstGlyph = g;
                    lastGlyph = g;
                    if (cluster < minCluster) minCluster = cluster;
                    if (cluster > maxCluster) maxCluster = cluster;
                }
            }

            if (minCluster == int.MaxValue) minCluster = -1;
            if (maxCluster == int.MinValue) maxCluster = -1;

            boundsData[r] = (firstGlyph, lastGlyph, minCluster, maxCluster);
        }

        rangeGlyphBoundsCache.count = rangeCount;
    }

    private void OnLayoutComplete()
    {
        if (ranges == null || ranges.Count == 0)
            return;

        if (isProcessingRelayout)
            return;

        ClearEllipsisState();
        iterationCount = 0;

        var rect = uniText.cachedTransformData.rect;
        var maxWidth = rect.width;
        var maxHeight = rect.height;
        var resultWidth = uniText.TextProcessor.ResultWidth;
        var resultHeight = uniText.TextProcessor.ResultHeight;

        var hasHeightOverflow = maxHeight > 0 && !float.IsInfinity(maxHeight) && resultHeight > maxHeight;
        var hasWidthOverflow = !uniText.EnableWordWrap && maxWidth > 0 && resultWidth > maxWidth;

        if (!hasHeightOverflow && !hasWidthOverflow)
            return;

        if (hasWidthOverflow && !uniText.EnableWordWrap)
            ProcessNonWordWrapOverflow(maxWidth);
        else
            ProcessOverflowIterative(maxHeight);
    }

    private void ProcessNonWordWrapOverflow(float maxWidth)
    {
        var buf = buffers;
        var glyphs = buf.shapedGlyphs.data;
        var glyphCount = buf.shapedGlyphs.count;
        var runs = buf.shapedRuns.data;
        var runCount = buf.shapedRuns.count;

        if (glyphCount == 0)
            return;

        var glyphScale = buf.GetGlyphScale(uniText.CurrentFontSize);
        var maxWidthInShapingUnits = glyphScale > 0 ? maxWidth / glyphScale : maxWidth;

        var ellipsisWidthDisplay = MeasureEllipsisWidth();
        var ellipsisWidth = glyphScale > 0 ? ellipsisWidthDisplay / glyphScale : ellipsisWidthDisplay;

        originalAdvances.EnsureCapacity(glyphCount);
        SaveOriginalAdvances(glyphs, glyphCount);

        isProcessingRelayout = true;
        needsRestore = true;

        var lines = buf.lines.data;
        var lineCount = buf.lines.count;
        var orderedRuns = buf.orderedRuns.data;

        for (var lineIdx = 0; lineIdx < lineCount; lineIdx++)
        {
            ref readonly var line = ref lines[lineIdx];
            if (line.width <= maxWidthInShapingUnits)
                continue;

            var lineExcess = line.width - maxWidthInShapingUnits;

            var lineFirstGlyph = int.MaxValue;
            var lineLastGlyph = int.MinValue;
            for (var r = line.runStart; r < line.runStart + line.runCount; r++)
            {
                ref readonly var run = ref orderedRuns[r];
                if (run.glyphStart < lineFirstGlyph) lineFirstGlyph = run.glyphStart;
                var runEnd = run.glyphStart + run.glyphCount - 1;
                if (runEnd > lineLastGlyph) lineLastGlyph = runEnd;
            }

            var lineRangeWidth = 0f;
            var rangesOnLine = 0;
            var rangeCount = ranges.Count;
            var boundsData = rangeGlyphBoundsCache.data;
            var origAdvances = originalAdvances.data;
            var clusterData = glyphToGlobalCluster.data;

            for (var r = 0; r < rangeCount; r++)
            {
                var (firstGlyph, lastGlyph, _, _) = boundsData[r];
                if (firstGlyph < 0 || lastGlyph < lineFirstGlyph || firstGlyph > lineLastGlyph)
                    continue;

                rangesOnLine++;

                var lineRangeFirst = Math.Max(firstGlyph, lineFirstGlyph);
                var lineRangeLast = Math.Min(lastGlyph, lineLastGlyph);

                for (var g = lineRangeFirst; g <= lineRangeLast; g++)
                    lineRangeWidth += origAdvances[g];
            }

            if (lineRangeWidth <= 0)
                continue;

            var lineWidthToRemove = lineExcess + ellipsisWidth * rangesOnLine;

            for (var r = 0; r < rangeCount; r++)
            {
                var (firstGlyph, lastGlyph, _, _) = boundsData[r];
                if (firstGlyph < 0)
                    continue;
                if (lastGlyph < lineFirstGlyph || firstGlyph > lineLastGlyph)
                    continue;

                var lineRangeFirst = Math.Max(firstGlyph, lineFirstGlyph);
                var lineRangeLast = Math.Min(lastGlyph, lineLastGlyph);

                var lineMinCluster = int.MaxValue;
                var lineMaxCluster = int.MinValue;
                for (var g = lineRangeFirst; g <= lineRangeLast; g++)
                {
                    var cluster = clusterData[g];
                    if (cluster < lineMinCluster) lineMinCluster = cluster;
                    if (cluster > lineMaxCluster) lineMaxCluster = cluster;
                }

                if (lineMinCluster > lineMaxCluster)
                    continue;

                var rangeWidth = 0f;
                for (var g = lineRangeFirst; g <= lineRangeLast; g++)
                    rangeWidth += origAdvances[g];

                var rangeWidthToRemove = lineWidthToRemove * (rangeWidth / lineRangeWidth);

                var clusterCount = lineMaxCluster - lineMinCluster + 1;
                Span<float> clusterWidths = clusterCount <= 256
                    ? stackalloc float[clusterCount]
                    : new float[clusterCount];
                clusterWidths.Clear();
                BuildClusterWidths(lineRangeFirst, lineRangeLast, lineMinCluster, clusterWidths);

                var range = ranges[r];
                var (truncMin, truncMax, ellipsisClusterTarget) = FindWidthBasedTruncation(
                    range.mode, clusterWidths, lineMinCluster, lineMaxCluster, rangeWidthToRemove);

                if (truncMin > truncMax)
                    continue;

                var ellipsisGlyph = ApplyTruncationToGlyphs(
                    glyphs, lineRangeFirst, lineRangeLast, truncMin, truncMax, ellipsisClusterTarget,
                    ellipsisWidth, origAdvances, null, 0, clusterData);

                if (ellipsisGlyph >= 0)
                {
                    lineTruncations.Add(new LineTruncation
                    {
                        truncateMinCluster = truncMin,
                        truncateMaxCluster = truncMax,
                        ellipsisCluster = clusterData[ellipsisGlyph]
                    });
                }
            }
        }

        RecalculateRunWidths(glyphs, runs, runCount);
        RecalculateRunWidths(glyphs, buf.orderedRuns.data, buf.orderedRuns.count);

        uniText.TextProcessor.ForceReposition();

        isProcessingRelayout = false;
    }

    private void BuildClusterWidths(int firstGlyph, int lastGlyph, int minCluster, Span<float> clusterWidths)
    {
        var clusterData = glyphToGlobalCluster.data;
        var origAdvances = originalAdvances.data;

        for (var g = firstGlyph; g <= lastGlyph; g++)
        {
            var cluster = clusterData[g];
            clusterWidths[cluster - minCluster] += origAdvances[g];
        }
    }

    private static (int truncMin, int truncMax, int ellipsisCluster) FindWidthBasedTruncation(
        TruncationMode mode, Span<float> clusterWidths, int minCluster, int maxCluster, float widthToRemove)
    {
        return mode switch
        {
            TruncationMode.Start => FindTruncationFromStart(clusterWidths, minCluster, maxCluster, widthToRemove),
            TruncationMode.Middle => FindTruncationFromMiddle(clusterWidths, minCluster, maxCluster, widthToRemove),
            _ => FindTruncationFromEnd(clusterWidths, minCluster, maxCluster, widthToRemove)
        };
    }

    private static (int truncMin, int truncMax, int ellipsisCluster) FindTruncationFromEnd(
        Span<float> clusterWidths, int minCluster, int maxCluster, float widthToRemove)
    {
        var accumulated = 0f;
        var truncMin = maxCluster + 1;

        for (var c = maxCluster; c >= minCluster; c--)
        {
            accumulated += clusterWidths[c - minCluster];
            truncMin = c;
            if (accumulated >= widthToRemove)
                break;
        }

        return (truncMin, maxCluster, truncMin);
    }

    private static (int truncMin, int truncMax, int ellipsisCluster) FindTruncationFromStart(
        Span<float> clusterWidths, int minCluster, int maxCluster, float widthToRemove)
    {
        var accumulated = 0f;
        var truncMax = minCluster - 1;

        for (var c = minCluster; c <= maxCluster; c++)
        {
            accumulated += clusterWidths[c - minCluster];
            truncMax = c;
            if (accumulated >= widthToRemove)
                break;
        }

        return (minCluster, truncMax, truncMax);
    }

    private static (int truncMin, int truncMax, int ellipsisCluster) FindTruncationFromMiddle(
        Span<float> clusterWidths, int minCluster, int maxCluster, float widthToRemove)
    {
        var midCluster = (minCluster + maxCluster) / 2;
        var truncMin = midCluster;
        var truncMax = midCluster;
        var accumulated = clusterWidths[midCluster - minCluster];

        while (accumulated < widthToRemove)
        {
            var canExpandLeft = truncMin > minCluster;
            var canExpandRight = truncMax < maxCluster;

            if (!canExpandLeft && !canExpandRight)
                break;

            if (canExpandLeft && canExpandRight)
            {
                var leftWidth = clusterWidths[truncMin - 1 - minCluster];
                var rightWidth = clusterWidths[truncMax + 1 - minCluster];

                if (leftWidth <= rightWidth)
                {
                    truncMin--;
                    accumulated += leftWidth;
                }
                else
                {
                    truncMax++;
                    accumulated += rightWidth;
                }
            }
            else if (canExpandLeft)
            {
                truncMin--;
                accumulated += clusterWidths[truncMin - minCluster];
            }
            else
            {
                truncMax++;
                accumulated += clusterWidths[truncMax - minCluster];
            }
        }

        return (truncMin, truncMax, midCluster);
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
            var (firstGlyph, lastGlyph, minCluster, maxCluster) = boundsData[r];
            if (firstGlyph < 0 || minCluster > maxCluster)
                continue;

            var clusterRange = maxCluster - minCluster + 1;
            var clustersToTruncate = (int)Math.Ceiling(clusterRange * ratio);
            if (clustersToTruncate <= 0)
                continue;

            var range = ranges[r];
            var (truncMin, truncMax, ellipsisCluster) = FindRatioBasedTruncation(
                range.mode, minCluster, maxCluster, clustersToTruncate);

            var ellipsisGlyph = ApplyTruncationToGlyphs(
                glyphs, firstGlyph, lastGlyph, truncMin, truncMax, ellipsisCluster,
                ellipsisWidth, origAdvances, cpWidths, cpCount, clusterData);

            UpdateRangeState(r, truncMin, truncMax, ellipsisGlyph, clusterData);
        }
    }

    private int ApplyTruncationToGlyphs(
        ShapedGlyph[] glyphs, int firstGlyph, int lastGlyph,
        int truncMin, int truncMax, int ellipsisClusterTarget, float ellipsisWidth,
        float[] origAdvances, float[] cpWidths, int cpCount, int[] clusterData)
    {
        var ellipsisGlyph = -1;

        for (var g = firstGlyph; g <= lastGlyph; g++)
        {
            var cluster = clusterData[g];
            if (cluster < truncMin || cluster > truncMax)
                continue;

            var oldAdvance = origAdvances[g];
            if (ellipsisGlyph < 0 || cluster == ellipsisClusterTarget)
                ellipsisGlyph = g;

            glyphs[g].advanceX = 0f;

            if (cpWidths != null && (uint)cluster < (uint)cpCount)
                cpWidths[cluster] -= oldAdvance;
        }

        if (ellipsisGlyph >= 0)
        {
            glyphs[ellipsisGlyph].advanceX = ellipsisWidth;
            if (cpWidths != null)
            {
                var cluster = clusterData[ellipsisGlyph];
                if ((uint)cluster < (uint)cpCount)
                    cpWidths[cluster] += ellipsisWidth;
            }
        }

        return ellipsisGlyph;
    }

    private void UpdateRangeState(int rangeIndex, int truncMin, int truncMax, int ellipsisGlyph, int[] clusterData)
    {
        var range = ranges[rangeIndex];
        range.truncateMinCluster = truncMin;
        range.truncateMaxCluster = truncMax;
        range.ellipsisCluster = ellipsisGlyph >= 0 ? clusterData[ellipsisGlyph] : -1;
        range.needsEllipsis = ellipsisGlyph >= 0;
        ranges[rangeIndex] = range;
    }

    private static (int truncMin, int truncMax, int ellipsisCluster) FindRatioBasedTruncation(
        TruncationMode mode, int minCluster, int maxCluster, int clustersToTruncate)
    {
        int truncMin, truncMax, ellipsisCluster;

        switch (mode)
        {
            case TruncationMode.Start:
                truncMin = minCluster;
                truncMax = Math.Min(minCluster + clustersToTruncate - 1, maxCluster);
                ellipsisCluster = truncMax;
                break;
            case TruncationMode.Middle:
                var midCluster = (minCluster + maxCluster) / 2;
                var halfTrunc = clustersToTruncate / 2;
                truncMin = Math.Max(midCluster - halfTrunc, minCluster);
                truncMax = Math.Min(midCluster + halfTrunc, maxCluster);
                ellipsisCluster = midCluster;
                break;
            default:
                truncMax = maxCluster;
                truncMin = Math.Max(maxCluster - clustersToTruncate + 1, minCluster);
                ellipsisCluster = truncMin;
                break;
        }

        return (truncMin, truncMax, ellipsisCluster);
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
        var gen = UniTextMeshGenerator.Current;
        var cluster = gen.currentCluster;

        if (lineTruncations != null && lineTruncations.Count > 0)
        {
            for (var i = 0; i < lineTruncations.Count; i++)
            {
                var lt = lineTruncations[i];
                if (cluster >= lt.truncateMinCluster && cluster <= lt.truncateMaxCluster)
                {
                    gen.vertexCount -= 4;
                    gen.triangleCount -= 6;
                    return;
                }
            }
        }

        if (ranges == null || ranges.Count == 0)
            return;

        for (var r = 0; r < ranges.Count; r++)
        {
            var range = ranges[r];
            if (!range.needsEllipsis)
                continue;

            if (cluster >= range.truncateMinCluster && cluster <= range.truncateMaxCluster)
            {
                gen.vertexCount -= 4;
                gen.triangleCount -= 6;
                return;
            }
        }
    }

    private void OnAfterGlyphsPerFont()
    {
        if (lineTruncations != null && lineTruncations.Count > 0)
        {
            for (var i = 0; i < lineTruncations.Count; i++)
                DrawEllipsisAtCluster(lineTruncations[i].ellipsisCluster);
        }

        if (ranges == null || ranges.Count == 0)
            return;

        for (var r = 0; r < ranges.Count; r++)
        {
            var range = ranges[r];
            if (!range.needsEllipsis)
                continue;

            DrawEllipsisAtCluster(range.ellipsisCluster);
        }
    }

    private void DrawEllipsisAtCluster(int ellipsisCluster)
    {
        var positionedGlyphs = buffers.positionedGlyphs.data;
        var positionedCount = buffers.positionedGlyphs.count;

        if (positionedCount == 0)
            return;

        var fontProvider = uniText.FontProvider;
        if (fontProvider == null)
            return;

        var gen = UniTextMeshGenerator.Current;
        var shapedGlyphs = buffers.shapedGlyphs.data;
        var glyphScale = buffers.GetGlyphScale(uniText.CurrentFontSize);

        for (var i = 0; i < positionedCount; i++)
        {
            if (positionedGlyphs[i].cluster == ellipsisCluster)
            {
                ref readonly var pg = ref positionedGlyphs[i];
                ref readonly var shapedGlyph = ref shapedGlyphs[pg.shapedGlyphIndex];

                // Restore baseline position by removing the original glyph's offsets
                var baselineX = pg.x - shapedGlyph.offsetX * glyphScale;
                var baselineY = pg.y + shapedGlyph.offsetY * glyphScale;

                var x = gen.offsetX + baselineX;
                var y = gen.offsetY - baselineY;
                GlyphRenderHelper.DrawString(fontProvider, EllipsisText, x, y, gen.defaultColor);
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

        RecalculateRunWidths(glyphs, buffers.shapedRuns.data, buffers.shapedRuns.count);
        RecalculateRunWidths(glyphs, buffers.orderedRuns.data, buffers.orderedRuns.count);

        originalAdvances.FakeClear();
    }
}