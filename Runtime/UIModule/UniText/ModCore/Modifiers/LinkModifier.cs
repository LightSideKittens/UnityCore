using System;
using System.Runtime.CompilerServices;
using UnityEngine;


public readonly struct LinkData
{
    public readonly int start;
    public readonly int end;
    public readonly string url;

    public LinkData(int start, int end, string url)
    {
        this.start = start;
        this.end = end;
        this.url = url;
    }

    public bool Contains(int cluster)
    {
        return cluster >= start && cluster < end;
    }
}


[Serializable]
public class LinkModifier : BaseModifier
{
    private const string AttributeKey = "links";

    [SerializeField] private Color32 linkColor = new(66, 133, 244, 255);
    [SerializeField] private bool enableUnderline = true;

    private PooledArrayAttribute<LinkData> linksAttribute;
    private ColorModifier colorModifier;
    private UnderlineModifier underlineModifier;
    private string cachedHexColor;

    public Color32 LinkColor
    {
        get => linkColor;
        set
        {
            linkColor = value;
            cachedHexColor = ColorToHex(value);
        }
    }

    private static string ColorToHex(Color32 c)
    {
        return $"#{c.r:X2}{c.g:X2}{c.b:X2}{c.a:X2}";
    }

    public bool EnableUnderline
    {
        get => enableUnderline;
        set => enableUnderline = value;
    }

    protected override void CreateBuffers()
    {
        linksAttribute = buffers.GetOrCreateAttributeData<PooledArrayAttribute<LinkData>>(AttributeKey);
        cachedHexColor = ColorToHex(linkColor);

        colorModifier = new ColorModifier();
        colorModifier.Initialize(uniText);

        underlineModifier = new UnderlineModifier();
        underlineModifier.Initialize(uniText);
    }

    protected override void Subscribe()
    {
    }

    protected override void Unsubscribe()
    {
    }

    protected override void ReleaseBuffers()
    {
        colorModifier?.Deinitialize();
        colorModifier = null;

        underlineModifier?.Deinitialize();
        underlineModifier = null;

        linksAttribute = null;
    }

    protected override void ClearBuffers()
    {
        colorModifier?.Reset();
        underlineModifier?.Reset();
    }

    protected override void OnApply(int start, int end, string parameter)
    {
        if (string.IsNullOrEmpty(parameter)) return;

        linksAttribute.Add(new LinkData(start, end, parameter));

        colorModifier.Apply(start, end, cachedHexColor);

        if (enableUnderline) underlineModifier.Apply(start, end, parameter);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetLinkUrl(UniTextBuffers buffers, int cluster)
    {
        var attr = buffers?.GetAttributeData<PooledArrayAttribute<LinkData>>(AttributeKey);
        if (attr == null) return null;

        for (var i = 0; i < attr.Count; i++)
            if (attr[i].Contains(cluster))
                return attr[i].url;
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLink(UniTextBuffers buffers, int cluster)
    {
        var attr = buffers?.GetAttributeData<PooledArrayAttribute<LinkData>>(AttributeKey);
        if (attr == null) return false;

        for (var i = 0; i < attr.Count; i++)
            if (attr[i].Contains(cluster))
                return true;
        return false;
    }

    public static bool TryGetLinkData(UniTextBuffers buffers, int cluster, out LinkData linkData)
    {
        var attr = buffers?.GetAttributeData<PooledArrayAttribute<LinkData>>(AttributeKey);
        if (attr != null)
        {
            for (var i = 0; i < attr.Count; i++)
                if (attr[i].Contains(cluster))
                {
                    linkData = attr[i];
                    return true;
                }
        }

        linkData = default;
        return false;
    }
}