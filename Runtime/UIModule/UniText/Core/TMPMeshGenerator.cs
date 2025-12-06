using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


/// <summary>
/// Результат генерации mesh — содержит mesh и материал для рендеринга.
/// </summary>
public struct MeshMaterialPair
{
    public Mesh mesh;
    public Material material;
    public TMP_FontAsset fontAsset;
}

/// <summary>
/// Генератор mesh для текста с использованием TMP шрифтов.
/// Поддерживает fallback шрифты через генерацию отдельных mesh для каждого материала.
/// </summary>
public sealed class TMPMeshGenerator
{
    // Данные для каждого материала
    private sealed class MaterialMeshData
    {
        public readonly List<Vector3> vertices = new(256);
        public readonly List<Vector4> uvs0 = new(256);
        public readonly List<Vector2> uvs2 = new(256);
        public readonly List<Color32> colors = new(256);
        public readonly List<int> triangles = new(384);
        public TMP_FontAsset fontAsset;
        public Material material;

        public void Clear()
        {
            vertices.Clear();
            uvs0.Clear();
            uvs2.Clear();
            colors.Clear();
            triangles.Clear();
            fontAsset = null;
            material = null;
        }
    }

    private readonly TMPFontProvider fontProvider;
    private readonly IUnicodeDataProvider unicodeData;
    private float fontSize = 36f;
    private Color32 defaultColor = new(255, 255, 255, 255);

    // Параметры для правильного расчёта SDF scale
    private float lossyScale = 1f;
    private float canvasScaleFactor = 1f;
    private RenderMode canvasRenderMode = RenderMode.ScreenSpaceOverlay;
    private bool isCameraAssigned = false;

    // Offset для корректного позиционирования относительно pivot
    private float rectOffsetX;
    private float rectOffsetY;

    // Группировка по материалам
    private readonly Dictionary<Material, MaterialMeshData> materialGroups = new();
    private readonly List<MaterialMeshData> meshDataPool = new();
    private readonly List<MeshMaterialPair> resultPairs = new();

    public TMPMeshGenerator(TMPFontProvider fontProvider, IUnicodeDataProvider unicodeData)
    {
        this.fontProvider = fontProvider ?? throw new ArgumentNullException(nameof(fontProvider));
        this.unicodeData = unicodeData ?? throw new ArgumentNullException(nameof(unicodeData));
    }

    public float FontSize
    {
        get => fontSize;
        set => fontSize = Mathf.Max(1f, value);
    }

    public Color32 DefaultColor
    {
        get => defaultColor;
        set => defaultColor = value;
    }

    /// <summary>
    /// Установить параметры canvas/transform для корректного SDF сглаживания.
    /// </summary>
    public void SetCanvasParameters(Transform transform, Canvas canvas)
    {
        if (transform != null)
        {
            lossyScale = Mathf.Abs(transform.lossyScale.y);
        }
        else
        {
            lossyScale = 1f;
        }

        if (canvas != null)
        {
            canvasScaleFactor = canvas.scaleFactor;
            canvasRenderMode = canvas.renderMode;
            isCameraAssigned = canvas.worldCamera != null;
        }
        else
        {
            canvasScaleFactor = 1f;
            canvasRenderMode = RenderMode.ScreenSpaceOverlay;
            isCameraAssigned = false;
        }
    }

    /// <summary>
    /// Установить offset для корректного позиционирования относительно pivot.
    /// rect.xMin и rect.yMax уже учитывают pivot RectTransform.
    /// </summary>
    public void SetRectOffset(Rect rect)
    {
        // Layout генерирует координаты от (0,0), но Unity UI рендерит относительно pivot.
        // rect.xMin — это левый край относительно pivot (отрицательный если pivot справа от левого края)
        // rect.yMax — это верхний край относительно pivot (положительный если pivot ниже верхнего края)
        rectOffsetX = rect.xMin;
        rectOffsetY = rect.yMax; // Y инвертируется в AddGlyph, поэтому используем yMax
    }

