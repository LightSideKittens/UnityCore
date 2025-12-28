using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(fileName = "UniTextAppearance", menuName = "UniText/Appearance")]
public class UniTextAppearance : ScriptableObject, ISerializationCallbackReceiver
{
    [Serializable]
    public struct FontMaterialPair
    {
        public UniTextFont font;
        public Material material;
    }

    [SerializeField] private Material defaultMaterial;
    [SerializeField] private FontMaterialPair[] fontMaterials;

    private Dictionary<int, Material> materialByFontId;

    public Material DefaultMaterial => defaultMaterial;

#if UNITY_EDITOR
    public event Action Changed;
    private void OnValidate()
    {
        Changed?.Invoke();
        RebuildLookup();
    }
#endif

    private void RebuildLookup()
    {
        materialByFontId ??= new Dictionary<int, Material>();
        materialByFontId.Clear();

        if (fontMaterials == null) return;

        for (var i = 0; i < fontMaterials.Length; i++)
        {
            var pair = fontMaterials[i];
            if (pair.font != null && pair.material != null)
                materialByFontId[pair.font.GetCachedInstanceId()] = pair.material;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Material GetMaterial(UniTextFont font)
    {
        if (font == null)
            return defaultMaterial;

        if (materialByFontId == null)
            RebuildLookup();

        return materialByFontId.TryGetValue(font.GetCachedInstanceId(), out var mat) ? mat : defaultMaterial;
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize() { }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        RebuildLookup();
    }
}