using System.Collections.Generic;
using UnityEngine;


public sealed class UniTextSettings : ScriptableObject
{
    private const string ResourcePath = "UniTextSettings";

    [SerializeField] private TextAsset unicodeDataAsset;

    [Header("Default Font")] [SerializeField] [Tooltip("Default font used when UniText has no font assigned")]
    private UniTextFontAsset defaultFontAsset;

    [Header("Font Fallback")]
    [SerializeField]
    [Tooltip("Global fallback font assets used when character is not found in primary font or its fallbacks")]
    private List<UniTextFontAsset> fallbackFontAssets;


    public TextAsset UnicodeDataAsset => unicodeDataAsset;


    public static UniTextFontAsset DefaultFontAsset => Instance?.defaultFontAsset;


    public static List<UniTextFontAsset> FallbackFontAssets
    {
        get => Instance?.fallbackFontAssets;
        set
        {
            if (Instance != null)
                Instance.fallbackFontAssets = value;
        }
    }

    private static UniTextSettings instance;


    public static UniTextSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<UniTextSettings>(ResourcePath);

                if (instance == null)
                    Debug.LogError(
                        $"UniTextUnicodeSettings not found at Resources/{ResourcePath}.asset. " +
                        "Create it via Assets > Create > UniText > Unicode Settings and place in Resources folder.");
            }

            return instance;
        }
    }


    public static void SetInstance(UniTextSettings settings)
    {
        instance = settings;
    }
}