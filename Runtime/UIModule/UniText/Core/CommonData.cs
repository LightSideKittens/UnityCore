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
    // Minimum sizes - ArrayPool will return at least this
    private const int MinCodepointCapacity = 256;
    private const int MinRunCapacity = 64;
    private const int MinGlyphCapacity = 256;
    private const int MinLineCapacity = 32;

    /// <summary>
    /// Текущий активный буфер. Устанавливается перед Rebuild.
    /// </summary>
    public static CommonData Current { get; set; }

    // Current buffers (from ArrayPool or initial allocation)
    public int[] codepoints;
    public int codepointCount;

    public byte[] bidiLevels;
    public BidiParagraph[] bidiParagraphs = Array.Empty<BidiParagraph>();
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
        scripts = ArrayPool<UnicodeScript>.Shared.Rent(MinCodepointCapacity);
        startMargins = ArrayPool<float>.Shared.Rent(MinCodepointCapacity);
        runs = ArrayPool<TextRun>.Shared.Rent(MinRunCapacity);
        shapedRuns = ArrayPool<ShapedRun>.Shared.Rent(MinRunCapacity);
        shapedGlyphs = ArrayPool<ShapedGlyph>.Shared.Rent(MinGlyphCapacity);
        lines = ArrayPool<TextLine>.Shared.Rent(MinLineCapacity);
        orderedRuns = ArrayPool<ShapedRun>.Shared.Rent(MinRunCapacity);
        positionedGlyphs = ArrayPool<PositionedGlyph>.Shared.Rent(MinGlyphCapacity);
        glyphColors = ArrayPool<Color32>.Shared.Rent(MinGlyphCapacity);
        bidiParagraphs = Array.Empty<BidiParagraph>();

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
        if (scripts != null) { ArrayPool<UnicodeScript>.Shared.Return(scripts); scripts = null; }
        if (startMargins != null) { ArrayPool<float>.Shared.Return(startMargins); startMargins = null; }
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
        if (codepointCount > peakCodepointCount) peakCodepointCount = codepointCount;
        if (runCount > peakRunCount) peakRunCount = runCount;
        if (shapedGlyphCount > peakGlyphCount) peakGlyphCount = shapedGlyphCount;

        codepointCount = 0;
        runCount = 0;
        shapedRunCount = 0;
        shapedGlyphCount = 0;
        lineCount = 0;
        orderedRunCount = 0;
        positionedGlyphCount = 0;
        bidiParagraphs = Array.Empty<BidiParagraph>();
        baseDirection = TextDirection.LeftToRight;

        // Clear margins (модификаторы заполняют заново каждый кадр)
        if (startMargins != null)
            startMargins.AsSpan().Clear();
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
            GrowCodepoints(required);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowCodepoints(int required)
    {
        int newSize = Math.Max(required, codepoints.Length * 2);

        // Grow all codepoint-indexed buffers together
        var newCodepoints = ArrayPool<int>.Shared.Rent(newSize);
        codepoints.AsSpan(0, codepointCount).CopyTo(newCodepoints);
        ArrayPool<int>.Shared.Return(codepoints);
        codepoints = newCodepoints;

        var newBidiLevels = ArrayPool<byte>.Shared.Rent(newSize);
        bidiLevels.AsSpan(0, Math.Min(codepointCount, bidiLevels.Length)).CopyTo(newBidiLevels);
        ArrayPool<byte>.Shared.Return(bidiLevels);
        bidiLevels = newBidiLevels;

        var newScripts = ArrayPool<UnicodeScript>.Shared.Rent(newSize);
        scripts.AsSpan(0, Math.Min(codepointCount, scripts.Length)).CopyTo(newScripts);
        ArrayPool<UnicodeScript>.Shared.Return(scripts);
        scripts = newScripts;

        var newMargins = ArrayPool<float>.Shared.Rent(newSize);
        startMargins.AsSpan(0, Math.Min(codepointCount, startMargins.Length)).CopyTo(newMargins);
        ArrayPool<float>.Shared.Return(startMargins);
        startMargins = newMargins;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureBidiCapacity(int required)
    {
        if (bidiLevels.Length < required)
            GrowBidiLevels(required);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowBidiLevels(int required)
    {
        int newSize = Math.Max(required, bidiLevels.Length * 2);
        var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
        bidiLevels.AsSpan().CopyTo(newBuffer);
        ArrayPool<byte>.Shared.Return(bidiLevels);
        bidiLevels = newBuffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureScriptCapacity(int required)
    {
        if (scripts.Length < required)
            GrowScripts(required);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowScripts(int required)
    {
        int newSize = Math.Max(required, scripts.Length * 2);
        var newBuffer = ArrayPool<UnicodeScript>.Shared.Rent(newSize);
        scripts.AsSpan().CopyTo(newBuffer);
        ArrayPool<UnicodeScript>.Shared.Return(scripts);
        scripts = newBuffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureRunCapacity(int required)
    {
        if (runs.Length < required)
            GrowRuns(required);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowRuns(int required)
    {
        int newSize = Math.Max(required, runs.Length * 2);
        var newBuffer = ArrayPool<TextRun>.Shared.Rent(newSize);
        runs.AsSpan(0, runCount).CopyTo(newBuffer);
        ArrayPool<TextRun>.Shared.Return(runs);
        runs = newBuffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureShapedRunCapacity(int required)
    {
        if (shapedRuns.Length < required)
            GrowShapedRuns(required);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowShapedRuns(int required)
    {
        int newSize = Math.Max(required, shapedRuns.Length * 2);
        var newBuffer = ArrayPool<ShapedRun>.Shared.Rent(newSize);
        shapedRuns.AsSpan(0, shapedRunCount).CopyTo(newBuffer);
        ArrayPool<ShapedRun>.Shared.Return(shapedRuns);
        shapedRuns = newBuffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureShapedGlyphCapacity(int required)
    {
        if (shapedGlyphs.Length < required)
            GrowShapedGlyphs(required);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowShapedGlyphs(int required)
    {
        int newSize = Math.Max(required, shapedGlyphs.Length * 2);
        var newBuffer = ArrayPool<ShapedGlyph>.Shared.Rent(newSize);
        shapedGlyphs.AsSpan(0, shapedGlyphCount).CopyTo(newBuffer);
        ArrayPool<ShapedGlyph>.Shared.Return(shapedGlyphs);
        shapedGlyphs = newBuffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureLineCapacity(int required)
    {
        if (lines.Length < required)
            GrowLines(required);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowLines(int required)
    {
        int newSize = Math.Max(required, lines.Length * 2);
        var newBuffer = ArrayPool<TextLine>.Shared.Rent(newSize);
        lines.AsSpan(0, lineCount).CopyTo(newBuffer);
        ArrayPool<TextLine>.Shared.Return(lines);
        lines = newBuffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureOrderedRunCapacity(int required)
    {
        if (orderedRuns.Length < required)
            GrowOrderedRuns(required);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowOrderedRuns(int required)
    {
        int newSize = Math.Max(required, orderedRuns.Length * 2);
        var newBuffer = ArrayPool<ShapedRun>.Shared.Rent(newSize);
        orderedRuns.AsSpan(0, orderedRunCount).CopyTo(newBuffer);
        ArrayPool<ShapedRun>.Shared.Return(orderedRuns);
        orderedRuns = newBuffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsurePositionedGlyphCapacity(int required)
    {
        if (positionedGlyphs.Length < required)
            GrowPositionedGlyphs(required);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowPositionedGlyphs(int required)
    {
        int newSize = Math.Max(required, positionedGlyphs.Length * 2);

        var newBuffer = ArrayPool<PositionedGlyph>.Shared.Rent(newSize);
        positionedGlyphs.AsSpan(0, positionedGlyphCount).CopyTo(newBuffer);
        ArrayPool<PositionedGlyph>.Shared.Return(positionedGlyphs);
        positionedGlyphs = newBuffer;

        // Grow glyphColors together (parallel array)
        var newColors = ArrayPool<Color32>.Shared.Rent(newSize);
        glyphColors.AsSpan(0, positionedGlyphCount).CopyTo(newColors);
        ArrayPool<Color32>.Shared.Return(glyphColors);
        glyphColors = newColors;
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