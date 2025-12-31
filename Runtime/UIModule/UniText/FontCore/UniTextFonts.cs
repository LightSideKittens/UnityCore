using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UniTextFonts", menuName = "UniText/Fonts")]
public class UniTextFonts : ScriptableObject
{
    public List<UniTextFont> fonts = new();
    public UniTextFont MainFont => fonts is { Count: > 0 } ? fonts[0] : null;

    public UniTextFont FindFontForCodepoint(uint unicode, HashSet<int> searched = null)
    {
        if (fonts == null || fonts.Count == 0)
            return null;

        searched ??= new HashSet<int>();

        for (var i = 0; i < fonts.Count; i++)
        {
            var font = fonts[i];
            if (!searched.Add(font.GetCachedInstanceId()))
                continue;

            var glyphIndex = HarfBuzzShapingEngine.GetGlyphIndex(font, unicode);
            if (glyphIndex != 0) return font;
        }

        return null;
    }

#if UNITY_EDITOR
    public event Action Changed;

    private void OnValidate()
    {
        for (var i = 0; i < fonts.Count; i++)
        {
            fonts[i].Changed -= CallChanged;
            fonts[i].Changed += CallChanged;
        }
    }

    private void OnDestroy()
    {
        for (var i = 0; i < fonts.Count; i++)
        {
            fonts[i].Changed -= CallChanged;
        }
    }

    private void CallChanged()
    {
        Changed?.Invoke();
    }
#endif
}