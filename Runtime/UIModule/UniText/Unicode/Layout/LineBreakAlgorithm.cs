using System;
using System.Runtime.CompilerServices;

/// <summary>
/// Implementation of UAX #14: Unicode Line Breaking Algorithm
/// Determines line break opportunities in text.
/// </summary>
public sealed class LineBreakAlgorithm
{
    private readonly IUnicodeDataProvider dataProvider;

    public LineBreakAlgorithm(IUnicodeDataProvider dataProvider)
    {
        this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
    }

    /// <summary>
    /// Get break opportunities for a sequence of codepoints.
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

        breaks[0] = false;        // LB2
        breaks[length] = true;    // LB3

        for (int i = 0; i < length - 1; i++)
            breaks[i + 1] = CanBreakBetween(codePoints, i);
    }

    public bool[] GetBreakOpportunities(ReadOnlySpan<int> codePoints)
    {
        bool[] breaks = new bool[codePoints.Length + 1];
        GetBreakOpportunities(codePoints, breaks);
        return breaks;
    }

    public bool CanBreakAt(ReadOnlySpan<int> codePoints, int index)
    {
        if (index <= 0) return false;
        if (index >= codePoints.Length) return true;
        return CanBreakBetween(codePoints, index - 1);
    }

    #region Inline Helpers
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsCM(LineBreakClass cls) => cls == LineBreakClass.CM || cls == LineBreakClass.ZWJ;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsLB9Exception(LineBreakClass cls) =>
        cls == LineBreakClass.SP || cls == LineBreakClass.BK || cls == LineBreakClass.CR || 
        cls == LineBreakClass.LF || cls == LineBreakClass.NL || cls == LineBreakClass.ZW || 
        cls == LineBreakClass.ZWJ;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAlphabetic(LineBreakClass cls) => cls == LineBreakClass.AL || cls == LineBreakClass.HL;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAksara(LineBreakClass cls) => cls == LineBreakClass.AK || cls == LineBreakClass.AS;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsVirama(LineBreakClass cls) => cls == LineBreakClass.VF || cls == LineBreakClass.VI;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsNumericAffix(LineBreakClass cls) => cls == LineBreakClass.PO || cls == LineBreakClass.PR;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsKorean(LineBreakClass cls) =>
        cls == LineBreakClass.JL || cls == LineBreakClass.JV || cls == LineBreakClass.JT || 
        cls == LineBreakClass.H2 || cls == LineBreakClass.H3;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsEastAsianWide(EastAsianWidth eaw) => eaw == EastAsianWidth.W || eaw == EastAsianWidth.F;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsEastAsianForLB19a(EastAsianWidth eaw) =>
        eaw == EastAsianWidth.F || eaw == EastAsianWidth.W || eaw == EastAsianWidth.H;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static LineBreakClass ResolveClass(LineBreakClass cls) => cls switch
    {
        LineBreakClass.CM or LineBreakClass.ZWJ => LineBreakClass.AL,
        LineBreakClass.AI or LineBreakClass.SG or LineBreakClass.XX or LineBreakClass.SA => LineBreakClass.AL,
        LineBreakClass.CJ => LineBreakClass.NS,
        _ => cls
    };
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsEffectivelyCombining(LineBreakClass cls, int cp)
    {
        if (IsCM(cls)) return true;
        if (cls == LineBreakClass.SA)
        {
            var gc = dataProvider.GetGeneralCategory(cp);
            return gc == GeneralCategory.Mn || gc == GeneralCategory.Mc;
        }
        return false;
    }
    
    #endregion

    private bool CanBreakBetween(ReadOnlySpan<int> codePoints, int index)
    {
        int beforeCp = codePoints[index];
        int afterCp = codePoints[index + 1];
        var beforeRaw = dataProvider.GetLineBreakClass(beforeCp);
        var afterRaw = dataProvider.GetLineBreakClass(afterCp);

        // LB9: Do not break combining character sequence
        var afterGc = dataProvider.GetGeneralCategory(afterCp);
        bool afterIsCombining = afterRaw == LineBreakClass.CM || afterRaw == LineBreakClass.ZWJ ||
            (afterRaw == LineBreakClass.SA && (afterGc == GeneralCategory.Mn || afterGc == GeneralCategory.Mc));
        
        if (afterIsCombining && !IsLB9Exception(beforeRaw))
            return false;

        // Find effective base (LB9)
        var effectiveBeforeRaw = beforeRaw;
        int effectiveIndex = index;
        int effectiveCp = beforeCp;
        
        while (effectiveIndex > 0 && IsEffectivelyCombining(effectiveBeforeRaw, effectiveCp))
        {
            effectiveIndex--;
            effectiveCp = codePoints[effectiveIndex];
            effectiveBeforeRaw = dataProvider.GetLineBreakClass(effectiveCp);
        }
        
        if (IsEffectivelyCombining(effectiveBeforeRaw, effectiveCp))
            effectiveBeforeRaw = LineBreakClass.AL;
        
        if (IsLB9Exception(effectiveBeforeRaw) && IsEffectivelyCombining(beforeRaw, beforeCp))
            effectiveBeforeRaw = LineBreakClass.AL;

        // LB8a: ZWJ × (check RAW before transform)
        if (beforeRaw == LineBreakClass.ZWJ)
            return false;

        var before = ResolveClass(effectiveBeforeRaw);
        var after = ResolveClass(afterRaw);

        // LB4-LB6: Hard breaks
        if (before == LineBreakClass.BK) return true;
        if (before == LineBreakClass.CR && after == LineBreakClass.LF) return false;
        if (before == LineBreakClass.CR || before == LineBreakClass.LF || before == LineBreakClass.NL) return true;
        if (after == LineBreakClass.BK || after == LineBreakClass.CR || 
            after == LineBreakClass.LF || after == LineBreakClass.NL) return false;

        // LB7: × SP, × ZW
        if (after == LineBreakClass.SP || after == LineBreakClass.ZW) return false;

        // LB8: ZW SP* ÷
        if (before == LineBreakClass.ZW) return true;
        if (before == LineBreakClass.SP)
        {
            for (int i = effectiveIndex - 1; i >= 0; i--)
            {
                var cls = dataProvider.GetLineBreakClass(codePoints[i]);
                if (cls == LineBreakClass.SP) continue;
                if (cls == LineBreakClass.ZW) return true;
                break;
            }
        }

        // LB11: × WJ, WJ ×
        if (before == LineBreakClass.WJ || after == LineBreakClass.WJ) return false;

        // LB12: GL ×
        if (before == LineBreakClass.GL) return false;

        // LB12a: [^SP BA HH HY] × GL
        if (after == LineBreakClass.GL && before != LineBreakClass.SP && before != LineBreakClass.BA && 
            before != LineBreakClass.HH && before != LineBreakClass.HY &&
            !dataProvider.IsUnambiguousHyphen(effectiveCp))
            return false;

        // LB13: × CL, × CP, × EX, × SY
        if (after == LineBreakClass.CL || after == LineBreakClass.CP || 
            after == LineBreakClass.EX || after == LineBreakClass.SY)
            return false;

        // LB14: OP SP* ×
        if (before == LineBreakClass.OP) return false;
        if (before == LineBreakClass.SP)
        {
            for (int i = effectiveIndex - 1; i >= 0; i--)
            {
                var cls = dataProvider.GetLineBreakClass(codePoints[i]);
                if (IsCM(cls)) continue;
                var resolved = ResolveClass(cls);
                if (resolved == LineBreakClass.OP) return false;
                if (resolved != LineBreakClass.SP) break;
            }
        }

        // LB15: QU_Pi SP* × OP
        if (after == LineBreakClass.OP && CheckLB15(codePoints, effectiveIndex, before))
            return false;

        // LB15c: SP ÷ IS NU
        if (before == LineBreakClass.SP && after == LineBreakClass.IS)
        {
            if (LookAheadGetClass(codePoints, index + 2) == LineBreakClass.NU)
                return true;
        }
        
        // LB15d: × IS
        if (after == LineBreakClass.IS) return false;

        // LB16: (CL | CP) SP* × NS
        if (after == LineBreakClass.NS && CheckClosingBeforeNS(codePoints, effectiveIndex, before))
            return false;

        // LB17: B2 SP* × B2
        if (after == LineBreakClass.B2 && CheckB2Pattern(codePoints, effectiveIndex, before))
            return false;

        // LB18: SP ÷ (with LB15a/LB15b)
        if (before == LineBreakClass.SP)
            return HandleLB18(codePoints, index, after, afterCp);

        // LB19/LB19a: QU handling
        if (after == LineBreakClass.QU && !CanBreakBeforeQU(codePoints, index, afterCp, effectiveCp))
            return false;
        if (before == LineBreakClass.QU && !CanBreakAfterQU(codePoints, effectiveIndex, effectiveCp, afterCp))
            return false;

        // LB20: ÷ CB, CB ÷
        if (before == LineBreakClass.CB || after == LineBreakClass.CB) return true;

        // LB20a: Word-initial hyphen
        if ((before == LineBreakClass.HY || dataProvider.IsUnambiguousHyphen(effectiveCp)) &&
            IsWordInitialHyphen(codePoints, effectiveIndex) && IsAlphabetic(after))
            return false;

        // LB21: × BA, × HY, × HH, × NS, BB ×
        if (after == LineBreakClass.BA || after == LineBreakClass.HY || 
            after == LineBreakClass.HH || after == LineBreakClass.NS ||
            dataProvider.IsUnambiguousHyphen(afterCp))
            return false;
        if (before == LineBreakClass.BB) return false;
        
        // LB21a: HL (HY | HH) × [^HL]
        if ((before == LineBreakClass.HY || before == LineBreakClass.HH || 
             dataProvider.IsUnambiguousHyphen(effectiveCp)) &&
            after != LineBreakClass.HL && IsHLBeforeHyphen(codePoints, effectiveIndex))
            return false;

        // LB21b: SY × HL
        if (before == LineBreakClass.SY && after == LineBreakClass.HL) return false;

        // LB22: × IN
        if (after == LineBreakClass.IN) return false;

        // LB23: (AL | HL) × NU, NU × (AL | HL)
        if (IsAlphabetic(before) && after == LineBreakClass.NU) return false;
        if (before == LineBreakClass.NU && IsAlphabetic(after)) return false;

        // LB23a: PR × (ID | EB | EM), (ID | EB | EM) × PO
        if (before == LineBreakClass.PR && 
            (after == LineBreakClass.ID || after == LineBreakClass.EB || after == LineBreakClass.EM))
            return false;
        if ((before == LineBreakClass.ID || before == LineBreakClass.EB || before == LineBreakClass.EM) && 
            after == LineBreakClass.PO)
            return false;

        // LB24: (PR | PO) × (AL | HL), (AL | HL) × (PR | PO)
        if (IsNumericAffix(before) && IsAlphabetic(after)) return false;
        if (IsAlphabetic(before) && IsNumericAffix(after)) return false;

        // LB25: Numeric expressions
        if (!CheckLB25(codePoints, index, effectiveIndex, before, after))
            return false;

        // LB26: Korean syllables
        if (before == LineBreakClass.JL && 
            (after == LineBreakClass.JL || after == LineBreakClass.JV || 
             after == LineBreakClass.H2 || after == LineBreakClass.H3))
            return false;
        if ((before == LineBreakClass.JV || before == LineBreakClass.H2) &&
            (after == LineBreakClass.JV || after == LineBreakClass.JT))
            return false;
        if ((before == LineBreakClass.JT || before == LineBreakClass.H3) && after == LineBreakClass.JT)
            return false;

        // LB27: Korean × PO, PR × Korean
        if (IsKorean(before) && after == LineBreakClass.PO) return false;
        if (before == LineBreakClass.PR && IsKorean(after)) return false;

        // LB28: (AL | HL) × (AL | HL)
        if (IsAlphabetic(before) && IsAlphabetic(after)) return false;

        // LB28a: Brahmic scripts
        if (!CheckLB28a(codePoints, index, effectiveIndex, before, after, 
                        beforeRaw, effectiveBeforeRaw, beforeCp, afterCp, effectiveCp))
            return false;

        // LB29: IS × (AL | HL)
        if (before == LineBreakClass.IS && IsAlphabetic(after)) return false;

        // LB30: (AL | HL | NU) × OP, CP × (AL | HL | NU)
        if ((IsAlphabetic(before) || before == LineBreakClass.NU) && after == LineBreakClass.OP &&
            !IsEastAsianWide(dataProvider.GetEastAsianWidth(afterCp)))
            return false;
        if (before == LineBreakClass.CP && (IsAlphabetic(after) || after == LineBreakClass.NU))
            return false;

        // LB30a: RI × RI (paired)
        if ((effectiveBeforeRaw == LineBreakClass.RI || before == LineBreakClass.RI) && 
            after == LineBreakClass.RI)
        {
            int riCount = 1;
            for (int i = effectiveIndex - 1; i >= 0; i--)
            {
                var cls = dataProvider.GetLineBreakClass(codePoints[i]);
                if (IsCM(cls)) continue;
                if (cls == LineBreakClass.RI) riCount++;
                else break;
            }
            if ((riCount & 1) == 1) return false;
        }

        // LB30b: EB × EM, ExtPict(Cn) × EM
        if (before == LineBreakClass.EB && after == LineBreakClass.EM) return false;
        if (after == LineBreakClass.EM && dataProvider.IsExtendedPictographic(effectiveCp) &&
            dataProvider.GetGeneralCategory(effectiveCp) == GeneralCategory.Cn)
            return false;

        // LB31: ALL ÷ ALL
        return true;
    }

    #region Rule Handlers
    
    private LineBreakClass LookAheadGetClass(ReadOnlySpan<int> codePoints, int start)
    {
        for (int i = start; i < codePoints.Length; i++)
        {
            var cls = dataProvider.GetLineBreakClass(codePoints[i]);
            if (!IsCM(cls)) return ResolveClass(cls);
        }
        return LineBreakClass.XX;
    }
    
    private bool CheckLB15(ReadOnlySpan<int> codePoints, int effectiveIndex, LineBreakClass before)
    {
        var prev = before;
        int i = effectiveIndex, quPos = -1;
        
        while (prev == LineBreakClass.SP && i > 0)
        {
            i--;
            var cls = dataProvider.GetLineBreakClass(codePoints[i]);
            if (IsCM(cls)) continue;
            prev = ResolveClass(cls);
            if (prev == LineBreakClass.QU) quPos = i;
        }
        
        if (prev != LineBreakClass.QU || quPos < 0 || 
            dataProvider.GetGeneralCategory(codePoints[quPos]) != GeneralCategory.Pi)
            return false;
            
        if (quPos == 0) return true;
        
        for (int j = quPos - 1; j >= 0; j--)
        {
            var cls = dataProvider.GetLineBreakClass(codePoints[j]);
            if (IsCM(cls)) continue;
            return cls == LineBreakClass.BK || cls == LineBreakClass.CR ||
                   cls == LineBreakClass.LF || cls == LineBreakClass.NL ||
                   cls == LineBreakClass.SP || cls == LineBreakClass.ZW ||
                   cls == LineBreakClass.CB || cls == LineBreakClass.GL;
        }
        return false;
    }
    
    private bool CheckClosingBeforeNS(ReadOnlySpan<int> codePoints, int effectiveIndex, LineBreakClass before)
    {
        var prev = before;
        for (int i = effectiveIndex; (prev == LineBreakClass.SP || prev == LineBreakClass.AL) && i > 0; )
        {
            i--;
            var cls = dataProvider.GetLineBreakClass(codePoints[i]);
            if (IsCM(cls)) continue;
            prev = ResolveClass(cls);
            if (prev != LineBreakClass.SP) break;
        }
        return prev == LineBreakClass.CL || prev == LineBreakClass.CP;
    }
    
    private bool CheckB2Pattern(ReadOnlySpan<int> codePoints, int effectiveIndex, LineBreakClass before)
    {
        var prev = before;
        for (int i = effectiveIndex; (prev == LineBreakClass.SP || prev == LineBreakClass.AL) && i > 0; )
        {
            i--;
            var cls = dataProvider.GetLineBreakClass(codePoints[i]);
            if (IsCM(cls)) continue;
            prev = ResolveClass(cls);
            if (prev != LineBreakClass.SP) break;
        }
        return prev == LineBreakClass.B2;
    }
    
    private bool HandleLB18(ReadOnlySpan<int> codePoints, int index, LineBreakClass after, int afterCp)
    {
        // LB15b: × QU_Pf (allowed followers)
        if (after == LineBreakClass.QU && dataProvider.GetGeneralCategory(afterCp) == GeneralCategory.Pf)
        {
            var nextCls = LookAheadGetClass(codePoints, index + 2);
            bool eot = true;
            for (int i = index + 2; i < codePoints.Length; i++)
            {
                var c = dataProvider.GetLineBreakClass(codePoints[i]);
                if (!IsCM(c)) { eot = false; break; }
            }
            
            if (eot || nextCls == LineBreakClass.SP || nextCls == LineBreakClass.GL ||
                nextCls == LineBreakClass.WJ || nextCls == LineBreakClass.CL ||
                nextCls == LineBreakClass.QU || nextCls == LineBreakClass.CP ||
                nextCls == LineBreakClass.EX || nextCls == LineBreakClass.IS ||
                nextCls == LineBreakClass.SY || nextCls == LineBreakClass.BK ||
                nextCls == LineBreakClass.CR || nextCls == LineBreakClass.LF ||
                nextCls == LineBreakClass.NL || nextCls == LineBreakClass.ZW)
                return false;
        }
        
        // LB15a: (context) (OP | QU_Pi) SP* ×
        for (int i = index - 1; i >= 0; i--)
        {
            var cls = dataProvider.GetLineBreakClass(codePoints[i]);
            if (cls == LineBreakClass.SP || IsCM(cls)) continue;
            
            bool isOpOrQuPi = cls == LineBreakClass.OP ||
                (cls == LineBreakClass.QU && dataProvider.GetGeneralCategory(codePoints[i]) == GeneralCategory.Pi);
            
            if (isOpOrQuPi)
            {
                if (i == 0) return false;
                for (int j = i - 1; j >= 0; j--)
                {
                    var prevCls = dataProvider.GetLineBreakClass(codePoints[j]);
                    if (IsCM(prevCls)) continue;
                    return !(prevCls == LineBreakClass.BK || prevCls == LineBreakClass.CR ||
                             prevCls == LineBreakClass.LF || prevCls == LineBreakClass.NL ||
                             prevCls == LineBreakClass.OP || prevCls == LineBreakClass.QU ||
                             prevCls == LineBreakClass.GL || prevCls == LineBreakClass.SP || 
                             prevCls == LineBreakClass.ZW || prevCls == LineBreakClass.CB);
                }
            }
            break;
        }
        return true;
    }
    
    private bool CanBreakBeforeQU(ReadOnlySpan<int> codePoints, int index, int afterCp, int effectiveCp)
    {
        if (dataProvider.GetGeneralCategory(afterCp) != GeneralCategory.Pi) return false;
        if (!IsEastAsianForLB19a(dataProvider.GetEastAsianWidth(effectiveCp))) return false;
        
        for (int i = index + 2; i < codePoints.Length; i++)
        {
            var cls = dataProvider.GetLineBreakClass(codePoints[i]);
            if (IsCM(cls)) continue;
            return IsEastAsianForLB19a(dataProvider.GetEastAsianWidth(codePoints[i]));
        }
        return false;
    }
    
    private bool CanBreakAfterQU(ReadOnlySpan<int> codePoints, int effectiveIndex, int effectiveCp, int afterCp)
    {
        if (dataProvider.GetGeneralCategory(effectiveCp) != GeneralCategory.Pf) return false;
        if (!IsEastAsianForLB19a(dataProvider.GetEastAsianWidth(afterCp))) return false;
        
        for (int i = effectiveIndex - 1; i >= 0; i--)
        {
            var cls = dataProvider.GetLineBreakClass(codePoints[i]);
            if (IsCM(cls)) continue;
            return IsEastAsianForLB19a(dataProvider.GetEastAsianWidth(codePoints[i]));
        }
        return false;
    }
    
    private bool IsWordInitialHyphen(ReadOnlySpan<int> codePoints, int effectiveIndex)
    {
        if (effectiveIndex == 0) return true;
        var prev = ResolveClass(dataProvider.GetLineBreakClass(codePoints[effectiveIndex - 1]));
        return prev == LineBreakClass.BK || prev == LineBreakClass.CR ||
               prev == LineBreakClass.LF || prev == LineBreakClass.NL ||
               prev == LineBreakClass.SP || prev == LineBreakClass.ZW ||
               prev == LineBreakClass.CB || prev == LineBreakClass.GL;
    }
    
    private bool IsHLBeforeHyphen(ReadOnlySpan<int> codePoints, int effectiveIndex)
    {
        for (int i = effectiveIndex - 1; i >= 0; i--)
        {
            var cls = dataProvider.GetLineBreakClass(codePoints[i]);
            if (IsCM(cls)) continue;
            return ResolveClass(cls) == LineBreakClass.HL;
        }
        return false;
    }
    
    private bool CheckLB25(ReadOnlySpan<int> codePoints, int index, int effectiveIndex,
                           LineBreakClass before, LineBreakClass after)
    {
        if (before == LineBreakClass.NU && IsNumericAffix(after)) return false;
        if (IsNumericAffix(before) && after == LineBreakClass.NU) return false;
        if ((before == LineBreakClass.HY || before == LineBreakClass.IS) && after == LineBreakClass.NU) return false;
        if (before == LineBreakClass.NU && 
            (after == LineBreakClass.NU || after == LineBreakClass.SY || after == LineBreakClass.IS))
            return false;
        
        // SY × NU (fractions)
        if (before == LineBreakClass.SY && after == LineBreakClass.NU)
        {
            for (int i = effectiveIndex - 1; i >= 0; i--)
            {
                var cls = dataProvider.GetLineBreakClass(codePoints[i]);
                if (IsCM(cls)) continue;
                if (cls == LineBreakClass.NU) return false;
                if (cls == LineBreakClass.SY || cls == LineBreakClass.IS) continue;
                break;
            }
        }
        
        // NU context × (PO|PR)
        if (IsNumericAffix(after))
        {
            for (int i = effectiveIndex; i >= 0; i--)
            {
                var cls = dataProvider.GetLineBreakClass(codePoints[i]);
                if (IsCM(cls)) continue;
                if (cls == LineBreakClass.NU) return false;
                if (cls == LineBreakClass.SY || cls == LineBreakClass.IS ||
                    cls == LineBreakClass.CL || cls == LineBreakClass.CP) continue;
                break;
            }
        }
        
        // (CL|CP) × (PO|PR)
        if ((before == LineBreakClass.CL || before == LineBreakClass.CP) && IsNumericAffix(after))
        {
            for (int i = effectiveIndex - 1; i >= 0; i--)
            {
                var cls = dataProvider.GetLineBreakClass(codePoints[i]);
                if (cls == LineBreakClass.NU) return false;
                if (cls == LineBreakClass.OP || cls == LineBreakClass.BK ||
                    cls == LineBreakClass.CR || cls == LineBreakClass.LF ||
                    cls == LineBreakClass.NL || cls == LineBreakClass.SP) break;
            }
        }
        
        // (PO|PR) × OP
        if (IsNumericAffix(before) && after == LineBreakClass.OP)
        {
            for (int i = index + 2; i < codePoints.Length; i++)
            {
                var cls = dataProvider.GetLineBreakClass(codePoints[i]);
                if (cls == LineBreakClass.NU) return false;
                if (cls == LineBreakClass.CL || cls == LineBreakClass.CP ||
                    cls == LineBreakClass.BK || cls == LineBreakClass.CR ||
                    cls == LineBreakClass.LF || cls == LineBreakClass.NL) break;
            }
        }
        
        return true;
    }
    
    private bool CheckLB28a(ReadOnlySpan<int> codePoints, int index, int effectiveIndex,
                            LineBreakClass before, LineBreakClass after,
                            LineBreakClass beforeRaw, LineBreakClass effectiveBeforeRaw,
                            int beforeCp, int afterCp, int effectiveCp)
    {
        if (before == LineBreakClass.AP && IsAksara(after)) return false;
        if (IsAksara(before) && IsVirama(after)) return false;
        
        // VI × (AK|AS) with valid predecessor
        if (before == LineBreakClass.VI && IsAksara(after))
        {
            for (int i = effectiveIndex - 1; i >= 0; i--)
            {
                var cls = dataProvider.GetLineBreakClass(codePoints[i]);
                if (IsCM(cls)) continue;
                if (IsAksara(cls) || cls == LineBreakClass.VI) return false;
                break;
            }
        }
        
        // Brahmic conjunct: (AK|AS) CM × (AK|AS) (VF|VI)
        if (beforeRaw == LineBreakClass.CM && IsAksara(after) && 
            IsAksara(effectiveBeforeRaw) && dataProvider.IsBrahmicForLB28a(beforeCp))
        {
            var nextCls = LookAheadGetClass(codePoints, index + 2);
            if (IsVirama(nextCls))
            {
                bool foundCM = false;
                for (int i = effectiveIndex - 1; i >= 0; i--)
                {
                    var cls = dataProvider.GetLineBreakClass(codePoints[i]);
                    if (IsCM(cls)) { foundCM = true; continue; }
                    if (IsVirama(cls) && foundCM) return true;
                    break;
                }
                return false;
            }
        }
        
        if (before == LineBreakClass.AP && dataProvider.IsDottedCircle(afterCp)) return false;
        if (dataProvider.IsDottedCircle(effectiveCp) && IsVirama(after)) return false;
        
        // (AK|25CC|AS) VI ×
        if (beforeRaw == LineBreakClass.VI)
        {
            for (int i = index - 1; i >= 0; i--)
            {
                var cls = dataProvider.GetLineBreakClass(codePoints[i]);
                if (IsCM(cls)) continue;
                if (IsAksara(cls) || dataProvider.IsDottedCircle(codePoints[i])) return false;
                break;
            }
        }
        
        return true;
    }
    
    #endregion
}