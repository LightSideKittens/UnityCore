using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public static class ArabicShaper
{
    private enum JoiningType : byte
    {
        U,
        R,
        L,
        D,
        C,
        T
    }

    private enum ArabicForm : byte
    {
        None = 0,
        Isolated,
        Final,
        Initial,
        Medial
    }

    private struct FormEntry
    {
        public readonly int isolated;
        public readonly int final;
        public readonly int initial;
        public readonly int medial;

        public FormEntry(int isolated, int final, int initial, int medial)
        {
            this.isolated = isolated;
            this.final = final;
            this.initial = initial;
            this.medial = medial;
        }

        public int GetCodepoint(ArabicForm form)
        {
            switch (form)
            {
                case ArabicForm.Isolated:
                    return isolated;
                case ArabicForm.Final:
                    return final != 0 ? final : isolated;
                case ArabicForm.Initial:
                    return initial != 0 ? initial : isolated;
                case ArabicForm.Medial:
                    return medial != 0 ? medial : isolated;
                default:
                    return isolated;
            }
        }
    }

    private const int ZWNJ = 0x200C;
    private const int ZWJ = 0x200D;

    private static readonly object initLock = new();
    private static bool initialized;

    private static Dictionary<int, JoiningType> joiningTypes;

    private static Dictionary<int, FormEntry> formEntries;

    static ArabicShaper()
    {
        lock (initLock)
        {
            if (initialized)
                return;

            joiningTypes = ParseDerivedJoiningTypes(Resources.Load<TextAsset>("DerivedJoiningType").text);
            formEntries = ParseArabicContextForms(Resources.Load<TextAsset>("ArabicContextForms").text);

            initialized = true;
        }
    }

    public static int[] Do(int[] codepoints, out int[] indexMap)
    {
        if (codepoints == null)
            throw new ArgumentNullException(nameof(codepoints));

        int length = codepoints.Length;

        if (length == 0)
        {
            indexMap = Array.Empty<int>();
            return Array.Empty<int>();
        }

        bool hasArabic = false;
        for (int i = 0; i < length; i++)
        {
            if (IsArabicScript(codepoints[i]))
            {
                hasArabic = true;
                break;
            }
        }

        if (!hasArabic)
        {
            var outputNoArabic = new int[length];
            indexMap = new int[length];
            for (int i = 0; i < length; i++)
            {
                outputNoArabic[i] = codepoints[i];
                indexMap[i] = i;
            }

            return outputNoArabic;
        }

        var joiningTypesOriginal = new JoiningType[length];
        var joinPrevOriginal = new bool[length];

        for (int i = 0; i < length; i++)
        {
            joiningTypesOriginal[i] = GetJoiningType(codepoints[i]);
        }

        int prevIndex = -1;
        for (int i = 0; i < length; i++)
        {
            JoiningType jt = joiningTypesOriginal[i];
            if (jt == JoiningType.T)
                continue;

            if (prevIndex >= 0)
            {
                JoiningType prevJt = joiningTypesOriginal[prevIndex];
                if (IsJoinPair(prevJt, jt))
                {
                    joinPrevOriginal[i] = true;
                }
            }

            prevIndex = i;
        }

        var newCodepoints = new List<int>(length);
        var newIndexMap = new List<int>(length);

        for (int i = 0; i < length;)
        {
            int cp = codepoints[i];

            if (cp == 0x0644 && i + 1 < length)
            {
                int j = i + 1;

                while (j < length)
                {
                    int cpj = codepoints[j];
                    var jt = joiningTypesOriginal[j];

                    if (jt == JoiningType.T)
                    {
                        j++;
                        continue;
                    }

                    if (TryGetLamAlefLigature(joinPrevOriginal[i], cpj, out var ligatureCp))
                    {
                        newCodepoints.Add(ligatureCp);
                        newIndexMap.Add(i);

                        for (int k = i + 1; k < j; k++)
                        {
                            int cpk = codepoints[k];
                            newCodepoints.Add(cpk);
                            newIndexMap.Add(k);
                        }

                        i = j + 1;
                        goto ContinueOuter;
                    }

                    break;
                }
            }

            newCodepoints.Add(cp);
            newIndexMap.Add(i);
            i++;

            ContinueOuter: ;
        }


        int newLen = newCodepoints.Count;
        var output = new int[newLen];
        indexMap = new int[newLen];

        newCodepoints.CopyTo(output, 0);
        newIndexMap.CopyTo(indexMap, 0);

        bool hasArabicAfterLig = false;
        for (int i = 0; i < newLen; i++)
        {
            if (IsArabicScript(output[i]))
            {
                hasArabicAfterLig = true;
                break;
            }
        }

        if (!hasArabicAfterLig)
            return output;

        var joiningTypes = new JoiningType[newLen];
        var joinPrev = new bool[newLen];
        var joinNext = new bool[newLen];

        for (int i = 0; i < newLen; i++)
        {
            joiningTypes[i] = GetJoiningType(output[i]);
        }

        prevIndex = -1;
        for (int i = 0; i < newLen; i++)
        {
            JoiningType jt = joiningTypes[i];
            if (IsTransparentForJoining(jt))
                continue;

            if (prevIndex >= 0)
            {
                JoiningType prevJt = joiningTypes[prevIndex];
                if (IsJoinPair(prevJt, jt))
                {
                    joinNext[prevIndex] = true;
                    joinPrev[i] = true;
                }
            }

            prevIndex = i;
        }

        for (int i = 0; i < newLen; i++)
        {
            int cp = output[i];

            if (!formEntries.ContainsKey(cp))
                continue;

            var jt = joiningTypes[i];
            if (jt == JoiningType.U)
                continue;

            if (!joinPrev[i])
            {
                int k = i - 1;
                while (k >= 0 && IsTransparentMark(output[k], joiningTypes[k]))
                    k--;

                if (k >= 0 && output[k] == ZWJ)
                {
                    joinPrev[i] = true;
                }
            }

            if (!joinNext[i])
            {
                int k = i + 1;
                while (k < newLen && IsTransparentMark(output[k], joiningTypes[k]))
                    k++;

                if (k < newLen && output[k] == ZWJ)
                {
                    joinNext[i] = true;
                }
            }
        }

        for (int i = 0; i < newLen; i++)
        {
            int cp = output[i];

            if (!IsArabicScript(cp))
                continue;

            if (!formEntries.TryGetValue(cp, out var forms))
                continue;

            ArabicForm form;
            if (!joinPrev[i] && !joinNext[i])
                form = ArabicForm.Isolated;
            else if (!joinPrev[i] && joinNext[i])
                form = ArabicForm.Initial;
            else if (joinPrev[i] && !joinNext[i])
                form = ArabicForm.Final;
            else
                form = ArabicForm.Medial;

            int shapedCp = forms.GetCodepoint(form);
            output[i] = shapedCp;
        }

        int count = 0;
        for (int i = 0; i < output.Length; i++)
        {
            int cp = output[i];
            if (cp == ZWJ || cp == ZWNJ)
                continue;
            count++;
        }

        if (count != output.Length)
        {
            var finalOutput = new int[count];
            var finalIndexMap = new int[count];

            int dst = 0;
            for (int i = 0; i < output.Length; i++)
            {
                int cp = output[i];
                if (cp == ZWJ || cp == ZWNJ)
                    continue;

                finalOutput[dst] = cp;
                finalIndexMap[dst] = indexMap[i];
                dst++;
            }

            output = finalOutput;
            indexMap = finalIndexMap;
        }

        return output;
    }

    public static void ComposeOriginalAndVisualMaps(
        int originalLength,
        int[] shapeIndexMap,
        int[] logicalToVisualMap,
        out int[] originalToVisual,
        out int[] visualToOriginal)
    {
        if (shapeIndexMap == null)
            throw new ArgumentNullException(nameof(shapeIndexMap));
        if (logicalToVisualMap == null)
            throw new ArgumentNullException(nameof(logicalToVisualMap));

        int shapedLength = shapeIndexMap.Length;
        if (logicalToVisualMap.Length != shapedLength)
        {
            throw new ArgumentException(
                "logicalToVisualMap length must match shapeIndexMap length. " +
                "Both maps must be for the same shaped sequence.");
        }

        if (originalLength < 0)
            throw new ArgumentOutOfRangeException(nameof(originalLength));

        originalToVisual = new int[originalLength];
        for (int i = 0; i < originalLength; i++)
            originalToVisual[i] = -1;

        visualToOriginal = new int[shapedLength];
        for (int i = 0; i < shapedLength; i++)
            visualToOriginal[i] = -1;

        for (int logicalIndex = 0; logicalIndex < shapedLength; logicalIndex++)
        {
            int originalIndex = shapeIndexMap[logicalIndex];
            int visualIndex = logicalToVisualMap[logicalIndex];

            if (visualIndex < 0 || visualIndex >= shapedLength)
                continue;

            visualToOriginal[visualIndex] = originalIndex;

            if (originalIndex >= 0 && originalIndex < originalLength)
            {
                int current = originalToVisual[originalIndex];
                if (current == -1 || visualIndex < current)
                {
                    originalToVisual[originalIndex] = visualIndex;
                }
            }
        }
    }


    private static bool TryGetLamAlefLigature(bool joinPrevLam, int alefCp, out int ligatureCp)
    {
        ligatureCp = 0;

        switch (alefCp)
        {
            case 0x0622:
                ligatureCp = joinPrevLam ? 0xFEF6 : 0xFEF5;
                return true;

            case 0x0623:
                ligatureCp = joinPrevLam ? 0xFEF8 : 0xFEF7;
                return true;

            case 0x0625:
                ligatureCp = joinPrevLam ? 0xFEFA : 0xFEF9;
                return true;

            case 0x0627:
                ligatureCp = joinPrevLam ? 0xFEFC : 0xFEFB;
                return true;

            default:
                return false;
        }
    }

    private static bool IsArabicScript(int cp)
    {
        if ((cp >= 0x0600 && cp <= 0x06FF) ||
            (cp >= 0x0750 && cp <= 0x077F) ||
            (cp >= 0x08A0 && cp <= 0x08FF) ||
            (cp >= 0xFB50 && cp <= 0xFDFF) ||
            (cp >= 0xFE70 && cp <= 0xFEFF))
        {
            return true;
        }

        return false;
    }

    private static JoiningType GetJoiningType(int codepoint)
    {
        if (codepoint == ZWJ)
        {
            return JoiningType.T;
        }

        if (codepoint == ZWNJ)
        {
            return JoiningType.U;
        }

        if (joiningTypes.TryGetValue(codepoint, out var jt))
            return jt;

        return JoiningType.U;
    }


    private static bool IsJoinPair(JoiningType prev, JoiningType curr)
    {
        if (prev == JoiningType.U || curr == JoiningType.U)
            return false;

        if (prev == JoiningType.T || curr == JoiningType.T)
            return false;

        bool prevJoinsToNext =
            prev == JoiningType.D ||
            prev == JoiningType.L ||
            prev == JoiningType.C;

        bool currJoinsToPrev =
            curr == JoiningType.D ||
            curr == JoiningType.R ||
            curr == JoiningType.C;

        return prevJoinsToNext && currJoinsToPrev;
    }

    private static Dictionary<int, JoiningType> ParseDerivedJoiningTypes(string text)
    {
        var dict = new Dictionary<int, JoiningType>();

        using (var reader = new StringReader(text))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;

                int hashIndex = line.IndexOf('#');
                if (hashIndex >= 0)
                    line = line.Substring(0, hashIndex);

                if (line.Length == 0)
                    continue;

                var parts = line.Split(';');
                if (parts.Length < 2)
                    continue;

                string rangePart = parts[0].Trim();
                string typePart = parts[1].Trim();

                if (string.IsNullOrEmpty(rangePart) || string.IsNullOrEmpty(typePart))
                    continue;

                JoiningType jt;
                switch (typePart)
                {
                    case "R": jt = JoiningType.R; break;
                    case "L": jt = JoiningType.L; break;
                    case "D": jt = JoiningType.D; break;
                    case "C": jt = JoiningType.C; break;
                    case "T": jt = JoiningType.T; break;
                    case "U": jt = JoiningType.U; break;
                    default:
                        continue;
                }

                int start, end;
                int dotDot = rangePart.IndexOf("..", StringComparison.Ordinal);
                if (dotDot >= 0)
                {
                    string startHex = rangePart.Substring(0, dotDot);
                    string endHex = rangePart.Substring(dotDot + 2);

                    if (!int.TryParse(startHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out start))
                        continue;
                    if (!int.TryParse(endHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out end))
                        continue;
                }
                else
                {
                    if (!int.TryParse(rangePart, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out start))
                        continue;
                    end = start;
                }

                for (int cp = start; cp <= end; cp++)
                {
                    dict[cp] = jt;
                }
            }
        }

        return dict;
    }

    private static Dictionary<int, FormEntry> ParseArabicContextForms(string text)
    {
        var dict = new Dictionary<int, FormEntry>();

        using (var reader = new StringReader(text))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split(';');
                if (parts.Length < 5)
                    continue;

                if (!TryParseHex(parts[0], out int baseCp))
                    continue;

                TryParseHex(parts[1], out int isoCp);
                TryParseHex(parts[2], out int finCp);
                TryParseHex(parts[3], out int initCp);
                TryParseHex(parts[4], out int medCp);

                var entry = new FormEntry(isoCp, finCp, initCp, medCp);
                dict[baseCp] = entry;
            }
        }

        return dict;
    }


    private static bool TryParseHex(string s, out int value)
    {
        value = 0;
        if (string.IsNullOrEmpty(s))
            return false;

        s = s.Trim();
        if (s == "0" || s == "0000")
        {
            value = 0;
            return true;
        }

        return int.TryParse(s,
            NumberStyles.HexNumber,
            CultureInfo.InvariantCulture,
            out value);
    }

    private static bool IsTransparentForJoining(JoiningType jt)
    {
        return jt == JoiningType.T;
    }

    private static bool IsTransparentMark(int codepoint, JoiningType jt)
    {
        return jt == JoiningType.T && codepoint != ZWJ;
    }
}