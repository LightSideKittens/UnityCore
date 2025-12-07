using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Shared static буферы для TextProcessor. Thread-safe для main thread UI rebuild.
/// </summary>
public static class SharedTextBuffers
{
    // Increased initial sizes to reduce allocations on first few frames
    // Most UI text is 100-500 characters, so 1024 is a good starting point
    public static int[] codepoints = new int[1024];
    public static int codepointCount;
    public static readonly List<TextAttributeBase> attributes = new(32);

    public static byte[] bidiLevels = new byte[1024];
    public static BidiParagraph[] bidiParagraphs = Array.Empty<BidiParagraph>();
    public static TextDirection baseDirection;

    public static UnicodeScript[] scripts = new UnicodeScript[1024];

    public static TextRun[] runs = new TextRun[64];
    public static int runCount;

    public static ShapedRun[] shapedRuns = new ShapedRun[64];
    public static int shapedRunCount;
    public static ShapedGlyph[] shapedGlyphs = new ShapedGlyph[1024];
    public static int shapedGlyphCount;

    public static TextLine[] lines = new TextLine[32];
    public static int lineCount;
    public static ShapedRun[] orderedRuns = new ShapedRun[64];
    public static int orderedRunCount;

    public static PositionedGlyph[] positionedGlyphs = new PositionedGlyph[1024];
    public static int positionedGlyphCount;

    public static void Reset()
    {
        codepointCount = 0;
        runCount = 0;
        shapedRunCount = 0;
        shapedGlyphCount = 0;
        lineCount = 0;
        orderedRunCount = 0;
        positionedGlyphCount = 0;
        bidiParagraphs = Array.Empty<BidiParagraph>();
        baseDirection = TextDirection.LeftToRight;
        attributes.Clear();
    }

    public static void EnsureCodepointCapacity(int required)
    {
        if (codepoints.Length < required)
            Array.Resize(ref codepoints, Math.Max(required, codepoints.Length * 2));
    }

    public static void EnsureBidiCapacity(int required)
    {
        if (bidiLevels.Length < required)
            Array.Resize(ref bidiLevels, Math.Max(required, bidiLevels.Length * 2));
    }

    public static void EnsureScriptCapacity(int required)
    {
        if (scripts.Length < required)
            Array.Resize(ref scripts, Math.Max(required, scripts.Length * 2));
    }

    public static void EnsureRunCapacity(int required)
    {
        if (runs.Length < required)
            Array.Resize(ref runs, Math.Max(required, runs.Length * 2));
    }

    public static void EnsureShapedRunCapacity(int required)
    {
        if (shapedRuns.Length < required)
            Array.Resize(ref shapedRuns, Math.Max(required, shapedRuns.Length * 2));
    }

    public static void EnsureShapedGlyphCapacity(int required)
    {
        if (shapedGlyphs.Length < required)
            Array.Resize(ref shapedGlyphs, Math.Max(required, shapedGlyphs.Length * 2));
    }

    public static void EnsureLineCapacity(int required)
    {
        if (lines.Length < required)
            Array.Resize(ref lines, Math.Max(required, lines.Length * 2));
    }

    public static void EnsureOrderedRunCapacity(int required)
    {
        if (orderedRuns.Length < required)
            Array.Resize(ref orderedRuns, Math.Max(required, orderedRuns.Length * 2));
    }

    public static void EnsurePositionedGlyphCapacity(int required)
    {
        if (positionedGlyphs.Length < required)
            Array.Resize(ref positionedGlyphs, Math.Max(required, positionedGlyphs.Length * 2));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        codepoints = new int[1024];
        bidiLevels = new byte[1024];
        scripts = new UnicodeScript[1024];
        runs = new TextRun[64];
        shapedRuns = new ShapedRun[64];
        shapedGlyphs = new ShapedGlyph[1024];
        lines = new TextLine[32];
        orderedRuns = new ShapedRun[64];
        positionedGlyphs = new PositionedGlyph[1024];
        bidiParagraphs = Array.Empty<BidiParagraph>();
        attributes.Clear();
        Reset();
    }
}

/// <summary>
/// Shared static кеш для font fallback lookup.
/// </summary>
public static class SharedFontCache
{
    private readonly struct FontCacheEntry
    {
        public readonly int codepoint;
        public readonly int preferredFontId;
        public readonly int resultFontId;

        public FontCacheEntry(int codepoint, int preferredFontId, int resultFontId)
        {
            this.codepoint = codepoint;
            this.preferredFontId = preferredFontId;
            this.resultFontId = resultFontId;
        }
    }

    // Increased from 256 to 4096 for much better hit rate
    // Most text uses ~100-500 unique codepoints, so 4096 should have very high hit rate
    private const int CacheSize = 4096;
    private const int CacheMask = CacheSize - 1;
    private static FontCacheEntry[] cache = new FontCacheEntry[CacheSize];

    static SharedFontCache() => Clear();

    public static bool TryGet(int codepoint, int preferredFontId, out int resultFontId)
    {
        int index = (codepoint ^ (preferredFontId << 16)) & CacheMask;
        ref var entry = ref cache[index];

        if (entry.codepoint == codepoint && entry.preferredFontId == preferredFontId)
        {
            resultFontId = entry.resultFontId;
            return true;
        }

        resultFontId = -1;
        return false;
    }

    public static void Set(int codepoint, int preferredFontId, int resultFontId)
    {
        int index = (codepoint ^ (preferredFontId << 16)) & CacheMask;
        cache[index] = new FontCacheEntry(codepoint, preferredFontId, resultFontId);
    }

    public static void Clear()
    {
        for (int i = 0; i < CacheSize; i++)
            cache[i] = new FontCacheEntry(-1, -1, -1);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload() => Clear();
}

/// <summary>
/// Shared static пул Mesh объектов для UniText.
/// </summary>
public static class SharedMeshPool
{
    private static readonly List<Mesh> available = new(16);
    private static bool initialized;

    public static Mesh Acquire(string name)
    {
        EnsureInitialized();

        Mesh mesh;
        int count = available.Count;

        if (count > 0)
        {
            mesh = available[count - 1];
            available.RemoveAt(count - 1);
        }
        else
        {
            mesh = new Mesh();
        }

        mesh.Clear();
        mesh.name = name;
        return mesh;
    }

    public static void Release(Mesh mesh)
    {
        if (mesh == null) return;
        mesh.Clear();
        available.Add(mesh);
    }

    public static void Release(List<Mesh> meshes)
    {
        if (meshes == null) return;

        foreach (var mesh in meshes)
        {
            if (mesh != null)
            {
                mesh.Clear();
                available.Add(mesh);
            }
        }
    }

    public static void ClearUnused()
    {
        foreach (var mesh in available)
            if (mesh != null)
                UnityEngine.Object.Destroy(mesh);
        available.Clear();
    }

    public static int PoolSize => available.Count;

    private static void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private static void OnSceneUnloaded(Scene scene) => ClearUnused();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        available.Clear();
        initialized = false;
    }
}
