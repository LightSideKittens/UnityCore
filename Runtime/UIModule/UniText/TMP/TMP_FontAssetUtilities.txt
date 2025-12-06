using System.Collections.Generic;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;


namespace TMPro
{
    public class TMP_FontAssetUtilities
    {
        private static readonly TMP_FontAssetUtilities s_Instance = new();

        static TMP_FontAssetUtilities()
        {
        }


        public static TMP_FontAssetUtilities instance
        {
            get { return s_Instance; }
        }


        private static HashSet<int> k_SearchedAssets;


        public static TMP_Character GetCharacterFromFontAsset(uint unicode, TMP_FontAsset sourceFontAsset,
            bool includeFallbacks, FontStyles fontStyle, FontWeight fontWeight, out bool isAlternativeTypeface)
        {
            if (includeFallbacks)
            {
                if (k_SearchedAssets == null)
                    k_SearchedAssets = new();
                else
                    k_SearchedAssets.Clear();
            }

            return GetCharacterFromFontAsset_Internal(unicode, sourceFontAsset, includeFallbacks, fontStyle, fontWeight,
                out isAlternativeTypeface);
        }


        private static TMP_Character GetCharacterFromFontAsset_Internal(uint unicode, TMP_FontAsset sourceFontAsset,
            bool includeFallbacks, FontStyles fontStyle, FontWeight fontWeight, out bool isAlternativeTypeface)
        {
            isAlternativeTypeface = false;
            TMP_Character character;

            #region FONT WEIGHT AND FONT STYLE HANDLING

            bool isItalic = (fontStyle & FontStyles.Italic) == FontStyles.Italic;

            if (isItalic || fontWeight != FontWeight.Regular)
            {
                uint compositeUnicodeLookupKey =
                    ((0x80u | ((uint)fontStyle << 4) | ((uint)fontWeight / 100)) << 24) | unicode;
                if (sourceFontAsset.characterLookupTable.TryGetValue(compositeUnicodeLookupKey, out character))
                {
                    isAlternativeTypeface = true;

                    if (character.textAsset != null)
                        return character;

                    sourceFontAsset.characterLookupTable.Remove(unicode);
                }

                TMP_FontWeightPair[] fontWeights = sourceFontAsset.fontWeightTable;

                int fontWeightIndex = 4;
                switch (fontWeight)
                {
                    case FontWeight.Thin:
                        fontWeightIndex = 1;
                        break;
                    case FontWeight.ExtraLight:
                        fontWeightIndex = 2;
                        break;
                    case FontWeight.Light:
                        fontWeightIndex = 3;
                        break;
                    case FontWeight.Regular:
                        fontWeightIndex = 4;
                        break;
                    case FontWeight.Medium:
                        fontWeightIndex = 5;
                        break;
                    case FontWeight.SemiBold:
                        fontWeightIndex = 6;
                        break;
                    case FontWeight.Bold:
                        fontWeightIndex = 7;
                        break;
                    case FontWeight.Heavy:
                        fontWeightIndex = 8;
                        break;
                    case FontWeight.Black:
                        fontWeightIndex = 9;
                        break;
                }

                TMP_FontAsset temp = isItalic
                    ? fontWeights[fontWeightIndex].italicTypeface
                    : fontWeights[fontWeightIndex].regularTypeface;

                if (temp != null)
                {
                    if (temp.characterLookupTable.TryGetValue(unicode, out character))
                    {
                        if (character.textAsset != null)
                        {
                            isAlternativeTypeface = true;
                            return character;
                        }

                        temp.characterLookupTable.Remove(unicode);
                    }

                    if (temp.atlasPopulationMode == AtlasPopulationMode.Dynamic ||
                        temp.atlasPopulationMode == AtlasPopulationMode.DynamicOS)
                    {
                        if (temp.TryAddCharacterInternal(unicode, out character))
                        {
                            isAlternativeTypeface = true;

                            return character;
                        }
                    }
                }

                if (includeFallbacks && sourceFontAsset.fallbackFontAssetTable != null)
                    return SearchFallbacksForCharacter(unicode, sourceFontAsset, fontStyle, fontWeight,
                        out isAlternativeTypeface);

                return null;
            }

            #endregion

            if (sourceFontAsset.characterLookupTable.TryGetValue(unicode, out character))
            {
                if (character.textAsset != null)
                    return character;

                sourceFontAsset.characterLookupTable.Remove(unicode);
            }

            if (sourceFontAsset.atlasPopulationMode == AtlasPopulationMode.Dynamic ||
                sourceFontAsset.atlasPopulationMode == AtlasPopulationMode.DynamicOS)
            {
                if (sourceFontAsset.TryAddCharacterInternal(unicode, out character))
                    return character;
            }

            if (includeFallbacks && sourceFontAsset.fallbackFontAssetTable != null)
                return SearchFallbacksForCharacter(unicode, sourceFontAsset, fontStyle, fontWeight,
                    out isAlternativeTypeface);

            return null;
        }

