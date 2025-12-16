using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Instance-based буферы для TextProcessor.
/// Каждый UniText имеет свой экземпляр. Static Current указывает на текущий активный.
/// Использует ArrayPool для zero-allocation при resize.
/// </summary>
public sealed class CommonData
{
    /// <summary>
    /// Enable pipeline logging to trace Layout/Rebuild flow.
    /// </summary>
    public static bool DebugPipelineLogging = false;

    // Minimum sizes - ArrayPool will return at least this
    private const int MinCodepointCapacity = 256;
    private const int MinRunCapacity = 64;
    private const int MinGlyphCapacity = 256;
    private const int MinLineCapacity = 32;
    private const int MinParagraphCapacity = 8;

    /// <summary>
    /// Текущий активный буфер. Устанавливается перед Rebuild.
    /// </summary>
    public static CommonData Current { get; set; }

    // Current buffers (from ArrayPool or initial allocation)
    public int[] codepoints;
    public int codepointCount;

    public byte[] bidiLevels;
    public BidiParagraph[] bidiParagraphs;
    public int bidiParagraphCount;
    public TextDirection baseDirection;

    public UnicodeScript[] scripts;

    public TextRun[] runs;
    public int runCount;

    public ShapedRun[] shapedRuns;
    public int shapedRunCount;
    public ShapedGlyph[] shapedGlyphs;
    public int shapedGlyphCount;
    public float shapingFontSize;

    public TextLine[] lines;
    public int lineCount;
    public ShapedRun[] orderedRuns;
    public int orderedRunCount;

    public PositionedGlyph[] positionedGlyphs;
    public int positionedGlyphCount;

    // ═══════════════════════════════════════════════════════════════════
    // GLYPH ATTRIBUTES - общие буферы для модификаторов
    // Индексируются по glyph index (параллельно positionedGlyphs)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Цвета глифов. Используется ColorModifier, GradientModifier и т.д.
    /// Общий буфер, т.к. цвет — универсальный атрибут для многих эффектов.
    /// </summary>
    public Color32[] glyphColors;

    // ═══════════════════════════════════════════════════════════════════
    // LAYOUT MARGINS - для hanging indent, blockquotes и т.д.
    // Индексируются по codepoint index (параллельно codepoints)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Start margin для каждого codepoint.
    /// LineBreaker использует margin первого codepoint строки.
    /// При LTR — отступ слева, при RTL — справа.
    /// </summary>
    public float[] startMargins;

    // Track peak usage for diagnostics
    public int peakCodepointCount;
    public int peakRunCount;
    public int peakGlyphCount;

    // Track if buffers are rented
    private bool isRented;

    public CommonData() { }

    /// <summary>
    /// Взять массивы из пулов. Вызывать в OnEnable.
    /// </summary>
    public void RentBuffers()
    {
        if (isRented) return;

        codepoints = ArrayPool<int>.Shared.Rent(MinCodepointCapacity);
        bidiLevels = ArrayPool<byte>.Shared.Rent(MinCodepointCapacity);
        bidiParagraphs = ArrayPool<BidiParagraph>.Shared.Rent(MinParagraphCapacity);
        scripts = ArrayPool<UnicodeScript>.Shared.Rent(MinCodepointCapacity);
        startMargins = ArrayPool<float>.Shared.Rent(MinCodepointCapacity);
        runs = ArrayPool<TextRun>.Shared.Rent(MinRunCapacity);
        shapedRuns = ArrayPool<ShapedRun>.Shared.Rent(MinRunCapacity);
        shapedGlyphs = ArrayPool<ShapedGlyph>.Shared.Rent(MinGlyphCapacity);
        lines = ArrayPool<TextLine>.Shared.Rent(MinLineCapacity);
        orderedRuns = ArrayPool<ShapedRun>.Shared.Rent(MinRunCapacity);
        positionedGlyphs = ArrayPool<PositionedGlyph>.Shared.Rent(MinGlyphCapacity);
        glyphColors = ArrayPool<Color32>.Shared.Rent(MinGlyphCapacity);
        
        isRented = true;
        Reset();
    }

    /// <summary>
    /// Вернуть массивы обратно в пулы. Вызывать в OnDisable.
    /// </summary>
    public void ReturnBuffers()
    {
        if (!isRented) return;

        if (codepoints != null) { ArrayPool<int>.Shared.Return(codepoints); codepoints = null; }
        if (bidiLevels != null) { ArrayPool<byte>.Shared.Return(bidiLevels); bidiLevels = null; }
        if (bidiParagraphs != null) { ArrayPool<BidiParagraph>.Shared.Return(bidiParagraphs); bidiParagraphs = null; }
        if (scripts != null) { ArrayPool<UnicodeScript>.Shared.Return(scripts); scripts = null; }

        if (startMargins != null)
        {
            startMargins.AsSpan().Clear();
            ArrayPool<float>.Shared.Return(startMargins); startMargins = null;
        }
        
        if (runs != null) { ArrayPool<TextRun>.Shared.Return(runs); runs = null; }
        if (shapedRuns != null) { ArrayPool<ShapedRun>.Shared.Return(shapedRuns); shapedRuns = null; }
        if (shapedGlyphs != null) { ArrayPool<ShapedGlyph>.Shared.Return(shapedGlyphs); shapedGlyphs = null; }
        if (lines != null) { ArrayPool<TextLine>.Shared.Return(lines); lines = null; }
        if (orderedRuns != null) { ArrayPool<ShapedRun>.Shared.Return(orderedRuns); orderedRuns = null; }
        if (positionedGlyphs != null) { ArrayPool<PositionedGlyph>.Shared.Return(positionedGlyphs); positionedGlyphs = null; }
        if (glyphColors != null) { ArrayPool<Color32>.Shared.Return(glyphColors); glyphColors = null; }

        isRented = false;

        // Clear Current if it points to this instance
        if (Current == this)
            Current = null;
    }

