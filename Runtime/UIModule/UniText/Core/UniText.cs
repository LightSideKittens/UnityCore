using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Компонент для отображения текста с полной Unicode поддержкой.
/// Использует UniTextFontAsset для рендеринга и собственный pipeline для:
/// - BiDi (правильное отображение RTL текста)
/// - Line breaking по Unicode правилам
/// - Script detection
/// - Rich text parsing
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

    [SerializeField]
    private bool enableRichText = true;

    [Header("Alignment")]
    [SerializeField]
    private HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left;

    [SerializeField]
    private VerticalAlignment verticalAlignment = VerticalAlignment.Top;

    // Runtime components
    private TextProcessor processor;
    private UniTextFontProvider fontProvider;
    private UniTextMeshGenerator meshGenerator;

    private bool isDirty = true;

    // Cached results
    private float lastResultWidth;
    private float lastResultHeight;

    // Sub-mesh objects для fallback шрифтов
    private readonly List<UniTextSubMesh> subMeshObjects = new();
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
                SetDirty();
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
                SetDirty();
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
                SetDirty();
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
                SetDirty();
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
                SetDirty();
            }
        }
    }

    public bool EnableRichText
    {
        get => enableRichText;
        set
        {
            if (enableRichText != value)
            {
                enableRichText = value;
                SetDirty();
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
                SetDirty();
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
                SetDirty();
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
        isDirty = true;
        processor = null;
        fontProvider = null;
        meshGenerator = null;
        cachedCanvas = null;
        shaderChannelsConfigured = false;

        CollectExistingSubMeshes();
        EnsureCanvasShaderChannels();
        EnsureMaterialAssigned();
    }

    /// <summary>
    /// Собирает существующие sub-mesh объекты после рекомпиляции.
    /// </summary>
    private void CollectExistingSubMeshes()
    {
        subMeshObjects.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var subMesh = child.GetComponent<UniTextSubMesh>();
            if (subMesh != null)
            {
                subMeshObjects.Add(subMesh);
            }
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetDirty();
    }
#endif

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetDirty();
    }

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();
        cachedCanvas = null;
        shaderChannelsConfigured = false;
        SetDirty();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Очистка sub-mesh объектов
        foreach (var subMesh in subMeshObjects)
        {
            if (subMesh != null)
            {
                if (Application.isPlaying)
                    Destroy(subMesh.gameObject);
                else
                    DestroyImmediate(subMesh.gameObject);
            }
        }
        subMeshObjects.Clear();

        // Возвращаем mesh'и в SharedMeshPool
        ReleaseMeshes();

        lastMeshPairs = null;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

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
            cachedMeshProvider = () => GetPooledMesh("UniText Mesh");
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
    /// Помечает текст как требующий перестройки.
    /// Перестройка произойдёт через систему CanvasUpdateRegistry (Graphic.Rebuild).
    /// </summary>
    public void SetDirty()
    {
        if (!isDirty)
        {
            isDirty = true;
            SetVerticesDirty();
        }
    }

    public void ForceMeshUpdate()
    {
        if (!isActiveAndEnabled || processor == null)
        {
            isDirty = true;
            return;
        }
        PerformRebuild();
    }

    private bool isRebuilding;

    protected override void OnPopulateMesh(VertexHelper vh) => vh.Clear();
    protected override void UpdateGeometry() { }

    public override void Rebuild(CanvasUpdate update)
    {
        base.Rebuild(update);

        if (update != CanvasUpdate.PreRender || !isDirty || isRebuilding)
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
            RebuildText();
            UpdateCanvasRenderer();
            isDirty = false;
        }
        finally
        {
            isRebuilding = false;
        }
    }

    private void UpdateCanvasRenderer()
    {
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
            canvasRenderer.SetMesh(firstPair.mesh);
            ApplyMaterial();
        }
        else
        {
            canvasRenderer.Clear();
        }

        UpdateSubMeshes(lastMeshPairs);
    }

    private void UpdateSubMeshes(List<UniTextMeshPair> meshPairs)
    {
        int requiredCount = meshPairs.Count - 1;
        int existingCount = subMeshObjects.Count;

        // Hide unused
        for (int i = requiredCount; i < existingCount; i++)
        {
            var subMesh = subMeshObjects[i];
            if (subMesh != null)
            {
                subMesh.Clear();
                subMesh.gameObject.SetActive(false);
            }
        }

        // Update or create required
        for (int i = 0; i < requiredCount; i++)
        {
            var pair = meshPairs[i + 1];

            if (i < existingCount)
            {
                var subMesh = subMeshObjects[i];
                if (subMesh != null)
                {
                    if (!subMesh.gameObject.activeSelf)
                        subMesh.gameObject.SetActive(true);
                    subMesh.SetMeshAndMaterial(pair.mesh, pair.material);
                    continue;
                }
            }

            var newSubMesh = CreateSubMesh(i + 1, pair.mesh, pair.material);
            if (i < existingCount)
                subMeshObjects[i] = newSubMesh;
            else
                subMeshObjects.Add(newSubMesh);
        }
    }

    private void ApplyMaterial()
    {
        if (primaryMaterial == null)
        {
            canvasRenderer.Clear();
            return;
        }

        canvasRenderer.materialCount = 1;
        canvasRenderer.SetMaterial(materialForRendering, 0);
        canvasRenderer.SetTexture(mainTexture);
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
        canvasRenderer?.SetMesh(null);

        int count = subMeshObjects.Count;
        for (int i = 0; i < count; i++)
        {
            var subMesh = subMeshObjects[i];
            if (subMesh != null)
                subMesh.Clear();
        }
    }

    private UniTextSubMesh CreateSubMesh(int index, Mesh mesh, Material material)
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

        go.AddComponent<CanvasRenderer>();
        var subMesh = go.AddComponent<UniTextSubMesh>();
        subMesh.Initialize(this);
        subMesh.SetMeshAndMaterial(mesh, material);

        return subMesh;
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

    private void RebuildText()
    {
        if (processor == null || fontProvider == null || meshGenerator == null)
        {
            Initialize();
            if (processor == null) return;
        }

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
            var rect = rectTransform.rect;
            var settings = new TextProcessSettings
            {
                maxWidth = enableWordWrap ? rect.width : float.MaxValue,
                maxHeight = rect.height,
                fontSize = fontSize,
                baseDirection = baseDirection,
                enableRichText = enableRichText,
                enableWordWrap = enableWordWrap,
                horizontalAlignment = horizontalAlignment,
                verticalAlignment = verticalAlignment
            };

            var glyphs = processor.Process(text.AsSpan(), settings);
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
