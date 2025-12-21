using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;
using UnityEngine.Rendering;


[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public partial class UniText : MaskableGraphic, ISerializationCallbackReceiver
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
        Material = 1 << 7,
        LayoutRebuild = Layout | FontSize | Alignment,
        FullRebuild = Text | Font | Direction,
        All = Color | Alignment | Layout | FontSize | FullRebuild
    }

    #region Serialized Fields

    [TextArea(3, 10)] [SerializeField] private string text = "";
    [Header("Font")] [SerializeField] private UniTextFonts fonts;
    [SerializeField] private UniTextAppearance appearance;
    [SerializeField] private float fontSize = 36f;
    [Header("Layout")] [SerializeField] private TextDirection baseDirection = TextDirection.Auto;
    [SerializeField] private bool enableWordWrap = true;
    [Header("Alignment")] [SerializeField] private HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left;
    [SerializeField] private VerticalAlignment verticalAlignment = VerticalAlignment.Top;
    [Header("Auto Size")] [SerializeField] private bool enableAutoSize;
    [SerializeField] private float minFontSize = 10f;
    [SerializeField] private float maxFontSize = 72f;
    [SerializeReference] private List<ModRegister> modRegisters = new();

    #endregion

    #region Runtime State

    private TextProcessor textProcessor;
    private UniTextFontProvider fontProvider;
    private UniTextMeshGenerator meshGenerator;
    private AttributeParser attributeParser;
    private UniTextBuffers buffers;

    private DirtyFlags dirtyFlags = DirtyFlags.All;
    private bool textIsParsed;

    private float lastResultWidth;
    private float lastResultHeight;
    private float autoSizedFontSize;
    private float lastAutoSizeWidth;
    private float lastAutoSizeHeight;
    private bool hasValidAutoSize;

    private readonly List<CanvasRenderer> subMeshRenderers = new();
    private readonly List<Material> subMeshStencilMaterials = new();
    private readonly List<Mesh> acquiredMeshes = new();
    private PooledList<UniTextRenderData> lastMeshPairs;
    
    private Rect cachedClipRect;
    private bool cachedValidClip;
    private Vector4 cachedClipSoftness;

    private float lastKnownWidth = -1;

    public event Action Rebuilding;

    #endregion

    #region Public API

    public TextProcessor TextProcessor => textProcessor;
    public UniTextMeshGenerator MeshGenerator => meshGenerator;
    public UniTextFontProvider FontProvider => fontProvider;
    public UniTextBuffers Buffers => buffers;
    public string CleanText => attributeParser?.CleanText ?? text;
    public Vector2 LastResultSize => new(lastResultWidth, lastResultHeight);
    public ReadOnlySpan<PositionedGlyph> LastResultGlyphs => textProcessor != null ? textProcessor.PositionedGlyphs : ReadOnlySpan<PositionedGlyph>.Empty;
    public UniTextFont MainFont => fonts?.MainFont;
    public float CurrentFontSize => enableAutoSize ? autoSizedFontSize : fontSize;

    public string Text
    {
        get => text;
        set
        {
            if (text == value) return;
            text = value;
            SetDirty(DirtyFlags.Text);
        }
    }

    public UniTextFonts Fonts
    {
        get => fonts;
        set
        {
            if (fonts == value) return;
#if UNITY_EDITOR
            UnlistenConfigChanged();
#endif

            fonts = value;
            
#if UNITY_EDITOR
            ListenConfigChanged();
#endif
            SetDirty(DirtyFlags.Font);
        }
    }

    public UniTextAppearance Appearance
    {
        get => appearance;
        set
        {
            if (appearance == value) return;
#if UNITY_EDITOR
            UnlistenConfigChanged();
#endif

            appearance = value;
            
#if UNITY_EDITOR
            ListenConfigChanged();
#endif
            SetDirty(DirtyFlags.Material);
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
            if (enableAutoSize) { hasValidAutoSize = false; SetDirty(DirtyFlags.Layout); }
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
            if (enableAutoSize) { hasValidAutoSize = false; SetDirty(DirtyFlags.Layout); }
        }
    }

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

    public void SetDirty(DirtyFlags flags)
    {
        if (flags == DirtyFlags.None) return;

        dirtyFlags |= flags;

        if ((flags & DirtyFlags.Font) != 0)
        {
            attributeParser?.DeinitializeModifiers();
            fontProvider = null;
            meshGenerator = null;
        }

        if ((flags & DirtyFlags.FullRebuild) != 0)
        {
            textProcessor?.InvalidateShapingData();
            textIsParsed = false;
            hasValidLayoutCache = false;
            hasValidAutoSize = false;
        }
        else if ((flags & (DirtyFlags.Layout | DirtyFlags.FontSize)) != 0)
        {
            textProcessor?.InvalidateLayoutData();
            hasValidAutoSize = false;
            hasValidLayoutCache = false;
        }
        else if ((flags & DirtyFlags.Alignment) != 0)
        {
            textProcessor?.InvalidatePositionedGlyphs();
        }

        if ((flags & ~(DirtyFlags.Material | DirtyFlags.Alignment | DirtyFlags.Color)) != 0)
        {
            hasValidPreferredWidth = false;
            hasValidPreferredHeight = false;
        }

        SetVerticesDirty();

        if ((flags & (DirtyFlags.Text | DirtyFlags.Font | DirtyFlags.FontSize | DirtyFlags.Layout)) != 0)
        {
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
    }

    private bool wasDirtied;
    
    public override void SetVerticesDirty()
    {
        if(wasDirtied) return;
        wasDirtied = true;
        base.SetVerticesDirty();
    }

    #endregion

    #region Modifiers
    
    public void RegisterModifier(ModRegister register)
    {
        if (!register.IsValid) return;
        modRegisters.Add(register);

        if (textProcessor != null)
        {
            EnsureAttributeParserCreated();
            register.Register(attributeParser);
            register.modifier.Initialize(this);
            SetDirty(DirtyFlags.Text);
        }
    }

    public void UnregisterModifier(ModRegister register)
    {
        modRegisters.Remove(register);

        if (attributeParser != null)
        {
            register.modifier.Deinitialize();
            attributeParser.Unregister(register.modifier);
            SetDirty(DirtyFlags.Text);
        }

        if (modRegisters.Count == 0)
        {
            DestroyAttributeParser();
        }
    }

    public void ReInitModifiers()
    {
        DestroyAttributeParser();
        EnsureAttributeParserCreated();
    }
    
    private void EnsureAttributeParserCreated()
    {
        if (attributeParser != null) return;
        if(textProcessor == null) return;
        
        if (modRegisters is { Count: > 0 })
        {
            attributeParser = new AttributeParser();
            for (var i = 0; i < modRegisters.Count; i++)
            {
                var mod = modRegisters[i];
                if (mod is { IsValid: true }) mod.Register(attributeParser);
            }
            textProcessor.Parsed += attributeParser.Apply;
            attributeParser.InitializeModifiers(this);
            SetDirty(DirtyFlags.Text);
        }
    }

    private void DestroyAttributeParser()
    {
        if(attributeParser == null) return;
        
        attributeParser.DeinitializeModifiers();
        attributeParser.Release();
        if (textProcessor != null)
        {
            textProcessor.Parsed -= attributeParser.Apply;
        }

        attributeParser = null;
        SetDirty(DirtyFlags.Text);
    }

    #endregion

    
    #region Lifecycle

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        attributeParser?.InitializeModifiers(this);
    }
