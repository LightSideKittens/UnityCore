using System;
using System.Runtime.CompilerServices;


public sealed class GraphemeBreaker
{
    private readonly IUnicodeDataProvider dataProvider;

    public GraphemeBreaker(IUnicodeDataProvider dataProvider)
    {
        this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
    }


    public void GetBreakOpportunities(ReadOnlySpan<int> codePoints, Span<bool> breaks)
    {
        var length = codePoints.Length;

        if (breaks.Length < length + 1)
            throw new ArgumentException($"breaks array must have length at least {length + 1}");

        if (length == 0)
        {
            breaks[0] = true;
            return;
        }

        breaks[0] = true;
        breaks[length] = true;

        var riCount = 0;

        for (var i = 0; i < length - 1; i++)
        {
            var cp1 = codePoints[i];
            var cp2 = codePoints[i + 1];
            var gcb1 = dataProvider.GetGraphemeClusterBreak(cp1);
            var gcb2 = dataProvider.GetGraphemeClusterBreak(cp2);

            if (gcb1 == GraphemeClusterBreak.Regional_Indicator)
                riCount++;
            else
                riCount = 0;

            breaks[i + 1] = ShouldBreak(gcb1, gcb2, riCount, codePoints, i);
        }
    }

    public bool[] GetBreakOpportunities(ReadOnlySpan<int> codePoints)
    {
        var breaks = new bool[codePoints.Length + 1];
        GetBreakOpportunities(codePoints, breaks);
        return breaks;
    }


    public int CountGraphemeClusters(ReadOnlySpan<int> codePoints)
    {
        if (codePoints.Length == 0)
            return 0;

        var count = 1;
        var riCount = 0;

        for (var i = 0; i < codePoints.Length - 1; i++)
        {
            var gcb1 = dataProvider.GetGraphemeClusterBreak(codePoints[i]);
            var gcb2 = dataProvider.GetGraphemeClusterBreak(codePoints[i + 1]);

            if (gcb1 == GraphemeClusterBreak.Regional_Indicator)
                riCount++;
            else
                riCount = 0;

            if (ShouldBreak(gcb1, gcb2, riCount, codePoints, i))
                count++;
        }

        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ShouldBreak(
        GraphemeClusterBreak gcb1,
        GraphemeClusterBreak gcb2,
        int riCount,
        ReadOnlySpan<int> codePoints,
        int index)
    {
        if (gcb1 == GraphemeClusterBreak.CR && gcb2 == GraphemeClusterBreak.LF)
            return false;

        if (gcb1 == GraphemeClusterBreak.Control ||
            gcb1 == GraphemeClusterBreak.CR ||
            gcb1 == GraphemeClusterBreak.LF)
            return true;

        if (gcb2 == GraphemeClusterBreak.Control ||
            gcb2 == GraphemeClusterBreak.CR ||
            gcb2 == GraphemeClusterBreak.LF)
            return true;

        if (gcb1 == GraphemeClusterBreak.L &&
            (gcb2 == GraphemeClusterBreak.L ||
             gcb2 == GraphemeClusterBreak.V ||
             gcb2 == GraphemeClusterBreak.LV ||
             gcb2 == GraphemeClusterBreak.LVT))
            return false;

        if ((gcb1 == GraphemeClusterBreak.LV || gcb1 == GraphemeClusterBreak.V) &&
            (gcb2 == GraphemeClusterBreak.V || gcb2 == GraphemeClusterBreak.T))
            return false;

        if ((gcb1 == GraphemeClusterBreak.LVT || gcb1 == GraphemeClusterBreak.T) &&
            gcb2 == GraphemeClusterBreak.T)
            return false;

        if (gcb2 == GraphemeClusterBreak.Extend || gcb2 == GraphemeClusterBreak.ZWJ)
            return false;

        if (gcb2 == GraphemeClusterBreak.SpacingMark)
            return false;

        if (gcb1 == GraphemeClusterBreak.Prepend)
            return false;

        var cp2 = codePoints[index + 1];
        if (IsInCBConsonant(cp2))
            if (HasPrecedingLinkerAndConsonant(codePoints, index))
                return false;

        if (gcb1 == GraphemeClusterBreak.ZWJ && dataProvider.IsExtendedPictographic(codePoints[index + 1]))
            if (HasPrecedingExtPict(codePoints, index))
                return false;

        if (gcb1 == GraphemeClusterBreak.Regional_Indicator && gcb2 == GraphemeClusterBreak.Regional_Indicator)
            if (riCount % 2 == 1)
                return false;

        return true;
    }


    private bool HasPrecedingLinkerAndConsonant(ReadOnlySpan<int> codePoints, int index)
    {
        var foundLinker = false;

        for (var i = index; i >= 0; i--)
        {
            var cp = codePoints[i];

            var icb = dataProvider.GetIndicConjunctBreak(cp);

            if (icb == IndicConjunctBreak.Linker)
            {
                foundLinker = true;
            }
            else if (icb == IndicConjunctBreak.Extend)
            {
            }
            else if (icb == IndicConjunctBreak.Consonant)
            {
                return foundLinker;
            }
            else
            {
                return false;
            }
        }

        return false;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsInCBLinker(int cp)
    {
        return dataProvider.GetIndicConjunctBreak(cp) == IndicConjunctBreak.Linker;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsInCBConsonant(int cp)
    {
        return dataProvider.GetIndicConjunctBreak(cp) == IndicConjunctBreak.Consonant;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsInCBExtend(int cp)
    {
        return dataProvider.GetIndicConjunctBreak(cp) == IndicConjunctBreak.Extend;
    }

    private bool HasPrecedingExtPict(ReadOnlySpan<int> codePoints, int index)
    {
        for (var i = index - 1; i >= 0; i--)
        {
            var gcb = dataProvider.GetGraphemeClusterBreak(codePoints[i]);
            if (dataProvider.IsExtendedPictographic(codePoints[i]))
                return true;
            if (gcb != GraphemeClusterBreak.Extend && gcb != GraphemeClusterBreak.ZWJ)
                break;
        }

        return false;
    }
}