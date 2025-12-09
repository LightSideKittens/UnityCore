using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Flags indicating what needs to be rebuilt.
/// Each flag represents a specific change type for precise rebuild control.
/// </summary>
[Flags]
public enum UniTextDirtyFlags
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

    /// <summary>Flags requiring full rebuild (text, font, direction)</summary>
    FullRebuild = Text | Font | Direction,

    All = Color | Alignment | Layout | FontSize | FullRebuild
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

    [SerializeReference]
    private BaseModRegister[] modRegisters;

    #endregion

    #region Runtime Components

    private TextProcessor processor;
    private UniTextFontProvider fontProvider;
    private UniTextMeshGenerator meshGenerator;
    private AttributeParser parser;

    #endregion

    #region State & Caching

    private bool isInitialized;
    private bool isRebuilding;
    private bool shaderChannelsConfigured;

    private UniTextDirtyFlags dirtyFlags = UniTextDirtyFlags.All;

    // Cached rect size for change detection
    private float cachedRectWidth;
    private float cachedRectHeight;

    // Cached results
    private float lastResultWidth;
    private float lastResultHeight;
    private string cachedCleanText;

    // Sub-mesh renderers для fallback шрифтов
    private readonly List<CanvasRenderer> subMeshRenderers = new();
    private List<UniTextMeshPair> lastMeshPairs;

    // Mesh tracking - mesh'и из SharedMeshPool
    private readonly List<Mesh> acquiredMeshes = new();

    // Cached delegate to avoid lambda allocation
    private Func<Mesh> cachedMeshProvider;

    // Материал для основного mesh
    private Material primaryMaterial;

    // Cached Canvas reference
    private Canvas cachedCanvas;

    #endregion

    #region Properties (Public API)

    /// <summary>
    /// Mesh generator instance. Used by modifiers to subscribe to events.
    /// </summary>
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
            if (text != value)
            {
                text = value;
                SetDirty(UniTextDirtyFlags.Text);
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
                SetDirty(UniTextDirtyFlags.Font);
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
                SetDirty(UniTextDirtyFlags.Direction);
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
        SetDirty(UniTextDirtyFlags.All);
    }
#endif

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetDirty(UniTextDirtyFlags.Layout);
    }

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();
        cachedCanvas = null;
        shaderChannelsConfigured = false;
        SetDirty(UniTextDirtyFlags.Layout);
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        if (isInitialized) return;
        if (!ValidatePrerequisites()) return;

        try
        {
            CreateParser();
            CreateProcessor();
            RebuildFontProvider();
            InitializeModifiers();
            cachedMeshProvider = GetPooledMeshForText;
            isInitialized = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"UniText: Failed to initialize: {ex.Message}");
            enabled = false;
        }
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
            Debug.LogError("UniText: Font not assigned.");
            enabled = false;
            return false;
        }

        return true;
    }

    private void CreateParser()
    {
        if (modRegisters == null || modRegisters.Length == 0) return;

        parser = new AttributeParser();
        for (int i = 0; i < modRegisters.Length; i++)
        {
            modRegisters[i]?.Register(parser);
        }
    }

    private void CreateProcessor()
    {
        processor = new TextProcessor();

        if (parser != null)
        {
            processor.OnBeforeItemize = parser.ApplyItemizationModifiers;
            processor.OnAfterLayout = () =>
            {
                parser.ApplyLayoutModifiers();
                parser.ApplyRenderModifiers();
            };
        }
    }

    private void RebuildFontProvider()
    {
        if (font == null) return;

        fontProvider = new UniTextFontProvider(font);
        meshGenerator = new UniTextMeshGenerator(fontProvider);
        processor?.SetFontProvider(fontProvider);
    }
    
    private void InitializeModifiers()
    {
        parser?.InitializeModifiers(this);
    }

    private void EnsureInitialized()
    {
        if (!isInitialized)
            InitializeComponents();
    }

    #endregion

    #region State Management

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
        cachedCleanText = null;
        shaderChannelsConfigured = false;
        cachedRectWidth = 0;
        cachedRectHeight = 0;
        dirtyFlags = UniTextDirtyFlags.All;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Принудительная полная реинициализация (для OnValidate).
    /// </summary>
    private void ForceFullReinitialization()
    {
        parser?.DeinitializeModifiers(this);
        isInitialized = false;
        parser = null;
        processor = null;
        fontProvider = null;
        meshGenerator = null;
        cachedCleanText = null;
    }