    public void Reset()
    {
        // Track peak usage before reset (for diagnostics)
        int cpCount = codepointCount;
        if (cpCount > peakCodepointCount) peakCodepointCount = cpCount;
        if (runCount > peakRunCount) peakRunCount = runCount;
        if (shapedGlyphCount > peakGlyphCount) peakGlyphCount = shapedGlyphCount;

        // Clear margins only for used portion (before resetting count)
        if (startMargins != null && cpCount > 0)
            startMargins.AsSpan(0, cpCount).Clear();

        codepointCount = 0;
        bidiParagraphCount = 0;
        runCount = 0;
        shapedRunCount = 0;
        shapedGlyphCount = 0;
        lineCount = 0;
        orderedRunCount = 0;
        positionedGlyphCount = 0;
        baseDirection = TextDirection.LeftToRight;
    }

    /// <summary>
    /// Log peak usage for tuning initial buffer sizes.
    /// </summary>
    public void LogPeakUsage()
    {
        UnityEngine.Debug.Log($"[SharedTextBuffers] Peak usage: codepoints={peakCodepointCount}, runs={peakRunCount}, glyphs={peakGlyphCount}");
        UnityEngine.Debug.Log($"[SharedTextBuffers] Buffer sizes: codepoints={codepoints?.Length ?? 0}, runs={runs?.Length ?? 0}, glyphs={shapedGlyphs?.Length ?? 0}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCodepointCapacity(int required)
    {
        if (codepoints.Length < required)
            BufferUtils.GrowCodepointBuffers(ref codepoints, ref bidiLevels, ref scripts, ref startMargins, codepointCount, required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureBidiCapacity(int required)
    {
        if (bidiLevels.Length < required)
            BufferUtils.Grow(ref bidiLevels, bidiLevels.Length, required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureBidiParagraphCapacity(int required)
    {
        if (bidiParagraphs.Length < required)
            BufferUtils.Grow(ref bidiParagraphs, bidiParagraphCount, required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureScriptCapacity(int required)
    {
        if (scripts.Length < required)
            BufferUtils.Grow(ref scripts, scripts.Length, required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureRunCapacity(int required) =>
        BufferUtils.EnsureCapacity(ref runs, runCount, required);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureShapedRunCapacity(int required) =>
        BufferUtils.EnsureCapacity(ref shapedRuns, shapedRunCount, required);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureShapedGlyphCapacity(int required) =>
        BufferUtils.EnsureCapacity(ref shapedGlyphs, shapedGlyphCount, required);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureLineCapacity(int required) =>
        BufferUtils.EnsureCapacity(ref lines, lineCount, required);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureOrderedRunCapacity(int required) =>
        BufferUtils.EnsureCapacity(ref orderedRuns, orderedRunCount, required);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsurePositionedGlyphCapacity(int required)
    {
        if (positionedGlyphs.Length < required)
            BufferUtils.GrowPositionedGlyphBuffers(ref positionedGlyphs, ref glyphColors, positionedGlyphCount, required);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        Current = null;
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

        // Ищем валидный mesh в пуле (не destroyed)
        while (available.Count > 0)
        {
            int lastIndex = available.Count - 1;
            var mesh = available[lastIndex];
            available.RemoveAt(lastIndex);

            // Проверяем что mesh не был уничтожен Unity
            if (mesh != null)
            {
                mesh.Clear();
                mesh.name = name;
                return mesh;
            }
        }

        // Пул пуст или все mesh'и destroyed — создаём новый
        var newMesh = new Mesh();
        newMesh.name = name;
        return newMesh;
    }

    public static void Release(Mesh mesh)
    {
        // Проверяем что mesh не destroyed
        if (mesh == null) return;
        mesh.Clear();
        available.Add(mesh);
    }

    public static void Release(List<Mesh> meshes)
    {
        if (meshes == null) return;

        foreach (var mesh in meshes)
        {
            // Проверяем что mesh не destroyed
            if (mesh != null)
            {
                mesh.Clear();
                available.Add(mesh);
            }
        }
    }

    public static void ClearUnused()
    {
        for (int i = available.Count - 1; i >= 0; i--)
        {
            var mesh = available[i];
            // Проверяем что mesh не destroyed перед уничтожением
            if (mesh != null)
                UnityEngine.Object.Destroy(mesh);
        }
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