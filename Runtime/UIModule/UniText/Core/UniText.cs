using System;
using System.Collections.Generic;
using LSCore;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;

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
public partial class UniText : MaskableGraphic
{
    /// <summary>
    /// Flags indicating what needs to be rebuilt.
    /// Each flag represents a specific change type for precise rebuild control.
    /// </summary>
    [Flags]
    public enum DirtyFlags
    {
        None = 0,

        /// <summary>Mesh colors only (cheapest)</summary>
        Color = 1 << 0,

        /// <summary>Alignment offset only</summary>
        Alignment = 1 << 1,

        /// <summary>Re-layout needed (rect size changed)</summary>
        Layout = 1 << 2,

        /// <summary>Font size changed</summary>
        FontSize = 1 << 3,

        /// <summary>Font asset changed</summary>
        Font = 1 << 4,

        /// <summary>Base direction changed</summary>
        Direction = 1 << 5,

        /// <summary>Text content changed - requires attribute parsing</summary>
        Text = 1 << 6,

        LayoutRebuild = Layout | FontSize | Alignment,
        /// <summary>Flags requiring full rebuild (text, font, direction)</summary>
        FullRebuild = Text | Font | Direction,

        All = Color | Alignment | Layout | FontSize | FullRebuild
    }
    
    
    #region Serialized Fields

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

    [Header("Auto Size")]
    [SerializeField]
    private bool enableAutoSize;

    [SerializeField]
    private float minFontSize = 10f;

    [SerializeField]
    private float maxFontSize = 72f;

    [SerializeReference]
    private List<ModRegister> modRegisters;
    
    #endregion

    #region Runtime Components

    private TextProcessor processor;
    private UniTextFontProvider fontProvider;
    private UniTextMeshGenerator meshGenerator;
    private AttributeParser parser;
    private CommonData textBuffers;

    /// <summary>Font provider для доступа к fontAsset и метрикам.</summary>
    public UniTextFontProvider FontProvider => fontProvider;

    #endregion

    #region State & Caching

    private bool isInitialized;
    private bool isRebuilding;
    private bool shaderChannelsConfigured;

    private DirtyFlags dirtyFlags = DirtyFlags.All;

    // Cached results
    private float lastResultWidth;
    private float lastResultHeight;
    private bool textIsParsed;

    // Auto size cache
    private float autoSizedFontSize;
    private float lastAutoSizeWidth;
    private float lastAutoSizeHeight;
    private bool hasValidAutoSize;

    // Layout cache (for preferredHeight with width-only constraint)
    private float layoutCachedFontSize;
    private float layoutCachedWidth;
    private bool hasValidLayoutCache;

    // Sub-mesh renderers для fallback шрифтов
    private readonly List<CanvasRenderer> subMeshRenderers = new();
    private LSList<UniTextMeshPair> lastMeshPairs;

    // Mesh tracking - mesh'и из SharedMeshPool
    private readonly List<Mesh> acquiredMeshes = new();

    public event Action Rebuilding;
    // Cached delegate to avoid lambda allocation
    private Func<Mesh> cachedMeshProvider;

    // Материал для основного mesh
    private Material primaryMaterial;

    // Cached Canvas reference
    private Canvas cachedCanvas;

    // Cached clip parameters for SubMesh
    private Rect cachedClipRect;
    private bool cachedValidClip;
    private Vector4 cachedClipSoftness;

    #endregion

    #region Properties (Public API)

    /// <summary>
    /// Mesh generator instance. Used by modifiers to subscribe to events.
    /// </summary>
    public TextProcessor TextProcessor => processor;
    public UniTextMeshGenerator MeshGenerator => meshGenerator;

    /// <summary>
    /// Размер текста после последней обработки.
    /// </summary>
    public Vector2 LastResultSize => new(lastResultWidth, lastResultHeight);

    /// <summary>
    /// Positioned glyphs после последней обработки.
    /// </summary>
    public ReadOnlySpan<PositionedGlyph> LastResultGlyphs =>
        processor != null ? processor.PositionedGlyphs : ReadOnlySpan<PositionedGlyph>.Empty;