#endif

    /// <summary>
    /// Помечает текст как требующий перестройки.
    /// </summary>
    public void SetDirty(UniTextDirtyFlags flags = UniTextDirtyFlags.All)
    {
        if (flags == UniTextDirtyFlags.None) return;

        var oldFlags = dirtyFlags;
        dirtyFlags |= flags;

        if (oldFlags == UniTextDirtyFlags.None)
        {
            SetVerticesDirty();
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
        if (dirtyFlags == UniTextDirtyFlags.None) return;
        if (isRebuilding) return;

        EnsureInitialized();
        if (!isInitialized) return;

        ExecuteRebuild();
    }

    private void ExecuteRebuild()
    {
        isRebuilding = true;
        try
        {
            var flags = dirtyFlags;
            dirtyFlags = UniTextDirtyFlags.None;

            if (RequiresFullRebuild(flags))
                RebuildFull(flags);
            else if (RequiresLayoutRebuild(flags))
                RebuildLayout();
            else
                RebuildMeshOnly();

            UpdateRendering();
        }
        finally
        {
            isRebuilding = false;
        }
    }

    private static bool RequiresFullRebuild(UniTextDirtyFlags flags)
        => (flags & UniTextDirtyFlags.FullRebuild) != 0;

    private static bool RequiresLayoutRebuild(UniTextDirtyFlags flags)
        => (flags & (UniTextDirtyFlags.Layout | UniTextDirtyFlags.FontSize)) != 0;

    /// <summary>
    /// Полная перестройка текста (text/font/direction изменились).
    /// </summary>
    private void RebuildFull(UniTextDirtyFlags flags)
    {
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
            cachedRectWidth = rect.width;
            cachedRectHeight = rect.height;

            // Parse attributes only when text changed
            string textToProcess;
            if ((flags & UniTextDirtyFlags.Text) != 0 || cachedCleanText == null)
            {
                parser?.ResetModifiers();
                textToProcess = parser != null ? parser.Parse(text) : text;
                cachedCleanText = textToProcess;
            }
            else
            {
                textToProcess = cachedCleanText;
            }

            var settings = CreateProcessSettings(rect);
            var glyphs = processor.Process(textToProcess.AsSpan(), settings, flags);

            lastResultWidth = processor.ResultWidth;
            lastResultHeight = processor.ResultHeight;

            GenerateMeshes(glyphs, rect);
        }
        catch (Exception ex)
        {
            Debug.LogError($"UniText: RebuildFull failed: {ex.Message}");
            lastMeshPairs = null;
        }
    }

    /// <summary>
    /// Перестройка layout (rect size изменился).
    /// </summary>
    private void RebuildLayout()
    {
        var rt = GetComponent<RectTransform>();
        if (rt == null) return;

        var rect = rt.rect;

        // Check if rect actually changed
        if (Mathf.Approximately(rect.width, cachedRectWidth) &&
            Mathf.Approximately(rect.height, cachedRectHeight))
        {
            RebuildMeshOnly();
            return;
        }

        // Full rebuild with layout flag
        RebuildFull(UniTextDirtyFlags.Layout);
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

        var rt = GetComponent<RectTransform>();
        if (rt == null) return;

        ReleaseMeshes();
        GenerateMeshes(glyphs, rt.rect);
    }

    private void GenerateMeshes(ReadOnlySpan<PositionedGlyph> glyphs, Rect rect)
    {
        meshGenerator.FontSize = fontSize;
        meshGenerator.DefaultColor = color;
        meshGenerator.SetCanvasParameters(transform, cachedCanvas);
        meshGenerator.SetRectOffset(rect);

        lastMeshPairs = meshGenerator.GenerateMeshes(glyphs, cachedMeshProvider);
    }

    private TextProcessSettings CreateProcessSettings(Rect rect)
    {
        return new TextProcessSettings
        {
            maxWidth = rect.width,
            maxHeight = rect.height,
            fontSize = fontSize,
            baseDirection = baseDirection,
            enableWordWrap = enableWordWrap,
            horizontalAlignment = horizontalAlignment,
            verticalAlignment = verticalAlignment
        };
    }

    #endregion

    #region Rendering

    private void UpdateRendering()
    {
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

        UpdateSubMeshes();
    }

    private void ApplyMaterial()
    {
        var cr = GetComponent<CanvasRenderer>();
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
        renderer.SetMaterial(GetModifiedMaterial(material), 0);
        renderer.SetTexture(material.mainTexture);
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
        parser?.DeinitializeModifiers(this);
        ClearAllRenderers();
        ReleaseMeshes();
    }

    /// <summary>
    /// Полная очистка ресурсов (OnDestroy).
    /// </summary>
    private void CleanupResources()
    {
        parser?.DeinitializeModifiers(this);
        DestroySubMeshObjects();
        ReleaseMeshes();
        lastMeshPairs = null;
    }

    private void ClearAllRenderers()
    {
        var cr = GetComponent<CanvasRenderer>();
        cr?.Clear();

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