#endif
    
    protected override void OnEnable()
    {
        base.OnEnable();
        ListenConfigChanged();
        CollectExistingSubMeshRenderers();
        SetDirty(DirtyFlags.All);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UnlistenConfigChanged();
        ClearAllRenderers();
        DeInit();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DeInit();
    }
    
    private void DeInit()
    {
        attributeParser?.DeinitializeModifiers();
        attributeParser?.Release();
        ReleaseSubMeshStencilMaterials();
        ReleaseMeshes();
        buffers?.EnsureReturnBuffers();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        var width = rectTransform.rect.width;

        if (!Mathf.Approximately(width, lastKnownWidth))
        {
            lastKnownWidth = width;
            SetDirty(DirtyFlags.Layout);
        }
        else
        {
            // Height only changed - trigger RebuildLayout for auto size / vertical alignment
            // but skip InvalidateLayoutData (lines don't depend on height)
            // Alignment flag triggers RebuildLayout without InvalidateLayoutData
            SetDirty(DirtyFlags.Alignment);
        }
    }

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();
        SetDirty(DirtyFlags.Layout);
    }

#if UNITY_EDITOR
    
    private void ListenConfigChanged()
    {
        if (fonts != null) fonts.Changed += OnConfigChanged;
        if (appearance != null) appearance.Changed += OnConfigChanged;
    }

    private void UnlistenConfigChanged()
    {
        if (fonts != null) fonts.Changed -= OnConfigChanged;
        if (appearance != null) appearance.Changed -= OnConfigChanged;
    }

    private void OnConfigChanged()
    {
        SetDirty(DirtyFlags.All);
    }
