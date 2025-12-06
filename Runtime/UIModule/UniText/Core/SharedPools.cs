using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Shared static буферы для TextProcessor.
/// Все UI rebuild'ы происходят на main thread последовательно,
/// поэтому можно безопасно использовать общие буферы.
/// </summary>
public static class SharedTextBuffers
{
    // Parsing buffers
    public static int[] codepoints = new int[256];
    public static int codepointCount;
    public static readonly List<TextAttributeBase> attributes = new(32);

    // BiDi buffers
    public static byte[] bidiLevels = new byte[256];
    public static BidiParagraph[] bidiParagraphs = Array.Empty<BidiParagraph>();
    public static TextDirection baseDirection;

    // Script analysis
    public static UnicodeScript[] scripts = new UnicodeScript[256];

    // Itemization
    public static TextRun[] runs = new TextRun[32];
    public static int runCount;

    // Shaping
    public static ShapedRun[] shapedRuns = new ShapedRun[32];
    public static int shapedRunCount;
    public static ShapedGlyph[] shapedGlyphs = new ShapedGlyph[256];
    public static int shapedGlyphCount;

    // Line breaking
    public static TextLine[] lines = new TextLine[16];
    public static int lineCount;
    public static ShapedRun[] orderedRuns = new ShapedRun[32];
    public static int orderedRunCount;

    // Layout
    public static PositionedGlyph[] positionedGlyphs = new PositionedGlyph[256];
    public static int positionedGlyphCount;

    /// <summary>
    /// Сбросить все счётчики перед новой обработкой.
    /// </summary>
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

    #region Capacity Management

    public static void EnsureCodepointCapacity(int required)
    {
        if (codepoints.Length >= required) return;
        Array.Resize(ref codepoints, Math.Max(required, codepoints.Length * 2));
    }

    public static void EnsureBidiCapacity(int required)
    {
        if (bidiLevels.Length >= required) return;
        Array.Resize(ref bidiLevels, Math.Max(required, bidiLevels.Length * 2));
    }

    public static void EnsureScriptCapacity(int required)
    {
        if (scripts.Length >= required) return;
        Array.Resize(ref scripts, Math.Max(required, scripts.Length * 2));
    }

    public static void EnsureRunCapacity(int required)
    {
        if (runs.Length >= required) return;
        Array.Resize(ref runs, Math.Max(required, runs.Length * 2));
    }

    public static void EnsureShapedRunCapacity(int required)
    {
        if (shapedRuns.Length >= required) return;
        Array.Resize(ref shapedRuns, Math.Max(required, shapedRuns.Length * 2));
    }

    public static void EnsureShapedGlyphCapacity(int required)
    {
        if (shapedGlyphs.Length >= required) return;
        Array.Resize(ref shapedGlyphs, Math.Max(required, shapedGlyphs.Length * 2));
    }

    public static void EnsureLineCapacity(int required)
    {
        if (lines.Length >= required) return;
        Array.Resize(ref lines, Math.Max(required, lines.Length * 2));
    }

    public static void EnsureOrderedRunCapacity(int required)
    {
        if (orderedRuns.Length >= required) return;
        Array.Resize(ref orderedRuns, Math.Max(required, orderedRuns.Length * 2));
    }

    public static void EnsurePositionedGlyphCapacity(int required)
    {
        if (positionedGlyphs.Length >= required) return;
        Array.Resize(ref positionedGlyphs, Math.Max(required, positionedGlyphs.Length * 2));
    }

    #endregion

    /// <summary>
    /// Сброс при Domain Reload (для Editor).
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        codepoints = new int[256];
        bidiLevels = new byte[256];
        scripts = new UnicodeScript[256];
        runs = new TextRun[32];
        shapedRuns = new ShapedRun[32];
        shapedGlyphs = new ShapedGlyph[256];
        lines = new TextLine[16];
        orderedRuns = new ShapedRun[32];
        positionedGlyphs = new PositionedGlyph[256];
        bidiParagraphs = Array.Empty<BidiParagraph>();
        attributes.Clear();
        Reset();
    }
}

/// <summary>
/// Shared static кеш для font lookup.
/// Позволяет избежать повторных поисков fallback шрифтов.
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

    private const int CacheSize = 256;
    private const int CacheMask = CacheSize - 1;
    private static FontCacheEntry[] cache;

    static SharedFontCache()
    {
        Initialize();
    }

    private static void Initialize()
    {
        cache = new FontCacheEntry[CacheSize];
        for (int i = 0; i < CacheSize; i++)
        {
            cache[i] = new FontCacheEntry(-1, -1, -1);
        }
    }

    /// <summary>
    /// Попытаться получить результат из кеша.
    /// </summary>
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

    /// <summary>
    /// Сохранить результат в кеш.
    /// </summary>
    public static void Set(int codepoint, int preferredFontId, int resultFontId)
    {
        int index = (codepoint ^ (preferredFontId << 16)) & CacheMask;
        cache[index] = new FontCacheEntry(codepoint, preferredFontId, resultFontId);
    }

    /// <summary>
    /// Очистить кеш. Вызывать при смене шрифтов.
    /// </summary>
    public static void Clear()
    {
        for (int i = 0; i < CacheSize; i++)
        {
            cache[i] = new FontCacheEntry(-1, -1, -1);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        Initialize();
    }
}

/// <summary>
/// Shared static пул Mesh объектов.
/// Mesh'и переиспользуются между разными UniText компонентами.
/// </summary>
public static class SharedMeshPool
{
    private static readonly List<Mesh> available = new(16);
    private static bool initialized;

    /// <summary>
    /// Получить mesh из пула или создать новый.
    /// </summary>
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

    /// <summary>
    /// Вернуть mesh в пул.
    /// </summary>
    public static void Release(Mesh mesh)
    {
        if (mesh == null) return;

        mesh.Clear();
        available.Add(mesh);
    }

    /// <summary>
    /// Вернуть несколько mesh'ей в пул.
    /// </summary>
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

    /// <summary>
    /// Очистить неиспользуемые mesh'и (освободить GPU память).
    /// Вызывается автоматически при выгрузке сцены.
    /// </summary>
    public static void ClearUnused()
    {
        foreach (var mesh in available)
        {
            if (mesh != null)
            {
                UnityEngine.Object.Destroy(mesh);
            }
        }
        available.Clear();
    }

    /// <summary>
    /// Текущий размер пула (для диагностики).
    /// </summary>
    public static int PoolSize => available.Count;

    private static void EnsureInitialized()
    {
        if (initialized) return;

        initialized = true;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        // Очищаем пул при выгрузке сцены чтобы избежать накопления mesh'ей
        ClearUnused();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        // Очищаем при Domain Reload (Editor)
        available.Clear();
        initialized = false;
    }
}
