using System;
using System.Collections.Generic;
using LSCore;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;
using UnityEngine.Rendering;


[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public partial class UniText : MaskableGraphic
{
    [Flags]
    public enum DirtyFlags
    {
        None = 0,


        Color = 1 << 0,


        Alignment = 1 << 1,


        Layout = 1 << 2,


        FontSize = 1 << 3,


        Font = 1 << 4,


        Direction = 1 << 5,


        Text = 1 << 6,

        LayoutRebuild = Layout | FontSize | Alignment,

        FullRebuild = Text | Font | Direction,

        All = Color | Alignment | Layout | FontSize | FullRebuild
    }

    #region Serialized Fields

    [TextArea(3, 10)] [SerializeField] private string text = "";

    [Header("Font")] [SerializeField] private UniTextFontAsset font;

    [SerializeField] private float fontSize = 36f;

    [Header("Layout")] [SerializeField] private TextDirection baseDirection = TextDirection.Auto;

    [SerializeField] private bool enableWordWrap = true;

    [Header("Alignment")] [SerializeField] private HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left;

    [SerializeField] private VerticalAlignment verticalAlignment = VerticalAlignment.Top;

    [Header("Auto Size")] [SerializeField] private bool enableAutoSize;

    [SerializeField] private float minFontSize = 10f;

    [SerializeField] private float maxFontSize = 72f;

    [SerializeReference] private List<ModRegister> modRegisters;

    #endregion

    #region Runtime Components

    private TextProcessor processor;
    private UniTextFontProvider fontProvider;
    private UniTextMeshGenerator meshGenerator;
    private AttributeParser parser;
    private UniTextBuffers buffers;


    public UniTextFontProvider FontProvider => fontProvider;
    public string CleanText => parser?.CleanText ?? text;

    #endregion

    #region State & Caching

    private bool isInitialized;
    private bool isRebuilding;
    private bool shaderChannelsConfigured;

    private DirtyFlags dirtyFlags = DirtyFlags.All;

    private float lastResultWidth;
    private float lastResultHeight;
    private bool textIsParsed;

    private float autoSizedFontSize;
    private float lastAutoSizeWidth;
    private float lastAutoSizeHeight;
    private bool hasValidAutoSize;

    private float layoutCachedFontSize;
    private float layoutCachedWidth;
    private bool hasValidLayoutCache;

    private float cachedPreferredWidth;
    private float cachedPreferredHeight;
    private float cachedPreferredHeightForWidth;
    private bool hasValidPreferredWidth;
    private bool hasValidPreferredHeight;

    private readonly List<CanvasRenderer> subMeshRenderers = new();
    private readonly List<Material> subMeshStencilMaterials = new();
    private LSList<UniTextMeshPair> lastMeshPairs;

    private readonly List<Mesh> acquiredMeshes = new();

    public event Action Rebuilding;
    private Func<Mesh> cachedMeshProvider;

    private Material primaryMaterial;

    private Rect cachedClipRect;
    private bool cachedValidClip;
    private Vector4 cachedClipSoftness;

    #endregion

    #region Properties (Public API)

    public TextProcessor TextProcessor => processor;
    public UniTextMeshGenerator MeshGenerator => meshGenerator;
    public UniTextBuffers Buffers => buffers;

    public Vector2 LastResultSize => new(lastResultWidth, lastResultHeight);


    public ReadOnlySpan<PositionedGlyph> LastResultGlyphs =>
        processor != null ? processor.PositionedGlyphs : ReadOnlySpan<PositionedGlyph>.Empty;

    public string Text
    {
        get => text;
        set
        {
            if (text == value) return;

            var wasEmpty = string.IsNullOrEmpty(text);
            var willBeEmpty = string.IsNullOrEmpty(value);

            text = value;

            if (wasEmpty && !willBeEmpty)
                OnTextAppeared();
            else if (!wasEmpty && willBeEmpty)
                OnTextDisappeared();
            else
                SetDirty(DirtyFlags.Text);
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

    public void RegisterModifier(ModRegister register)
    {
        if (register == null || !register.IsValid) return;

        modRegisters ??= new List<ModRegister>();
        modRegisters.Add(register);

        if (isInitialized)
        {
            EnsureParser();
            register.Register(parser);
            register.modifier.Initialize(this);
            SetDirty(DirtyFlags.Text);
        }
    }

    public void UnregisterModifier(ModRegister register)
    {
        if (register == null) return;

        modRegisters?.Remove(register);

        if (isInitialized && parser != null)
        {
            register.modifier.Deinitialize();
            parser.Unregister(register.modifier);
            SetDirty(DirtyFlags.Text);
        }
    }

    private void EnsureParser()
    {
        if (parser != null) return;
        parser = new AttributeParser();
        processor.Parsed += parser.Apply;
    }

    #region Lifecycle

    protected override void OnEnable()
    {
        base.OnEnable();
        ResetAfterDomainReload();
        CollectExistingSubMeshRenderers();
        EnsureMaterialAssigned();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Cleanup();
        buffers?.ReturnBuffers();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        CleanupResources();
    }


    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetDirty(DirtyFlags.Layout);
    }

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();
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
        buffers = new UniTextBuffers();
        buffers.RentBuffers(text?.Length ?? 0);
        processor = new TextProcessor(buffers);

        if (modRegisters == null || modRegisters.Count == 0) return;

        parser = new AttributeParser();
        for (var i = 0; i < modRegisters.Count; i++)
        {
            var mod = modRegisters[i];
            if (mod is { IsValid: true }) mod.Register(parser);
        }

        processor.Parsed += parser.Apply;
    }


    private void RebuildFontProvider()
    {
        if (font == null) return;

        fontProvider = new UniTextFontProvider(font);
        meshGenerator = new UniTextMeshGenerator(fontProvider, buffers);
        processor?.SetFontProvider(fontProvider);
    }

    private void EnsureInitialized()
    {
        if (!isInitialized) InitializeComponents();
    }

    #endregion

    #region State Management

    private void OnTextAppeared()
    {
        ResetAfterDomainReload();
        CollectExistingSubMeshRenderers();
        EnsureMaterialAssigned();
        SetVerticesDirty();
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }


    private void OnTextDisappeared()
    {
        Cleanup();
        buffers?.ReturnBuffers();
        SetDirty(DirtyFlags.Text);
    }


    private void ResetAfterDomainReload()
    {
        isInitialized = false;
        processor = null;
        parser = null;
        fontProvider = null;
        meshGenerator = null;
        cachedMeshProvider = null;
        textIsParsed = false;
        shaderChannelsConfigured = false;
        hasValidAutoSize = false;
        hasValidLayoutCache = false;
        hasValidPreferredWidth = false;
        hasValidPreferredHeight = false;
        dirtyFlags = DirtyFlags.All;
    }

#if UNITY_EDITOR

    public void ForceFullReinitialization()
    {
        parser?.DeinitializeModifiers();
        buffers?.ReturnBuffers();
        isInitialized = false;
        parser = null;
        processor = null;
        fontProvider = null;
        meshGenerator = null;
        buffers = null;
        textIsParsed = false;
        hasValidAutoSize = false;
        hasValidLayoutCache = false;
        hasValidPreferredWidth = false;
        hasValidPreferredHeight = false;
    }
#endif


    public void SetDirtyAll()
    {
        SetDirty(DirtyFlags.All);
    }

    public void SetDirty(DirtyFlags flags)
    {
        if (flags == DirtyFlags.None) return;

        var oldFlags = dirtyFlags;
        dirtyFlags |= flags;

        if ((flags & DirtyFlags.FullRebuild) != 0)
        {
            processor?.InvalidateShapingData();
            textIsParsed = false;
        }
        else if ((flags & DirtyFlags.Layout) != 0)
        {
            processor?.InvalidateLayoutData();
        }

        hasValidPreferredWidth = false;
        hasValidPreferredHeight = false;

        if (oldFlags == DirtyFlags.None) SetVerticesDirty();

        if ((flags & (DirtyFlags.Text | DirtyFlags.Font | DirtyFlags.FontSize | DirtyFlags.Layout)) != 0)
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    #endregion

    #region Rebuild Pipeline

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
    }

    protected override void UpdateGeometry()
    {
    }
    
    public override void Rebuild(CanvasUpdate update)
    {
        base.Rebuild(update);

        if (update != CanvasUpdate.PreRender) return;
        if (dirtyFlags == DirtyFlags.None) return;
        if (isRebuilding) return;
        
        EnsureInitialized();
        if (!isInitialized) return;
        
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
    {
        return (flags & DirtyFlags.FullRebuild) != 0;
    }

    private static bool RequiresLayoutRebuild(DirtyFlags flags)
    {
        return (flags & DirtyFlags.LayoutRebuild) != 0;
    }


    private void RebuildFull()
    {
        var rt = rectTransform;
        if (rt == null) return;

        ReleaseMeshes();
        hasValidAutoSize = false;
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
            Profiler.BeginSample("TryParseAttributes");
            var textSpan = TryParseAttributes();
            Profiler.EndSample();

            var effectiveFontSize = fontSize;
            if (enableAutoSize)
                effectiveFontSize = CalculateAutoSize(textSpan, rect);
            else
                autoSizedFontSize = fontSize;

            var settings = CreateProcessSettings(rect, effectiveFontSize);
            Profiler.BeginSample("processor.Process");
            var glyphs = processor.Process(textSpan, settings);
            Profiler.EndSample();
            lastResultWidth = processor.ResultWidth;
            lastResultHeight = processor.ResultHeight;
            Profiler.BeginSample("GenerateMeshes");
            GenerateMeshes(glyphs, rect, effectiveFontSize);
            Profiler.EndSample();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            lastMeshPairs = null;
        }
    }

    private ReadOnlySpan<char> TryParseAttributes()
    {
        if (!textIsParsed)
        {
            parser?.ResetModifiers();
            parser?.Parse(text);
            textIsParsed = true;
        }

        var textSpan = parser != null ? parser.CleanTextSpan : text.AsSpan();
        return textSpan;
    }


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

        var textSpan = parser != null ? parser.CleanTextSpan : text.AsSpan();

        var effectiveFontSize = fontSize;
        if (enableAutoSize)
        {
            var canReuseAutoSize = hasValidAutoSize &&
                                   Mathf.Approximately(lastAutoSizeWidth, rect.width) &&
                                   Mathf.Approximately(lastAutoSizeHeight, rect.height);

            if (canReuseAutoSize)
                effectiveFontSize = autoSizedFontSize;
            else
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
        meshGenerator.SetCanvasParameters(transform, canvas);
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


    private float CalculateAutoSize(ReadOnlySpan<char> textSpan, Rect rect)
    {
        if (processor == null || fontProvider == null)
            return fontSize;

        var baseSettings = CreateProcessSettings(rect, maxFontSize);
        processor.EnsureShaping(textSpan, baseSettings);

        var optimalSize = processor.FindOptimalFontSize(
            minFontSize,
            maxFontSize,
            rect.width,
            rect.height,
            baseSettings);

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
            return;

        cr.materialCount = 1;
        cr.SetMaterial(materialForRendering, 0);
        cr.SetTexture(mainTexture);
    }

    protected override void UpdateMaterial()
    {
        ApplyMaterial();
    }

    private void EnsureMaterialAssigned()
    {
        if (primaryMaterial == null && font?.material != null)
        {
            primaryMaterial = font.material;
            ApplyMaterial();
        }
    }

    private Material GetActiveMaterial()
    {
        return primaryMaterial ?? font?.material;
    }

    private Texture GetActiveTexture()
    {
        return GetActiveMaterial()?.mainTexture;
    }

    public override void SetClipRect(Rect clipRect, bool validRect)
    {
        base.SetClipRect(clipRect, validRect);

        cachedClipRect = clipRect;
        cachedValidClip = validRect;

        var count = subMeshRenderers.Count;
        for (var i = 0; i < count; i++)
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

        var count = subMeshRenderers.Count;
        for (var i = 0; i < count; i++)
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
        var cull = cr != null && cr.cull;

        var count = subMeshRenderers.Count;
        for (var i = 0; i < count; i++)
        {
            var renderer = subMeshRenderers[i];
            if (renderer != null)
                renderer.cull = cull;
        }
    }

    public override void RecalculateMasking()
    {
        base.RecalculateMasking();
        ReleaseSubMeshStencilMaterials();
        SetVerticesDirty();
    }

    #endregion

    #region Sub-mesh Management

    private void CollectExistingSubMeshRenderers()
    {
        subMeshRenderers.Clear();
        for (var i = 0; i < transform.childCount; i++)
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

        var requiredCount = lastMeshPairs.Count - 1;
        var existingCount = subMeshRenderers.Count;

        for (var i = requiredCount; i < existingCount; i++)
        {
            var renderer = subMeshRenderers[i];
            if (renderer != null)
            {
                renderer.Clear();
                renderer.gameObject.SetActive(false);
            }
        }

        for (var i = 0; i < requiredCount; i++)
        {
            var pair = lastMeshPairs[i + 1];

            if (i < existingCount)
            {
                var renderer = subMeshRenderers[i];
                if (renderer != null)
                {
                    if (!renderer.gameObject.activeSelf)
                        renderer.gameObject.SetActive(true);
                    var subRT = renderer.GetComponent<RectTransform>();
                    if (subRT != null)
                        subRT.pivot = rectTransform.pivot;
                    SetSubMeshRendererData(renderer, pair.mesh, pair.material, i);
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

    private void SetSubMeshRendererData(CanvasRenderer renderer, Mesh mesh, Material material, int subMeshIndex)
    {
        if (mesh == null || mesh.vertexCount == 0)
        {
            renderer.Clear();
            return;
        }

        renderer.SetMesh(mesh);
        renderer.materialCount = 1;

        var matToUse = material;
        if (maskable && material != null)
        {
            var rootCanvas = MaskUtilities.FindRootSortOverrideCanvas(transform);
            var stencilDepth = MaskUtilities.GetStencilDepth(transform, rootCanvas);
            if (stencilDepth > 0)
            {
                var stencilId = (1 << stencilDepth) - 1;
                var stencilMat = StencilMaterial.Add(material, stencilId, StencilOp.Keep, CompareFunction.Equal,
                    ColorWriteMask.All, stencilId, 0);

                while (subMeshStencilMaterials.Count <= subMeshIndex)
                    subMeshStencilMaterials.Add(null);

                if (subMeshStencilMaterials[subMeshIndex] != null)
                    StencilMaterial.Remove(subMeshStencilMaterials[subMeshIndex]);

                subMeshStencilMaterials[subMeshIndex] = stencilMat;
                matToUse = stencilMat;
            }
        }

        renderer.SetMaterial(matToUse, 0);
        var tex = material != null ? material.mainTexture : null;
        renderer.SetTexture(tex);
    }

    private CanvasRenderer CreateSubMeshRenderer(int index, Mesh mesh, Material material)
    {
        var go = new GameObject($"UniText SubMesh [{index}]")
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        go.transform.SetParent(transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.pivot = rectTransform.pivot;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var renderer = go.AddComponent<CanvasRenderer>();
        SetSubMeshRendererData(renderer, mesh, material, index - 1);

        if (cachedValidClip)
            renderer.EnableRectClipping(cachedClipRect);
        renderer.clippingSoftness = cachedClipSoftness;

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

    private Mesh GetPooledMeshForText()
    {
        return GetPooledMesh("UniText Mesh");
    }

    #endregion

    #region Cleanup

    private void Cleanup()
    {
        parser?.DeinitializeModifiers();
        ClearAllRenderers();
        ReleaseSubMeshStencilMaterials();
        ReleaseMeshes();
    }


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

        var count = subMeshRenderers.Count;
        for (var i = 0; i < count; i++) subMeshRenderers[i]?.Clear();
    }

    private void ReleaseSubMeshStencilMaterials()
    {
        for (var i = 0; i < subMeshStencilMaterials.Count; i++)
            if (subMeshStencilMaterials[i] != null)
            {
                StencilMaterial.Remove(subMeshStencilMaterials[i]);
                subMeshStencilMaterials[i] = null;
            }

        subMeshStencilMaterials.Clear();
    }

    private void DestroySubMeshObjects()
    {
        ReleaseSubMeshStencilMaterials();

        foreach (var renderer in subMeshRenderers)
            if (renderer != null)
            {
                if (Application.isPlaying)
                    Destroy(renderer.gameObject);
                else
                    DestroyImmediate(renderer.gameObject);
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

    #endregion
}