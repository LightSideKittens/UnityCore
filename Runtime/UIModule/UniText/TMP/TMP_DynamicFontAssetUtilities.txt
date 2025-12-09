using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;


namespace TMPro
{
    internal class TMP_DynamicFontAssetUtilities
    {
        private static TMP_DynamicFontAssetUtilities s_Instance = new();

        private Dictionary<ulong, FontReference> s_SystemFontLookup;
        private string[] s_SystemFontPaths;
        private uint s_RegularStyleNameHashCode = 1291372090;

        public struct FontReference
        {
            public string familyName;
            public string styleName;
            public int faceIndex;
            public string filePath;
            public ulong hashCode;


            public FontReference(string fontFilePath, string faceNameAndStyle, int index)
            {
                familyName = null;
                styleName = null;
                faceIndex = index;
                uint familyNameHashCode = 0;
                uint styleNameHashCode = 0;
                filePath = fontFilePath;

                int length = faceNameAndStyle.Length;
                char[] conversionArray = new char[length];

                int readingFlag = 0;
                int writingIndex = 0;

                for (int i = 0; i < length; i++)
                {
                    char c = faceNameAndStyle[i];

                    if (readingFlag == 0)
                    {
                        bool isSeparator = i + 2 < length && c == ' ' && faceNameAndStyle[i + 1] == '-' &&
                                           faceNameAndStyle[i + 2] == ' ';

                        if (isSeparator)
                        {
                            readingFlag = 1;
                            familyName = new(conversionArray, 0, writingIndex);
                            i += 2;
                            writingIndex = 0;
                            continue;
                        }

                        familyNameHashCode = (familyNameHashCode << 5) + familyNameHashCode ^
                                             TMP_TextUtilities.ToUpperFast(c);
                        conversionArray[writingIndex++] = c;
                        continue;
                    }

                    if (readingFlag == 1)
                    {
                        styleNameHashCode = (styleNameHashCode << 5) + styleNameHashCode ^
                                            TMP_TextUtilities.ToUpperFast(c);
                        conversionArray[writingIndex++] = c;

                        if (i + 1 == length)
                            styleName = new(conversionArray, 0, writingIndex);
                    }
                }

                hashCode = (ulong)styleNameHashCode << 32 | familyNameHashCode;
            }
        }


        private void InitializeSystemFontReferenceCache()
        {
            if (s_SystemFontLookup == null)
                s_SystemFontLookup = new();
            else
                s_SystemFontLookup.Clear();

            if (s_SystemFontPaths == null)
                s_SystemFontPaths = Font.GetPathsToOSFonts();

            for (int i = 0; i < s_SystemFontPaths.Length; i++)
            {
                FontEngineError error = FontEngine.LoadFontFace(s_SystemFontPaths[i]);
                if (error != FontEngineError.Success)
                {
                    Debug.LogWarning("Error [" + error + "] trying to load the font at path [" + s_SystemFontPaths[i] +
                                     "].");
                    continue;
                }

                string[] fontFaces = FontEngine.GetFontFaces();

                for (int j = 0; j < fontFaces.Length; j++)
                {
                    FontReference fontRef = new(s_SystemFontPaths[i], fontFaces[j], j);

                    if (s_SystemFontLookup.ContainsKey(fontRef.hashCode))
                    {
                        continue;
                    }

                    s_SystemFontLookup.Add(fontRef.hashCode, fontRef);

                    Debug.Log("[" + i + "] Family Name [" + fontRef.familyName + "]   Style Name [" +
                              fontRef.styleName + "]   Index [" + fontRef.faceIndex + "]   HashCode [" +
                              fontRef.hashCode + "]    Path [" + fontRef.filePath + "].");
                }

                FontEngine.UnloadFontFace();
            }
        }


        public static bool TryGetSystemFontReference(string familyName, out FontReference fontRef)
        {
            return s_Instance.TryGetSystemFontReferenceInternal(familyName, null, out fontRef);
        }


        public static bool TryGetSystemFontReference(string familyName, string styleName, out FontReference fontRef)
        {
            return s_Instance.TryGetSystemFontReferenceInternal(familyName, styleName, out fontRef);
        }

        private bool TryGetSystemFontReferenceInternal(string familyName, string styleName, out FontReference fontRef)
        {
            if (s_SystemFontLookup == null)
                InitializeSystemFontReferenceCache();

            fontRef = new();

            uint familyNameHashCode = TMP_TextUtilities.GetHashCodeCaseInSensitive(familyName);
            uint styleNameHashCode = string.IsNullOrEmpty(styleName)
                ? s_RegularStyleNameHashCode
                : TMP_TextUtilities.GetHashCodeCaseInSensitive(styleName);
            ulong key = (ulong)styleNameHashCode << 32 | familyNameHashCode;

            if (s_SystemFontLookup.ContainsKey(key))
            {
                fontRef = s_SystemFontLookup[key];
                return true;
            }

            if (styleNameHashCode != s_RegularStyleNameHashCode)
                return false;

            foreach (KeyValuePair<ulong, FontReference> pair in s_SystemFontLookup)
            {
                if (pair.Value.familyName == familyName)
                {
                    fontRef = pair.Value;
                    return true;
                }
            }

            return false;
        }
    }
}