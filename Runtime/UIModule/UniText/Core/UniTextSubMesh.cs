using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sub-mesh компонент для отображения глифов из fallback шрифтов.
/// Аналог TMP_SubMeshUI.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(RectTransform))]
public sealed class UniTextSubMesh : MaskableGraphic
{
    private UniText parentText;
    private Mesh mesh;
    private Material sharedMaterial;

    protected override void Awake()
    {
        base.Awake();
        raycastTarget = false;
    }

    public void Initialize(UniText parent)
    {
        parentText = parent;
    }

    public void SetMeshAndMaterial(Mesh newMesh, Material newMaterial)
    {
        mesh = newMesh;
        sharedMaterial = newMaterial;

        var cr = GetCanvasRenderer();
        if (cr == null) return;

        if (mesh == null || mesh.vertexCount == 0)
        {
            cr.Clear();
            return;
        }

        // Устанавливаем mesh напрямую
        cr.SetMesh(mesh);

        // Применяем материал напрямую для немедленного отображения
        ApplyMaterialToRenderer(cr);
    }

    public void Clear()
    {
        mesh = null;
        sharedMaterial = null;

        var cr = GetCanvasRenderer();
        cr?.Clear();
    }

    private CanvasRenderer GetCanvasRenderer()
    {
        // canvasRenderer от Graphic может быть null сразу после AddComponent
        return GetComponent<CanvasRenderer>();
    }

    /// <summary>
    /// Применяет материал к CanvasRenderer.
    /// </summary>
    private void ApplyMaterialToRenderer(CanvasRenderer cr)
    {
        if (cr == null || sharedMaterial == null)
        {
            cr?.Clear();
            return;
        }

        cr.materialCount = 1;
        cr.SetMaterial(materialForRendering, 0);
        cr.SetTexture(mainTexture);
    }

    /// <summary>
    /// Вызывается Unity UI системой для применения материала.
    /// </summary>
    protected override void UpdateMaterial()
    {
        ApplyMaterialToRenderer(GetCanvasRenderer());
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
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

    public override Texture mainTexture
    {
        get
        {
            if (sharedMaterial != null)
                return sharedMaterial.mainTexture;

            return base.mainTexture;
        }
    }

    public override Material material
    {
        get => sharedMaterial ?? base.material;
        set
        {
            if (sharedMaterial == value)
                return;

            sharedMaterial = value;
            ApplyMaterialToRenderer(GetCanvasRenderer());
        }
    }
}