    /// <summary>
    /// Сгенерировать mesh из результата layout.
    /// Для обратной совместимости — все глифы в один mesh (работает только если все из одного шрифта).
    /// </summary>
    public void GenerateMesh(ReadOnlySpan<PositionedGlyph> glyphs, Mesh mesh)
    {
        var pairs = GenerateMeshes(glyphs);

        mesh.Clear();
        if (pairs.Count > 0 && pairs[0].mesh != null)
        {
            // Копируем данные из первого mesh
            mesh.vertices = pairs[0].mesh.vertices;
            mesh.SetUVs(0, new List<Vector4>(pairs[0].mesh.uv.Length)); // Workaround
            pairs[0].mesh.GetUVs(0, new List<Vector4>());

            // Прямое копирование
            mesh.SetVertices(new List<Vector3>(pairs[0].mesh.vertices));
            var uvs0List = new List<Vector4>();
            pairs[0].mesh.GetUVs(0, uvs0List);
            mesh.SetUVs(0, uvs0List);
            var uvs1List = new List<Vector2>();
            pairs[0].mesh.GetUVs(1, uvs1List);
            mesh.SetUVs(1, uvs1List);
            mesh.SetColors(new List<Color32>(pairs[0].mesh.colors32));
            mesh.SetTriangles(pairs[0].mesh.triangles, 0);
            mesh.RecalculateBounds();
        }
    }

    /// <summary>
    /// Сгенерировать несколько mesh — по одному для каждого уникального материала.
    /// Используется для корректного рендеринга fallback шрифтов.
    /// </summary>
    public List<MeshMaterialPair> GenerateMeshes(ReadOnlySpan<PositionedGlyph> glyphs)
    {
        ClearAll();

        foreach (var glyph in glyphs)
        {
            AddGlyph(glyph);
        }

        return BuildMeshes();
    }

    private void ClearAll()
    {
        // Возвращаем данные в пул
        foreach (var kvp in materialGroups)
        {
            kvp.Value.Clear();
            meshDataPool.Add(kvp.Value);
        }
        materialGroups.Clear();
        resultPairs.Clear();
    }

    private MaterialMeshData GetOrCreateMeshData(Material material, TMP_FontAsset fontAsset)
    {
        if (!materialGroups.TryGetValue(material, out var data))
        {
            // Берём из пула или создаём новый
            if (meshDataPool.Count > 0)
            {
                data = meshDataPool[meshDataPool.Count - 1];
                meshDataPool.RemoveAt(meshDataPool.Count - 1);
            }
            else
            {
                data = new MaterialMeshData();
            }

            data.material = material;
            data.fontAsset = fontAsset;
            materialGroups[material] = data;
        }
        return data;
    }

    private List<MeshMaterialPair> BuildMeshes()
    {
        foreach (var kvp in materialGroups)
        {
            var data = kvp.Value;
            if (data.vertices.Count == 0) continue;

            var mesh = new Mesh { name = $"UniText Mesh ({data.fontAsset?.name ?? "unknown"})" };
            mesh.SetVertices(data.vertices);
            mesh.SetUVs(0, data.uvs0);
            mesh.SetUVs(1, data.uvs2);
            mesh.SetColors(data.colors);
            mesh.SetTriangles(data.triangles, 0);
            mesh.RecalculateBounds();

            resultPairs.Add(new MeshMaterialPair
            {
                mesh = mesh,
                material = data.material,
                fontAsset = data.fontAsset
            });
        }

        return resultPairs;
    }

    /// <summary>
    /// Проверка на символы, которые не должны генерировать видимый mesh.
    /// Использует Unicode General_Category из IUnicodeDataProvider.
    /// </summary>
    private bool ShouldSkipGlyph(int codepoint)
    {
        var gc = unicodeData.GetGeneralCategory(codepoint);

        // Категории, не требующие mesh:
        // Zs - Space Separator (пробелы)
        // Zl - Line Separator
        // Zp - Paragraph Separator
        // Cc - Control characters (tab, newline, etc.)
        // Cf - Format characters (zero-width joiners, etc.)
        return gc == GeneralCategory.Zs
            || gc == GeneralCategory.Zl
            || gc == GeneralCategory.Zp
            || gc == GeneralCategory.Cc
            || gc == GeneralCategory.Cf;
    }