#endif

    #endregion

    #region Rebuild

    public override void Rebuild(CanvasUpdate update)
    {
        if (update != CanvasUpdate.PreRender) return;
        if (dirtyFlags == DirtyFlags.None) return;

        Profiler.BeginSample("UniText.Rebuild");

        if (!ValidateAndInitialize())
        {
            Profiler.EndSample();
            return;
        }

        Rebuilding?.Invoke();

        var flags = dirtyFlags;
        dirtyFlags = DirtyFlags.None;

        if ((flags & DirtyFlags.FullRebuild) != 0) RebuildFull();
        else if ((flags & DirtyFlags.LayoutRebuild) != 0) RebuildLayout();
        else if ((flags & DirtyFlags.Material) != 0) UpdateMaterialsOnly();
        else RebuildMeshOnly();

        UpdateRendering();
        
        wasDirtied = false;
        Profiler.EndSample();
    }
    
    private bool ValidateAndInitialize()
    {
        Profiler.BeginSample("UniText.ValidateAndInitialize");

        if (!UnicodeData.IsInitialized)
        {
            UnicodeData.EnsureInitialized();
            if (!UnicodeData.IsInitialized)
            {
                Debug.LogError("UniText: Unicode data not initialized.");
                Profiler.EndSample();
                return false;
            }
        }

        if (!TryInitFontsAndAppearance())
        {
            Profiler.EndSample();
            return false;
        }

        buffers ??= new UniTextBuffers();
        buffers.EnsureRentBuffers(text.Length);

        if (textProcessor == null)
        {
            textProcessor = new TextProcessor(buffers);
            EnsureAttributeParserCreated();
        }

        if (fontProvider == null)
        {
            fontProvider = new UniTextFontProvider(fonts, appearance);
            meshGenerator = new UniTextMeshGenerator(fontProvider, buffers);
            textProcessor.SetFontProvider(fontProvider);
        }

        Profiler.EndSample();
        return true;
    }

    private bool TryInitFontsAndAppearance()
    {
        if (fonts == null || appearance == null)
        {
            fonts = UniTextSettings.DefaultFonts;
            appearance = UniTextSettings.DefaultAppearance;
            if (fonts == null || appearance == null)
            {
                return false;
            }
        }    
        
        return true;
    }

    private void RebuildFull()
    {
        Profiler.BeginSample("UniText.RebuildFull");

        ReleaseMeshes();
        hasValidAutoSize = false;

        if (string.IsNullOrEmpty(text))
        {
            lastMeshPairs = null;
            lastResultWidth = lastResultHeight = 0;
            autoSizedFontSize = fontSize;
            Profiler.EndSample();
            return;
        }

        var rect = rectTransform.rect;
        var textSpan = ParseOrGetParsedAttributes();
        var effectiveFontSize = enableAutoSize ? CalculateAutoSize(textSpan, rect) : fontSize;
        if (!enableAutoSize) autoSizedFontSize = fontSize;

        var settings = CreateProcessSettings(rect, effectiveFontSize);

        var glyphs = textProcessor.Process(textSpan, settings);

        lastResultWidth = textProcessor.ResultWidth;
        lastResultHeight = textProcessor.ResultHeight;

        GenerateMeshes(glyphs, rect, effectiveFontSize);

        Profiler.EndSample();
    }

    private void RebuildLayout()
    {
        Profiler.BeginSample("UniText.RebuildLayout");

        ReleaseMeshes();

        if (string.IsNullOrEmpty(text))
        {
            lastMeshPairs = null;
            Profiler.EndSample();
            return;
        }

        var rect = rectTransform.rect;
        var textSpan = ParseOrGetParsedAttributes();

        var effectiveFontSize = fontSize;
        if (enableAutoSize)
        {
            var canReuse = hasValidAutoSize &&
                           Mathf.Approximately(lastAutoSizeWidth, rect.width) &&
                           Mathf.Approximately(lastAutoSizeHeight, rect.height);

            if (canReuse)
            {
                effectiveFontSize = autoSizedFontSize;
            }
            else if (hasValidPreferredHeight &&
                     Mathf.Approximately(cachedPreferredHeightForWidth, rect.width) &&
                     rect.height >= cachedPreferredHeight - 0.01f &&
                     (enableWordWrap || hasValidLayoutCache))
            {
                effectiveFontSize = enableWordWrap ? maxFontSize : layoutCachedFontSize;
                autoSizedFontSize = effectiveFontSize;
                lastAutoSizeWidth = rect.width;
                lastAutoSizeHeight = rect.height;
                hasValidAutoSize = true;
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
        var glyphs = textProcessor.Process(textSpan, settings);

        lastResultWidth = textProcessor.ResultWidth;
        lastResultHeight = textProcessor.ResultHeight;

        GenerateMeshes(glyphs, rect, effectiveFontSize);

        Profiler.EndSample();
    }

    private void RebuildMeshOnly()
    {
        var glyphs = textProcessor.PositionedGlyphs;
        if (glyphs.IsEmpty)
        {
            lastMeshPairs = null;
            return;
        }

        ReleaseMeshes();
        var effectiveFontSize = enableAutoSize ? autoSizedFontSize : fontSize;
        GenerateMeshes(glyphs, rectTransform.rect, effectiveFontSize);
    }

    private void UpdateMaterialsOnly()
    {
        if (lastMeshPairs == null || fontProvider == null) return;

        fontProvider.InvalidateMaterialCache();

        for (var i = 0; i < lastMeshPairs.Count; i++)
        {
            ref var pair = ref lastMeshPairs.buffer.data[i];
            pair.material = fontProvider.GetMaterial(pair.fontId);
        }
    }

    private ReadOnlySpan<char> ParseOrGetParsedAttributes()
    {
        if (!textIsParsed)
        {
            Profiler.BeginSample("UniText.ParseAttributes");
            attributeParser?.ResetModifiers();
            attributeParser?.Parse(text);
            textIsParsed = true;
            Profiler.EndSample();
        }
        
        return attributeParser != null ? attributeParser.CleanTextSpan : text.AsSpan();
    }

    private void GenerateMeshes(ReadOnlySpan<PositionedGlyph> glyphs, Rect rect, float effectiveFontSize = -1)
    {
        Profiler.BeginSample("UniText.GenerateMeshes");
        meshGenerator.FontSize = effectiveFontSize > 0 ? effectiveFontSize : fontSize;
        meshGenerator.DefaultColor = color;
        meshGenerator.SetCanvasParameters(transform, canvas);
        meshGenerator.SetRectOffset(rect);
        meshGenerator.SetHorizontalAlignment(horizontalAlignment);

        lastMeshPairs = meshGenerator.GenerateMeshes(glyphs, GetPooledMeshForText);
        Profiler.EndSample();
    }

    private TextProcessSettings CreateProcessSettings(Rect rect, float effectiveFontSize) => new()
    {
        MaxWidth = rect.width,
        MaxHeight = rect.height,
        HorizontalAlignment = horizontalAlignment,
        VerticalAlignment = verticalAlignment,
        fontSize = effectiveFontSize,
        baseDirection = baseDirection,
        enableWordWrap = enableWordWrap
    };

    private float CalculateAutoSize(ReadOnlySpan<char> textSpan, Rect rect)
    {
        var baseSettings = CreateProcessSettings(rect, maxFontSize);
        textProcessor.EnsureShaping(textSpan, baseSettings);

        var optimalSize = textProcessor.FindOptimalFontSize(minFontSize, maxFontSize, rect.width, rect.height, baseSettings);

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
        Profiler.BeginSample("UniText.UpdateRendering");

        canvasRenderer?.Clear();

        if (lastMeshPairs == null || lastMeshPairs.Count == 0)
        {
            ClearAllRenderers();
            Profiler.EndSample();
            return;
        }

        UpdateSubMeshes();

        Profiler.EndSample();
    }

    protected override void UpdateMaterial() { }

    public override void SetClipRect(Rect clipRect, bool validRect)
    {
        base.SetClipRect(clipRect, validRect);
        cachedClipRect = clipRect;
        cachedValidClip = validRect;

        for (var i = 0; i < subMeshRenderers.Count; i++)
        {
            var r = subMeshRenderers[i];
            if (r == null) continue;
            if (validRect) r.EnableRectClipping(clipRect);
            else r.DisableRectClipping();
        }
    }

    public override void SetClipSoftness(Vector2 clipSoftness)
    {
        base.SetClipSoftness(clipSoftness);
        cachedClipSoftness = new Vector4(clipSoftness.x, clipSoftness.y, 0, 0);

        for (var i = 0; i < subMeshRenderers.Count; i++)
            if (subMeshRenderers[i] != null)
                subMeshRenderers[i].clippingSoftness = cachedClipSoftness;
    }

    public override void Cull(Rect clipRect, bool validRect)
    {
        base.Cull(clipRect, validRect);
        var cull = canvasRenderer != null && canvasRenderer.cull;

        for (var i = 0; i < subMeshRenderers.Count; i++)
            if (subMeshRenderers[i] != null)
                subMeshRenderers[i].cull = cull;
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
            if (child.name.StartsWith("-_UTSM_-"))
            {
                var r = child.GetComponent<CanvasRenderer>();
                if (r != null) subMeshRenderers.Add(r);
            }
        }
    }

    private void UpdateSubMeshes()
    {
        Profiler.BeginSample("UniText.UpdateSubMeshes");

        if (lastMeshPairs == null)
        {
            Profiler.EndSample();
            return;
        }

        var requiredCount = lastMeshPairs.Count;
        var existingCount = subMeshRenderers.Count;

        for (var i = requiredCount; i < existingCount; i++)
        {
            var r = subMeshRenderers[i];
            if (r != null) { r.Clear(); r.gameObject.SetActive(false); }
        }

        for (var i = 0; i < requiredCount; i++)
        {
            var pair = lastMeshPairs[i];

            if (i < existingCount)
            {
                var r = subMeshRenderers[i];
                if (r != null)
                {
                    if (!r.gameObject.activeSelf) r.gameObject.SetActive(true);
                    var subRT = r.GetComponent<RectTransform>();
                    if (subRT != null) subRT.pivot = rectTransform.pivot;
                    SetSubMeshRendererData(r, pair.mesh, pair.material, pair.texture, i);
                    continue;
                }
            }

            var newR = CreateSubMeshRenderer(i, pair.mesh, pair.material, pair.texture);
            if (i < existingCount) subMeshRenderers[i] = newR;
            else subMeshRenderers.Add(newR);
        }

        Profiler.EndSample();
    }

    private void SetSubMeshRendererData(CanvasRenderer r, Mesh mesh, Material mat, Texture tex, int subMeshIndex)
    {
        if (mesh == null || mesh.vertexCount == 0) { r.Clear(); return; }

        r.SetMesh(mesh);
        r.materialCount = 1;

        var matToUse = mat;
        if (maskable && mat != null)
        {
            var rootCanvas = MaskUtilities.FindRootSortOverrideCanvas(transform);
            var stencilDepth = MaskUtilities.GetStencilDepth(transform, rootCanvas);
            if (stencilDepth > 0)
            {
                var stencilId = (1 << stencilDepth) - 1;
                var stencilMat = StencilMaterial.Add(mat, stencilId, StencilOp.Keep, CompareFunction.Equal, ColorWriteMask.All, stencilId, 0);

                while (subMeshStencilMaterials.Count <= subMeshIndex) subMeshStencilMaterials.Add(null);
                if (subMeshStencilMaterials[subMeshIndex] != null) StencilMaterial.Remove(subMeshStencilMaterials[subMeshIndex]);

                subMeshStencilMaterials[subMeshIndex] = stencilMat;
                matToUse = stencilMat;
            }
        }

        r.SetMaterial(matToUse, 0);
        r.SetTexture(tex);
    }

    
    private CanvasRenderer CreateSubMeshRenderer(int index, Mesh mesh, Material mat, Texture tex)
    {
        var go = new GameObject("-_UTSM_-") { hideFlags = HideFlags.HideAndDontSave };
        go.transform.SetParent(transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.pivot = rectTransform.pivot;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var r = go.AddComponent<CanvasRenderer>();
        SetSubMeshRendererData(r, mesh, mat, tex, index);

        if (cachedValidClip) r.EnableRectClipping(cachedClipRect);
        r.clippingSoftness = cachedClipSoftness;
        r.cull = subMeshRenderers.Count > 0 && subMeshRenderers[0] != null && subMeshRenderers[0].cull;

        return r;
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

    private Mesh GetPooledMeshForText()
    {
        var mesh = SharedMeshPool.Acquire("UniText Mesh");
        acquiredMeshes.Add(mesh);
        return mesh;
    }

    #endregion

    #region Cleanup

    private void ClearAllRenderers()
    {
        canvasRenderer?.Clear();
        for (var i = 0; i < subMeshRenderers.Count; i++) subMeshRenderers[i]?.Clear();
    }

    private void ReleaseSubMeshStencilMaterials()
    {
        for (var i = 0; i < subMeshStencilMaterials.Count; i++)
        {
            if (subMeshStencilMaterials[i] != null)
            {
                StencilMaterial.Remove(subMeshStencilMaterials[i]);
                subMeshStencilMaterials[i] = null;
            }
        }
        subMeshStencilMaterials.Clear();
    }

    #endregion

    void ISerializationCallbackReceiver.OnBeforeSerialize() { }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
#if UNITY_EDITOR
        EditorApplication.update += OnUpdate;

        void OnUpdate()
        { 
            if(Application.isPlaying) return;
            if(this ==  null) return;
            EditorApplication.update -= OnUpdate;
            ReInitModifiers();
        }
#endif
    }
}
