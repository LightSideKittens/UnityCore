using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.TextCore;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable 0649

namespace TMPro
{
    [Serializable]
    [ExcludeFromPresetAttribute]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/TextMeshPro/Settings.html")]
    public class TMP_Settings : ScriptableObject
    {
        private static TMP_Settings s_Instance;

        public static string version
        {
            get { return "1.4.0"; }
        }

        [SerializeField] internal string assetVersion;

        internal static string s_CurrentAssetVersion = "2";

        internal void SetAssetVersion()
        {
            assetVersion = s_CurrentAssetVersion;
        }

        public static TextWrappingModes textWrappingMode
        {
            get { return instance.m_TextWrappingMode; }
        }

        [FormerlySerializedAs("m_enableWordWrapping")] [SerializeField]
        private TextWrappingModes m_TextWrappingMode;

        [SerializeField] private bool m_enableKerning;

        public static List<OTL_FeatureTag> fontFeatures
        {
            get { return instance.m_ActiveFontFeatures; }
        }

        [SerializeField] private List<OTL_FeatureTag> m_ActiveFontFeatures = new() { 0 };

        public static bool enableExtraPadding
        {
            get { return instance.m_enableExtraPadding; }
        }

        [SerializeField] private bool m_enableExtraPadding;

        public static bool enableTintAllSprites
        {
            get { return instance.m_enableTintAllSprites; }
        }

        [SerializeField] private bool m_enableTintAllSprites;

        public static bool enableParseEscapeCharacters
        {
            get { return instance.m_enableParseEscapeCharacters; }
        }

        [SerializeField] private bool m_enableParseEscapeCharacters;

        public static bool enableRaycastTarget
        {
            get { return instance.m_EnableRaycastTarget; }
        }

        [SerializeField] private bool m_EnableRaycastTarget = true;

        public static bool getFontFeaturesAtRuntime
        {
            get { return instance.m_GetFontFeaturesAtRuntime; }
        }

        [SerializeField] private bool m_GetFontFeaturesAtRuntime = true;

        public static int missingGlyphCharacter
        {
            get { return instance.m_missingGlyphCharacter; }
            set { instance.m_missingGlyphCharacter = value; }
        }

        [SerializeField] private int m_missingGlyphCharacter;

        public static bool clearDynamicDataOnBuild
        {
            get { return instance.m_ClearDynamicDataOnBuild; }
        }

        [SerializeField] private bool m_ClearDynamicDataOnBuild = true;

        public static bool warningsDisabled
        {
            get { return instance.m_warningsDisabled; }
        }

        [SerializeField] private bool m_warningsDisabled;

        public static TMP_FontAsset defaultFontAsset
        {
            get { return instance.m_defaultFontAsset; }
            set { instance.m_defaultFontAsset = value; }
        }

        [SerializeField] private TMP_FontAsset m_defaultFontAsset;

        public static string defaultFontAssetPath
        {
            get { return instance.m_defaultFontAssetPath; }
        }

        [SerializeField] private string m_defaultFontAssetPath;

        public static float defaultFontSize
        {
            get { return instance.m_defaultFontSize; }
        }

        [SerializeField] private float m_defaultFontSize;

        public static float defaultTextAutoSizingMinRatio
        {
            get { return instance.m_defaultAutoSizeMinRatio; }
        }

        [SerializeField] private float m_defaultAutoSizeMinRatio;

        public static float defaultTextAutoSizingMaxRatio
        {
            get { return instance.m_defaultAutoSizeMaxRatio; }
        }

        [SerializeField] private float m_defaultAutoSizeMaxRatio;

        public static Vector2 defaultTextMeshProTextContainerSize
        {
            get { return instance.m_defaultTextMeshProTextContainerSize; }
        }

        [SerializeField] private Vector2 m_defaultTextMeshProTextContainerSize;

        public static Vector2 defaultTextMeshProUITextContainerSize
        {
            get { return instance.m_defaultTextMeshProUITextContainerSize; }
        }

        [SerializeField] private Vector2 m_defaultTextMeshProUITextContainerSize;

        public static List<TMP_FontAsset> fallbackFontAssets
        {
            get { return instance.m_fallbackFontAssets; }
            set { instance.m_fallbackFontAssets = value; }
        }

        [SerializeField] private List<TMP_FontAsset> m_fallbackFontAssets;

        public static bool matchMaterialPreset
        {
            get { return instance.m_matchMaterialPreset; }
        }

        [SerializeField] private bool m_matchMaterialPreset;

        public static bool hideSubTextObjects
        {
            get { return instance.m_HideSubTextObjects; }
        }

        [SerializeField] private bool m_HideSubTextObjects = true;

