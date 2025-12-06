using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Компонент для отображения текста с полной Unicode поддержкой.
/// Использует TMP шрифты для рендеринга, но собственный pipeline для:
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
    private TMP_FontAsset font;

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

    [Header("Unicode Data")]
    [SerializeField]
    private TextAsset unicodeDataAsset;

    // Runtime components
    private TextProcessor processor;
    private TMPFontProvider fontProvider;
    private TMPMeshGenerator meshGenerator;
    private IUnicodeDataProvider unicodeData;

    private bool isDirty = true;

    // Cached results
    private float lastResultWidth;
    private float lastResultHeight;

    // Sub-mesh objects для fallback шрифтов
    private readonly List<UniTextSubMesh> subMeshObjects = new();
    private List<MeshMaterialPair> lastMeshPairs;

    // Материал для основного mesh (из первого MeshMaterialPair)
    private Material primaryMaterial;

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

    public TMP_FontAsset Font
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

        // После рекомпиляции или enable нужно пересобрать текст
        // так как runtime данные (lastMeshPairs, processor и т.д.) не сериализуются
        isDirty = true;

        // Восстанавливаем существующие sub-mesh объекты после рекомпиляции
        CollectExistingSubMeshes();

        // Настраиваем shader channels сразу при enable, а не при первом рендере
        // Это предотвращает сброс фокуса с текстового поля при первом вводе символа
        EnsureCanvasShaderChannels();

        // Назначаем материал сразу при enable, чтобы избежать перехода null→material
        // при первом вводе символа (это вызывает сброс фокуса в Editor)
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

        // Очистка mesh из lastMeshPairs
        if (lastMeshPairs != null)
        {
            foreach (var pair in lastMeshPairs)
            {
                if (pair.mesh != null)
                {
                    if (Application.isPlaying)
                        Destroy(pair.mesh);
                    else
                        DestroyImmediate(pair.mesh);
                }
            }
            lastMeshPairs = null;
        }
    }

    private void Initialize()
    {
        if (unicodeDataAsset == null)
        {
            Debug.LogWarning("UniText: Unicode data asset not assigned.");
            return;
        }

        if (font == null)
        {
            Debug.LogWarning("UniText: Font not assigned.");
            return;
        }

        try
        {
            unicodeData = new BinaryUnicodeDataProvider(unicodeDataAsset.bytes);
            processor = new TextProcessor(unicodeData);
            RebuildFontProvider();
        }
        catch (Exception ex)
        {
            Debug.LogError($"UniText: Failed to initialize: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void RebuildFontProvider()
    {
        if (font == null) return;

        fontProvider = new TMPFontProvider(font);
        meshGenerator = new TMPMeshGenerator(fontProvider, unicodeData);

        if (processor != null)
        {
            processor.SetFontProvider(fontProvider);
        }
    }

    /// <summary>
    /// Помечает текст как требующий перестройки и немедленно перестраивает его.
    /// </summary>
    public void SetDirty()
    {
        // Если компонент неактивен или не инициализирован - откладываем
        if (!isActiveAndEnabled || processor == null)
        {
            isDirty = true;
            return;
        }

        // Защита от рекурсии
        if (isRebuilding)
        {
            return;
        }

        isRebuilding = true;
        try
        {
            // Перестраиваем синхронно чтобы избежать мерцания
            RebuildText();
            UpdateCanvasRenderer();
            isDirty = false;
        }
        finally
        {
            isRebuilding = false;
        }
    }

    private bool isRebuilding;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        // Очищаем VertexHelper - мы используем mesh напрямую через CanvasRenderer
        vh.Clear();
    }

    /// <summary>
    /// Переопределяем чтобы Unity не перестраивал геометрию.
    /// Мы управляем mesh напрямую через CanvasRenderer.SetMesh().
    /// </summary>
    protected override void UpdateGeometry()
    {
        // Пустой - не даём Unity перестраивать геометрию
    }

    private void LateUpdate()
    {
        // Обрабатываем отложенный dirty (если SetDirty был вызван когда компонент был неактивен)
        if (isDirty)
        {
            RebuildText();
            UpdateCanvasRenderer();
            isDirty = false;
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

        // Первый mesh отображается на основном CanvasRenderer
        if (lastMeshPairs.Count > 0)
        {
            var firstPair = lastMeshPairs[0];
            if (firstPair.mesh != null && firstPair.mesh.vertexCount > 0)
            {
                // Сохраняем материал первого mesh для property material
                primaryMaterial = firstPair.material;

                // Устанавливаем mesh
                canvasRenderer.SetMesh(firstPair.mesh);

                // Применяем материал напрямую для немедленного отображения
                ApplyMaterial();
            }
            else
            {
                canvasRenderer.Clear();
            }
        }

        // Остальные mesh отображаются через sub-mesh objects
        UpdateSubMeshes(lastMeshPairs);
    }

    /// <summary>
    /// Обновляет sub-mesh объекты для fallback шрифтов.
    /// </summary>
    private void UpdateSubMeshes(List<MeshMaterialPair> meshPairs)
    {
        int requiredCount = meshPairs.Count - 1;

        // Скрываем лишние
        for (int i = requiredCount; i < subMeshObjects.Count; i++)
        {
            var subMesh = subMeshObjects[i];
            if (subMesh != null)
            {
                subMesh.Clear();
                subMesh.gameObject.SetActive(false);
            }
        }

        // Обновляем или создаём нужные
        for (int i = 0; i < requiredCount; i++)
        {
            var pair = meshPairs[i + 1];

            if (i < subMeshObjects.Count && subMeshObjects[i] != null)
            {
                // Переиспользуем существующий
                var subMesh = subMeshObjects[i];
                if (!subMesh.gameObject.activeSelf)
                {
                    subMesh.gameObject.SetActive(true);
                }
                subMesh.SetMeshAndMaterial(pair.mesh, pair.material);
            }
            else
            {
                // Создаём новый с данными сразу
                var subMesh = CreateSubMesh(i + 1, pair.mesh, pair.material);
                if (i < subMeshObjects.Count)
                {
                    subMeshObjects[i] = subMesh;
                }
                else
                {
                    subMeshObjects.Add(subMesh);
                }
            }
        }
    }

    /// <summary>
    /// Применяет материал к CanvasRenderer.
    /// </summary>
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

    /// <summary>
    /// Вызывается Unity UI системой для применения материала.
    /// </summary>
    protected override void UpdateMaterial()
    {
        ApplyMaterial();
    }

    /// <summary>
    /// Назначает материал шрифта на CanvasRenderer сразу, даже если текста нет.
    /// Это предотвращает переход null→material при первом вводе символа,
    /// который вызывает сброс фокуса в Editor.
    /// </summary>
    private void EnsureMaterialAssigned()
    {
        if (font == null || font.material == null) return;
        if (canvasRenderer == null) return;

        // Если материал ещё не назначен - назначаем материал шрифта
        if (primaryMaterial == null)
        {
            primaryMaterial = font.material;
            canvasRenderer.materialCount = 1;
            canvasRenderer.SetMaterial(materialForRendering, 0);
            canvasRenderer.SetTexture(mainTexture);
        }
    }

    private void ClearAllRenderers()
    {
        // НЕ сбрасываем primaryMaterial в null - оставляем материал назначенным
        // чтобы избежать перехода null↔material который сбрасывает фокус в Editor

        // Очищаем только mesh, но не материал
        canvasRenderer?.SetMesh(null);

        foreach (var subMesh in subMeshObjects)
        {
            if (subMesh != null)
                subMesh.Clear();
        }
    }

    private UniTextSubMesh CreateSubMesh(int index, Mesh mesh, Material material)
    {
        GameObject go;

#if UNITY_EDITOR
        // В Editor используем способ, который не регистрирует Undo и не сбрасывает фокус
        go = UnityEditor.EditorUtility.CreateGameObjectWithHideFlags(
            $"UniText SubMesh [{index}]",
            HideFlags.HideAndDontSave,
            typeof(RectTransform), typeof(CanvasRenderer), typeof(UniTextSubMesh));
#else
        go = new GameObject($"UniText SubMesh [{index}]", typeof(RectTransform), typeof(CanvasRenderer), typeof(UniTextSubMesh));
        go.hideFlags = HideFlags.HideAndDontSave;
#endif

        go.transform.SetParent(transform, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var subMesh = go.GetComponent<UniTextSubMesh>();
        subMesh.Initialize(this);

        // Устанавливаем mesh и material сразу после создания
        subMesh.SetMeshAndMaterial(mesh, material);

        return subMesh;
    }

    private bool shaderChannelsConfigured;

    private void EnsureCanvasShaderChannels()
    {
        // Делаем только один раз чтобы не сбрасывать фокус при каждом обновлении
        if (shaderChannelsConfigured) return;

        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        // TMP SDF шейдер использует UV1 (TEXCOORD1) для SDF scale данных
        var requiredChannels = AdditionalCanvasShaderChannels.TexCoord1;

        if ((canvas.additionalShaderChannels & requiredChannels) != requiredChannels)
        {
            canvas.additionalShaderChannels |= requiredChannels;
        }

        shaderChannelsConfigured = true;
    }

    private void RebuildText()
    {
        if (processor == null || fontProvider == null || meshGenerator == null)
        {
            Initialize();
            if (processor == null)
            {
                return;
            }
        }

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

            // Обработать текст через pipeline
            var glyphs = processor.Process(text.AsSpan(), settings);
            lastResultWidth = processor.ResultWidth;
            lastResultHeight = processor.ResultHeight;

            // Настроить mesh generator
            meshGenerator.FontSize = fontSize;
            meshGenerator.DefaultColor = color;

            // Установить параметры canvas/transform для правильного SDF сглаживания
            var canvas = GetComponentInParent<Canvas>();
            meshGenerator.SetCanvasParameters(transform, canvas);

            // Установить offset для корректного позиционирования относительно pivot
            meshGenerator.SetRectOffset(rect);

            // Сгенерировать mesh — возвращает список пар mesh/material
            lastMeshPairs = meshGenerator.GenerateMeshes(glyphs);
        }
        catch (Exception ex)
        {
            Debug.LogError($"UniText: Failed to rebuild text: {ex.Message}\n{ex.StackTrace}");
            lastMeshPairs = null;
        }
    }

    public override Texture mainTexture
    {
        get
        {
            // Используем материал из текущего mesh (для поддержки fallback)
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
            // Используем материал из текущего mesh (для поддержки fallback)
            if (primaryMaterial != null)
                return primaryMaterial;

            if (font != null)
                return font.material;

            return base.material;
        }
        set => base.material = value;
    }

    /// <summary>
    /// Получить позицию символа по индексу.
    /// </summary>
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

    /// <summary>
    /// Получить индекс символа по позиции (hit testing).
    /// </summary>
    public int GetCharacterIndexAtPosition(Vector2 localPosition)
    {
        if (processor == null)
            return -1;

        var glyphs = processor.PositionedGlyphs;
        float closestDist = float.MaxValue;
        int closestIndex = -1;

        for (int i = 0; i < glyphs.Length; i++)
        {
            var glyph = glyphs[i];
            float dist = Vector2.Distance(localPosition, new Vector2(glyph.x, glyph.y));

            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        return closestIndex;
    }
}