    public string Text
    {
        get => text;
        set
        {
            if (text == value) return;

            bool wasEmpty = string.IsNullOrEmpty(text);
            bool willBeEmpty = string.IsNullOrEmpty(value);

            text = value;

            if (wasEmpty && !willBeEmpty)
            {
                OnTextAppeared();
            }
            else if (!wasEmpty && willBeEmpty)
            {
                OnTextDisappeared();
            }
            else
            {
                SetDirty(DirtyFlags.Text);
            }
        }
    }

    public UniTextFontAsset Font
    {
        get => font;
        set
        {
            if (value == null)
                value = UniTextSettings.DefaultFontAsset;
            if (font == value) return;
            font = value;
            RebuildFontProvider();
            SetDirty(DirtyFlags.Font);
        }
    }

    public float FontSize
    {
        get => fontSize;
        set
        {
            if (Mathf.Approximately(fontSize, value)) return;
            fontSize = Mathf.Max(1f, value);
            SetDirty(DirtyFlags.FontSize);
        }
    }

    public TextDirection BaseDirection
    {
        get => baseDirection;
        set
        {
            if (baseDirection == value) return;
            baseDirection = value;
            SetDirty(DirtyFlags.Direction);
        }
    }

    public bool EnableWordWrap
    {
        get => enableWordWrap;
        set
        {
            if (enableWordWrap == value) return;
            enableWordWrap = value;
            SetDirty(DirtyFlags.Layout);
        }
    }

    public HorizontalAlignment HorizontalAlignment
    {
        get => horizontalAlignment;
        set
        {
            if (horizontalAlignment == value) return;
            horizontalAlignment = value;
            SetDirty(DirtyFlags.Alignment);
        }
    }

    public VerticalAlignment VerticalAlignment
    {
        get => verticalAlignment;
        set
        {
            if (verticalAlignment == value) return;
            verticalAlignment = value;
            SetDirty(DirtyFlags.Alignment);
        }
    }

    public bool EnableAutoSize
    {
        get => enableAutoSize;
        set
        {
            if (enableAutoSize == value) return;
            enableAutoSize = value;
            hasValidAutoSize = false;
            hasValidLayoutCache = false;
            SetDirty(DirtyFlags.Layout);
        }
    }

    public float MinFontSize
    {
        get => minFontSize;
        set
        {
            value = Mathf.Max(1f, value);
            if (Mathf.Approximately(minFontSize, value)) return;
            minFontSize = value;
            if (enableAutoSize)
            {
                hasValidAutoSize = false;
                hasValidLayoutCache = false;
                SetDirty(DirtyFlags.Layout);
            }
        }
    }

    public float MaxFontSize
    {
        get => maxFontSize;
        set
        {
            value = Mathf.Max(1f, value);
            if (Mathf.Approximately(maxFontSize, value)) return;
            maxFontSize = value;
            if (enableAutoSize)
            {
                hasValidAutoSize = false;
                hasValidLayoutCache = false;
                SetDirty(DirtyFlags.Layout);
            }
        }
    }

    public float CurrentFontSize => enableAutoSize ? autoSizedFontSize : fontSize;

    public override Color color
    {
        get => base.color;
        set
        {
            if (base.color == value) return;
            base.color = value;
            SetDirty(DirtyFlags.Color);
        }
    }

    public override Texture mainTexture => GetActiveTexture() ?? base.mainTexture;

    public override Material material
    {
        get => GetActiveMaterial() ?? base.material;
        set => base.material = value;
    }

    #endregion

    #region Lifecycle