        private static TMP_Character SearchFallbacksForCharacter(uint unicode, TMP_FontAsset sourceFontAsset,
            FontStyles fontStyle, FontWeight fontWeight, out bool isAlternativeTypeface)
        {
            isAlternativeTypeface = false;

            List<TMP_FontAsset> fallbackFontAssets = sourceFontAsset.fallbackFontAssetTable;
            int fallbackCount = fallbackFontAssets.Count;

            if (fallbackCount == 0)
                return null;

            for (int i = 0; i < fallbackCount; i++)
            {
                TMP_FontAsset temp = fallbackFontAssets[i];

                if (temp == null)
                    continue;

                int id = temp.instanceID;

                if (!k_SearchedAssets.Add(id))
                    continue;

                TMP_Character character = GetCharacterFromFontAsset_Internal(unicode, temp, true, fontStyle, fontWeight,
                    out isAlternativeTypeface);

                if (character != null)
                    return character;
            }

            return null;
        }


        public static TMP_Character GetCharacterFromFontAssets(uint unicode, TMP_FontAsset sourceFontAsset,
            List<TMP_FontAsset> fontAssets, bool includeFallbacks, FontStyles fontStyle, FontWeight fontWeight,
            out bool isAlternativeTypeface)
        {
            isAlternativeTypeface = false;

            if (fontAssets == null || fontAssets.Count == 0)
                return null;

            if (includeFallbacks)
            {
                if (k_SearchedAssets == null)
                    k_SearchedAssets = new();
                else
                    k_SearchedAssets.Clear();
            }

            int fontAssetCount = fontAssets.Count;

            for (int i = 0; i < fontAssetCount; i++)
            {
                TMP_FontAsset fontAsset = fontAssets[i];

                if (fontAsset == null) continue;

                TMP_Character character = GetCharacterFromFontAsset_Internal(unicode, fontAsset, includeFallbacks,
                    fontStyle, fontWeight, out isAlternativeTypeface);

                if (character != null)
                    return character;
            }

            return null;
        }

        internal static TMP_TextElement GetTextElementFromTextAssets(uint unicode, TMP_FontAsset sourceFontAsset,
            List<TMP_Asset> textAssets, bool includeFallbacks, FontStyles fontStyle, FontWeight fontWeight,
            out bool isAlternativeTypeface)
        {
            isAlternativeTypeface = false;

            if (textAssets == null || textAssets.Count == 0)
                return null;

            if (includeFallbacks)
            {
                if (k_SearchedAssets == null)
                    k_SearchedAssets = new();
                else
                    k_SearchedAssets.Clear();
            }

            int textAssetCount = textAssets.Count;

            for (int i = 0; i < textAssetCount; i++)
            {
                TMP_Asset textAsset = textAssets[i];

                if (textAsset == null) continue;

                TMP_FontAsset fontAsset = textAsset as TMP_FontAsset;
                TMP_Character character = GetCharacterFromFontAsset_Internal(unicode, fontAsset, includeFallbacks,
                    fontStyle, fontWeight, out isAlternativeTypeface);

                if (character != null)
                    return character;
            }

            return null;
        }


        internal static uint GetCodePoint(string text, ref int index)
        {
            char c = text[index];
            if (char.IsHighSurrogate(c)
                && index + 1 < text.Length
                && char.IsLowSurrogate(text[index + 1]))
            {
                uint cp = (uint)char.ConvertToUtf32(c, text[index + 1]);
                index++;
                return cp;
            }

            return c;
        }


        internal static uint GetCodePoint(uint[] codesPoints, ref int index)
        {
            char c = (char)codesPoints[index];
            if (char.IsHighSurrogate(c)
                && index + 1 < codesPoints.Length
                && char.IsLowSurrogate((char)codesPoints[index + 1]))
            {
                uint cp = (uint)char.ConvertToUtf32(c, (char)codesPoints[index + 1]);
                index++;
                return cp;
            }

            return c;
        }
    }
}