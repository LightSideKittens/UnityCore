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

    private void RebuildText()
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
