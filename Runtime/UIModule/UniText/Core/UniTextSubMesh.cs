using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sub-mesh компонент для отображения глифов из fallback шрифтов.
/// НЕ наследует MaskableGraphic чтобы избежать rebuild loop при создании во время Rebuild().
/// Управляет CanvasRenderer напрямую.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public sealed class UniTextSubMesh : MonoBehaviour, IMaterialModifier
{
    private UniText parentText;
    private CanvasRenderer canvasRenderer;
    private Mesh mesh;
    private Material sharedMaterial;

    private void Awake()
    {
        canvasRenderer = GetComponent<CanvasRenderer>();
    }

    public void Initialize(UniText parent)
    {
        parentText = parent;
        canvasRenderer = GetComponent<CanvasRenderer>();
    }

    public void SetMeshAndMaterial(Mesh newMesh, Material newMaterial)
    {
        mesh = newMesh;
        sharedMaterial = newMaterial;

        if (canvasRenderer == null)
            canvasRenderer = GetComponent<CanvasRenderer>();

        if (canvasRenderer == null) return;

        if (mesh == null || mesh.vertexCount == 0)
        {
            canvasRenderer.Clear();
            return;
        }

        // Устанавливаем mesh напрямую
        canvasRenderer.SetMesh(mesh);

        // Применяем материал
        ApplyMaterial();
    }

    public void Clear()
    {
        mesh = null;
        // НЕ сбрасываем sharedMaterial чтобы избежать мерцания

        if (canvasRenderer == null)
            canvasRenderer = GetComponent<CanvasRenderer>();

        canvasRenderer?.SetMesh(null);
    }

    /// <summary>
    /// Применяет материал к CanvasRenderer.
    /// </summary>
    private void ApplyMaterial()
    {
        if (canvasRenderer == null || sharedMaterial == null)
        {
            canvasRenderer?.Clear();
            return;
        }

        // Получаем модифицированный материал (для масок и т.д.)
        var materialToUse = GetModifiedMaterial(sharedMaterial);

        canvasRenderer.materialCount = 1;
        canvasRenderer.SetMaterial(materialToUse, 0);
        canvasRenderer.SetTexture(sharedMaterial.mainTexture);
    }

    /// <summary>
    /// IMaterialModifier implementation для поддержки масок.
    /// </summary>
    public Material GetModifiedMaterial(Material baseMaterial)
    {
        if (parentText == null)
            return baseMaterial;

        // Получаем mask материал через parent
        var maskable = parentText as IMaterialModifier;
        if (maskable != null)
        {
            return maskable.GetModifiedMaterial(baseMaterial);
        }

        return baseMaterial;
    }

    public Material Material
    {
        get => sharedMaterial;
        set
        {
            if (sharedMaterial == value)
                return;

            sharedMaterial = value;
            ApplyMaterial();
        }
    }
}