    protected override void Awake()
    {
        base.Awake();
        InitializeComponents();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ResetAfterDomainReload();
        CollectExistingSubMeshRenderers();
        EnsureCanvasShaderChannels();
        EnsureMaterialAssigned();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Cleanup();
        textBuffers?.ReturnBuffers();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        CleanupResources();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        ForceFullReinitialization();
        SetDirty(DirtyFlags.All);
    }
#endif

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetDirty(DirtyFlags.Layout);
    }

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();
        cachedCanvas = null;
        shaderChannelsConfigured = false;
        SetDirty(DirtyFlags.Layout);
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        if (isInitialized) return;
        if (!ValidatePrerequisites()) return;
        
        CreateProcessor();
        RebuildFontProvider();
        parser?.InitializeModifiers(this);
        cachedMeshProvider = GetPooledMeshForText;
        isInitialized = true;
    }

    
    private bool ValidatePrerequisites()
    {
        if (!UnicodeData.IsInitialized)
        {
            UnicodeData.EnsureInitialized();
            if (!UnicodeData.IsInitialized)
            {
                Debug.LogError("UniText: Unicode data not initialized. Check UniTextSettings.");
                enabled = false;
                return false;
            }
        }

        if (font == null)
        {
            font = UniTextSettings.DefaultFontAsset;
            if (font == null)
            {
                Debug.LogError("UniText: Font not assigned and no default font in UniTextSettings.");
                enabled = false;
                return false;
            }
        }

        return true;
    }

    private void CreateProcessor()
    {
        textBuffers = new CommonData();
        textBuffers.RentBuffers();
        processor = new TextProcessor();
        
        if (modRegisters == null || modRegisters.Count == 0) return;
        
        parser = new AttributeParser();
        for (int i = 0; i < modRegisters.Count; i++)
        {
            var mod = modRegisters[i];
            if (mod is { IsValid: true })
            { 
                mod.Register(parser);
            }
        }
        
        processor.Parsed += parser.Apply;
    }

    
    private void RebuildFontProvider()
    {
        if (font == null) return;

        fontProvider = new UniTextFontProvider(font);
        meshGenerator = new UniTextMeshGenerator(fontProvider);
        processor?.SetFontProvider(fontProvider);
    }

    private void EnsureInitialized()
    {
        if (!isInitialized)
        {
            InitializeComponents();
        }
    }

    #endregion

    #region State Management

    /// <summary>
    /// Вызывается когда текст появляется (был пустым, стал непустым).
    /// Эквивалент OnEnable.
    /// </summary>
    private void OnTextAppeared()
    {
        ResetAfterDomainReload();
        CollectExistingSubMeshRenderers();
        EnsureCanvasShaderChannels();
        EnsureMaterialAssigned();
        SetVerticesDirty();
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    /// <summary>
    /// Вызывается когда текст исчезает (был непустым, стал пустым).
    /// Эквивалент OnDisable.
    /// </summary>
    private void OnTextDisappeared()
    {
        Cleanup();
        textBuffers?.ReturnBuffers();
        SetDirty(DirtyFlags.Text);
    }

    /// <summary>
    /// Сброс runtime состояния после Domain Reload.
    /// </summary>
    private void ResetAfterDomainReload()
    {
        isInitialized = false;
        processor = null;
        parser = null;
        fontProvider = null;
        meshGenerator = null;
        cachedCanvas = null;
        cachedMeshProvider = null;
        textIsParsed = false;
        shaderChannelsConfigured = false;
        hasValidAutoSize = false;
        hasValidLayoutCache = false;
        dirtyFlags = DirtyFlags.All;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Принудительная полная реинициализация (для OnValidate).
    /// </summary>
    private void ForceFullReinitialization()
    {
        parser?.DeinitializeModifiers();
        textBuffers?.ReturnBuffers();
        isInitialized = false;
        parser = null;
        processor = null;
        fontProvider = null;
        meshGenerator = null;
        textBuffers = null;
        textIsParsed = false;
        hasValidAutoSize = false;
        hasValidLayoutCache = false;
    }
#endif

    /// <summary>
    /// Помечает текст как требующий перестройки.
    /// </summary>
    public void SetDirty(DirtyFlags flags = DirtyFlags.All)
    {
        if (flags == DirtyFlags.None) return;

        var oldFlags = dirtyFlags;
        dirtyFlags |= flags;

        // Invalidate shaping when text/font/direction changes
        if ((flags & DirtyFlags.FullRebuild) != 0)
        {
            processor?.InvalidateShapingData();
            textIsParsed = false;
        }

        if (oldFlags == DirtyFlags.None)
        {
            SetVerticesDirty();
        }

        // Notify layout system when text properties affecting size change
        if ((flags & (DirtyFlags.Text | DirtyFlags.Font | DirtyFlags.FontSize | DirtyFlags.Layout)) != 0)
        {
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
    }

    #endregion

    #region Rebuild Pipeline

    protected override void OnPopulateMesh(VertexHelper vh) => vh.Clear();
    protected override void UpdateGeometry() { }

    public override void Rebuild(CanvasUpdate update)
    {
        base.Rebuild(update);

        if (update != CanvasUpdate.PreRender) return;
        if (dirtyFlags == DirtyFlags.None) return;
        if (isRebuilding) return;

        if (CommonData.DebugPipelineLogging)
            UnityEngine.Debug.Log($"[Rebuild] dirtyFlags={dirtyFlags}");

        EnsureInitialized();
        if (!isInitialized) return;

        // Устанавливаем Current и вызываем Rebuilding ПОСЛЕ инициализации
        CommonData.Current = textBuffers;
        Rebuilding?.Invoke();

        ExecuteRebuild();
    }

    private void ExecuteRebuild()
    {
        isRebuilding = true;

        try
        {
            var flags = dirtyFlags;
            dirtyFlags = DirtyFlags.None;

            if (RequiresFullRebuild(flags)) RebuildFull();
            else if (RequiresLayoutRebuild(flags)) RebuildLayout();
            else RebuildMeshOnly();

            UpdateRendering();
        }
        finally
        {
            isRebuilding = false;
        }
    }

    private static bool RequiresFullRebuild(DirtyFlags flags)
        => (flags & DirtyFlags.FullRebuild) != 0;

    private static bool RequiresLayoutRebuild(DirtyFlags flags)
        => (flags & DirtyFlags.LayoutRebuild) != 0;

    /// <summary>
    /// Полная перестройка текста (text/font/direction изменились).
    /// </summary>
    private void RebuildFull()
    {
        var rt = rectTransform;
        if (rt == null) return;

        ReleaseMeshes();
        hasValidAutoSize = false; // Text changed, need to recalculate auto size
        hasValidLayoutCache = false;

        if (string.IsNullOrEmpty(text))
        {
            lastMeshPairs = null;
            lastResultWidth = 0;
            lastResultHeight = 0;
            autoSizedFontSize = fontSize;
            return;
        }

        try
        {
            var rect = rt.rect;

            // Parse attributes only if not already parsed
            if (!textIsParsed)
            {
                parser?.ResetModifiers();
                parser?.Parse(text);
                textIsParsed = true;
            }

            // Get text span: from parser if available, otherwise original text
            var textSpan = parser != null ? parser.CleanTextSpan : text.AsSpan();

            // Calculate auto size if enabled
            float effectiveFontSize = fontSize;
            if (enableAutoSize)
            {
                effectiveFontSize = CalculateAutoSize(textSpan, rect);
            }
            else
            {
                autoSizedFontSize = fontSize;
            }

            var settings = CreateProcessSettings(rect, effectiveFontSize);
            var glyphs = processor.Process(textSpan, settings);

            lastResultWidth = processor.ResultWidth;
            lastResultHeight = processor.ResultHeight;

            GenerateMeshes(glyphs, rect, effectiveFontSize);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            lastMeshPairs = null;
        }
    }
    
    
    /// <summary>
    /// Перестройка layout (rect size, fontSize или layout settings изменились).
    /// </summary>
    private void RebuildLayout()
    {
        var rt = rectTransform;
        if (rt == null) return;

        var rect = rt.rect;

        ReleaseMeshes();

        if (string.IsNullOrEmpty(text) || processor == null)
        {
            lastMeshPairs = null;
            return;
        }

        // Get text span: from parser if available, otherwise original text
        var textSpan = parser != null ? parser.CleanTextSpan : text.AsSpan();

        // Determine effective font size
        float effectiveFontSize = fontSize;
        if (enableAutoSize)
        {
            // Check if we can reuse cached auto size
            bool canReuseAutoSize = hasValidAutoSize &&
                                    Mathf.Approximately(lastAutoSizeWidth, rect.width) &&
                                    Mathf.Approximately(lastAutoSizeHeight, rect.height);

            if (canReuseAutoSize)
            {
                effectiveFontSize = autoSizedFontSize;
            }
            else
            {
                effectiveFontSize = CalculateAutoSize(textSpan, rect);
            }
        }
        else
        {
            autoSizedFontSize = fontSize;
        }

        var settings = CreateProcessSettings(rect, effectiveFontSize);
        var glyphs = processor.Process(textSpan, settings);

        lastResultWidth = processor.ResultWidth;
        lastResultHeight = processor.ResultHeight;

        GenerateMeshes(glyphs, rect, effectiveFontSize);
    }

    /// <summary>
    /// Regenerate mesh from existing glyphs (alignment/color change).
    /// </summary>
    private void RebuildMeshOnly()
    {
        var glyphs = processor != null ? processor.PositionedGlyphs : ReadOnlySpan<PositionedGlyph>.Empty;
        if (glyphs.IsEmpty)
        {
            lastMeshPairs = null;
            return;
        }

        var rt = rectTransform;
        if (rt == null) return;

        ReleaseMeshes();
        GenerateMeshes(glyphs, rt.rect);
    }

    private void GenerateMeshes(ReadOnlySpan<PositionedGlyph> glyphs, Rect rect, float effectiveFontSize = -1)
    {
        meshGenerator.FontSize = effectiveFontSize > 0 ? effectiveFontSize : fontSize;
        meshGenerator.DefaultColor = color;
        meshGenerator.SetCanvasParameters(transform, cachedCanvas);
        meshGenerator.SetRectOffset(rect);
        meshGenerator.SetHorizontalAlignment(horizontalAlignment);

        Profiler.BeginSample("TextProcessor.GenerateMeshes");
        lastMeshPairs = meshGenerator.GenerateMeshes(glyphs, cachedMeshProvider);
        Profiler.EndSample();
    }

    private TextProcessSettings CreateProcessSettings(Rect rect, float effectiveFontSize = -1)
    {
        return new TextProcessSettings
        {
            layout = new LayoutSettings
            {
                maxWidth = rect.width,
                maxHeight = rect.height,
                horizontalAlignment = horizontalAlignment,
                verticalAlignment = verticalAlignment
            },
            fontSize = effectiveFontSize > 0 ? effectiveFontSize : fontSize,
            baseDirection = baseDirection,
            enableWordWrap = enableWordWrap
        };
    }

    /// <summary>
    /// Calculate optimal font size for auto sizing.
    /// Shaping is done once, then binary search over font sizes.
    /// </summary>
    private float CalculateAutoSize(ReadOnlySpan<char> textSpan, Rect rect)
    {
        if (processor == null || fontProvider == null)
            return fontSize;

        // Ensure shaping is done with maxFontSize for best precision
        var baseSettings = CreateProcessSettings(rect, maxFontSize);
        processor.EnsureShaping(textSpan, baseSettings);

        // Find optimal size
        float optimalSize = processor.FindOptimalFontSize(
            minFontSize,
            maxFontSize,
            rect.width,
            rect.height,
            baseSettings);

        // Cache result
        autoSizedFontSize = optimalSize;
        lastAutoSizeWidth = rect.width;
        lastAutoSizeHeight = rect.height;
        hasValidAutoSize = true;

        return optimalSize;
    }

    #endregion

    #region Rendering

    private void UpdateRendering()
    {
        var cr = canvasRenderer;
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

        UpdateSubMeshes();
    }

    private void ApplyMaterial()
    {
        var cr = canvasRenderer;
        if (cr == null || primaryMaterial == null)
        {
            cr?.Clear();
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

    private Material GetActiveMaterial() => primaryMaterial ?? font?.material;

    private Texture GetActiveTexture() => GetActiveMaterial()?.mainTexture;

    public override void SetClipRect(Rect clipRect, bool validRect)
    {
        base.SetClipRect(clipRect, validRect);

        cachedClipRect = clipRect;
        cachedValidClip = validRect;

        int count = subMeshRenderers.Count;
        for (int i = 0; i < count; i++)
        {
            var renderer = subMeshRenderers[i];
            if (renderer == null) continue;
            if (validRect)
                renderer.EnableRectClipping(clipRect);
            else
                renderer.DisableRectClipping();
        }
    }

    public override void SetClipSoftness(Vector2 clipSoftness)
    {
        base.SetClipSoftness(clipSoftness);

        cachedClipSoftness = new Vector4(clipSoftness.x, clipSoftness.y, 0, 0);

        int count = subMeshRenderers.Count;
        for (int i = 0; i < count; i++)
        {
            var renderer = subMeshRenderers[i];
            if (renderer != null)
                renderer.clippingSoftness = cachedClipSoftness;
        }
    }

    public override void Cull(Rect clipRect, bool validRect)
    {
        base.Cull(clipRect, validRect);

        var cr = canvasRenderer;
        bool cull = cr != null && cr.cull;

        int count = subMeshRenderers.Count;
        for (int i = 0; i < count; i++)
        {
            var renderer = subMeshRenderers[i];
            if (renderer != null)
                renderer.cull = cull;
        }
    }

    #endregion

    #region Sub-mesh Management

    private void CollectExistingSubMeshRenderers()
    {
        subMeshRenderers.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.name.StartsWith("UniText SubMesh"))
            {
                var renderer = child.GetComponent<CanvasRenderer>();
                if (renderer != null)
                    subMeshRenderers.Add(renderer);
            }
        }
    }

    private void UpdateSubMeshes()
    {
        if (lastMeshPairs == null) return;

        int requiredCount = lastMeshPairs.Count - 1;
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
            var pair = lastMeshPairs[i + 1];

            if (i < existingCount)
            {
                var renderer = subMeshRenderers[i];
                if (renderer != null)
                {
                    if (!renderer.gameObject.activeSelf)
                        renderer.gameObject.SetActive(true);
                    // Sync pivot with parent in case it changed
                    var subRT = renderer.GetComponent<RectTransform>();
                    if (subRT != null)
                        subRT.pivot = rectTransform.pivot;
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
        var modMat = GetModifiedMaterial(material);
        renderer.SetMaterial(modMat, 0);
        // Use texture from material (important for multi-atlas)
        var tex = material != null ? material.mainTexture : null;
        renderer.SetTexture(tex);

        if (UniTextMeshGenerator.DebugLogging)
            Debug.Log($"[UniText.SetSubMeshRendererData] mesh={mesh.name}, verts={mesh.vertexCount}, mat={material?.name}, tex={tex?.name}");
    }

    private CanvasRenderer CreateSubMeshRenderer(int index, Mesh mesh, Material material)
    {
        var go = new GameObject($"UniText SubMesh [{index}]")
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        go.transform.SetParent(transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.pivot = rectTransform.pivot; // Match parent pivot for correct mesh coordinate interpretation
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var renderer = go.AddComponent<CanvasRenderer>();
        SetSubMeshRendererData(renderer, mesh, material);

        // Apply current clip state
        if (cachedValidClip)
            renderer.EnableRectClipping(cachedClipRect);
        renderer.clippingSoftness = cachedClipSoftness;

        // Apply current cull state
        var cr = canvasRenderer;
        if (cr != null)
            renderer.cull = cr.cull;

        return renderer;
    }

    #endregion

    #region Mesh Pooling

    private void ReleaseMeshes()
    {
        if (acquiredMeshes.Count > 0)
        {
            SharedMeshPool.Release(acquiredMeshes);
            acquiredMeshes.Clear();
        }
    }

    private Mesh GetPooledMesh(string name)
    {
        var mesh = SharedMeshPool.Acquire(name);
        acquiredMeshes.Add(mesh);
        return mesh;
    }

    private Mesh GetPooledMeshForText() => GetPooledMesh("UniText Mesh");

    #endregion

    #region Cleanup

    /// <summary>
    /// Временная очистка (OnDisable — можно OnEnable обратно).
    /// </summary>
    private void Cleanup()
    {
        parser?.DeinitializeModifiers();
        ClearAllRenderers();
        ReleaseMeshes();
    }

    /// <summary>
    /// Полная очистка ресурсов (OnDestroy).
    /// </summary>
    private void CleanupResources()
    {
        parser?.DeinitializeModifiers();
        parser?.DestroyModifiers();
        DestroySubMeshObjects();
        ReleaseMeshes();
        lastMeshPairs = null;
    }

    private void ClearAllRenderers()
    {
        canvasRenderer?.Clear();

        int count = subMeshRenderers.Count;
        for (int i = 0; i < count; i++)
        {
            subMeshRenderers[i]?.Clear();
        }
    }

    private void DestroySubMeshObjects()
    {
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
    }

    #endregion

    #region Utility

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

    #endregion
}
