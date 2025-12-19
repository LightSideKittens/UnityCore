using System;
using System.Collections.Generic;
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
    [SerializeField] private Color32 linkColor = new(66, 133, 244, 255);

    [SerializeField] private bool enableUnderline = true;

    private readonly List<LinkData> links = new(8);
    private static LinkModifier current;

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

    public IReadOnlyList<LinkData> Links => links;

    protected override void CreateBuffers()
    {
        current = this;
        cachedHexColor = ColorToHex(linkColor);

        colorModifier = new ColorModifier();
        colorModifier.Initialize(uniText);

        underlineModifier = new UnderlineModifier();
        underlineModifier.Initialize(uniText);
    }

    protected override void Subscribe()
    {
        uniText.Rebuilding += OnRebuilding;
    }

    protected override void Unsubscribe()
    {
        uniText.Rebuilding -= OnRebuilding;
    }

    protected override void ReleaseBuffers()
    {
        if (current == this) current = null;

        colorModifier?.Deinitialize();
        colorModifier = null;

        underlineModifier?.Deinitialize();
        underlineModifier = null;
    }

    protected override void ClearBuffers()
    {
        colorModifier.Reset();
        underlineModifier.Reset();
        links.Clear();
    }

    private void OnRebuilding()
    {
        current = this;
    }

    protected override void ApplyModifier(int start, int end, string parameter)
    {
        if (string.IsNullOrEmpty(parameter)) return;

        links.Add(new LinkData(start, end, parameter));

        colorModifier.Apply(start, end, cachedHexColor);

        if (enableUnderline) underlineModifier.Apply(start, end, parameter);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetLinkUrl(int cluster)
    {
        if (current == null) return null;
        var links = current.links;
        for (var i = 0; i < links.Count; i++)
            if (links[i].Contains(cluster))
                return links[i].url;
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLink(int cluster)
    {
        if (current == null) return false;
        var links = current.links;
        for (var i = 0; i < links.Count; i++)
            if (links[i].Contains(cluster))
                return true;
        return false;
    }

    public static bool TryGetLinkData(int cluster, out LinkData linkData)
    {
        if (current != null)
        {
            var links = current.links;
            for (var i = 0; i < links.Count; i++)
                if (links[i].Contains(cluster))
                {
                    linkData = links[i];
                    return true;
                }
        }

        linkData = default;
        return false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        current = null;
    }
}