using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Flags indicating what needs to be rebuilt.
/// Lower flags require less work, higher flags trigger more processing.
/// </summary>
[Flags]
public enum UniTextDirtyFlags
{
    None = 0,

    /// <summary>Mesh colors only (cheapest)</summary>
    Color = 1 << 0,

    /// <summary>Alignment offset only</summary>
    Alignment = 1 << 1,

    /// <summary>Re-layout needed (rect size changed with word wrap)</summary>
    Layout = 1 << 2,

    /// <summary>Font size changed - re-layout + mesh</summary>
    FontSize = 1 << 3,

    /// <summary>Full rebuild (text, font, direction changed)</summary>
    Full = 1 << 4,

    All = Color | Alignment | Layout | FontSize | Full
}

/// <summary>
/// Компонент для отображения текста с полной Unicode поддержкой.
/// Использует UniTextFontAsset для рендеринга и собственный pipeline для:
/// - BiDi (правильное отображение RTL текста)
/// - Line breaking по Unicode правилам
/// - Script detection
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class UniText : MaskableGraphic
{
    [TextArea(3, 10)]
    [SerializeField]
    private string text = "";

    [Header("Font")]
    [SerializeField]
    private UniTextFontAsset font;

    [SerializeField]
    private float fontSize = 36f;

    [Header("Layout")]
    [SerializeField]
    private TextDirection baseDirection = TextDirection.Auto;

    [SerializeField]
    private bool enableWordWrap = true;

    [Header("Alignment")]
    [SerializeField]
    private HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left;

    [SerializeField]
    private VerticalAlignment verticalAlignment = VerticalAlignment.Top;

    // Runtime components
    private TextProcessor processor;
    private UniTextFontProvider fontProvider;
    private UniTextMeshGenerator meshGenerator;

    private UniTextDirtyFlags dirtyFlags = UniTextDirtyFlags.Full;

    // Cached settings for change detection
    private float cachedRectWidth;
    private float cachedRectHeight;

    // Cached results
    private float lastResultWidth;
    private float lastResultHeight;

    // Sub-mesh renderers для fallback шрифтов (работаем напрямую с CanvasRenderer)
    private readonly List<CanvasRenderer> subMeshRenderers = new();
    private List<UniTextMeshPair> lastMeshPairs;

    // Mesh tracking - mesh'и из SharedMeshPool, которые нужно вернуть при OnDisable/OnDestroy
    private readonly List<Mesh> acquiredMeshes = new();

    // Cached delegate to avoid lambda allocation
    private Func<Mesh> cachedMeshProvider;

    // Материал для основного mesh (из первого MeshMaterialPair)
    private Material primaryMaterial;

    // Cached Canvas reference (avoid GetComponentInParent every frame)
    private Canvas cachedCanvas;

    public string Text
    {
        get => text;
        set
        {
            if (text != value)
            {
                text = value;
                SetDirty(UniTextDirtyFlags.Full);
            }
        }
    }

    public UniTextFontAsset Font
    {
        get => font;
        set
        {
            if (font != value)
            {
                font = value;
                RebuildFontProvider();
                SetDirty(UniTextDirtyFlags.Full);
            }
        }
    }

    public float FontSize
    {
        get => fontSize;
        set
        {
            if (!Mathf.Approximately(fontSize, value))
            {
                fontSize = Mathf.Max(1f, value);
                SetDirty(UniTextDirtyFlags.FontSize);
            }
        }
    }

    public TextDirection BaseDirection
    {
        get => baseDirection;
        set
        {
            if (baseDirection != value)
            {
                baseDirection = value;
                SetDirty(UniTextDirtyFlags.Full);
            }
        }
    }

    public bool EnableWordWrap
    {
        get => enableWordWrap;
        set
        {
            if (enableWordWrap != value)
            {
                enableWordWrap = value;
                SetDirty(UniTextDirtyFlags.Layout);
            }
        }
    }

    public HorizontalAlignment HorizontalAlignment
    {
        get => horizontalAlignment;
        set
        {
            if (horizontalAlignment != value)
            {
                horizontalAlignment = value;
                SetDirty(UniTextDirtyFlags.Alignment);
            }
        }
    }

    public VerticalAlignment VerticalAlignment
    {
        get => verticalAlignment;
        set
        {
            if (verticalAlignment != value)
            {
                verticalAlignment = value;
                SetDirty(UniTextDirtyFlags.Alignment);
            }
        }
    }

    /// <summary>
    /// Размер текста после последней обработки.
    /// </summary>
    public Vector2 LastResultSize => new(lastResultWidth, lastResultHeight);

    /// <summary>
    /// Positioned glyphs после последней обработки.
    /// </summary>
    public ReadOnlySpan<PositionedGlyph> LastResultGlyphs
    {
        get
        {
            if (processor == null)
                return ReadOnlySpan<PositionedGlyph>.Empty;
            return processor.PositionedGlyphs;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // Re-initialize after Domain Reload (runtime data not serialized)
        dirtyFlags = UniTextDirtyFlags.Full;
        processor = null;
        fontProvider = null;
        meshGenerator = null;
        cachedCanvas = null;
        shaderChannelsConfigured = false;
        cachedRectWidth = 0;
        cachedRectHeight = 0;

        CollectExistingSubMeshRenderers();
        EnsureCanvasShaderChannels();
        EnsureMaterialAssigned();
    }

    /// <summary>
    /// Собирает существующие sub-mesh CanvasRenderer'ы после рекомпиляции.
    /// </summary>
    private void CollectExistingSubMeshRenderers()
    {
        subMeshRenderers.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            // Проверяем что это наш submesh объект по имени
            if (child.name.StartsWith("UniText SubMesh"))
            {
                var renderer = child.GetComponent<CanvasRenderer>();
                if (renderer != null)
                {
                    subMeshRenderers.Add(renderer);
                }
            }
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetDirty(UniTextDirtyFlags.Full);
    }
#endif

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        // Layout only needed if word wrap is enabled, otherwise just alignment
        SetDirty(enableWordWrap ? UniTextDirtyFlags.Layout : UniTextDirtyFlags.Alignment);
    }

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();
        cachedCanvas = null;
        shaderChannelsConfigured = false;
        SetDirty(UniTextDirtyFlags.Full);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Очистка sub-mesh объектов
        foreach (var renderer in subMeshRenderers)
        {
            if (renderer != null)
            {
                if (Application.isPlaying)
                    Destroy(renderer.gameObject);
                else
                    DestroyImmediate(renderer.gameObject);
            }
        }
        subMeshRenderers.Clear();

        // Возвращаем mesh'и в SharedMeshPool
        ReleaseMeshes();

        lastMeshPairs = null;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Очищаем все CanvasRenderer'ы (основной и submesh)
        ClearAllRenderers();

        // Возвращаем mesh'и в пул когда компонент отключён
        ReleaseMeshes();
    }

    /// <summary>
    /// Вернуть все acquired mesh'и в SharedMeshPool.
    /// </summary>
    private void ReleaseMeshes()
    {
        if (acquiredMeshes.Count > 0)
        {
            SharedMeshPool.Release(acquiredMeshes);
            acquiredMeshes.Clear();
        }
    }

    /// <summary>
    /// Получить mesh из SharedMeshPool.
    /// </summary>
    private Mesh GetPooledMesh(string name)
    {
        var mesh = SharedMeshPool.Acquire(name);
        acquiredMeshes.Add(mesh);
        return mesh;
    }

    // Non-capturing method for Func<Mesh> delegate - avoids lambda allocation
    private Mesh GetPooledMeshForText() => GetPooledMesh("UniText Mesh");

    private void Initialize()
    {
        if (!UnicodeData.IsInitialized)
        {
            UnicodeData.EnsureInitialized();
            if (!UnicodeData.IsInitialized)
            {
                Debug.LogError("UniText: Unicode data not initialized. Check UniTextSettings.");
                enabled = false;
                return;
            }
        }

        if (font == null)
        {
            Debug.LogError("UniText: Font not assigned.");
            enabled = false;
            return;
        }

        try
        {
            processor = new TextProcessor();
            cachedMeshProvider = GetPooledMeshForText;
            RebuildFontProvider();
        }
        catch (Exception ex)
        {
            Debug.LogError($"UniText: Failed to initialize: {ex.Message}");
            enabled = false;
        }
    }

    private void RebuildFontProvider()
    {
        if (font == null) return;

        fontProvider = new UniTextFontProvider(font);
        meshGenerator = new UniTextMeshGenerator(fontProvider);

        if (processor != null)
        {
            processor.SetFontProvider(fontProvider);
        }
    }

    /// <summary>
    /// Помечает текст как требующий перестройки с указанным уровнем.
    /// Перестройка произойдёт через систему CanvasUpdateRegistry (Graphic.Rebuild).
    /// </summary>
    public void SetDirty(UniTextDirtyFlags flags = UniTextDirtyFlags.Full)
    {
        if (flags == UniTextDirtyFlags.None) return;

        var oldFlags = dirtyFlags;
        dirtyFlags |= flags;

        if (oldFlags == UniTextDirtyFlags.None)
        {
            SetVerticesDirty();
        }
    }

    private bool isRebuilding;

    protected override void OnPopulateMesh(VertexHelper vh) => vh.Clear();
    protected override void UpdateGeometry() { }

    public override void Rebuild(CanvasUpdate update)
    {
        base.Rebuild(update);

        if (update != CanvasUpdate.PreRender || dirtyFlags == UniTextDirtyFlags.None || isRebuilding)
            return;

        if (processor == null)
        {
            Initialize();
            if (processor == null)
                return;
        }
        PerformRebuild();
    }

    private void PerformRebuild()
    {
        if (isRebuilding) return;

        isRebuilding = true;
        try
        {
            var flags = dirtyFlags;
            dirtyFlags = UniTextDirtyFlags.None;

            // Determine what level of rebuild is needed
            if ((flags & UniTextDirtyFlags.Full) != 0)
            {
                // Full rebuild: text, font, or direction changed
                RebuildText(UniTextDirtyFlags.Full);
            }
            else if ((flags & UniTextDirtyFlags.FontSize) != 0)
            {
                // Font size changed - need full rebuild
                RebuildText(UniTextDirtyFlags.FontSize);
            }
            else if ((flags & UniTextDirtyFlags.Layout) != 0)
            {
                // Rect changed with word wrap - layout only (skip shaping)
                RebuildLayout();
            }
            else if ((flags & UniTextDirtyFlags.Alignment) != 0)
            {
                // Only alignment changed - just regenerate mesh with new offset
                RebuildMeshOnly();
            }
            else if ((flags & UniTextDirtyFlags.Color) != 0)
            {
                // Only color changed - regenerate mesh
                RebuildMeshOnly();
            }

            UpdateCanvasRenderer();
        }
        finally
        {
            isRebuilding = false;
        }
    }

    /// <summary>
    /// Re-layout existing glyphs (when rect size changes with word wrap).
    /// Uses cached shaping data if available.
    /// </summary>
    private void RebuildLayout()
    {
        if (processor == null || fontProvider == null || string.IsNullOrEmpty(text))
        {
            RebuildText(UniTextDirtyFlags.Full);
            return;
        }

        var rt = GetComponent<RectTransform>();
        if (rt == null) return;

        var rect = rt.rect;

        // Check if rect actually changed
        if (Mathf.Approximately(rect.width, cachedRectWidth) &&
            Mathf.Approximately(rect.height, cachedRectHeight))
        {
            // Rect didn't change, just regenerate mesh
            RebuildMeshOnly();
            return;
        }

        // Use layout-only rebuild if shaping data is valid
        RebuildText(UniTextDirtyFlags.Layout);
    }

    /// <summary>
    /// Regenerate mesh from existing positioned glyphs (alignment/color change).
    /// </summary>
    private void RebuildMeshOnly()
    {
        if (processor == null || meshGenerator == null)
        {
            RebuildText();
            return;
        }

        var glyphs = processor.PositionedGlyphs;
        if (glyphs.IsEmpty)
        {
            lastMeshPairs = null;
            return;
        }

        var rt = GetComponent<RectTransform>();
        if (rt == null) return;

        ReleaseMeshes();

        meshGenerator.FontSize = fontSize;
        meshGenerator.DefaultColor = color;
        meshGenerator.SetCanvasParameters(transform, cachedCanvas);
        meshGenerator.SetRectOffset(rt.rect);

        lastMeshPairs = meshGenerator.GenerateMeshes(glyphs, cachedMeshProvider);
    }

    private void UpdateCanvasRenderer()
    {
        // Получаем CanvasRenderer безопасно
        var cr = GetComponent<CanvasRenderer>();
        if (cr == null) return;

        if (lastMeshPairs == null || lastMeshPairs.Count == 0)
        {
            ClearAllRenderers();
            return;
        }

        EnsureCanvasShaderChannels();

        var firstPair = lastMeshPairs[0];
        if (firstPair.mesh != null && firstPair.mesh.vertexCount > 0)
        {
            primaryMaterial = firstPair.material;
            cr.SetMesh(firstPair.mesh);
            ApplyMaterial();
        }
        else
        {
            cr.Clear();
        }

        UpdateSubMeshes(lastMeshPairs);
    }

    private void UpdateSubMeshes(List<UniTextMeshPair> meshPairs)
    {
        int requiredCount = meshPairs.Count - 1;
        int existingCount = subMeshRenderers.Count;

        // Hide unused
        for (int i = requiredCount; i < existingCount; i++)
        {
            var renderer = subMeshRenderers[i];
            if (renderer != null)
            {
                renderer.Clear();
                renderer.gameObject.SetActive(false);
            }
        }

        // Update or create required
        for (int i = 0; i < requiredCount; i++)
        {
            var pair = meshPairs[i + 1];

            if (i < existingCount)
            {
                var renderer = subMeshRenderers[i];
                if (renderer != null)
                {
                    if (!renderer.gameObject.activeSelf)
                        renderer.gameObject.SetActive(true);
                    SetSubMeshRendererData(renderer, pair.mesh, pair.material);
                    continue;
                }
            }

            var newRenderer = CreateSubMeshRenderer(i + 1, pair.mesh, pair.material);
            if (i < existingCount)
                subMeshRenderers[i] = newRenderer;
            else
                subMeshRenderers.Add(newRenderer);
        }
    }

    private void SetSubMeshRendererData(CanvasRenderer renderer, Mesh mesh, Material material)
    {
        if (mesh == null || mesh.vertexCount == 0)
        {
            renderer.Clear();
            return;
        }

        renderer.SetMesh(mesh);
        renderer.materialCount = 1;

        // Применяем материал с учётом масок
        var materialToUse = GetModifiedMaterial(material);
        renderer.SetMaterial(materialToUse, 0);
        renderer.SetTexture(material.mainTexture);
    }

    private void ApplyMaterial()
    {
        var cr = GetComponent<CanvasRenderer>();
        if (cr == null) return;

        if (primaryMaterial == null)
        {
            cr.Clear();
            return;
        }

        cr.materialCount = 1;
        cr.SetMaterial(materialForRendering, 0);
        cr.SetTexture(mainTexture);
    }

    protected override void UpdateMaterial() => ApplyMaterial();

    private void EnsureMaterialAssigned()
    {
        if (primaryMaterial == null && font?.material != null)
        {
            primaryMaterial = font.material;
            ApplyMaterial();
        }
    }

    private void ClearAllRenderers()
    {
        // Используем GetComponent вместо свойства canvasRenderer,
        // так как свойство может выбросить исключение после Domain Reload
        var cr = GetComponent<CanvasRenderer>();
        if (cr != null)
            cr.Clear();

        int count = subMeshRenderers.Count;
        for (int i = 0; i < count; i++)
        {
            var renderer = subMeshRenderers[i];
            if (renderer != null)
                renderer.Clear();
        }
    }

    private CanvasRenderer CreateSubMeshRenderer(int index, Mesh mesh, Material material)
    {
        var go = new GameObject($"UniText SubMesh [{index}]")
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        go.transform.SetParent(transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var renderer = go.AddComponent<CanvasRenderer>();
        SetSubMeshRendererData(renderer, mesh, material);

        return renderer;
    }

    private bool shaderChannelsConfigured;

    private void EnsureCanvasShaderChannels()
    {
        if (shaderChannelsConfigured) return;

        cachedCanvas = GetComponentInParent<Canvas>();
        if (cachedCanvas == null) return;

        const AdditionalCanvasShaderChannels requiredChannels = AdditionalCanvasShaderChannels.TexCoord1;
        if ((cachedCanvas.additionalShaderChannels & requiredChannels) != requiredChannels)
            cachedCanvas.additionalShaderChannels |= requiredChannels;

        shaderChannelsConfigured = true;
    }

    private void RebuildText(UniTextDirtyFlags dirtyFlags = UniTextDirtyFlags.Full)
    {
        if (processor == null || fontProvider == null || meshGenerator == null)
        {
            Initialize();
            if (processor == null) return;
        }

        // Получаем RectTransform безопасно
        var rt = GetComponent<RectTransform>();
        if (rt == null) return;

        ReleaseMeshes();

        if (string.IsNullOrEmpty(text))
        {
            lastMeshPairs = null;
            lastResultWidth = 0;
            lastResultHeight = 0;
            return;
        }

        try
        {
            var rect = rt.rect;

            // Cache rect size for change detection
            cachedRectWidth = rect.width;
            cachedRectHeight = rect.height;

            var settings = new TextProcessSettings
            {
                maxWidth = enableWordWrap ? rect.width : float.MaxValue,
                maxHeight = rect.height,
                fontSize = fontSize,
                baseDirection = baseDirection,
                enableWordWrap = enableWordWrap,
                horizontalAlignment = horizontalAlignment,
                verticalAlignment = verticalAlignment
            };

            var glyphs = processor.Process(text.AsSpan(), settings, dirtyFlags);
            lastResultWidth = processor.ResultWidth;
            lastResultHeight = processor.ResultHeight;

            meshGenerator.FontSize = fontSize;
            meshGenerator.DefaultColor = color;
            meshGenerator.SetCanvasParameters(transform, cachedCanvas);
            meshGenerator.SetRectOffset(rect);

            lastMeshPairs = meshGenerator.GenerateMeshes(glyphs, cachedMeshProvider);
        }
        catch (Exception ex)
        {
            Debug.LogError($"UniText: RebuildText failed: {ex.Message}");
            lastMeshPairs = null;
        }
    }

    public override Texture mainTexture
    {
        get
        {
            if (primaryMaterial != null)
                return primaryMaterial.mainTexture;
            if (font != null && font.material != null)
                return font.material.mainTexture;
            return base.mainTexture;
        }
    }

    public override Material material
    {
        get
        {
            if (primaryMaterial != null)
                return primaryMaterial;
            if (font != null)
                return font.material;
            return base.material;
        }
        set => base.material = value;
    }

    public bool TryGetCharacterPosition(int charIndex, out Vector2 position)
    {
        position = Vector2.zero;

        if (processor == null || charIndex < 0)
            return false;

        var glyphs = processor.PositionedGlyphs;
        if (charIndex >= glyphs.Length)
            return false;

        var glyph = glyphs[charIndex];
        position = new Vector2(glyph.x, glyph.y);
        return true;
    }

    public int GetCharacterIndexAtPosition(Vector2 localPosition)
    {
        if (processor == null)
            return -1;

        var glyphs = processor.PositionedGlyphs;
        int glyphCount = glyphs.Length;
        float closestDistSq = float.MaxValue;
        int closestIndex = -1;
        float localX = localPosition.x;
        float localY = localPosition.y;

        for (int i = 0; i < glyphCount; i++)
        {
            ref readonly var glyph = ref glyphs[i];
            float dx = localX - glyph.x;
            float dy = localY - glyph.y;
            float distSq = dx * dx + dy * dy;

            if (distSq < closestDistSq)
            {
                closestDistSq = distSq;
                closestIndex = i;
            }
        }

        return closestIndex;
    }
}
