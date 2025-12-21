using UnityEngine;


public sealed class UniTextSettings : ScriptableObject
{
    private const string ResourcePath = "UniTextSettings";

    [SerializeField] private TextAsset unicodeDataAsset;

#if UNITY_EDITOR
    [Header("Editor Defaults")]
    [SerializeField] [Tooltip("Default fonts assigned to new UniText components")]
    private UniTextFonts defaultFonts;

    [SerializeField] [Tooltip("Default appearance assigned to new UniText components")]
    private UniTextAppearance defaultAppearance;

    public static UniTextFonts DefaultFonts => Instance?.defaultFonts;
    public static UniTextAppearance DefaultAppearance => Instance?.defaultAppearance;
#endif

    public TextAsset UnicodeDataAsset => unicodeDataAsset;

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
                        $"UniTextSettings not found at Resources/{ResourcePath}.asset. " +
                        "Create it via Assets > Create > UniText > Settings and place in Resources folder.");
            }

            return instance;
        }
    }

    public static void SetInstance(UniTextSettings settings)
    {
        instance = settings;
    }
}