        public static string defaultSpriteAssetPath
        {
            get { return instance.m_defaultSpriteAssetPath; }
        }

        [SerializeField] private string m_defaultSpriteAssetPath;

        public static uint missingCharacterSpriteUnicode
        {
            get { return instance.m_MissingCharacterSpriteUnicode; }
            set { instance.m_MissingCharacterSpriteUnicode = value; }
        }

        [SerializeField] private uint m_MissingCharacterSpriteUnicode;


        public static string defaultColorGradientPresetsPath
        {
            get { return instance.m_defaultColorGradientPresetsPath; }
        }

        [SerializeField] private string m_defaultColorGradientPresetsPath;

        public static TMP_StyleSheet defaultStyleSheet
        {
            get { return instance.m_defaultStyleSheet; }
            set { instance.m_defaultStyleSheet = value; }
        }

        [SerializeField] private TMP_StyleSheet m_defaultStyleSheet;

        public static string styleSheetsResourcePath
        {
            get { return instance.m_StyleSheetsResourcePath; }
        }

        [SerializeField] private string m_StyleSheetsResourcePath;

        public static TextAsset leadingCharacters
        {
            get { return instance.m_leadingCharacters; }
        }

        [SerializeField] private TextAsset m_leadingCharacters;

        public static TextAsset followingCharacters
        {
            get { return instance.m_followingCharacters; }
        }

        [SerializeField] private TextAsset m_followingCharacters;

        public static LineBreakingTable linebreakingRules
        {
            get
            {
                if (instance.m_linebreakingRules == null)
                    LoadLinebreakingRules();

                return instance.m_linebreakingRules;
            }
        }

        [SerializeField] private LineBreakingTable m_linebreakingRules;

        public static bool useModernHangulLineBreakingRules
        {
            get { return instance.m_UseModernHangulLineBreakingRules; }
            set { instance.m_UseModernHangulLineBreakingRules = value; }
        }

        [SerializeField] private bool m_UseModernHangulLineBreakingRules;

        public static TMP_Settings instance
        {
            get
            {
                if (isTMPSettingsNull)
                {
                    s_Instance = Resources.Load<TMP_Settings>("TMP Settings");

#if UNITY_EDITOR
                    if (isTMPSettingsNull && Time.frameCount != 0 ||
                        (!isTMPSettingsNull && s_Instance.assetVersion != s_CurrentAssetVersion))
                    {
                        DelayShowPackageImporterWindow();
                    }
#endif

                    if (!isTMPSettingsNull && s_Instance.m_ActiveFontFeatures.Count == 1 &&
                        s_Instance.m_ActiveFontFeatures[0] == 0)
                    {
                        s_Instance.m_ActiveFontFeatures.Clear();

                        if (s_Instance.m_enableKerning)
                            s_Instance.m_ActiveFontFeatures.Add(OTL_FeatureTag.kern);
                    }
                }

                return s_Instance;
            }
        }

        internal static bool isTMPSettingsNull
        {
            get { return s_Instance == null; }
        }

#if UNITY_EDITOR
        public static async void DelayShowPackageImporterWindow()
        {
            await Task.Delay(TimeSpan.FromSeconds(1f));
            TMP_PackageResourceImporterWindow.ShowPackageImporterWindow();
        }
#endif


        public static TMP_Settings LoadDefaultSettings()
        {
            if (s_Instance == null)
            {
                TMP_Settings settings = Resources.Load<TMP_Settings>("TMP Settings");
                if (settings != null)
                    s_Instance = settings;
            }

            return s_Instance;
        }


        public static TMP_Settings GetSettings()
        {
            if (instance == null) return null;

            return instance;
        }


        public static TMP_FontAsset GetFontAsset()
        {
            if (instance == null) return null;

            return instance.m_defaultFontAsset;
        }


        public static TMP_StyleSheet GetStyleSheet()
        {
            if (instance == null) return null;

            return instance.m_defaultStyleSheet;
        }


        public static void LoadLinebreakingRules()
        {
            if (instance == null) return;

            if (s_Instance.m_linebreakingRules == null)
                s_Instance.m_linebreakingRules = new();

            s_Instance.m_linebreakingRules.leadingCharacters = GetCharacters(s_Instance.m_leadingCharacters);
            s_Instance.m_linebreakingRules.followingCharacters = GetCharacters(s_Instance.m_followingCharacters);
        }


        private static HashSet<uint> GetCharacters(TextAsset file)
        {
            HashSet<uint> dict = new();
            string text = file.text;

            for (int i = 0; i < text.Length; i++)
            {
                dict.Add(text[i]);
            }

            return dict;
        }


        public class LineBreakingTable
        {
            public HashSet<uint> leadingCharacters;
            public HashSet<uint> followingCharacters;
        }
    }
}