    private void AddGlyph(in PositionedGlyph glyph)
    {
        // Пропускаем whitespace и control символы
        if (ShouldSkipGlyph(glyph.glyphId))
        {
            return;
        }

        // Получить информацию о глифе из font provider
        if (!fontProvider.TryGetGlyphRenderInfo(glyph.fontId, glyph.glyphId, out var renderInfo))
        {
            return;
        }

        // Пропускаем глифы с нулевым размером
        if (renderInfo.width <= 0 || renderInfo.height <= 0)
        {
            return;
        }

        var sourceFontAsset = renderInfo.sourceFontAsset;
        if (sourceFontAsset == null)
        {
            return;
        }

        // Получаем или создаём данные для этого материала
        var material = sourceFontAsset.material;
        if (material == null)
        {
            return;
        }

        var meshData = GetOrCreateMeshData(material, sourceFontAsset);

        // Используем default color (TODO: поддержка атрибутов цвета)
        Color32 color = defaultColor;

        // Padding и scale из шрифта-источника
        var faceInfo = sourceFontAsset.faceInfo;
        float scale = fontSize / faceInfo.pointSize * faceInfo.scale;
        float atlasPadding = sourceFontAsset.atlasPadding;
        float padding = atlasPadding * scale;

        float atlasWidth = renderInfo.atlasWidth;
        float atlasHeight = renderInfo.atlasHeight;

        // Позиции вершин с учётом offset для pivot
        float baseX = glyph.x + rectOffsetX;
        float baseY = rectOffsetY - glyph.y; // Инвертируем Y для Unity UI и добавляем offset

        float left = baseX + renderInfo.bearingX - padding;
        float top = baseY + renderInfo.bearingY + padding;
        float right = left + renderInfo.width + padding * 2;
        float bottom = top - renderInfo.height - padding * 2;

        // UV координаты с padding для SDF
        float uvLeft = (renderInfo.uvX - atlasPadding) / atlasWidth;
        float uvBottom = (renderInfo.uvY - atlasPadding) / atlasHeight;
        float uvRight = (renderInfo.uvX + renderInfo.uvWidth + atlasPadding) / atlasWidth;
        float uvTop = (renderInfo.uvY + renderInfo.uvHeight + atlasPadding) / atlasHeight;

        // xScale для SDF сглаживания
        float xScale = scale;
        switch (canvasRenderMode)
        {
            case RenderMode.ScreenSpaceOverlay:
                xScale *= lossyScale / canvasScaleFactor;
                break;
            case RenderMode.ScreenSpaceCamera:
                xScale *= isCameraAssigned ? lossyScale : 1f;
                break;
            case RenderMode.WorldSpace:
                xScale *= lossyScale;
                break;
        }

        // Добавить quad в соответствующую группу
        int vertexIndex = meshData.vertices.Count;

        // TMP vertex order: BL, TL, TR, BR
        meshData.vertices.Add(new Vector3(left, bottom, 0));
        meshData.vertices.Add(new Vector3(left, top, 0));
        meshData.vertices.Add(new Vector3(right, top, 0));
        meshData.vertices.Add(new Vector3(right, bottom, 0));

        // UV0: xy = UV, w = xScale
        meshData.uvs0.Add(new Vector4(uvLeft, uvBottom, 0, xScale));
        meshData.uvs0.Add(new Vector4(uvLeft, uvTop, 0, xScale));
        meshData.uvs0.Add(new Vector4(uvRight, uvTop, 0, xScale));
        meshData.uvs0.Add(new Vector4(uvRight, uvBottom, 0, xScale));

        // UV2
        meshData.uvs2.Add(new Vector2(0, 0));
        meshData.uvs2.Add(new Vector2(0, 1));
        meshData.uvs2.Add(new Vector2(1, 1));
        meshData.uvs2.Add(new Vector2(1, 0));

        // Colors
        meshData.colors.Add(color);
        meshData.colors.Add(color);
        meshData.colors.Add(color);
        meshData.colors.Add(color);

        // Triangles - TMP порядок: (0,1,2), (2,3,0)
        meshData.triangles.Add(vertexIndex);
        meshData.triangles.Add(vertexIndex + 1);
        meshData.triangles.Add(vertexIndex + 2);

        meshData.triangles.Add(vertexIndex + 2);
        meshData.triangles.Add(vertexIndex + 3);
        meshData.triangles.Add(vertexIndex);
    }

}