using System;
using System.Runtime.CompilerServices;

/// <summary>
/// Implementation of UAX #29: Unicode Text Segmentation (Grapheme Cluster Boundaries)
/// Reference: https://www.unicode.org/reports/tr29/
/// </summary>
public sealed class GraphemeBreaker
{
    private readonly IUnicodeDataProvider dataProvider;

    public GraphemeBreaker(IUnicodeDataProvider dataProvider)
    {
        this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
    }

    /// <summary>
    /// Get grapheme cluster break opportunities.
    /// breaks[i] = true means there's a break before codePoints[i].
    /// breaks[0] is always true (sot), breaks[length] is always true (eot).
    /// </summary>
    public void GetBreakOpportunities(ReadOnlySpan<int> codePoints, Span<bool> breaks)
    {
        int length = codePoints.Length;
        
        if (breaks.Length < length + 1)
            throw new ArgumentException($"breaks array must have length at least {length + 1}");

        if (length == 0)
        {
            breaks[0] = true;
            return;
        }

        // GB1: sot ÷ Any
        breaks[0] = true;
        // GB2: Any ÷ eot
        breaks[length] = true;

        // State for RI counting (GB12, GB13)
        int riCount = 0;

        for (int i = 0; i < length - 1; i++)
        {
            int cp1 = codePoints[i];
            int cp2 = codePoints[i + 1];
            var gcb1 = dataProvider.GetGraphemeClusterBreak(cp1);
            var gcb2 = dataProvider.GetGraphemeClusterBreak(cp2);
            
            // Update RI count
            if (gcb1 == GraphemeClusterBreak.Regional_Indicator)
                riCount++;
            else
                riCount = 0;

            breaks[i + 1] = ShouldBreak(gcb1, gcb2, riCount, codePoints, i);
        }
    }

    public bool[] GetBreakOpportunities(ReadOnlySpan<int> codePoints)
    {
        bool[] breaks = new bool[codePoints.Length + 1];
        GetBreakOpportunities(codePoints, breaks);
        return breaks;
    }

    /// <summary>
    /// Count grapheme clusters in text.
    /// </summary>
    public int CountGraphemeClusters(ReadOnlySpan<int> codePoints)
    {
        if (codePoints.Length == 0)
            return 0;

        int count = 1;
        int riCount = 0;

        for (int i = 0; i < codePoints.Length - 1; i++)
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
        // GB3: CR × LF
        if (gcb1 == GraphemeClusterBreak.CR && gcb2 == GraphemeClusterBreak.LF)
            return false;

        // GB4: (Control | CR | LF) ÷
        if (gcb1 == GraphemeClusterBreak.Control || 
            gcb1 == GraphemeClusterBreak.CR || 
            gcb1 == GraphemeClusterBreak.LF)
            return true;

        // GB5: ÷ (Control | CR | LF)
        if (gcb2 == GraphemeClusterBreak.Control || 
            gcb2 == GraphemeClusterBreak.CR || 
            gcb2 == GraphemeClusterBreak.LF)
            return true;

        // GB6: L × (L | V | LV | LVT)
        if (gcb1 == GraphemeClusterBreak.L &&
            (gcb2 == GraphemeClusterBreak.L || 
             gcb2 == GraphemeClusterBreak.V || 
             gcb2 == GraphemeClusterBreak.LV || 
             gcb2 == GraphemeClusterBreak.LVT))
            return false;

        // GB7: (LV | V) × (V | T)
        if ((gcb1 == GraphemeClusterBreak.LV || gcb1 == GraphemeClusterBreak.V) &&
            (gcb2 == GraphemeClusterBreak.V || gcb2 == GraphemeClusterBreak.T))
            return false;

        // GB8: (LVT | T) × T
        if ((gcb1 == GraphemeClusterBreak.LVT || gcb1 == GraphemeClusterBreak.T) &&
            gcb2 == GraphemeClusterBreak.T)
            return false;

        // GB9: × (Extend | ZWJ)
        if (gcb2 == GraphemeClusterBreak.Extend || gcb2 == GraphemeClusterBreak.ZWJ)
            return false;

        // GB9a: × SpacingMark
        if (gcb2 == GraphemeClusterBreak.SpacingMark)
            return false;

        // GB9b: Prepend ×
        if (gcb1 == GraphemeClusterBreak.Prepend)
            return false;

        // GB9c: Indic Conjunct Break
        // \p{InCB=Consonant} [\p{InCB=Extend}\p{InCB=Linker}]* \p{InCB=Linker} [\p{InCB=Extend}\p{InCB=Linker}]* × \p{InCB=Consonant}
        int cp2 = codePoints[index + 1];
        if (IsInCBConsonant(cp2))
        {
            // Check if preceded by Linker (with possible Extend/Linker in between) and Consonant before that
            if (HasPrecedingLinkerAndConsonant(codePoints, index))
                return false;
        }

        // GB11: ZWJ × \p{Extended_Pictographic}
        if (gcb1 == GraphemeClusterBreak.ZWJ && dataProvider.IsExtendedPictographic(codePoints[index + 1]))
        {
            // Check if preceded by ExtendedPictographic (with possible Extend/ZWJ in between)
            if (HasPrecedingExtPict(codePoints, index))
                return false;
        }

        // GB12: sot (RI RI)* RI × RI
        // GB13: [^RI] (RI RI)* RI × RI
        if (gcb1 == GraphemeClusterBreak.Regional_Indicator && gcb2 == GraphemeClusterBreak.Regional_Indicator)
        {
            // Don't break if we have an odd number of RI before this position
            if (riCount % 2 == 1)
                return false;
        }

        // GB999: Any ÷ Any
        return true;
    }

    /// <summary>
    /// Check for GB9c: Consonant [Extend|Linker]* Linker [Extend|Linker]* × Consonant
    /// </summary>
    private bool HasPrecedingLinkerAndConsonant(ReadOnlySpan<int> codePoints, int index)
    {
        bool foundLinker = false;
        
        for (int i = index; i >= 0; i--)
        {
            int cp = codePoints[i];
            
            if (IsInCBLinker(cp))
            {
                foundLinker = true;
            }
            else if (IsInCBExtend(cp))
            {
                // Continue looking back
            }
            else if (IsInCBConsonant(cp))
            {
                // Found consonant - check if we saw a linker
                return foundLinker;
            }
            else
            {
                // Something else - break
                return false;
            }
        }
        
        return false;
    }

    /// <summary>
    /// Check if codepoint has InCB=Linker property.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsInCBLinker(int cp)
    {
        return dataProvider.GetIndicConjunctBreak(cp) == IndicConjunctBreak.Linker;
    }

    /// <summary>
    /// Check if codepoint has InCB=Consonant property.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsInCBConsonant(int cp)
    {
        return dataProvider.GetIndicConjunctBreak(cp) == IndicConjunctBreak.Consonant;
    }

    /// <summary>
    /// Check if codepoint has InCB=Extend property.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsInCBExtend(int cp)
    {
        return dataProvider.GetIndicConjunctBreak(cp) == IndicConjunctBreak.Extend;
    }

    private bool HasPrecedingExtPict(ReadOnlySpan<int> codePoints, int index)
    {
        for (int i = index - 1; i >= 0; i--)
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